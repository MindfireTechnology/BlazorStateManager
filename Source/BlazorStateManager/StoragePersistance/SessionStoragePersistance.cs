using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorStateManager.StoragePersistance;

public class SessionStoragePersistance : ISessionStorage
{
	protected IJSRuntime Runtime { get; }

	public SessionStoragePersistance(IJSRuntime runtime)
	{
		ArgumentNullException.ThrowIfNull(runtime, nameof(runtime));
		Runtime = runtime;
	}

	public async ValueTask Store<T>(string name, T? data)
	{
		try
		{
			string storeData = JsonSerializer.Serialize(data);
			await Runtime.InvokeVoidAsync("sessionStorage.setItem", name, storeData);
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.ToString());
		}
	}

	public async ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		try { 
		string data = await Runtime.InvokeAsync<string>("sessionStorage.getItem", name);

		if (string.IsNullOrWhiteSpace(data))
			return null;

		return JsonSerializer.Deserialize<T>(data);
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.ToString());
			return null;
		}
	}

}
