using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Stores values in the browsers local storage
/// </summary>
public class LocalStoragePersistance : JsInteropStoragePersistance, ILocalStorage
{
	protected IJSRuntime Runtime { get; }

	public LocalStoragePersistance(IJSRuntime runtime)
	{
		ArgumentNullException.ThrowIfNull(runtime, nameof(runtime));
		Runtime = runtime;
	}

	protected override ValueTask StoreInBrowserAsync(string name, string json)
		=> Runtime.InvokeVoidAsync("localStorage.setItem", name, json);

	protected override async ValueTask<string?> RetrieveFromBrowserAsync(string name)
	{
		string? data = await Runtime.InvokeAsync<string>("localStorage.getItem", name);
		return string.IsNullOrWhiteSpace(data) ? null : data;
	}
}
