# BlazorStateManager
A set of interfaces that make working with different components in Blazor easier

# IMediator
The mediator is a great way for different components in your application to work together with no coupling between types.

```
public interface IMediator
{
	/// <summary>
	/// Subscribe to published events for a given topic
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="subscriber">The object that is subscribing to the topic</param>
	/// <param name="handler">The function to invoke when this topic is published</param>
	void Subscribe<T>(object subscriber, AsyncHandler<T> handler);

	/// <summary>
	/// Subscribe to published events for a given topic
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="subscriber">The object that is subscribing to the topic</param>
	/// <param name="topic">A named topic</param>
	/// <param name="handler">The function to invoke when this topic is published</param>
	void Subscribe<T>(object subscriber, string topic, AsyncHandler<T> handler);

	/// <summary>
	/// Publish a topic for subscribers to consume
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="sender">The object that is publishing the topic</param>
	/// <param name="value">The value for the topic</param>
	/// <returns></returns>
	Task Publish<T>(object sender, T value);

	/// <summary>
	/// Publish a topic for subscribers to consume
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="sender">The object that is publishing the topic</param>
	/// <param name="topic">A named topic</param>
	/// <param name="value">The value for the topic</param>
	/// <returns></returns>
	Task Publish<T>(object sender, string topic, T value);

	/// <summary>
	/// Remove a topic subscription
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="subscriber">The object that wishes to unsubscribe from the topic. This should match the object used to subscribe</param>
	void UnSubscribe<T>(object subscriber);

	/// <summary>
	/// Remove a topic subscription
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="subscriber">The object that wishes to unsubscribe from the topic. This should match the object used to subscribe</param>
	/// <param name="topic">A named topic</param>
	void UnSubscribe<T>(object subscriber, string topic);

	/// <summary>
	/// Unsubscribes a subscriber from all topics it is associated with
	/// </summary>
	/// <param name="subscriber">The object that wishes to unsubscribe from the topic. This should match the object used to subscribe</param>
	void UnSubscribeAll(object subscriber);
}
```

# IStateManager
The State Manager is a way to store typed data into the configured StoragePersistance provider (below) 

```
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

```

# IStoragePersistance
The IStoragePersistance class provides a way for the StateManager to store values in a variety of places including Cookies, Local Storage, and Session Storage.
Typically you wouldn't use this class directly and instead you would utilize the IStateManager class (above)

```
/// <summary>
/// Interface for storing and retreiving data in the browser
/// </summary>
/// <remarks>
/// The three varients of this interface are the LocalStoragePersistance, CookieStoragePersistance, and SessionStoragePersistance
/// </remarks>
public interface IStoragePersistance
{

	/// <summary>
	/// Retreive a value from the persistance storage
	/// </summary>
	/// <typeparam name="T">The type of object to receive</typeparam>
	/// <param name="name">A key that indicates the name of the object to retreive</param>
	/// <returns></returns>
	ValueTask<T?> Retreive<T>(string name) where T : class, new();

	/// <summary>
	/// Stores a value into the persistance storage
	/// </summary>
	/// <typeparam name="T">The type of the value to store</typeparam>
	/// <param name="name">The key to store the value as</param>
	/// <param name="data">The value to store</param>
	/// <returns></returns>
	ValueTask Store<T>(string name, T? data);
}
```

# Usage
It is recommended that you segregate "Notification" topics (like X value has changed) from "Command" messages like "Add this to my cart". 
Keeping clear which kind of message you're publishing and subscribing to will avoid bugs and issues

```
@inject IMediator Events
...
@code {

	protected override async Task OnInitializedAsync()
	{
		// Subscribe to an event -- KeyConstraints.CheckoutFinishedTopic is just a static string "CheckoutFinished"
		Events.Subscribe<string>(this, KeyConstants.CheckoutFinishedTopic, (obj, value) =>
		{
			Count = 0;
			StateHasChanged();
			return Task.CompletedTask;
		});
	}

	async Task UpdateCart(ChangeEventArgs args)
	{
		await Events.Publish<string>(this, args[0]);
	}

}
```

Example usage of the StoragePersistance
```
@inject IStoragePersistance StorageService
...

@code {
	protected Cart ShoppingCart { get; set; }

	protected override async Task OnInitializedAsync()
	{
		ShoppingCart = await StorageService.Retreive<Cart>("ShoppingCart"); // Note: It is recommended that these strings are stored as static values in a class
	}

	async Task PlaceOrder()
	{
		await StorageService.Store<Cart>("ShoppingCart", ShoppingCart);
	}
}


```

Registering the services:

```
using Microsoft.Extensions.DependencyInjection;
...
builder.Services.AddBlazorStateManagerServices<LocalStoragePersistance>();
// OR
builder.Services.AddBlazorStateManagerServices<CookieStoragePersistance>();
// OR
builder.Services.AddBlazorStateManagerServices<SessionStoragePersistance>();

```
