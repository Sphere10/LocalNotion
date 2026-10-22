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
using Notion.Client;
using NUnit.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Unit")]
[Parallelizable(ParallelScope.Children)]
public class PageMetadataTests {

	[TestCase("Sphere 10 packages .NET applications into reproducible deployments. We own the release.", "Sphere 10 packages .NET applications into reproducible deployments.")]
	[TestCase("Sphere 10 builds ASP.NET Core systems for business. The server owns validation.", "Sphere 10 builds ASP.NET Core systems for business.")]
	[TestCase("We connect Node.js clients to .NET 10 services. We verify the API.", "We connect Node.js clients to .NET 10 services.")]
	[TestCase("Version 1.5 supports your workflow. Upgrades preserve records.", "Version 1.5 supports your workflow.")]
	[TestCase("We build software for your business. We maintain it.", "We build software for your business.")]
	[TestCase("A useful introduction without a terminal mark", "A useful introduction without a terminal mark")]
	[TestCase("The operator said \"Ready.\" Then the job started.", "The operator said \"Ready.\"")]
	public void SummaryRetainsTechnicalDotsAndTheFirstCompleteSentence(string prose, string expected) {
		var source = Source(Paragraph(Text(prose)));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.EqualTo(expected));
	}

	[Test]
	public void SummaryConcatenatesNativeRunsWithoutInventingSpaces() {
		var source = Source(Paragraph(Text("Sphere "), Text("10"), Text(" builds "), Text(".NET"), Text(" applications. More detail.")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.EqualTo("Sphere 10 builds .NET applications."));
	}

	[Test]
	public void SummarySkipsBlankParagraphsAndNormalizesWhitespace() {
		var source = Source(Paragraph(Text(" \t\r\n ")), Paragraph(Text("  We build\n reliable\tsoftware. More detail.  ")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.EqualTo("We build reliable software."));
	}

	[Test]
	public void SummaryRetainsExisting280CharacterLimit() {
		var source = Source(Paragraph(Text(new string('a', 340) + ". More detail.")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Has.Length.EqualTo(280));
	}

	[Test]
	public void NoParagraphHasNoAutomaticSummary() {
		var source = Source();
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.Null);
	}

	[Test]
	public void SummaryUsesLinkedLabelAndFreshNativeTextContent() {
		var run = Text("Sphere10 Framework", "https://hidden.example/private-target");
		run.PlainText = null;
		var source = Source(Paragraph(run, Text(" supports the application. More detail.")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.EqualTo("Sphere10 Framework supports the application."));
	}

	[Test]
	public void SummaryKeepsTechnicalNameSplitAcrossLinkedRunsAdjacent() {
		var source = Source(Paragraph(Text("We build "), Text("ASP", "https://hidden.example/framework"), Text(".NET"), Text(" services. More detail.")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects), Is.EqualTo("We build ASP.NET services."));
	}

	[TestCase(279)]
	[TestCase(280)]
	[TestCase(281)]
	public void SummaryCapIncludesAnyEllipsis(int length) {
		var prose = new string('a', length);
		var source = Source(Paragraph(Text(prose)));
		var summary = LocalNotionHelper.ExtractDefaultPageSummary(source.Graph, source.Objects);
		Assert.That(summary.Length, Is.EqualTo(Math.Min(length, 280)));
		Assert.That(summary, Is.EqualTo(length > 280 ? new string('a', 279) + "…" : prose));
	}

	[TestCase("An explicit CMS summary.")]
	[TestCase("")]
	public void ExplicitSummaryIncludingFramingEmptyDoesNotReadOrReplaceBody(string summary) {
		var page = new LocalNotionPage { CMSProperties = new CMSProperties { Summary = summary } };
		LocalNotionHelper.PopulateDefaultPageSummary(page, null, null);
		Assert.That(page.CMSProperties.Summary, Is.EqualTo(summary));
	}

	[Test]
	public void MissingCmsSummaryUsesDerivedIntroduction() {
		var page = new LocalNotionPage { CMSProperties = new CMSProperties() };
		var source = Source(Paragraph(Text("Sphere 10 builds ASP.NET Core services. More detail.")));
		LocalNotionHelper.PopulateDefaultPageSummary(page, source.Graph, source.Objects);
		Assert.That(page.CMSProperties.Summary, Is.EqualTo("Sphere 10 builds ASP.NET Core services."));
	}

	[Test]
	public void OrdinaryResourceDoesNotAcquireCmsMetadata() {
		var page = new LocalNotionPage();
		LocalNotionHelper.PopulateDefaultPageSummary(page, null, null);
		Assert.That(page.CMSProperties, Is.Null);
	}

	[Test]
	public void KeywordsUseAuthoredLinkLabelsAndDoNotLeakHrefTargets() {
		var source = Source(Paragraph(Text("Sphere10 Framework", "https://poisoned.example/hidden-payments"), Text(" provides typed application services.")));
		var keywords = LocalNotionHelper.ExtractDefaultPageKeywords(source.Graph, source.Objects);
		Assert.That(keywords, Is.Not.Empty);
		Assert.That(keywords.Any(x => x.Contains("framework")), Is.True, "The visible label belongs to searchable copy.");
		Assert.That(keywords, Has.None.Contains("poisoned"));
		Assert.That(keywords, Has.None.Contains("hidden-payments"));
		Assert.That(keywords, Has.None.Contains("https"));
	}

	[Test]
	public void KeywordsKeepSeparateVisibleBlockPhrases() {
		var heading = new HeadingOneBlock { Id = Guid.NewGuid().ToString(), Heading_1 = new HeadingOneBlock.Info { RichText = [Text("Alpha engineering")] } };
		var source = Source(heading, Paragraph(Text("Beta pipelines")));
		Assert.That(LocalNotionHelper.ExtractDefaultPageKeywords(source.Graph, source.Objects), Is.EquivalentTo(new[] { "alpha engineering", "beta pipelines" }));
	}

	[Test]
	public void KeywordsIgnoreInjectedMarkupAndRetainOrdinaryCode() {
		var injection = new CodeBlock { Id = Guid.NewGuid().ToString(), Code = new CodeBlock.Info {
			Language = "html", Caption = [Text("{INJECT}")], RichText = [Text("<script>poisonedMarkup()</script>")]
		} };
		var code = new CodeBlock { Id = Guid.NewGuid().ToString(), Code = new CodeBlock.Info {
			Language = "plain text", RichText = [Text("engineering example")]
		} };
		var source = Source(injection, code);
		var keywords = LocalNotionHelper.ExtractDefaultPageKeywords(source.Graph, source.Objects);
		Assert.That(keywords.Any(x => x.Contains("engineering")), Is.True);
		Assert.That(keywords, Has.None.Contains("poisoned"));
		Assert.That(keywords, Has.None.Contains("script"));
	}

	[Test]
	public void KeywordsRetainTheExistingTenPhraseLimit() {
		var source = Source(Enumerable.Range(0, 15).Select(x => Paragraph(Text("Topic" + x + " delivery"))).ToArray());
		Assert.That(LocalNotionHelper.ExtractDefaultPageKeywords(source.Graph, source.Objects), Has.Length.EqualTo(10));
	}

	private static RichTextText Text(string value, string target = null) => new() {
		PlainText = value,
		Text = new Text { Content = value, Link = target == null ? null : new Link { Url = target } }
	};

	private static ParagraphBlock Paragraph(params RichTextBase[] runs) => new() {
		Id = Guid.NewGuid().ToString(), Paragraph = new ParagraphBlock.Info { RichText = runs }
	};

	private static (NotionObjectGraph Graph, Dictionary<string, IObject> Objects) Source(params IObject[] blocks) {
		// Match a cached native Page root and its ordered body graph without a repository or network.
		var page = new Page { Id = Guid.NewGuid().ToString() };
		var objects = blocks.Append(page).ToDictionary(x => x.Id);
		return (new NotionObjectGraph {
			ObjectID = page.Id, Children = blocks.Select(x => new NotionObjectGraph { ObjectID = x.Id }).ToArray()
		}, objects);
	}
}
