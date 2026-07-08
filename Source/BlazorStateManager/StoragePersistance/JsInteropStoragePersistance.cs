using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Base storage that mirrors data in an in-memory cache and falls back to it when JS interop is unavailable during SSR/prerender.
/// </summary>
public abstract class JsInteropStoragePersistance : IStoragePersistance
{
	private readonly Dictionary<string, string> memoryCache = new();

	public async ValueTask Store<T>(string name, T? data)
	{
		string storeData = JsonSerializer.Serialize(data);
		memoryCache[name] = storeData;

		try
		{
			await StoreInBrowserAsync(name, storeData);
		}
		catch (InvalidOperationException)
		{
			// JS interop unavailable during SSR/prerender
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.ToString());
		}
	}

	public async ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		try
		{
			string? data = await RetrieveFromBrowserAsync(name);

			if (!string.IsNullOrWhiteSpace(data))
			{
				memoryCache[name] = data;
				return JsonSerializer.Deserialize<T>(data);
			}
		}
		catch (InvalidOperationException)
		{
			// JS interop unavailable during SSR/prerender - fall through to memory cache
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.ToString());
		}

		if (memoryCache.TryGetValue(name, out string? cachedData) && !string.IsNullOrWhiteSpace(cachedData))
		{
			return JsonSerializer.Deserialize<T>(cachedData);
		}

		return null;
	}

	protected abstract ValueTask StoreInBrowserAsync(string name, string json);

	protected abstract ValueTask<string?> RetrieveFromBrowserAsync(string name);
}
