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
using LocalNotion.Repository;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Sphere10.Framework;

namespace LocalNotion.Core.Tests;

[TestFixture]
[Category("Integration")]
[NonParallelizable]
public class RepositoryGitSettingsTests {
	private string _fixtureRoot;
	private string _themesRoot;
	private string _repositoryRoot;
	private string _registryFile;

	[OneTimeSetUp]
	public async Task OneTimeSetUp() {
		_fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-repository-git-" + Guid.NewGuid().ToString("N"));
		_themesRoot = Path.Combine(_fixtureRoot, "themes");
		var seedRoot = Path.Combine(_fixtureRoot, "seed");
		Directory.CreateDirectory(seedRoot);
		using var seed = await LocalNotionRepository.CreateNew(seedRoot, pathProfile: new LocalNotionPathProfile { ThemesPathR = _themesRoot });
	}

	[SetUp]
	public void SetUp() {
		_repositoryRoot = Path.Combine(_fixtureRoot, "repository-" + Guid.NewGuid().ToString("N"));
		_registryFile = Path.Combine(_repositoryRoot, ".localnotion", "registry.json");
		Directory.CreateDirectory(Path.GetDirectoryName(_registryFile));
		WriteRegistry();
	}

	[TearDown]
	public void TearDown() {
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_repositoryRoot)), Is.EqualTo(_fixtureRoot));
		Assert.That(Path.GetFileName(_repositoryRoot), Does.StartWith("repository-"));
		if (Directory.Exists(_repositoryRoot))
			Directory.Delete(_repositoryRoot, recursive: true);
	}

	[OneTimeTearDown]
	public void OneTimeTearDown() {
		Assert.That(Path.GetDirectoryName(Path.GetFullPath(_fixtureRoot)), Is.EqualTo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath()))));
		Assert.That(Path.GetFileName(_fixtureRoot), Does.StartWith("localnotion-repository-git-"));
		if (Directory.Exists(_fixtureRoot))
			Directory.Delete(_fixtureRoot, recursive: true);
	}

	[Test]
	public void UnloadedRepositoryRejectsGitSettingsAccessAndDisposesSafely() {
		using var repository = new LocalNotionRepository(_registryFile);
		Assert.That(() => repository.GitSettings, Throws.TypeOf<InvalidOperationException>());
		Assert.That(() => repository.UpdateGitSettings(enabled: true), Throws.TypeOf<InvalidOperationException>());
	}

	[Test]
	public async Task DisposedRepositoryRejectsGitSettingsAccessAndReload() {
		var repository = await LocalNotionRepository.Open(_repositoryRoot);
		await repository.DisposeAsync();
		Assert.That(() => repository.GitSettings, Throws.TypeOf<ObjectDisposedException>());
		Assert.That(() => repository.UpdateGitSettings(enabled: true), Throws.TypeOf<ObjectDisposedException>());
		await Assert.ThatAsync(repository.LoadAsync, Throws.TypeOf<ObjectDisposedException>());
		Assert.That(repository.Dispose, Throws.Nothing);
	}

	[Test]
	public async Task NoOpUpdateLeavesRepositoryCleanWithoutNotificationsOrRegistryWrite() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var before = await File.ReadAllBytesAsync(_registryFile);
		var notifications = new List<string>();
		repository.Changing += _ => notifications.Add("changing");
		repository.Changed += _ => notifications.Add("changed");
		repository.UpdateGitSettings();
		repository.UpdateGitSettings(enabled: false, push: true);
		await repository.SaveAsync();
		Assert.That(repository.RequiresSave, Is.False);
		Assert.That(notifications, Is.Empty);
		Assert.That(await File.ReadAllBytesAsync(_registryFile), Is.EqualTo(before));
	}

	[Test]
	public async Task ReadOnlyRegistryLoadsAndNoOpSavePreservesItsBytes() {
		var before = await File.ReadAllBytesAsync(_registryFile);
		var attributes = File.GetAttributes(_registryFile);
		File.SetAttributes(_registryFile, attributes | FileAttributes.ReadOnly);
		using var restoreAttributes = Tools.Scope.ExecuteOnDispose(() => File.SetAttributes(_registryFile, attributes));
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		repository.UpdateGitSettings(enabled: false, push: true);
		await repository.SaveAsync();
		Assert.That(repository.RequiresSave, Is.False);
		Assert.That(await File.ReadAllBytesAsync(_registryFile), Is.EqualTo(before));
	}

	[Test]
	public async Task UpdateUsesRepositoryNotificationsAndRequiresExplicitSave() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var before = await File.ReadAllBytesAsync(_registryFile);
		var observations = new List<(string Event, bool Enabled, bool Dirty)>();
		repository.Changing += sender => {
			Assert.That(sender, Is.SameAs(repository));
			observations.Add(("changing", repository.GitSettings.Enabled, repository.RequiresSave));
		};
		repository.Changed += sender => {
			Assert.That(sender, Is.SameAs(repository));
			observations.Add(("changed", repository.GitSettings.Enabled, repository.RequiresSave));
		};
		repository.UpdateGitSettings(enabled: true);
		repository.UpdateGitSettings(enabled: true);
		Assert.That(observations, Is.EqualTo(new[] { ("changing", false, false), ("changed", true, true) }));
		Assert.That(repository.GitSettings.Push, Is.True, "An omitted push option retains its existing value.");
		Assert.That(await File.ReadAllBytesAsync(_registryFile), Is.EqualTo(before), "Mutation belongs to the repository; SaveAsync owns persistence.");
	}

	[Test]
	public async Task ChangingHandlerFailureLeavesSettingsAndDirtyStateUnchanged() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		repository.Changing += _ => throw new InvalidOperationException("test-only notification failure");
		Assert.That(() => repository.UpdateGitSettings(enabled: true), Throws.TypeOf<InvalidOperationException>());
		Assert.That(repository.GitSettings.Enabled, Is.False);
		Assert.That(repository.RequiresSave, Is.False);
	}

	[Test]
	public async Task SettingsExposeRegistryOwnedDataAndTrackedUpdatesReplaceThatValue() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var original = repository.GitSettings;
		Assert.That(repository.GitSettings, Is.SameAs(original), "Git exposes the registry-owned DTO, like Nginx settings.");
		repository.UpdateGitSettings(enabled: true, push: false);
		Assert.That(repository.GitSettings, Is.Not.SameAs(original));
		Assert.That(repository.GitSettings.AdditionalData, Is.SameAs(original.AdditionalData), "Unknown options retain their existing ownership.");
		Assert.That(repository.RequiresSave, Is.True);
		Assert.That(original.Enabled, Is.False);
		Assert.That(original.Push, Is.True);
	}

	[Test]
	public void RegistrySettingsUseEstablishedDefaultValues() {
		var first = new LocalNotionRegistry();
		var second = new LocalNotionRegistry();
		Assert.That(first.GitSettings, Is.SameAs(GitSettings.Default));
		Assert.That(second.GitSettings, Is.SameAs(GitSettings.Default));
		Assert.That(first.NGinxSettings, Is.SameAs(NGinxSettings.Default));
	}

	[TestCase(false)]
	[TestCase(true)]
	public async Task MissingOrNullGitSettingsUseDefaultsWithoutDirtyingRepository(bool explicitNull) {
		var registry = JObject.Parse(await File.ReadAllTextAsync(_registryFile));
		if (explicitNull)
			registry["git"] = null;
		else
			registry.Remove("git");
		await File.WriteAllTextAsync(_registryFile, registry.ToString());
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var defaults = GitSettings.Default;
		var originalEnabled = defaults.Enabled;
		var originalPush = defaults.Push;
		Assert.That(repository.GitSettings.Enabled, Is.EqualTo(GitSettings.Default.Enabled));
		Assert.That(repository.GitSettings.Push, Is.EqualTo(GitSettings.Default.Push));
		Assert.That(repository.RequiresSave, Is.False);
		repository.UpdateGitSettings(enabled: !GitSettings.Default.Enabled);
		await repository.SaveAsync();
		using var reopened = await LocalNotionRepository.Open(_repositoryRoot);
		Assert.That(reopened.GitSettings.Enabled, Is.EqualTo(!GitSettings.Default.Enabled));
		Assert.That(GitSettings.Default, Is.SameAs(defaults));
		Assert.That(defaults.Enabled, Is.EqualTo(originalEnabled));
		Assert.That(defaults.Push, Is.EqualTo(originalPush));
	}

	[TestCase("{}", false, false)]
	[TestCase("{\"enabled\":true}", true, false)]
	[TestCase("{\"push\":true}", false, true)]
	[TestCase("{\"enabled\":true,\"push\":true}", true, true)]
	public async Task LoadingSettingsDoesNotPopulateTheSharedDefault(string json, bool enabled, bool push) {
		var defaults = GitSettings.Default;
		var originalEnabled = defaults.Enabled;
		var originalPush = defaults.Push;
		var registry = JObject.Parse(await File.ReadAllTextAsync(_registryFile));
		registry["git"] = JObject.Parse(json);
		await File.WriteAllTextAsync(_registryFile, registry.ToString());
		using var first = await LocalNotionRepository.Open(_repositoryRoot);
		using var second = await LocalNotionRepository.Open(_repositoryRoot);
		Assert.That(first.GitSettings.Enabled, Is.EqualTo(enabled));
		Assert.That(first.GitSettings.Push, Is.EqualTo(push));
		Assert.That(first.GitSettings, Is.Not.SameAs(defaults));
		Assert.That(first.GitSettings, Is.Not.SameAs(second.GitSettings));
		Assert.That(defaults.Enabled, Is.EqualTo(originalEnabled));
		Assert.That(defaults.Push, Is.EqualTo(originalPush));
	}

	[Test]
	public async Task SaveAndReopenRetainIndependentConfigurationAndUnknownSettings() {
		using (var repository = await LocalNotionRepository.Open(_repositoryRoot)) {
			repository.UpdateGitSettings(push: false);
			await repository.SaveAsync();
			Assert.That(repository.RequiresSave, Is.False);
		}
		using var reopened = await LocalNotionRepository.Open(_repositoryRoot);
		Assert.That(reopened.GitSettings.Enabled, Is.False, "An omitted enabled option retains its value.");
		Assert.That(reopened.GitSettings.Push, Is.False);
		Assert.That(reopened.DefaultNotionApiKey, Is.EqualTo("test-only-api-key"));
		Assert.That(reopened.DefaultThemes, Is.EqualTo(new[] { "cms", "custom" }));
		Assert.That(reopened.NGinxSettings.Enabled, Is.True);
		Assert.That(reopened.ApacheSettings.Enabled, Is.True);
		var saved = JObject.Parse(await File.ReadAllTextAsync(_registryFile));
		Assert.That((string)saved["future_registry"]["value"], Is.EqualTo("preserved"));
		Assert.That((string)saved["git"]["future_option"]["value"], Is.EqualTo("preserved"));
		Assert.That(File.Exists(_registryFile + ".persist"), Is.False);
		Assert.That(File.Exists(_registryFile + ".commit"), Is.False);
		Assert.That(Directory.Exists(Path.Combine(_repositoryRoot, ".git")), Is.False, "Saving configuration does not initialize or invoke Git.");
	}

	[Test]
	public async Task DecoratorForwardsSettingsMutationAndNotifications() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var decorator = new ObservingRepository(repository);
		var notificationCount = 0;
		decorator.Changed += _ => notificationCount++;
		decorator.UpdateGitSettings(enabled: true);
		Assert.That(decorator.GitSettings.Enabled, Is.True);
		Assert.That(repository.RequiresSave, Is.True);
		Assert.That(notificationCount, Is.EqualTo(1));
	}

	[Test]
	public async Task SynchronizedDecoratorScopesSettingsReadsAndUpdates() {
		using var repository = await LocalNotionRepository.Open(_repositoryRoot);
		var observingRepository = new ObservingRepository(repository);
		var synchronized = new SynchronizedLocalNotionRepository(observingRepository);
		observingRepository.ReadingGitSettings = () => Assert.That(synchronized.ThreadLock.IsReadLockHeld, Is.True);
		repository.Changing += _ => Assert.That(synchronized.ThreadLock.IsWriteLockHeld, Is.True);
		Assert.That(synchronized.GitSettings.Enabled, Is.False);
		synchronized.UpdateGitSettings(enabled: true);
		Assert.That(synchronized.GitSettings.Enabled, Is.True);
		Assert.That(synchronized.ThreadLock.IsReadLockHeld, Is.False);
		Assert.That(synchronized.ThreadLock.IsWriteLockHeld, Is.False);
	}

	[Test]
	public async Task MonitoredRepositoryRejectsSettingsMutation() {
		var before = await File.ReadAllBytesAsync(_registryFile);
		using var monitored = new MonitoredRepository(_repositoryRoot);
		Assert.That(() => monitored.UpdateGitSettings(enabled: true, push: false), Throws.TypeOf<NotSupportedException>());
		Assert.That(await File.ReadAllBytesAsync(_registryFile), Is.EqualTo(before));
	}

	private void WriteRegistry() {
		Tools.Json.WriteToFile(_registryFile, new LocalNotionRegistry {
			Paths = new LocalNotionPathProfile { ThemesPathR = _themesRoot },
			NotionApiKey = "test-only-api-key",
			DefaultThemes = ["cms", "custom"],
			GitSettings = new GitSettings {
				Enabled = false,
				Push = true,
				AdditionalData = new Dictionary<string, JToken> { ["future_option"] = new JObject { ["value"] = "preserved" } }
			},
			NGinxSettings = new NGinxSettings { Enabled = true },
			ApacheSettings = new ApacheSettings { Enabled = true },
			AdditionalData = new Dictionary<string, JToken> { ["future_registry"] = new JObject { ["value"] = "preserved" } }
		});
	}

	private sealed class ObservingRepository(ILocalNotionRepository repository) : LocalNotionRepositoryDecorator(repository) {
		public Action ReadingGitSettings { get; set; }

		public override GitSettings GitSettings {
			get {
				ReadingGitSettings?.Invoke();
				return base.GitSettings;
			}
		}
	}
}
