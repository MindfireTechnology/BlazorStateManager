using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Stores values in the browsers local storage
/// </summary>
public class LocalStoragePersistance : ILocalStorage
{
	protected IJSRuntime? Runtime { get; }

	public LocalStoragePersistance(IJSRuntime? runtime)
	{
		Runtime = runtime;
	}

	public ValueTask Store<T>(string name, T? data)
	{
		if (Runtime != null)
		{
			try
			{
				string storeData = JsonSerializer.Serialize(data);
				return Runtime.InvokeVoidAsync("localStorage.setItem", name, storeData);				
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.ToString());
				return ValueTask.CompletedTask;
			}
		}
		else
		{
			Trace.TraceWarning("No JavaScript runtime available");
			return ValueTask.CompletedTask;
		}
	}

	public async ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		if (Runtime != null)
		{
			string data = await Runtime.InvokeAsync<string>("localStorage.getItem", name);

			if (string.IsNullOrWhiteSpace(data))
				return null;

			return JsonSerializer.Deserialize<T>(data);
		}
		else
		{
			return null;
		}
	}
}
