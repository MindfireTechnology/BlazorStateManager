using System;

namespace BlazorStateManager.Mediator;

public partial class BlazorMediator
{
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
