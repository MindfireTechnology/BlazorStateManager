using BlazorStateManager.Mediator;
using BlazorStateManager.State;
using BlazorStateManager.StoragePersistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class DependencyRegistrations
	{
		public static void RegisterStateManagerServices(this IServiceCollection services)
		{
			services.AddSingleton<IMediator, BlazorMediator>();
			services.AddSingleton<IStoragePersistance, LocalStoragePersistance>();
			//services.AddSingleton<IStoragePersistance, CookieStoragePersistance>();
			services.AddScoped<IStateManager, StateManager>();
		}
	}
}
