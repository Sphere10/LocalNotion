// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using LocalNotion.Core;

namespace LocalNotion.Core.Tests;

internal static class WindowsPathProfiles {
	public static LocalNotionPathProfile Create() => new() {
		RepositoryPathR = @"..\",
		RegistryPathR = @".localnotion\registry.json",
		ObjectsPathR = @".localnotion\objects",
		GraphsPathR = @".localnotion\graphs",
		PropertiesPathR = @".localnotion\properties",
		ThemesPathR = @".localnotion\themes",
		LogsPathR = @".localnotion\logs",
		FilesPathR = @"files\",
		DatabasesPathR = @"databases\",
		PagesPathR = @"pages\",
		WorkspacePathR = @"workspaces\",
		CMSPathR = @"cms\",
		BaseUrl = @"https://example.invalid/base\literal"
	};
}