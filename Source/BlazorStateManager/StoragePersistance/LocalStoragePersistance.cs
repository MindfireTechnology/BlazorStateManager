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
	protected IJSRuntime Runtime { get; }

	public LocalStoragePersistance(IJSRuntime runtime)
	{
		ArgumentNullException.ThrowIfNull(runtime, nameof(runtime));
		Runtime = runtime;
	}

	public async ValueTask Store<T>(string name, T? data)
	{
		try
		{
			string storeData = JsonSerializer.Serialize(data);
			await Runtime.InvokeVoidAsync("localStorage.setItem", name, storeData);				
		}
		catch (Exception ex)
		{
			Trace.TraceError(ex.ToString());
		}
	}

	public async ValueTask<T?> Retreive<T>(string name) where T : class, new()
	{
		if (Runtime != null)
		{
			try {
			string data = await Runtime.InvokeAsync<string>("localStorage.getItem", name);

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
		else
		{
			return null;
		}
	}
}
