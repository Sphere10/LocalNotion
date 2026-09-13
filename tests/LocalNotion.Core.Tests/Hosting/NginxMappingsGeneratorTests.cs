// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.IO;
using System.Threading.Tasks;
using LocalNotion.Core;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[Parallelizable(ParallelScope.Children)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class NginxMappingsGeneratorTests {

	private string _fixtureRoot;
	private string _cmsPath;
	private CMSLocalNotionRepository _repository;
	private NginxMappingsGenerator _generator;

	[SetUp]
	public async Task SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-nginx-cleanup-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_fixtureRoot);
		var databaseId = Guid.NewGuid().ToString();
		_repository = (CMSLocalNotionRepository)await LocalNotionRepository.CreateNew(_fixtureRoot, cmsDatabaseID: databaseId, logger: new NoOpLogger());
		_repository.AddResource(new LocalNotionDatabase { ID = databaseId, Name = "example.invalid", Title = "CMS", PrimaryDataSourceID = Guid.NewGuid().ToString() });
		_cmsPath = _repository.Paths.GetResourceTypeFolderPath(LocalNotionResourceType.CMS, FileSystemPathType.Absolute);
		_generator = new NginxMappingsGenerator(_repository);
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
				Directory.Delete(fullPath, recursive: true);
		}
	}

	[TestCase("cms/keep.html")]
	[TestCase("./cms/keep.html")]
	[TestCase("cms/../cms/keep.html")]
	public async Task CleanupPreservesRegisteredRenderWithEquivalentPath(string renderPath) {
		var renderFile = CreateRender("keep.html", "registered render");
		RegisterRender(renderPath);
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(renderFile), Is.True, "A registered render must survive hosting-file generation.");
		Assert.That(await File.ReadAllTextAsync(renderFile), Is.EqualTo("registered render"));
	}

	[TestCase(@"cms\keep.html")]
	[TestCase(@"cms\.\keep.html")]
	[TestCase(@"cms\../cms/keep.html")]
	[Platform("Win")]
	public async Task CleanupPreservesRegisteredRenderWithWindowsSeparators(string renderPath) {
		var renderFile = CreateRender("keep.html");
		RegisterRender(renderPath);
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(renderFile), Is.True);
	}

	[Test]
	public async Task CleanupPreservesRegisteredAbsolutePath() {
		var renderFile = CreateRender("keep.html");
		RegisterRender(renderFile);
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(renderFile), Is.True);
	}

	[Test]
	[Platform("Win")]
	public async Task CleanupComparesWindowsPathsWithoutCaseSensitivity() {
		var renderFile = CreateRender("Keep.html");
		RegisterRender("CMS/KEEP.HTML");
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(renderFile), Is.True);
	}

	[Test]
	[Platform("Linux")]
	public async Task CleanupDistinguishesLinuxFileNameCase() {
		var registeredFile = CreateRender("Keep.html");
		var obsoleteFile = CreateRender("keep.html");
		RegisterRender("cms/Keep.html");
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(registeredFile), Is.True);
		Assert.That(File.Exists(obsoleteFile), Is.False);
	}

	[Test]
	public async Task CleanupRemovesOnlyUnregisteredFiles() {
		var registeredFile = CreateRender("keep.html");
		var obsoleteFile = CreateRender("obsolete.html");
		var sitemapFile = CreateRender("sitemap.xml");
		RegisterRender("cms/keep.html");
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(registeredFile), Is.True);
		Assert.That(File.Exists(obsoleteFile), Is.False);
		Assert.That(File.Exists(sitemapFile), Is.True);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase(" ")]
	public async Task CleanupAllowsItemsWithoutRenders(string renderPath) {
		var obsoleteFile = CreateRender("obsolete.html");
		var sitemapFile = CreateRender("sitemap.xml");
		RegisterRender(renderPath);
		await _generator.RemoveOldCmsRenders();
		Assert.That(File.Exists(obsoleteFile), Is.False);
		Assert.That(File.Exists(sitemapFile), Is.True);
	}

	[Test]
	public async Task CleanupLeavesFilesOutsideCmsDirectoryUntouched() {
		var outsideFile = Path.Combine(_fixtureRoot, "outside.html");
		await File.WriteAllTextAsync(outsideFile, "outside CMS");
		CreateRender("obsolete.html");
		await _generator.RemoveOldCmsRenders();
		Assert.That(await File.ReadAllTextAsync(outsideFile), Is.EqualTo("outside CMS"));
	}

	[Test]
	public async Task CleanupAllowsAbsentCmsDirectory() {
		if (Directory.Exists(_cmsPath))
			Directory.Delete(_cmsPath);
		await _generator.RemoveOldCmsRenders();
		Assert.That(Directory.Exists(_cmsPath), Is.False);
	}

	[Test]
	public async Task GenerateHostingFilesKeepsMappedRendersAndDeletesObsoleteRenders() {
		var renderFile = CreateRender("keep.html", "registered render");
		var obsoleteFile = CreateRender("obsolete.html");
		RegisterRender("cms/keep.html");
		var nginxPath = await NginxMappingsGenerator.GenerateNGinxFiles(_repository);
		var mappings = await File.ReadAllTextAsync(Path.Combine(nginxPath, "conf", NginxMappingsGenerator.MappingFileName));
		Assert.That(mappings, Does.Contain("try_files \"/cms/keep.html\" =404;"));
		Assert.That(File.Exists(Path.Combine(_cmsPath, "sitemap.xml")), Is.True);
		Assert.That(File.Exists(renderFile), Is.True, "A generated Nginx mapping must retain its target file.");
		Assert.That(await File.ReadAllTextAsync(renderFile), Is.EqualTo("registered render"));
		Assert.That(File.Exists(obsoleteFile), Is.False);
	}

	private string CreateRender(string fileName, string content = "render") {
		Directory.CreateDirectory(_cmsPath);
		var filePath = Path.Combine(_cmsPath, fileName);
		File.WriteAllText(filePath, content);
		return filePath;
	}

	private void RegisterRender(string renderPath) => _repository.AddOrUpdateCMSItem(new CMSItem { Slug = "", RenderPath = renderPath });
}