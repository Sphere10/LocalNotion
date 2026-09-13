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
using Notion.Client;
using NUnit.Framework;
using R = Sphere10.VisualRenderer;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.Children)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class RichTextLinkProjectionTests {
	private string _fixtureRoot;

	[SetUp]
	public void SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-rich-text-links-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
	}

	[TearDown]
	public void TearDown() {
		if (Directory.Exists(_fixtureRoot))
			Directory.Delete(_fixtureRoot, recursive: true);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase(" ")]
	public void AbsentSourceLinkHasNullVisualUrl(string rawUrl) {
		var repository = CreateRepository();
		var source = CreateSource(repository);
		Assert.That(Project(repository, source, rawUrl).Url, Is.Null);
	}

	[Test]
	public void ExplicitLinkToCurrentDocumentPreservesEmptyDestination([Values] LocalNotionMode mode) {
		var repository = CreateRepository(mode);
		var source = CreateSource(repository);
		var projected = Project(repository, source, "/" + source.ID);
		Assert.That(projected.Url, Is.EqualTo(string.Empty));
		Assert.That(projected.Text, Is.EqualTo("[2]"));
	}

	[Test]
	public void OnlinePageRouteToCurrentDocumentUsesCanonicalCmsUrl(
		[Values("N", "D")] string idFormat,
		[Values(false, true)] bool withAnchor,
		[Values("/", "https://example.test/site")] string baseUrl
	) {
		var repository = CreateRepository(LocalNotionMode.Online, baseUrl);
		var source = CreateSource(repository);
		var fragment = withAnchor ? "#reference-anchor" : string.Empty;
		var rawUrl = "/p/" + Guid.Parse(source.ID).ToString(idFormat) + fragment;
		var projected = Project(repository, source, rawUrl);
		Assert.That(projected.Url, Is.EqualTo(baseUrl.TrimEnd('/') + "/tech/self" + fragment));
		Assert.That(projected.PlainTextOverride, Is.EqualTo(rawUrl), "Text projection retains the author's source URL.");
	}

	[TestCase("/38051e4d5fa149e694c300db431f03e6#8076b90a5d3a4c7087d451ad42ce4b25")]
	[TestCase("/p/38051e4d5fa149e694c300db431f03e6")]
	[TestCase("https://www.notion.so/Reference-38051e4d5fa149e694c300db431f03e6")]
	public void UnmirroredSourceTargetRetainsAuthoredUrl(string rawUrl) {
		var repository = CreateRepository(LocalNotionMode.Online);
		var source = CreateSource(repository);
		var projected = Project(repository, source, rawUrl);
		Assert.That(projected.Url, Is.EqualTo(rawUrl));
		Assert.That(projected.Text, Is.EqualTo("[2]"));
	}

	[TestCase("resource://malformed")]
	[TestCase("localnotion://malformed")]
	public void InvalidInternalHandleHasNoVisualLink(string rawUrl) {
		var repository = CreateRepository();
		var source = CreateSource(repository);
		Assert.That(Project(repository, source, rawUrl).Url, Is.Null);
		Assert.That(new NotionRenderModelBuilder(repository).ResolveUrl(source, rawUrl, _fixtureRoot), Is.EqualTo(string.Empty));
	}

	[Test]
	public void UnresolvedResourceHandleHasNoVisualLink() {
		var repository = CreateRepository();
		var source = CreateSource(repository);
		var rawUrl = LocalNotionRenderLink.GenerateUrl(Guid.NewGuid().ToString("N"), RenderType.HTML);
		Assert.That(Project(repository, source, rawUrl).Url, Is.Null);
	}

	[Test]
	public void ResourceHandleToCurrentDocumentPreservesEmptyDestination() {
		var repository = CreateRepository();
		var source = CreateSource(repository);
		var rawUrl = LocalNotionRenderLink.GenerateUrl(source.ID, RenderType.HTML);
		Assert.That(Project(repository, source, rawUrl).Url, Is.EqualTo(string.Empty));
	}

	private FixtureRepository CreateRepository(LocalNotionMode mode = LocalNotionMode.Offline, string baseUrl = "/")
		=> new(new PathResolver(_fixtureRoot, new LocalNotionPathProfile { Mode = mode, BaseUrl = baseUrl }));

	private static LocalNotionPage CreateSource(FixtureRepository repository) {
		var source = new LocalNotionPage {
			ID = "0a1f1a97-ddeb-82d5-8703-81b7ed708c46",
			Title = "Source document",
			CMSProperties = new CMSProperties { CustomSlug = "tech/self" }
		};
		source.Renders[RenderType.HTML] = new RenderEntry { LocalPath = "pages/source.html", Slug = "source" };
		repository.ResourceMap.Add(source.ID, source);
		return source;
	}

	private R.TextInline Project(FixtureRepository repository, LocalNotionPage source, string rawUrl) {
		var run = new RichTextText { Text = new Text { Content = "[2]", Link = rawUrl == null ? null : new Link { Url = rawUrl } }, PlainText = "[2]", Href = rawUrl };
		var paragraph = new ParagraphBlock { Id = Guid.NewGuid().ToString("D"), Paragraph = new ParagraphBlock.Info { RichText = [run] } };
		var graph = new NotionObjectGraph { ObjectID = source.ID, Children = [new NotionObjectGraph { ObjectID = paragraph.Id, Children = [] }] };
		var objects = new Dictionary<string, IObject> { [source.ID] = new Page { Id = source.ID }, [paragraph.Id] = paragraph };
		var document = new NotionRenderModelBuilder(repository).Build(source, graph, objects, Path.Combine(_fixtureRoot, "cms"));
		return (R.TextInline)((R.ParagraphBlock)document.Children.Single()).Text.Single();
	}
}