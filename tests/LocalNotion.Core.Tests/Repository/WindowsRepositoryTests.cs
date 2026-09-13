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
using LocalNotion.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[Parallelizable(ParallelScope.Children)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class WindowsRepositoryTests {
	private string _fixtureRoot = null;
	private string _repositoryRoot = null;
	private string _registryFile = null;
	private string _resourceId = null;
	private string _resourceRender = null;
	private LocalNotionRegistry _registry = null;

	[SetUp]
	public async Task SetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-path-compatibility-" + Guid.NewGuid().ToString("N"));
		_repositoryRoot = Path.Combine(_fixtureRoot, "existing repository");
		_resourceId = Guid.NewGuid().ToString("N");
		_registryFile = Path.Combine(_repositoryRoot, ".localnotion", "registry.json");
		_resourceRender = Path.Combine(_repositoryRoot, "files", _resourceId, "existing.txt");
		var cmsRender = Path.Combine(_repositoryRoot, "cms", "existing.html");
		Directory.CreateDirectory(Path.GetDirectoryName(_registryFile));
		Directory.CreateDirectory(Path.GetDirectoryName(_resourceRender));
		Directory.CreateDirectory(Path.GetDirectoryName(cmsRender));
		await File.WriteAllTextAsync(_resourceRender, "original render");
		await File.WriteAllTextAsync(cmsRender, "original CMS render");
		_registry = new LocalNotionRegistry {
			Paths = WindowsPathProfiles.Create(),
			NotionApiKey = @"test-only\key",
			GitSettings = new GitSettings { Enabled = true, Push = true },
			NGinxSettings = new NGinxSettings { Enabled = true, ReloadCommand = @"C:\nginx\nginx.exe -s reload" },
			ApacheSettings = new ApacheSettings { Enabled = true },
			Resources = new[] {
				new LocalNotionFile {
					ID = _resourceId, Title = @"unchanged\title",
					Renders = new Dictionary<RenderType, RenderEntry> {
						[RenderType.File] = new() { LocalPath = $@"files\{_resourceId}\existing.txt", Slug = @"unchanged\slug" }
					}
				}
			}.Concat(ExternalPaths().Select(path => new LocalNotionFile {
				ID = Guid.NewGuid().ToString("N"), Title = path,
				Renders = new Dictionary<RenderType, RenderEntry> {
					[RenderType.File] = new() { LocalPath = path, Slug = "external" }
				}
			})).ToArray(),
			CMSItems = [
				new CMSItem { Slug = @"cms\unchanged", RenderPath = @"cms\existing.html", Image = @"https://example.invalid/image\name" },
				new CMSItem { Slug = "empty", RenderPath = "" },
				new CMSItem { Slug = "missing", RenderPath = null }
			]
		};
		Tools.Json.WriteToFile(_registryFile, _registry);
	}

	[TearDown]
	public void TearDown() {
		if (Directory.Exists(_fixtureRoot))
			Directory.Delete(_fixtureRoot, recursive: true);
	}

	[Test]
	public async Task LoadResolvesRepositoryAndInternalFolders() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(Path.TrimEndingDirectorySeparator(repository.Paths.GetRepositoryPath(FileSystemPathType.Absolute)), Is.EqualTo(_repositoryRoot));
		Assert.That(repository.Paths.GetInternalResourceFolderPath(InternalResourceType.Objects, FileSystemPathType.Absolute), Is.EqualTo(Path.Combine(_repositoryRoot, ".localnotion", "objects")));
	}

	[Test]
	public async Task LoadNormalizesResourceRenderToExistingPortablePath() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		var file = repository.Resources.Single(resource => resource.ID == _resourceId);
		Assert.That(file.Renders[RenderType.File].LocalPath, Is.EqualTo($"files/{_resourceId}/existing.txt"));
		Assert.That(File.Exists(Path.Join(_repositoryRoot, file.Renders[RenderType.File].LocalPath)), Is.True);
	}

	[TestCase(@"cms\unchanged", "cms/existing.html")]
	[TestCase("empty", "")]
	[TestCase("missing", null)]
	public async Task LoadNormalizesCmsRenderPathsAndPreservesAbsentValues(string slug, string expected) {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(repository.CMSItems.Single(item => item.Slug == slug).RenderPath, Is.EqualTo(expected));
	}

	[Test]
	public async Task LoadResolvesExistingCmsRender() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(File.Exists(Path.Join(_repositoryRoot, repository.CMSItems.First().RenderPath)), Is.True);
	}

	[Test]
	public async Task LoadPreservesResourceMetadata() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		var file = repository.Resources.Single(resource => resource.ID == _resourceId);
		Assert.That(file.Renders[RenderType.File].Slug, Is.EqualTo(@"unchanged\slug"));
		Assert.That(file.Title, Is.EqualTo(@"unchanged\title"));
		Assert.That(repository.CMSItems.First().Image, Is.EqualTo(@"https://example.invalid/image\name"));
	}

	[TestCaseSource(nameof(ExternalPaths))]
	public async Task LoadPreservesAbsolutePathsAndUris(string path) {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(repository.Resources.Single(resource => resource.Title == path).Renders[RenderType.File].LocalPath, Is.EqualTo(path));
	}

	[Test]
	public async Task LoadAndNoOpSavePreserveRegistryBytes() {
		var beforeLoad = await File.ReadAllBytesAsync(_registryFile);
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(repository.RequiresSave, Is.False, "Loading must not request a registry write");
		await repository.SaveAsync();
		Assert.That(await File.ReadAllBytesAsync(_registryFile), Is.EqualTo(beforeLoad));
	}

	[Test]
	public async Task ImportResourceRenderReplacesExistingRenderPath() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		var replacement = Path.Combine(_fixtureRoot, "replacement.txt");
		await File.WriteAllTextAsync(replacement, "replacement render");
		var actualRender = repository.ImportResourceRender(_resourceId, RenderType.File, replacement);
		Assert.That(Path.GetFullPath(actualRender), Is.EqualTo(Path.GetFullPath(_resourceRender)));
		Assert.That(await File.ReadAllTextAsync(_resourceRender), Is.EqualTo("replacement render"));
	}

	[TestCase(nameof(LocalNotionPathProfile.RepositoryPathR), "../")]
	[TestCase(nameof(LocalNotionPathProfile.ObjectsPathR), ".localnotion/objects")]
	[TestCase(nameof(LocalNotionPathProfile.GraphsPathR), ".localnotion/graphs")]
	[TestCase(nameof(LocalNotionPathProfile.PropertiesPathR), ".localnotion/properties")]
	[TestCase(nameof(LocalNotionPathProfile.ThemesPathR), ".localnotion/themes")]
	[TestCase(nameof(LocalNotionPathProfile.LogsPathR), ".localnotion/logs")]
	[TestCase(nameof(LocalNotionPathProfile.FilesPathR), "files/")]
	[TestCase(nameof(LocalNotionPathProfile.DatabasesPathR), "databases/")]
	[TestCase(nameof(LocalNotionPathProfile.PagesPathR), "pages/")]
	[TestCase(nameof(LocalNotionPathProfile.WorkspacePathR), "workspaces/")]
	[TestCase(nameof(LocalNotionPathProfile.CMSPathR), "cms/")]
	public async Task SaveNormalizesWindowsProfilePaths(string propertyName, string expected) {
		await ReplaceRenderAndSave();
		var saved = JsonConvert.DeserializeObject<LocalNotionRegistry>(await File.ReadAllTextAsync(_registryFile));
		Assert.That(typeof(LocalNotionPathProfile).GetProperty(propertyName).GetValue(saved.Paths), Is.EqualTo(expected));
	}

	[Test]
	public async Task SavePreservesNonPathRegistryMetadata() {
		await ReplaceRenderAndSave();
		var saved = JsonConvert.DeserializeObject<LocalNotionRegistry>(await File.ReadAllTextAsync(_registryFile));
		Assert.That(saved.Paths.BaseUrl, Is.EqualTo(_registry.Paths.BaseUrl));
		Assert.That(saved.NotionApiKey, Is.EqualTo(_registry.NotionApiKey));
		Assert.That(saved.GitSettings.Enabled, Is.True);
		Assert.That(saved.GitSettings.Push, Is.True);
		Assert.That(saved.NGinxSettings.Enabled, Is.True);
		Assert.That(saved.NGinxSettings.ReloadCommand, Is.EqualTo(_registry.NGinxSettings.ReloadCommand));
		Assert.That(saved.ApacheSettings.Enabled, Is.True);
	}

	[Test]
	public async Task SavedPortableResourceRenderReopens() {
		await ReplaceRenderAndSave();
		var saved = JsonConvert.DeserializeObject<LocalNotionRegistry>(await File.ReadAllTextAsync(_registryFile));
		Assert.That(saved.Resources.Single(resource => resource.ID == _resourceId).Renders[RenderType.File].LocalPath, Is.EqualTo($"files/{_resourceId}/existing.txt"));
		using var reopened = await LocalNotionRepository.OpenRegistry(_registryFile);
		Assert.That(File.Exists(Path.Join(_repositoryRoot, reopened.Resources.Single(resource => resource.ID == _resourceId).Renders[RenderType.File].LocalPath)), Is.True);
	}

	public static string[] ExternalPaths() => [@"C:\external\page.html", @"\\server\share\page.html", @"\rooted\page.html", "/external/page.html", @"https://example.invalid/file\name"];

	private async Task ReplaceRenderAndSave() {
		using var repository = await LocalNotionRepository.OpenRegistry(_registryFile);
		var replacement = Path.Combine(_fixtureRoot, "replacement.txt");
		await File.WriteAllTextAsync(replacement, "replacement render");
		repository.ImportResourceRender(_resourceId, RenderType.File, replacement);
		await repository.SaveAsync();
	}
}