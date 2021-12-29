using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStateManager.StoragePersistance;

public class SessionStoragePersistance : IStoragePersistance
{
	protected Dictionary<string, object?> State = new();

	public ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		if (State.ContainsKey(name))
			return ValueTask.FromResult(State[name] as T);

		return ValueTask.FromResult<T?>(default);
	}

	public ValueTask Store<T>(string name, T? data)
	{
		if (State.ContainsKey(name))
			State[name] = data;
		else
			State.Add(name, data);

		return ValueTask.CompletedTask;
	}
}
