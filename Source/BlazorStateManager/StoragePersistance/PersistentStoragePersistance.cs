using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Stores values in memory and persists them across SSR prerender to interactive render using PersistentComponentState.
/// </summary>
public class PersistentStoragePersistance : IStoragePersistance, IDisposable
{
	private const string StorageKey = "__bsm_state";

	private readonly PersistentComponentState persistentState;
	private readonly PersistingComponentStateSubscription subscription;
	private readonly Dictionary<string, string> cache;

	public PersistentStoragePersistance(PersistentComponentState persistentState)
	{
		ArgumentNullException.ThrowIfNull(persistentState, nameof(persistentState));

		this.persistentState = persistentState;
		persistentState.TryTakeFromJson(StorageKey, out Dictionary<string, string>? restored);
		cache = restored ?? new Dictionary<string, string>();
		subscription = persistentState.RegisterOnPersisting(PersistAsync);
	}

	public ValueTask Store<T>(string name, T? data)
	{
		cache[name] = JsonSerializer.Serialize(data);
		return ValueTask.CompletedTask;
	}

	public ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		if (cache.TryGetValue(name, out string? data) && !string.IsNullOrWhiteSpace(data))
		{
			return ValueTask.FromResult(JsonSerializer.Deserialize<T>(data));
		}

		return ValueTask.FromResult<T?>(null);
	}

	private Task PersistAsync()
	{
		persistentState.PersistAsJson(StorageKey, cache);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		subscription.Dispose();
	}
}
