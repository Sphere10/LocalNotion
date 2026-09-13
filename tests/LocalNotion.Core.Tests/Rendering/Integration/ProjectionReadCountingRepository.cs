// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using LocalNotion.Core;
using Notion.Client;

namespace LocalNotion.Core.Tests;

internal sealed class ProjectionReadCountingRepository : LocalNotionRepositoryDecorator {

	public ProjectionReadCountingRepository(ILocalNotionRepository repository)
		: base(repository) {
	}

	public int ObjectReads { get; private set; }

	public int GraphReads { get; private set; }

	public override bool TryGetObject(string objectID, out IObject source) {
		ObjectReads++;
		return base.TryGetObject(objectID, out source);
	}

	public override bool TryGetResourceGraph(string resourceID, out NotionObjectGraph graph) {
		GraphReads++;
		return base.TryGetResourceGraph(resourceID, out graph);
	}
}
