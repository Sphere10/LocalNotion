// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com/products/localnotion)
// Author: Herman Schoenfeld <herman@sphere10.com>
//
// Distributed under the GPLv3 software license, see the accompanying file LICENSE
// or visit https://github.com/HermanSchoenfeld/localnotion/blob/master/LICENSE
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using Notion.Client;

namespace LocalNotion.Core;

/// <summary>Caches raw source reads for one render batch while destination metadata remains live.</summary>
internal sealed class RenderSourceRepository : LocalNotionRepositoryDecorator {

	private readonly Dictionary<string, IObject> _objects = new(StringComparer.Ordinal);

	private readonly Dictionary<string, NotionObjectGraph> _graphs = new(StringComparer.Ordinal);

	public RenderSourceRepository(ILocalNotionRepository repository)
		: base(repository) {
	}

	public override bool ContainsObject(string objectID) => TryGetObject(objectID, out _);

	public override bool TryGetObject(string objectID, out IObject source) {
		if (!_objects.TryGetValue(objectID, out source)) {
			InternalRepository.TryGetObject(objectID, out source);
			_objects.Add(objectID, source);
		}
		return source is not null;
	}

	public override bool TryGetResourceGraph(string resourceID, out NotionObjectGraph graph) {
		if (!_graphs.TryGetValue(resourceID, out graph)) {
			InternalRepository.TryGetResourceGraph(resourceID, out graph);
			_graphs.Add(resourceID, graph);
		}
		return graph is not null;
	}

	public override bool TryGetParentResource(string objectID, out LocalNotionResource parent) {
		parent = null;
		var visited = new HashSet<string>(StringComparer.Ordinal);
		while (!string.IsNullOrWhiteSpace(objectID) && visited.Add(objectID) && TryGetObject(objectID, out var source)) {
			objectID = source.GetParent()?.Id;
			if (!string.IsNullOrWhiteSpace(objectID) && TryGetResource(objectID, out parent))
				return true;
		}
		return false;
	}
}
