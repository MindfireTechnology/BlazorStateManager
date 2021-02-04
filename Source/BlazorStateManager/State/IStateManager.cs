using BlazorStateManager.Mediator;
using System;
using System.Threading.Tasks;

namespace BlazorStateManager.State
{
	public interface IStateManager
	{
		Task CommitState<T>(string name, T value);
		Task CommitState<T>(T value);

		ValueTask<T> GetState<T>() where T : class, new();
		ValueTask<T> GetState<T>(string name) where T : class, new();

		void OnCommitted<T>(object subscriber, AsyncHandler<T> handler);
		void OnCommitted<T>(object subscriber, string name, AsyncHandler<T> handler);
	}
}
