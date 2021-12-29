using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStateManager.Mediator;

/// <summary>
/// This is the main delegate type for a subscriber action
/// </summary>
/// <typeparam name="T">The parameter type</typeparam>
/// <param name="sender">The object that published the topic</param>
/// <param name="value">The value for the topic</param>
/// <returns></returns>
public delegate Task AsyncHandler<T>(object sender, T? value);

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
	Task Publish<T>(object sender, T? value);

	/// <summary>
	/// Publish a topic for subscribers to consume
	/// </summary>
	/// <typeparam name="T">The type of the topic</typeparam>
	/// <param name="sender">The object that is publishing the topic</param>
	/// <param name="topic">A named topic</param>
	/// <param name="value">The value for the topic</param>
	/// <returns></returns>
	Task Publish<T>(object sender, string topic, T? value);

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
