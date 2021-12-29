using BlazorStateManager.Mediator;
using System;
using System.Threading.Tasks;

namespace BlazorStateManager.State;

public interface IStateManager
{
	/// <summary>
	/// Commit the value to a key in an underlying store
	/// </summary>
	/// <typeparam name="T?">The type of value to store</typeparam>
	/// <param name="key">The key to store the value under</param>
	/// <param name="value">The value to store</param>
	Task CommitState<T>(string key, T? value);

	/// <summary>
	/// Commit the value to a key
	/// </summary>
	/// <typeparam name="T?">The type of value to store</typeparam>
	/// <param name="value">The value to store</param>
	/// <remarks>In this instance, the type itself is the key</remarks>
	Task CommitState<T>(T? value);

	/// <summary>
	/// Gets a value from the underlying store based on the type
	/// </summary>
	/// <typeparam name="T">The type of value to retreive</typeparam>
	/// <returns>The value previously saved or default</returns>
	ValueTask<T?> GetState<T>() where T : class, new();

	/// <summary>
	/// Gets a value from the underlying store based on a key name
	/// </summary>
	/// <typeparam name="T">The type of value to retreive</typeparam>
	/// <param name="key">The key to retreive the value from</param>
	/// <returns>Previously stored value or default</returns>
	ValueTask<T?> GetState<T>(string key) where T : class, new();

	/// <summary>
	/// Register a handler to be notified if a type has changed
	/// </summary>
	/// <typeparam name="T">The type we're interested in (and all assignable subtypes)</typeparam>
	/// <param name="subscriber">The object that is subscribing to these events</param>
	/// <param name="handler">An async handler that is given the new value</param>
	void OnCommitted<T>(object subscriber, AsyncHandler<T> handler);

	/// <summary>
	/// Register a handler to be notified if a key of a type has changed
	/// </summary>
	/// <typeparam name="T">The type we're interested in (and all assignable subtypes)</typeparam>
	/// <param name="subscriber">The object that is subscribing to these events</param>
	/// <param name="key">The key of the commit we're interested in</param>
	/// <param name="handler">An async handler that is given the new value</param>
	void OnCommitted<T>(object subscriber, string key, AsyncHandler<T> handler);
}
