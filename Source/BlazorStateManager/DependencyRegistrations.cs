using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorStateManager.Mediator;
using BlazorStateManager.State;
using BlazorStateManager.StoragePersistance;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyRegistrations
{
	/// <summary>
	/// Register the types required to run BlazorStateManager
	/// </summary>
	/// <typeparam name="TStorage">The type of storage persistance to use. There are three built-in types: CookieStoragePersistance, LocalStorageStoragePersistance, and SessionStoragePersistance</typeparam>
	/// <param name="services">The IServicesCollection to configure</param>
	/// <remarks>Both the CookieStoragePersistance and the LocalStoragePersistance require the IJSInterop to work correctly</remarks>
	public static void AddBlazorStateManagerServices<TStorage>(this IServiceCollection services) where TStorage : IStoragePersistance
	{
		services.AddSingleton<IMediator, BlazorMediator>();
		services.AddSingleton<IStateManager, StateManager>();

		services.AddSingleton(typeof(IStoragePersistance), typeof(TStorage));
	}
}
