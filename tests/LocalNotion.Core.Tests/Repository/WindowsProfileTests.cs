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
public class WindowsProfileTests {
	[Test]
	public async Task CreateNewCreatesNestedRegistryAndObjectsDirectory() {
		var fixtureRoot = Path.Combine(Path.GetTempPath(), "localnotion-windows-profile-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(fixtureRoot);
		try {
			using var repository = await LocalNotionRepository.CreateNew(fixtureRoot, pathProfile: WindowsPathProfiles.Create());
			Assert.That(File.Exists(repository.Paths.GetRegistryFilePath(FileSystemPathType.Absolute)), Is.True);
			Assert.That(Directory.Exists(Path.Combine(fixtureRoot, ".localnotion", "objects")), Is.True);
		} finally {
			Directory.Delete(fixtureRoot, recursive: true);
		}
	}
}