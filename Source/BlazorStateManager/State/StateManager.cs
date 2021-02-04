using BlazorStateManager.Mediator;
using BlazorStateManager.StoragePersistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStateManager.State
{

	public class StateManager : IStateManager
	{
		protected IStoragePersistance Store { get; }
		public IMediator Mediator { get; }

		public StateManager(IStoragePersistance store, IMediator mediator)
		{
			Store = store;
			Mediator = mediator;
		}

		public async ValueTask<T> GetState<T>() where T : class, new()
		{
			var result = await Store.Retreive<T>(typeof(T).FullName);
			if (result == null)
				return new();
			else
				return result;
		}

		public async ValueTask<T> GetState<T>(string name) where T : class, new()
		{
			var result = await Store.Retreive<T>(name);
			if (result == null)
				return new();
			else
				return result;
		}

		public async Task CommitState<T>(T value)
		{
			await CommitState(typeof(T).FullName, value);
			await Mediator.Publish<T>(this, value);
		}

		public async Task CommitState<T>(string name, T value)
		{
			await Store.Store(name, value);
			await Mediator.Publish<T>(this, name, value);
		}

		public void OnCommitted<T>(object subscriber, AsyncHandler<T> handler)
		{
			Mediator.Subscribe<T>(subscriber, handler);
		}

		public void OnCommitted<T>(object subscriber, string name, AsyncHandler<T> handler)
		{
			Mediator.Subscribe<T>(subscriber, name, handler);
		}
	}
}
