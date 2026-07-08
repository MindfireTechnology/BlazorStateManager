using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.JSInterop;

namespace BlazorStateManager.StoragePersistance;

/// <summary>
/// Stores values in the browsers cookies
/// </summary>
public class CookieStoragePersistance : JsInteropStoragePersistance, IStoragePersistance
{
	protected IJSRuntime Runtime { get; }

	// TODO: Implement these some day
	//public string Domain { get; set; }
	//public string Path { get; set; }
	//public DateTime? Expiration { get; set; }
	//public TimeSpan? MaxAge { get; set; }
	//public bool Secure { get; set; }
	//public SameSiteMode SameSite { get; set; }

	public CookieStoragePersistance(IJSRuntime runtime)
	{
		ArgumentNullException.ThrowIfNull(runtime, nameof(runtime));
		Runtime = runtime;
	}

	protected override async ValueTask StoreInBrowserAsync(string name, string json)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new InvalidCastException($"{nameof(name)} cannot be empty");

		string cookieData = $"{SanitizeCookieName(name)}={HttpUtility.UrlEncode(json)}";

		// TODO: Support the settings on the cookie

		await Runtime.InvokeAsync<string>("eval", $"document.cookie='{cookieData}';");
	}

	protected override async ValueTask<string?> RetrieveFromBrowserAsync(string name)
	{
		string cookies = await Runtime.InvokeAsync<string>("eval", "document.cookie");

		if (string.IsNullOrWhiteSpace(cookies))
			return null;

		foreach (string cookie in cookies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
		{
			if (GetCookieName(cookie) == name)
			{
				string? cookieValue = GetCookieValue(cookie);
				if (string.IsNullOrWhiteSpace(cookieValue) is not true)
					return HttpUtility.UrlDecode(cookieValue);
				else
					return null;
			}
		}

		return null;
	}

	protected static string SanitizeCookieName(string name)
	{
		return name.Trim()
			.Replace(" ", "")
			.Replace("\t", "")
			.Replace("\r", "")
			.Replace("\n", "")
			.Replace("=", "")
			.Replace(";", "")
			.Replace(",", "");
	}

	protected static string? GetCookieName(string cookie)
	{
		if (string.IsNullOrWhiteSpace(cookie))
			return null;

		return cookie[..cookie.IndexOf('=')].Trim();
	}

	protected static string? GetCookieValue(string cookie)
	{
		return cookie?[(cookie.IndexOf('=') + 1)..];
	}
}
