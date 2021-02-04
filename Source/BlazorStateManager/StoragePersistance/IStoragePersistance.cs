using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStateManager.StoragePersistance
{
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
		ValueTask<T> Retreive<T>(string name) where T : class, new();

		/// <summary>
		/// Stores a value into the persistance storage
		/// </summary>
		/// <typeparam name="T">The type of the value to store</typeparam>
		/// <param name="name">The key to store the value as</param>
		/// <param name="data">The value to store</param>
		/// <returns></returns>
		ValueTask Store<T>(string name, T data);
	}
}
