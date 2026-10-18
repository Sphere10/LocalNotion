// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Notion.Client;
using Sphere10.Framework;

namespace LocalNotion.Core;

/// <summary>
/// Creates one CMS database, seeds presets through a temporary Hidden row, and verifies zero active rows.
/// Does not initialize a local repository, copy source content or retry creation requests.
/// </summary>
public sealed class CMSDatabaseCreator {
	private readonly INotionClient _client;
	private readonly CMSDatabaseTemplate _template;
	private readonly TimeSpan _requestInterval;

	public CMSDatabaseCreator(INotionClient client)
		: this(client, new CMSDatabaseTemplate()) {
	}

	public CMSDatabaseCreator(INotionClient client, CMSDatabaseTemplate template, TimeSpan? requestInterval = null) {
		Guard.ArgumentNotNull(client, nameof(client));
		Guard.ArgumentNotNull(template, nameof(template));
		Guard.Argument(!requestInterval.HasValue || requestInterval.Value >= TimeSpan.Zero, nameof(requestInterval), "Request spacing must not be negative.");
		_client = client;
		_template = template;
		_requestInterval = requestInterval ?? TimeSpan.FromMilliseconds(360);
	}

	public async Task<Database> CreateAsync(Guid parentPageID, string title, CancellationToken cancellationToken = default) {
		// Creation is not retried: a lost response can still mean Notion created the database.
		cancellationToken.ThrowIfCancellationRequested();
		var request = _template.CreateRequest(parentPageID, title);
		var database = await _client.Databases.CreateAsync(request, cancellationToken);
		var validDatabaseID = Guid.TryParse(database?.Id, out var databaseID) && databaseID != Guid.Empty;
		Guard.Ensure(validDatabaseID, "Notion returned no usable database ID. Check the parent before retrying.");
		var setupPageID = Guid.Empty;
		string dataSourceID = null;
		Exception setupError = null;

		// Page selections add presets beyond Notion's 100-option schema limit. Keep the row ID before sending further requests.
		try {
			dataSourceID = GetDataSourceID(database);
			var batches = _template.CreateOverflowBatches();
			if (batches.Length > 0) {
				setupPageID = await CreateSetupPageAsync(dataSourceID, batches[0], cancellationToken);
				await SeedPresetsAsync(setupPageID, batches.Skip(1), cancellationToken);
			}
		} catch (Exception error) {
			setupError = error;
		}

		// Archival is an awaited Notion operation, including after cancellation. Preserve both setup and cleanup failures.
		var setupPageArchived = false;
		Exception cleanupError = null;
		if (setupPageID != Guid.Empty) {
			try {
				await ArchiveSetupPageAsync(setupPageID);
				setupPageArchived = true;
			} catch (Exception error) {
				cleanupError = error;
			}
		}
		if (setupError != null || cleanupError != null)
			throw new CMSDatabaseCreationException(databaseID, setupPageID, setupPageArchived, setupError, cleanupError);

		// Report an incomplete database if readback fails; never retry creation or remove unrelated rows.
		try {
			await VerifyDatabaseAsync(dataSourceID, cancellationToken);
		} catch (Exception error) {
			throw new CMSDatabaseCreationException(databaseID, setupPageID, setupPageArchived, error);
		}
		return database;
	}

	private static string GetDataSourceID(Database database) {
		var dataSources = database.DataSources?.ToArray();
		Guard.Ensure(dataSources?.Length == 1 && Guid.TryParse(dataSources[0].DataSourceId, out var dataSourceID) && dataSourceID != Guid.Empty,
			"Notion returned no unique initial data source.");
		return dataSources[0].DataSourceId;
	}

	private async Task<Guid> CreateSetupPageAsync(string dataSourceID, Dictionary<string, PropertyValue> presetProperties, CancellationToken cancellationToken) {
		var properties = new Dictionary<string, PropertyValue>(presetProperties) {
			[Constants.TitlePropertyName] = new TitlePropertyValue {
				Title = new List<RichTextBase> { new RichTextText { Text = new Text { Content = "Local Notion CMS setup (temporary)" } } }
			},
			[Constants.PageTypePropertyName] = new SelectPropertyValue { Select = new SelectOption { Name = "Page" } },
			[Constants.StatusPropertyName] = new SelectPropertyValue { Select = new SelectOption { Name = "Hidden" } }
		};
		await Task.Delay(_requestInterval, cancellationToken);
		var setupPage = await _client.Pages.CreateAsync(new PagesCreateParameters {
			Parent = new DataSourceParentRequest { DataSourceId = dataSourceID },
			Properties = properties
		}, cancellationToken);
		var validPageID = Guid.TryParse(setupPage?.Id, out var setupPageID) && setupPageID != Guid.Empty;
		Guard.Ensure(validPageID, "Notion returned no usable setup row ID.");
		return setupPageID;
	}

	private async Task SeedPresetsAsync(Guid setupPageID, IEnumerable<Dictionary<string, PropertyValue>> batches, CancellationToken cancellationToken) {
		foreach (var batch in batches) {
			await Task.Delay(_requestInterval, cancellationToken);
			await _client.Pages.UpdateAsync(setupPageID.ToString("D"), new PagesUpdateParameters { Properties = batch }, cancellationToken);
		}
	}

	private async Task ArchiveSetupPageAsync(Guid setupPageID) {
		// Cleanup must outlive caller cancellation, but it must not wait indefinitely.
		using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
		await Task.Delay(_requestInterval, timeout.Token);
		var archived = await _client.Pages.UpdateAsync(setupPageID.ToString("D"), new PagesUpdateParameters { InTrash = true }, timeout.Token);
		Guard.Ensure(archived?.InTrash == true, "The temporary CMS setup row was not confirmed in the trash.");
	}

	private async Task VerifyDatabaseAsync(string dataSourceID, CancellationToken cancellationToken) {
		// Read back the saved schema and confirm there are no active rows.
		await Task.Delay(_requestInterval, cancellationToken);
		var dataSource = await _client.DataSources.RetrieveAsync(new RetrieveDataSourceRequest { DataSourceId = dataSourceID }, cancellationToken);
		_template.Validate(dataSource);
		await Task.Delay(_requestInterval, cancellationToken);
		var activeRows = await _client.DataSources.QueryAsync(new QueryDataSourceRequest { DataSourceId = dataSourceID, PageSize = 1 }, cancellationToken);
		Guard.Ensure(activeRows?.Results != null && activeRows.Results.Count == 0 && !activeRows.HasMore, "The new CMS database was not empty after setup.");
	}
}
