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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using LocalNotion.Core;
using Notion.Client;
using NUnit.Framework;
using Sphere10.Framework;
using R = Sphere10.VisualRenderer;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Characterization")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Children)]
public class LegacyRenderingTests {
	private string _fixtureRoot = null;
	private LocalNotionRepository _repository;
	private R.DocumentBlock _htmlModel = null;
	private R.DocumentBlock _textModel = null;

	[SetUp]
	public async Task SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "renderer-characterization-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
		var profile = LocalNotionPathProfile.Default;
		_repository = await LocalNotionRepository.CreateNew(_fixtureRoot, pathProfile: profile, logger: new NoOpLogger());
		var resource = new LocalNotionPage {
			ID = ObjectId(1),
			Title = "Characterization page",
			Name = "characterization-page",
			Keywords = ["fixture"],
			CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			LastEditedOn = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
		};
		_repository.AddResource(resource);
		var objects = new Dictionary<string, IObject>();
		var pageValue = new Page { Id = resource.ID, CreatedTime = resource.CreatedOn, LastEditedTime = resource.LastEditedOn };
		objects.Add(pageValue.Id, pageValue);
		var paragraph = Paragraph(2, "Paragraph with & and a line\nbreak");
		paragraph.Paragraph.RichText = [
			RichText("Bold text", bold: true), RichText(" plain "), RichText("Linked", url: "https://example.test/path#anchor")
		];
		var root = new NotionObjectGraph {
			ObjectID = pageValue.Id,
			Children = [
				Graph(paragraph, Graph(Paragraph(3, "Nested paragraph"))),
				Graph(new HeadingOneBlock { Id = ObjectId(4), HasChildren = true, Heading_1 = new() { RichText = [RichText("Heading one")], Color = Color.Blue, IsToggleable = true } }, Graph(Paragraph(5, "Toggle heading child"))),
				Graph(new HeadingTwoBlock { Id = ObjectId(6), Heading_2 = new() { RichText = [RichText("Heading two")], Color = Color.Default } }),
				Graph(new HeadingThreeBlock { Id = ObjectId(7), Heading_3 = new() { RichText = [RichText("Heading three")], Color = Color.Default } }),
				Graph(new BulletedListItemBlock { Id = ObjectId(8), HasChildren = true, BulletedListItem = new() { RichText = [RichText("Bullet one")], Color = Color.Default } },
					Graph(new NumberedListItemBlock { Id = ObjectId(9), NumberedListItem = new() { RichText = [RichText("Nested number")], Color = Color.Default } })),
				Graph(new BulletedListItemBlock { Id = ObjectId(10), BulletedListItem = new() { RichText = [RichText("Bullet two")], Color = Color.Default } }),
				Graph(Paragraph(11, "List separator")),
				Graph(new NumberedListItemBlock { Id = ObjectId(12), NumberedListItem = new() { RichText = [RichText("Number one")], Color = Color.Default } }),
				Graph(new NumberedListItemBlock { Id = ObjectId(13), NumberedListItem = new() { RichText = [RichText("Number two")], Color = Color.Default } }),
				Graph(new QuoteBlock { Id = ObjectId(14), HasChildren = true, Quote = new() { RichText = [RichText("A quote")], Color = Color.Gray } }, Graph(Paragraph(15, "Quote child"))),
				Graph(new CalloutBlock { Id = ObjectId(16), HasChildren = true, Callout = new() { RichText = [RichText("Callout text")], Color = Color.YellowBackground, Icon = new EmojiPageIcon { Emoji = "⭐" } } }, Graph(Paragraph(17, "Callout child"))),
				Graph(new CodeBlock { Id = ObjectId(18), Code = new() { RichText = [RichText("var x = \"literal\";\nConsole.WriteLine(x);")], Language = "c#", Caption = [] } }),
				Graph(new ToDoBlock { Id = ObjectId(19), ToDo = new() { RichText = [RichText("Todo item")], Color = Color.Default, IsChecked = true } }),
				Graph(new ToggleBlock { Id = ObjectId(20), HasChildren = true, Toggle = new() { RichText = [RichText("Toggle item")], Color = Color.Default } }, Graph(Paragraph(21, "Toggle child"))),
				Graph(new TableBlock { Id = ObjectId(22), HasChildren = true, Table = new() { TableWidth = 2, HasColumnHeader = true, HasRowHeader = true } },
					Graph(new TableRowBlock { Id = ObjectId(23), TableRow = new() { Cells = [[RichText("Header A")], [RichText("Header B")]] } }),
					Graph(new TableRowBlock { Id = ObjectId(24), TableRow = new() { Cells = [[RichText("Row header")], [RichText("Cell data")]] } })),
				Graph(new ColumnListBlock { Id = ObjectId(25), HasChildren = true, ColumnList = new() }, Graph(new ColumnBlock { Id = ObjectId(26), HasChildren = true, Column = new() }, Graph(Paragraph(27, "Column A"))), Graph(new ColumnBlock { Id = ObjectId(28), HasChildren = true, Column = new() }, Graph(Paragraph(29, "Column B")))),
				Graph(new DividerBlock { Id = ObjectId(30) }),
				Graph(new EquationBlock { Id = ObjectId(31), Equation = new() { Expression = "x^{2}" } })
			]
		};
		foreach (var sourceObject in objects.Values)
			_repository.AddObject(sourceObject);
		_repository.AddResourceGraph(root);
		_repository.ImportBlankResourceRender(resource.ID, RenderType.HTML);

		var builder = new NotionRenderModelBuilder(_repository, new NoOpLogger());
		_htmlModel = builder.Build(resource, root, objects, _fixtureRoot);
		_textModel = builder.Build(resource, root, objects, purpose: ProjectionPurpose.Text);

		NotionObjectGraph Graph(IObject sourceObject, params NotionObjectGraph[] children) {
			objects.Add(sourceObject.Id, sourceObject);
			if (sourceObject is Block blockValue)
				blockValue.HasChildren = children.Length > 0;
			return new NotionObjectGraph { ObjectID = sourceObject.Id, Children = children };
		}
	}

	[TearDown]
	public void TearDown() {
		try {
			_repository?.Dispose();
		} finally {
			if (_fixtureRoot is not null) {
				var fullPath = Path.GetFullPath(_fixtureRoot);
				if (!fullPath.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException("Fixture path escaped its temporary parent.");
				if (Directory.Exists(fullPath))
					Directory.Delete(fullPath, true);
			}
		}
	}

	[Test]
	public void HtmlMainMatchesLegacyDom() {
		var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Rendering", "Characterization", "legacy-main.txt")).Replace("\r\n", "\n");
		var html = new R.HtmlRenderer().Render(_htmlModel, new R.RenderOptions()).Html;
		var main = new HtmlParser().ParseDocument(html).QuerySelector("main");
		Assert.That(main, Is.Not.Null);
		Assert.That(Canonical(main), Is.EqualTo(expected), "Main DOM differs from the characterized legacy-main.txt fixture.");
	}

	[Test]
	public void PlainTextMatchesLegacyText() {
		var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Rendering", "Characterization", "legacy-text.txt"));
		var text = new R.TextRenderer().Render(_textModel);
		Assert.That(text.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")),
			"Plain text differs from the characterized legacy-text.txt fixture.");
	}

	private static string ObjectId(int number) => number.ToString("x32");

	private static ParagraphBlock Paragraph(int number, string textValue) => new() { Id = ObjectId(number), Paragraph = new() { RichText = [RichText(textValue)], Color = Color.Default } };

	private static RichTextText RichText(string textValue, bool bold = false, string url = null) => new() {
		PlainText = textValue,
		Annotations = new Annotations { Color = Color.Default, IsBold = bold },
		Text = new Text { Content = textValue, Link = url is null ? null : new Link { Url = url } }
	};

	private static string Canonical(INode node) {
		if (node is IText textValue) {
			var value = textValue.ParentElement?.Closest("pre,code") is null ? Regex.Replace(textValue.Data, @"\s+", " ").Trim() : textValue.Data.Replace("\r\n", "\n");
			return value.Length == 0 ? "" : "[" + value + "]";
		}
		if (node is not IElement element)
			return "";
		var attributes = element.Attributes.Where(attribute =>
			!(attribute.Name == "id" && (attribute.Value.Length == 0 || element.LocalName is "ol" or "ul" || Regex.IsMatch(attribute.Value, @"^visual_\d+$"))) &&
			!(attribute.Name == "start" && attribute.Value is "1" or "{start}"))
			.OrderBy(attribute => attribute.Name, StringComparer.Ordinal)
			.Select(attribute => attribute.Name + "=" + attribute.Value.Replace("ln-color-{color}", "ln-color-default", StringComparison.Ordinal));
		return "<" + element.LocalName + " " + string.Join(" ", attributes) + ">" + string.Concat(element.ChildNodes.Select(Canonical)) + "</" + element.LocalName + ">";
	}
}
