using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Stores values in memory only. Does not require JS interop and works during SSR/prerender.
/// </summary>
public class MemoryStoragePersistance : IStoragePersistance
{
	private readonly Dictionary<string, string> storage = new();

	public ValueTask Store<T>(string name, T? data)
	{
		storage[name] = JsonSerializer.Serialize(data);
		return ValueTask.CompletedTask;
	}

	public ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		if (storage.TryGetValue(name, out string? data) && !string.IsNullOrWhiteSpace(data))
		{
			return ValueTask.FromResult(JsonSerializer.Deserialize<T>(data));
		}

		return ValueTask.FromResult<T?>(null);
	}
}
