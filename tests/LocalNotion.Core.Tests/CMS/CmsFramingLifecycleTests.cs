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
using System.Threading.Tasks;
using LocalNotion.Core;
using Notion.Client;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[Parallelizable(ParallelScope.Children)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CmsFramingLifecycleTests {
	private string _fixtureRoot;
	private CMSLocalNotionRepository _repository;

	[SetUp]
	public async Task SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-cms-framing-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
		var databaseId = Guid.NewGuid().ToString();
		_repository = (CMSLocalNotionRepository)await LocalNotionRepository.CreateNew(_fixtureRoot, cmsDatabaseID: databaseId);
		_repository.AddObject(new Database { Id = databaseId, Title = [Text("CMS")], Description = [], Parent = new WorkspaceParent() });
		_repository.AddResourceGraph(new NotionObjectGraph { ObjectID = databaseId });
		_repository.AddResource(new LocalNotionDatabase { ID = databaseId, PrimaryDataSourceID = Guid.NewGuid().ToString(), Name = "cms", Title = "CMS", Keywords = [] });
	}

	[TearDown]
	public void TearDown() {
		_repository?.Dispose();
		if (!Directory.Exists(_fixtureRoot))
			return;
		var expectedParent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar);
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_fixtureRoot)), Is.EqualTo(expectedParent));
		Assert.That(Path.GetFileName(_fixtureRoot), Does.StartWith("localnotion-cms-framing-"));
		Directory.Delete(_fixtureRoot, recursive: true);
	}

	[Test]
	public void RemovingFramingClearsReferencesWhenCmsContentIsMissing(
		[Values(CMSPageType.Header, CMSPageType.NavBar, CMSPageType.Footer, CMSPageType.Internal)] CMSPageType framingType,
		[Values] bool hiddenContent
	) {
		var article = AddPage(CMSPageType.Page, "docs/article");
		AddPage(CMSPageType.Page, "other/article");
		var framing = AddPage(framingType, "docs/article");
		var staleItem = AddStaleItem(framingType, framing.ID, hiddenContent);
		var articleItem = _repository.GetCMSItem(article.CMSProperties.CustomSlug);
		var unrelatedItem = _repository.GetCMSItem("other/article");
		articleItem.Dirty = false;
		unrelatedItem.Dirty = false;
		Assert.That(articleItem.ReferencesResource(framing.ID), Is.True);
		Assert.That(_repository.CMSDatabase.GetContent(staleItem.Slug), Is.Null);

		Assert.That(() => _repository.RemoveResource(framing.ID, false), Throws.Nothing);

		Assert.That(_repository.ContainsResource(framing.ID), Is.False);
		Assert.That(articleItem.ReferencesResource(framing.ID), Is.False);
		Assert.That(articleItem.Dirty, Is.True);
		Assert.That(staleItem.ReferencesResource(framing.ID), Is.False, "A missing content node must not retain a removed framing resource.");
		Assert.That(staleItem.Dirty, Is.True);
		Assert.That(_repository.GetCMSItem(staleItem.Slug), Is.SameAs(staleItem), "Framing maintenance must preserve content until its normal lifecycle handles it.");
		Assert.That(staleItem.Parts, Has.Length.EqualTo(1));
		Assert.That(File.ReadAllText(Path.Combine(_fixtureRoot, staleItem.RenderPath)), Is.EqualTo("existing published content"));
		Assert.That(unrelatedItem.Dirty, Is.False, "Changing framing at another slug must not dirty unrelated content.");
	}

	[Test]
	public void AddingFramingUpdatesValidContentWhenAnotherCmsItemIsMissing(
		[Values(CMSPageType.Header, CMSPageType.NavBar, CMSPageType.Footer, CMSPageType.Internal)] CMSPageType framingType,
		[Values] bool hiddenContent
	) {
		var article = AddPage(CMSPageType.Page, "docs/article");
		AddPage(CMSPageType.Page, "other/article");
		var staleReference = Guid.NewGuid().ToString();
		var staleItem = AddStaleItem(framingType, staleReference, hiddenContent);
		var articleItem = _repository.GetCMSItem(article.CMSProperties.CustomSlug);
		var unrelatedItem = _repository.GetCMSItem("other/article");
		articleItem.Dirty = false;
		unrelatedItem.Dirty = false;
		LocalNotionPage framing = null;

		Assert.That(() => framing = AddPage(framingType, "docs/article"), Throws.Nothing);

		Assert.That(articleItem.ReferencesResource(framing.ID), Is.True);
		Assert.That(articleItem.Dirty, Is.True);
		Assert.That(staleItem.ReferencesResource(staleReference), Is.False);
		Assert.That(staleItem.Dirty, Is.True);
		Assert.That(_repository.GetCMSItem(staleItem.Slug), Is.SameAs(staleItem));
		Assert.That(File.ReadAllText(Path.Combine(_fixtureRoot, staleItem.RenderPath)), Is.EqualTo("existing published content"));
		Assert.That(unrelatedItem.Dirty, Is.False);
	}

	[Test]
	public void RefreshingFramingPreservesDescendantCmsItemWhileItsAncestryIsTemporarilyMissing(
		[Values(CMSPageType.Header, CMSPageType.NavBar, CMSPageType.Footer, CMSPageType.Internal)] CMSPageType framingType
	) {
		var framing = AddPage(framingType, "home");
		var child = AddPage(CMSPageType.Page, "home/website-terms-of-use", parentId: framing.ID, tags: [Constants.TagUseParentHeader]);
		var item = _repository.GetCMSItem(child.CMSProperties.CustomSlug);
		item.RenderPath = "cms/child.html";
		Directory.CreateDirectory(Path.Combine(_fixtureRoot, "cms"));
		File.WriteAllText(Path.Combine(_fixtureRoot, item.RenderPath), "existing child output");
		Assert.That(item.ReferencesResource(framing.ID), Is.True);

		Assert.That(() => _repository.RemoveResource(framing.ID, false), Throws.Nothing);

		Assert.That(_repository.ContainsResource(child.ID), Is.True);
		Assert.That(_repository.CMSDatabase.GetContent(item.Slug), Is.Null, "Removing the parent temporarily detaches its descendant from the CMS database.");
		Assert.That(_repository.GetCMSItem(item.Slug), Is.SameAs(item));
		Assert.That(item.Parts, Is.EqualTo(new[] { child.ID }));
		Assert.That(item.ReferencesResource(framing.ID), Is.False);
		Assert.That(File.ReadAllText(Path.Combine(_fixtureRoot, item.RenderPath)), Is.EqualTo("existing child output"));
		item.Dirty = false;

		Assert.That(() => _repository.AddResource(framing), Throws.Nothing);

		Assert.That(_repository.CMSDatabase.GetContent(item.Slug), Is.Not.Null);
		Assert.That(_repository.GetCMSItem(item.Slug), Is.SameAs(item));
		Assert.That(item.ReferencesResource(framing.ID), Is.True, "Refreshing the parent must restore framing before its descendant is rendered.");
		Assert.That(item.Dirty, Is.True);
		Assert.That(File.ReadAllText(Path.Combine(_fixtureRoot, item.RenderPath)), Is.EqualTo("existing child output"));
	}

	private LocalNotionPage AddPage(CMSPageType pageType, string slug, CMSPageStatus status = CMSPageStatus.Published, string parentId = null, string[] tags = null) {
		parentId ??= _repository.CMSDatabaseID;
		var pageId = Guid.NewGuid().ToString();
		_repository.AddObject(new Page {
			Id = pageId,
			Parent = new PageParent { PageId = parentId },
			Properties = new Dictionary<string, PropertyValue> { ["Name"] = new TitlePropertyValue { Id = "title", Title = [Text("Home")] } }
		});
		_repository.AddResourceGraph(new NotionObjectGraph { ObjectID = pageId });
		var page = new LocalNotionPage {
			ID = pageId,
			Name = "page-" + pageId,
			Title = "Home",
			ParentResourceID = parentId,
			Keywords = [],
			CMSProperties = new CMSProperties { PageType = pageType, Status = status, CustomSlug = slug, Themes = [], Tags = tags ?? [] }
		};
		_repository.AddResource(page);
		return page;
	}

	private CMSItem AddStaleItem(CMSPageType framingType, string framingId, bool hiddenContent) {
		var partId = hiddenContent ? AddPage(CMSPageType.Page, "stale/article", CMSPageStatus.Hidden).ID : Guid.NewGuid().ToString();
		var item = new CMSItem {
			ItemType = CMSItemType.Page,
			Slug = "stale/article",
			Title = "Stale article",
			Parts = [partId],
			HeaderID = framingType == CMSPageType.Header ? framingId : null,
			MenuID = framingType == CMSPageType.NavBar ? framingId : null,
			FooterID = framingType == CMSPageType.Footer ? framingId : null,
			InternalID = framingType == CMSPageType.Internal ? framingId : null,
			RenderPath = "cms/stale.html"
		};
		Directory.CreateDirectory(Path.Combine(_fixtureRoot, "cms"));
		File.WriteAllText(Path.Combine(_fixtureRoot, item.RenderPath), "existing published content");
		_repository.AddOrUpdateCMSItem(item);
		return item;
	}

	private static RichTextText Text(string text) => new() { PlainText = text, Text = new Text { Content = text } };
}
