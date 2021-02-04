using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStateManager.Mediator
{
	public delegate Task AsyncHandler<T>(object sender, T value);

	public interface IMediator
	{
		void Subscribe<T>(object subscriber, AsyncHandler<T> handler);
		void Subscribe<T>(object subscriber, string topic, AsyncHandler<T> handler);

		Task Publish<T>(object sender, T value);
		Task Publish<T>(object sender, string topic, T value);

		void UnSubscribe<T>(object subscriber);
		void UnSubscribe<T>(object subscriber, string topic);
		void UnSubscribeAll(object subscriber);
	}
}
