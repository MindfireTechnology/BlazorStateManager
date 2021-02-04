using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.JSInterop;
using Microsoft.Net.Http.Headers;

namespace BlazorStateManager.StoragePersistance
{
	/// <summary>
	/// Stores values in the browsers cookies
	/// </summary>
	public class CookieStoragePersistance : IStoragePersistance
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
			Runtime = runtime;
		}


		public async ValueTask Store<T>(string name, T data)
		{
			string storeData = JsonSerializer.Serialize(data);

			if (string.IsNullOrWhiteSpace(name))
				throw new InvalidCastException($"{nameof(name)} cannot be empty");

			string cookieData = $"{SanitizeCookieName(name)}={HttpUtility.UrlEncode(storeData)}";

			// TODO: Support the settings on the cookie

			string cookies = await Runtime.InvokeAsync<string>("eval", $"document.cookie='{cookieData}';");
		}

		public async ValueTask<T> Retreive<T>(string name) where T : class, new()
		{
			string cookies = await Runtime.InvokeAsync<string>("eval", "document.cookie");			

			if (string.IsNullOrWhiteSpace(cookies))
				return null;

			foreach (string cookie in cookies.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
			{
				if (GetCookieName(cookie) == name)
					return JsonSerializer.Deserialize<T>(HttpUtility.UrlDecode(GetCookieValue(cookie)));
			}

			return null;
		}

		protected string SanitizeCookieName(string name)
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

		protected string GetCookieName(string cookie)
		{
			if (string.IsNullOrWhiteSpace(cookie))
				return null;

			return cookie.Substring(0, cookie.IndexOf('=')).Trim();
		}

		protected string GetCookieValue(string cookie)
		{
			return cookie.Substring(cookie.IndexOf('=') + 1);
		}
	}
}
