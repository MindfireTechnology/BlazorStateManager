using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorStateManager.StoragePersistance;

public class SessionStoragePersistance : JsInteropStoragePersistance, ISessionStorage
{
	protected IJSRuntime Runtime { get; }

	public SessionStoragePersistance(IJSRuntime runtime)
	{
		ArgumentNullException.ThrowIfNull(runtime, nameof(runtime));
		Runtime = runtime;
	}

	protected override ValueTask StoreInBrowserAsync(string name, string json)
		=> Runtime.InvokeVoidAsync("sessionStorage.setItem", name, json);

	protected override async ValueTask<string?> RetrieveFromBrowserAsync(string name)
	{
		string? data = await Runtime.InvokeAsync<string>("sessionStorage.getItem", name);
		return string.IsNullOrWhiteSpace(data) ? null : data;
	}
}
