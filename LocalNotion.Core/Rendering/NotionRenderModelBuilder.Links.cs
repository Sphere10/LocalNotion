// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Notion.Client;
using Sphere10.Framework;

namespace LocalNotion.Core;

public sealed partial class NotionRenderModelBuilder {

	/// <summary>Resolves source URLs before the independent renderer sees them.</summary>
	public string ResolveUrl(LocalNotionEditableResource source, string url, string outputDirectory) {
		if (string.IsNullOrWhiteSpace(url))
			return string.Empty;
		ArgumentNullException.ThrowIfNull(source);
		if (url.StartsWith('#'))
			return "#" + NormalizeID(url[1..]);
		if (LocalNotionRenderLink.TryParse(url, out var renderLink))
			return TryResolveResourceUrl(source, renderLink.ResourceID, renderLink.RenderType, outputDirectory, out var resolved, out _) ? resolved : string.Empty;
		if (IsInternalResourceUrl(url))
			return string.Empty;

		if (TryGetSourceLink(url, out var objectID, out var fragment)) {
			if (TryResolveResourceUrl(source, objectID, RenderType.HTML, outputDirectory, out var resolved, out _, canonicalOnlineUrl: url.StartsWith("/p/", StringComparison.OrdinalIgnoreCase)))
				return string.IsNullOrEmpty(fragment) ? resolved : resolved.Split('#')[0] + "#" + NormalizeID(fragment);
			// Preserve authored URLs when their targets are outside the mirrored repository.
			return url;
		}

		if (_repository is not null && _repository.Paths.Mode == LocalNotionMode.Online)
			return Links.Process(url);
		return RebaseRelativeUrl(source, url, outputDirectory);
	}

	public string ResolveMediaUrl(LocalNotionEditableResource source, FileObject file, string outputDirectory)
		=> file switch {
			ExternalFile external => ResolveUrl(source, external.External?.Url, outputDirectory),
			UploadedFile uploaded => ResolveUploadedUrl(source, uploaded.File?.Url, outputDirectory),
			_ => string.Empty
		};

	public string ResolveMediaUrl(LocalNotionEditableResource source, FileObjectWithName file, string outputDirectory)
		=> file switch {
			ExternalFileWithName external => ResolveUrl(source, external.External?.Url, outputDirectory),
			UploadedFileWithName uploaded => ResolveUploadedUrl(source, uploaded.File?.Url, outputDirectory),
			_ => string.Empty
		};

	private string ResolveRichTextUrl(ProjectionContext context, string url) {
		if (string.IsNullOrWhiteSpace(url))
			return null;
		if (context.Purpose == ProjectionPurpose.Text)
			return url;
		if (LocalNotionRenderLink.TryParse(url, out var renderLink))
			return TryResolveResourceUrl(context.Resource, renderLink.ResourceID, renderLink.RenderType, context.OutputDirectory, out var resolved, out _) ? resolved : null;
		if (IsInternalResourceUrl(url))
			return null;
		return ResolveUrl(context.Resource, url, context.OutputDirectory);
	}

	private static bool IsInternalResourceUrl(string url)
		=> url.StartsWith("resource://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("localnotion://", StringComparison.OrdinalIgnoreCase);

	private string ResolveUploadedUrl(LocalNotionEditableResource source, string url, string outputDirectory) {
		if (string.IsNullOrWhiteSpace(url))
			return string.Empty;
		// Older stored uploads can contain a render slug instead of a resource handle.
		if (_repository is not null && Links.TryResolveResourceRender(url, out var resource, out _)
			&& TryResolveResourceUrl(source, resource.ID, RenderType.File, outputDirectory, out var resolved, out _))
			return resolved;
		return ResolveUrl(source, url, outputDirectory);
	}

	private bool TryResolveResourceUrl(LocalNotionEditableResource source, string objectID, RenderType type, string outputDirectory, out string url, out LocalNotionResource target, bool canonicalOnlineUrl = false) {
		url = string.Empty;
		target = null;
		if (_repository is null || string.IsNullOrWhiteSpace(objectID))
			return false;
		// Repository IDs follow the existing persisted representation (commonly dashed).
		// DOM anchors are normalized separately; changing lookup IDs breaks saved graphs.
		objectID = FindStoredObjectID(source, objectID);
		// Legacy online /p/ routes retain the hosted destination even when they point to the current page.
		var from = canonicalOnlineUrl && _repository.Paths.Mode == LocalNotionMode.Online ? null : source;
		if (!Links.TryGenerate(from, objectID, type, out var generated, out target))
			return false;
		generated = NormalizeFragment(generated);
		url = _repository.Paths.Mode == LocalNotionMode.Offline
			? RebaseRelativeUrl(source, generated, outputDirectory, generatedFromResourceFolder: true)
			: Links.Process(generated);
		return true;
	}

	private string RebaseRelativeUrl(LocalNotionEditableResource source, string url, string outputDirectory, bool generatedFromResourceFolder = false) {
		if (string.IsNullOrEmpty(url) || url.StartsWith('#') || url.StartsWith('/') || Uri.TryCreate(url, UriKind.Absolute, out _)
			|| _repository is null || string.IsNullOrWhiteSpace(outputDirectory))
			return url ?? string.Empty;
		var suffixIndex = url.IndexOfAny(['?', '#']);
		var pathValue = suffixIndex < 0 ? url : url[..suffixIndex];
		var suffix = suffixIndex < 0 ? string.Empty : url[suffixIndex..];
		if (string.IsNullOrEmpty(pathValue))
			return url;
		var originalDirectory = _repository.Paths.GetResourceFolderPath(source.Type, source.ID, FileSystemPathType.Absolute);
		if (!generatedFromResourceFolder && source.TryGetRender(RenderType.HTML, out var render) && !string.IsNullOrWhiteSpace(render.LocalPath))
			originalDirectory = Path.GetDirectoryName(Path.GetFullPath(render.LocalPath, _repository.Paths.GetRepositoryPath(FileSystemPathType.Absolute)));
		var absolute = Path.GetFullPath(Uri.UnescapeDataString(pathValue).Replace('/', Path.DirectorySeparatorChar), originalDirectory);
		return Path.GetRelativePath(Path.GetFullPath(outputDirectory), absolute).Replace('\\', '/') + suffix;
	}

	private static bool TryGetSourceLink(string url, out string objectID, out string fragment) {
		objectID = null;
		fragment = string.Empty;
		var candidate = url;
		if (!url.StartsWith('/') && Uri.TryCreate(url, UriKind.Absolute, out var uriValue)) {
			if (uriValue.Scheme is not ("http" or "https") ||
				!(uriValue.Host.Equals("notion.so", StringComparison.OrdinalIgnoreCase) || uriValue.Host.EndsWith(".notion.so", StringComparison.OrdinalIgnoreCase)
				|| uriValue.Host.Equals("notion.site", StringComparison.OrdinalIgnoreCase) || uriValue.Host.EndsWith(".notion.site", StringComparison.OrdinalIgnoreCase)))
				return false;
			candidate = uriValue.AbsolutePath + uriValue.Fragment;
		}
		if (!candidate.StartsWith('/'))
			return false;
		var hash = candidate.IndexOf('#');
		if (hash >= 0) {
			fragment = candidate[(hash + 1)..];
			candidate = candidate[..hash];
		}
		candidate = candidate.Split('?')[0].Trim('/');
		if (candidate.StartsWith("p/", StringComparison.OrdinalIgnoreCase))
			candidate = candidate[2..];
		if (Guid.TryParse(candidate, out var guidValue)) {
			objectID = guidValue.ToString("N");
			return true;
		}
		var match = Regex.Match(candidate, @"(?:^|[-/])([0-9a-fA-F]{32})$");
		if (!match.Success)
			return false;
		objectID = match.Groups[1].Value.ToLowerInvariant();
		return true;
	}

	private string FindStoredObjectID(LocalNotionEditableResource source, string id) {
		if (NormalizeID(source.ID) == NormalizeID(id))
			return source.ID;
		foreach (var candidate in IDCandidates(id))
			if (_repository.TryGetResource(candidate, out var resource))
				return resource.ID;
		foreach (var candidate in IDCandidates(id))
			if (_repository.TryGetObject(candidate, out var sourceObject))
				return sourceObject.Id;
		return id;
	}

	private bool TryGetResource(string id, out LocalNotionResource resource) {
		resource = null;
		if (_repository is null || string.IsNullOrWhiteSpace(id))
			return false;
		foreach (var candidate in IDCandidates(id))
			if (_repository.TryGetResource(candidate, out resource))
				return true;
		return false;
	}

	private static IEnumerable<string> IDCandidates(string id) {
		if (string.IsNullOrWhiteSpace(id))
			yield break;
		yield return id;
		if (!Guid.TryParse(id, out var guidValue))
			yield break;
		var dashed = guidValue.ToString("D");
		var compact = guidValue.ToString("N");
		if (dashed != id)
			yield return dashed;
		if (compact != id)
			yield return compact;
	}

	private static string NormalizeFragment(string url) {
		if (string.IsNullOrEmpty(url))
			return url ?? string.Empty;
		var hash = url.IndexOf('#');
		return hash < 0 ? url : url[..(hash + 1)] + NormalizeID(url[(hash + 1)..]);
	}

	private static string NormalizeID(string id) => Guid.TryParse(id, out var guidValue) ? guidValue.ToString("N") : id ?? string.Empty;
}
