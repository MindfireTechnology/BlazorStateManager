using BlazorStateManager.Mediator;
using BlazorStateManager.StoragePersistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorStateManager.State;

public class StateManager : IStateManager
{
	protected IStoragePersistance Store { get; }
	public IMediator Mediator { get; }

	public StateManager(IStoragePersistance store, IMediator mediator)
	{
		Store = store;
		Mediator = mediator;
	}

	public async ValueTask<T?> GetState<T>() where T : class, new()
	{
		if (Store == null)
			return default;

		string fullName = typeof(T).FullName ?? throw new InvalidOperationException("The type of value must exist");
		var result = await Store.Retreive<T>(fullName);
		if (result == null)
			return new();
		else
			return result;

	}

	public async ValueTask<T?> GetState<T>(string name) where T : class, new()
	{
		if (Store == null)
			return default;

		var result = await Store.Retreive<T>(name);
		if (result == null)
			return new();
		else
			return result;
	}

	public async Task CommitState<T>(T? value)
	{
		string fullName = typeof(T).FullName ?? throw new InvalidOperationException("The type of value must exist");
		await CommitState(fullName, value);
		await Mediator.Publish(this, value);
	}

	public async Task CommitState<T>(string name, T? value)
	{
		await Store.Store(name, value);
		await Mediator.Publish(this, name, value);
	}

	public void OnCommitted<T>(object subscriber, AsyncHandler<T> handler)
	{
		Mediator.Subscribe(subscriber, handler);
	}

	public void OnCommitted<T>(object subscriber, string name, AsyncHandler<T> handler)
	{
		Mediator.Subscribe(subscriber, name, handler);
	}
}
