using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStateManager.Mediator
{
	public class BlazorMediator : IMediator
	{
		protected IList<TopicMap> Topics = new List<TopicMap>();
		protected ILogger<BlazorMediator> Logger { get; }

		public BlazorMediator(ILogger<BlazorMediator> logger)
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



		public async Task Publish<T>(object sender, T value)
		{
			Logger?.LogInformation($"Publish received from '{sender?.GetHashCode()}:{sender}' For Type '{typeof(T).Name}'");
			await PublishInternal(typeof(T), null, sender, value);
		}

		public async Task Publish<T>(object sender, string topic, T value)
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

		public void UnSubscribeAll(object subscriber)
		{
			Logger?.LogInformation($"Unsubscribe All received from '{subscriber?.GetHashCode()}:{subscriber}'");
			foreach (var topicMap in Topics)
			{
				topicMap.Subscribers.Where(n => n.Subscriber.IsAlive && n.Subscriber.Target == subscriber)
					.ToList()
					.ForEach(n => topicMap.Subscribers.Remove(n));
			}
		}


		protected virtual void Add(Type type, string topic, SubscriberInfo subscriberInfo)
		{
			var topicMap = Topics.SingleOrDefault(n => n.TopicString == topic && n.TopicType == type);

			if (topicMap == null)
			{
				topicMap = new TopicMap
				{
					TopicString = topic,
					TopicType = type,
				};

				Topics.Add(topicMap);
			}

			topicMap.Subscribers.Add(subscriberInfo);
		}

		protected virtual async Task PublishInternal(Type T, string topicString, object sender, object value)
		{
			var topicList = Topics
				.Where(n => n.TopicString == topicString &&
				((T == null && n.TopicType == null) || n.TopicType.IsAssignableFrom(T)));

			foreach (var topic in topicList)
			{
				var deadSubscriberList = new Lazy<List<SubscriberInfo>>();
				foreach (var subscriber in topic.Subscribers)
				{
					if (!subscriber.Subscriber.IsAlive)
						deadSubscriberList.Value.Add(subscriber);

					Logger?.LogInformation(
						$"Invoking Topic '{(topic.TopicType == null ? string.Empty : $"Type: {topic.TopicType}")} {(string.IsNullOrWhiteSpace(topic.TopicString) ? string.Empty : topic.TopicString)}' on '{subscriber?.GetHashCode()}:{subscriber}'");

					await (Task)subscriber.Action.DynamicInvoke(sender, value);
				}

				if (deadSubscriberList.IsValueCreated)
				{
					// Prune the list
					foreach (var subscriber in deadSubscriberList.Value)
					{
						Logger?.LogInformation($"Pruning Dead Subscriber");
						topic.Subscribers.Remove(subscriber);
					}

					if (topic.Subscribers.Count == 0)
					{
						Logger?.LogInformation($"Removing Unused Topic '{(topic.TopicType == null ? string.Empty : $"Type: {topic.TopicType}")} {(string.IsNullOrWhiteSpace(topic.TopicString) ? string.Empty : topic.TopicString)}'");
						Topics.Remove(topic);
					}
				}
			}
		}

		protected virtual void UnSubscribeInternal(Type type, string topic, object subscriber)
		{
			var topicMap = Topics
				.SingleOrDefault(n => (n.TopicString == topic) && (n.TopicType == type));

			if (topicMap != null)
			{
				topicMap.Subscribers
					.Where(n => n.Subscriber.IsAlive && n.Subscriber.Target == subscriber)
					.ToList()
					.ForEach(n => topicMap.Subscribers.Remove(n));
			}
		}


		internal protected class TopicMap
		{
			public Type TopicType { get; set; }
			public string TopicString { get; set; }
			public virtual IList<SubscriberInfo> Subscribers { get; set; } = new List<SubscriberInfo>();
		}

		internal protected class SubscriberInfo
		{
			public WeakReference Subscriber { get; set; }
			public Delegate Action { get; set; }
			public Type DelegateParameterType { get; set; }

			public SubscriberInfo() { }

			public SubscriberInfo(object subscriber, Type delegateParameterType, Delegate action)
			{
				Subscriber = new WeakReference(subscriber);
				DelegateParameterType = delegateParameterType;
				Action = action;
			}
		}

	}
}
