using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorStateManager.Mediator;

public class BlazorMediator : IMediator
{
	protected readonly ReaderWriterLockSlim TopicLock = new();

	protected IList<TopicMap> Topics = new List<TopicMap>();
	protected ILogger<BlazorMediator>? Logger { get; }

	public BlazorMediator(ILogger<BlazorMediator>? logger)
	{
		Logger = logger;
	}


	public void Subscribe<T>(object subscriber, AsyncHandler<T> handler)
	{
		Add(typeof(T), null, new SubscriberInfo(subscriber, typeof(T), handler));
		Logger?.LogInformation($"Subscription received from '{subscriber?.GetHashCode()}:{subscriber}' For Type '{typeof(T).Name}'");
	}

	public void Subscribe<T>(object subscriber, string topic, AsyncHandler<T> handler)
	{
		Add(typeof(T), topic, new SubscriberInfo(subscriber, typeof(T), handler));
		Logger?.LogInformation($"Subscription received from '{subscriber?.GetHashCode()}:{subscriber}' For Type '{typeof(T).Name}' / Topic '{topic}'");
	}



	public async Task Publish<T>(object sender, T? value)
	{
		Logger?.LogInformation($"Publish received from '{sender?.GetHashCode()}:{sender}' For Type '{typeof(T).Name}'");
		await PublishInternal(typeof(T), null, sender, value);
	}

	public async Task Publish<T>(object sender, string topic, T? value)
	{
		Logger?.LogInformation($"Publish received from '{sender?.GetHashCode()}:{sender}' For Type '{typeof(T).Name}' / Topic '{topic}'");
		await PublishInternal(typeof(T), topic, sender, value);
	}



	public void UnSubscribe<T>(object subscriber)
	{
		Logger?.LogInformation($"Unsubscribe received from '{subscriber?.GetHashCode()}:{subscriber}' For Type '{typeof(T).Name}'");
		UnSubscribeInternal(typeof(T), null, subscriber);
	}

	public void UnSubscribe<T>(object subscriber, string topic)
	{
		Logger?.LogInformation($"Unsubscribe received from '{subscriber?.GetHashCode()}:{subscriber}' For Type '{typeof(T).Name}' / Topic '{topic}'");
		UnSubscribeInternal(typeof(T), topic, subscriber);
	}

	public void UnSubscribeAll(object? subscriber)
	{
		TopicLock.EnterWriteLock();

		try { 
			Logger?.LogInformation($"Unsubscribe All received from '{subscriber?.GetHashCode()}:{subscriber}'");

			foreach (var topicMap in Topics)
			{
				var deleteList = topicMap.Subscribers.Where(n => n.Subscriber.IsAlive && n.Subscriber.Target == subscriber).ToList();
				deleteList.ForEach(n => topicMap.Subscribers.Remove(n));
			}
		}
		finally
		{
			TopicLock.ExitWriteLock();
		}
	}


	protected virtual void Add(Type? type, string? topic, SubscriberInfo subscriberInfo)
	{
		TopicLock.EnterWriteLock();

		try { 
			var topicMap = Topics.SingleOrDefault(n => n.TopicString == topic  /* if it's null or not */ && n.TopicType == type);

			if (topicMap == null)
			{
				topicMap = new TopicMap(type, topic);

				Topics.Add(topicMap);
			}

			topicMap.Subscribers.Add(subscriberInfo);
		}
		finally
		{ 
			TopicLock.ExitWriteLock();
		}
	}

	protected virtual async Task PublishInternal(Type? T, string? topicString, object? sender, object? value)
	{
		TopicLock.EnterUpgradeableReadLock();
		try { 
			var topicList = Topics
				.Where(n => n.TopicString == topicString /* if it's null or not */ &&
				((T == null && n.TopicType == null) || (n.TopicType != null && n.TopicType.IsAssignableFrom(T))));

			List<Task> tasks = new();

			foreach (var topic in topicList)
			{
				var deadSubscriberList = new Lazy<List<SubscriberInfo>>();

				foreach (var subscriber in topic.Subscribers)
				{
					if (!subscriber.Subscriber.IsAlive)
						deadSubscriberList.Value.Add(subscriber);

					Logger?.LogInformation(
						$"Invoking Topic '{(topic.TopicType == null ? string.Empty : $"Type: {topic.TopicType}")} {(string.IsNullOrWhiteSpace(topic.TopicString) ? string.Empty : topic.TopicString)}' on '{subscriber?.GetHashCode()}:{subscriber}'");

					if (subscriber != null && subscriber.Action != null)
						tasks.Add((Task)(subscriber.Action.DynamicInvoke(sender, value) ?? Task.CompletedTask));
				}

				if (deadSubscriberList.IsValueCreated)
				{
					TopicLock.EnterWriteLock();

					try {
						// Prune the list
						foreach (var subscriber in deadSubscriberList.Value)
						{
							Logger?.LogDebug($"Pruning Dead Subscriber");
							topic.Subscribers.Remove(subscriber);
						}

						if (topic.Subscribers.Count == 0)
						{
							Logger?.LogDebug($"Removing Unused Topic '{(topic.TopicType == null ? string.Empty : $"Type: {topic.TopicType}")} {(string.IsNullOrWhiteSpace(topic.TopicString) ? string.Empty : topic.TopicString)}'");
							Topics.Remove(topic);
						}
					}
					finally
					{
						TopicLock.ExitWriteLock();
					}
				}
			}

			await Task.WhenAll(tasks);
		}
		finally
		{
			TopicLock.ExitUpgradeableReadLock();
		}
	}

	protected virtual void UnSubscribeInternal(Type? type, string? topic, object? subscriber)
	{
		TopicLock.EnterWriteLock();

		try { 
			var topicMap = Topics
				.FirstOrDefault(n => (n.TopicString == topic) && (n.TopicType == type));

			topicMap?.Subscribers
					.Where(n => n.Subscriber.IsAlive && n.Subscriber.Target == subscriber)
					.ToList()
					.ForEach(n => topicMap.Subscribers.Remove(n));
		}
		finally
		{ 
			TopicLock.ExitWriteLock();
		}
	}

	internal protected record TopicMap
	{
		public Type? TopicType { get; init; }
		public string? TopicString { get; init; }
		public virtual IList<SubscriberInfo> Subscribers { get; } = new List<SubscriberInfo>();

		public TopicMap(Type? topicType, string? topicString)
		{
			TopicType = topicType;
			TopicString = topicString;
		}
	}

	internal protected record SubscriberInfo
	{
		public WeakReference Subscriber { get; init; }
		public Delegate? Action { get; init; }
		public Type? DelegateParameterType { get; init; }

		public SubscriberInfo(object subscriber, Type delegateParameterType, Delegate action)
		{
			Subscriber = new WeakReference(subscriber);
			DelegateParameterType = delegateParameterType;
			Action = action;
		}
	}
}
