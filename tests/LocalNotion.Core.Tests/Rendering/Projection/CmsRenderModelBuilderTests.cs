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
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using LocalNotion.Core;
using Notion.Client;
using NUnit.Framework;
using Sphere10.Framework;
using R = Sphere10.VisualRenderer;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Children)]
public class CmsRenderModelBuilderTests {
	private string _fixtureRoot;
	private CMSLocalNotionRepository _repository;

	[SetUp]
	public void SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-cms-projection-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
	}

	[TearDown]
	public void TearDown() {
		try {
			_repository?.Dispose();
		} finally {
			var fullPath = Path.GetFullPath(_fixtureRoot);
			if (!fullPath.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("Fixture path escaped its temporary parent.");
			if (Directory.Exists(fullPath))
				Directory.Delete(fullPath, true);
		}
	}

	[Test]
	public async Task ArticleProjectionPreservesSlugAndProvidesResolvedUrl(
		[Values("articles_category", "articles_category_active", "articles_summary", "articles_summary_alt")] string templateName,
		[Values("/", "https://example.test/content/")] string baseUrl
	) {
		var document = await CreateDocument(LocalNotionMode.Online, baseUrl);
		var template = Templates(document).Single(block => block.Template == templateName);
		var expectedSlug = ExpectedSlug(templateName);
		Assert.That(template.Tokens["slug"], Is.EqualTo(expectedSlug), "Legacy themes prepend '/' to this slug.");
		Assert.That(template.Tokens["url"], Is.EqualTo(baseUrl.TrimEnd('/') + "/" + expectedSlug), "Resolved URLs retain the configured hosting prefix.");
	}

	[Test]
	public async Task LegacyArticleOverridesKeepRootRelativeSlugLinks(
		[Values("articles_category", "articles_category_active", "articles_summary", "articles_summary_alt")] string templateName,
		[Values("/", "https://example.test/content/")] string baseUrl
	) {
		var document = await CreateDocument(LocalNotionMode.Online, baseUrl);
		var template = Templates(document).Single(block => block.Template == templateName);
		var themesDirectory = Path.Combine(_fixtureRoot, "overrides");
		var themeDirectory = Path.Combine(themesDirectory, "cms_articles");
		Directory.CreateDirectory(themeDirectory);
		// This is the pre-extraction theme's slug contract, including its explicit slash.
		var templatePath = Path.Combine(themeDirectory, templateName + ".html");
		const string legacyTemplate = "<a href=\"/{slug}\">{title}</a>";
		await File.WriteAllTextAsync(templatePath, legacyTemplate);
		var renderer = new R.HtmlRenderer(new R.ThemeCatalog(new R.ThemeOptions { ThemesDirectory = themesDirectory }));
		var html = renderer.Render(Fragment(template), new R.RenderOptions { Environment = R.RenderEnvironment.Online }).Html;
		var links = new HtmlParser().ParseDocument(html).QuerySelectorAll("a[href]");
		Assert.That(links, Has.Length.EqualTo(1));
		Assert.That(links[0].GetAttribute("href"), Is.EqualTo("/" + ExpectedSlug(templateName)));
		Assert.That(await File.ReadAllTextAsync(templatePath), Is.EqualTo(legacyTemplate), "Existing template files must remain unchanged.");
	}

	[TestCase(null, "Local Notion")]
	[TestCase("", "Local Notion")]
	[TestCase(" ", "Local Notion")]
	[TestCase("\t\r\n", "Local Notion")]
	[TestCase("Sphere10", "Sphere10")]
	[TestCase("  Named & Co.  ", "  Named & Co.  ")]
	public async Task AuthorUsesLegacyDefaultOnlyWhenMissing(string author, string expectedAuthor) {
		var document = await CreateDocument(LocalNotionMode.Online, "/", author);
		Assert.That(document.Author, Is.EqualTo(expectedAuthor));
		var html = new R.HtmlRenderer().Render(document, new R.RenderOptions { Environment = R.RenderEnvironment.Online }).Html;
		var parsed = new HtmlParser().ParseDocument(html);
		Assert.That(parsed.QuerySelector("meta[name=author]")?.GetAttribute("content"), Is.EqualTo(expectedAuthor));
		Assert.That(_repository.GetCMSItem("docs").Author, Is.EqualTo(author), "Rendering defaults must not mutate the stored author.");
	}

	[TestCaseSource(nameof(EmbeddedLinkCases))]
	public async Task EmbeddedArticleTemplatesUseResolvedUrls(string templateName, LocalNotionMode mode, string baseUrl, string expectedUrl) {
		var document = await CreateDocument(mode, baseUrl);
		var template = Templates(document).Single(block => block.Template == templateName);
		var options = new R.RenderOptions {
			Environment = mode == LocalNotionMode.Online ? R.RenderEnvironment.Online : R.RenderEnvironment.Offline
		};
		var html = new R.HtmlRenderer().Render(Fragment(template), options).Html;
		var links = new HtmlParser().ParseDocument(html).QuerySelectorAll("a[href]");
		Assert.That(links, Has.Length.EqualTo(templateName.StartsWith("articles_summary", StringComparison.Ordinal) ? 2 : 1));
		Assert.That(links.Select(link => link.GetAttribute("href")), Is.All.EqualTo(expectedUrl));
	}

	private async Task<R.DocumentBlock> CreateDocument(LocalNotionMode mode, string baseUrl, string author = null) {
		var databaseId = Guid.NewGuid().ToString();
		_repository = (CMSLocalNotionRepository)await LocalNotionRepository.CreateNew(
			_fixtureRoot, cmsDatabaseID: databaseId,
			pathProfile: new LocalNotionPathProfile { Mode = mode, BaseUrl = baseUrl }, logger: new NoOpLogger()
		);
		_repository.AddObject(new Database { Id = databaseId });
		_repository.AddResourceGraph(new NotionObjectGraph { ObjectID = databaseId });
		_repository.AddResource(new LocalNotionDatabase { ID = databaseId, Name = "cms", Title = "CMS", PrimaryDataSourceID = Guid.NewGuid().ToString("N") });
		var first = AddArticle(databaseId, "First article", "docs/first", 1);
		var second = AddArticle(databaseId, "Second article", "docs/second", 2);
		var items = new[] {
			new CMSItem { ItemType = CMSItemType.CategoryPage, Slug = "", Title = "All articles", Parts = [first.ID, second.ID], RenderPath = "cms/export/index.html" },
			new CMSItem { ItemType = CMSItemType.CategoryPage, Slug = "docs", Title = "Docs", Parts = [first.ID, second.ID], RenderPath = "cms/export/docs.html" },
			new CMSItem { ItemType = CMSItemType.Page, Slug = "docs/first", Title = first.Title, Parts = [first.ID], RenderPath = "cms/export/first-article.html" },
			new CMSItem { ItemType = CMSItemType.Page, Slug = "docs/second", Title = second.Title, Parts = [second.ID], RenderPath = "cms/export/second-article.html" }
		};
		items[1].Author = author;
		foreach (var item in items)
			_repository.AddOrUpdateCMSItem(item);
		return new CmsRenderModelBuilder(_repository, Path.Combine(_fixtureRoot, "cms", "export")).Build(items[1]);
	}

	private LocalNotionPage AddArticle(string databaseId, string title, string slug, int sequence) {
		var pageId = Guid.NewGuid().ToString();
		var page = new LocalNotionPage {
			ID = pageId, Name = title, Title = title, ParentResourceID = databaseId, Keywords = [],
			CreatedOn = new DateTime(2026, 1, 1), LastEditedOn = new DateTime(2026, 1, 2)
		};
		_repository.AddResource(page);
		page.CMSProperties = new CMSProperties {
			PageType = CMSPageType.Page, CustomSlug = slug, Root = "Docs", Category1 = "Articles",
			Status = CMSPageStatus.Published, Themes = [], Tags = [], Summary = "Article summary", Sequence = sequence
		};
		return page;
	}

	private static R.DocumentBlock Fragment(R.TemplateBlock template) => new() {
		RenderFrame = false, ShowPageHeader = false, Themes = ["cms_articles"], Children = [template]
	};

	private static IEnumerable<R.TemplateBlock> Templates(R.VisualNode node) {
		if (node is R.TemplateBlock template) {
			yield return template;
			foreach (var nested in template.Slots.Values.SelectMany(Templates))
				yield return nested;
		}
		var children = node switch {
			R.DocumentBlock document => document.Children,
			R.GroupBlock group => group.Children,
			_ => Array.Empty<R.VisualNode>()
		};
		foreach (var nested in children.SelectMany(Templates))
			yield return nested;
	}

	private static string ExpectedSlug(string templateName) => templateName switch {
		"articles_category" => "",
		"articles_category_active" => "docs",
		"articles_summary" => "docs/first",
		"articles_summary_alt" => "docs/second",
		_ => throw new ArgumentOutOfRangeException(nameof(templateName))
	};

	private static IEnumerable<TestCaseData> EmbeddedLinkCases() {
		foreach (var (templateName, slug, file) in new[] {
			("articles_category", "", "index.html"),
			("articles_category_active", "docs", "docs.html"),
			("articles_summary", "docs/first", "first-article.html"),
			("articles_summary_alt", "docs/second", "second-article.html")
		}) {
			yield return new TestCaseData(templateName, LocalNotionMode.Online, "/", "/" + slug);
			yield return new TestCaseData(templateName, LocalNotionMode.Online, "https://example.test/content/", "https://example.test/content/" + slug);
			yield return new TestCaseData(templateName, LocalNotionMode.Offline, "", file);
		}
	}
}
