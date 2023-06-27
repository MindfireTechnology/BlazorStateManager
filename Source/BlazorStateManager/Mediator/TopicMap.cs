using System;
using System.Collections.Generic;

namespace BlazorStateManager.Mediator;

public partial class BlazorMediator
{
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
}
