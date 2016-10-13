using System;
using System.Collections.Generic;

namespace Slider.WorkDone.IoC
{
	public interface IDepencencyInjectionContainer : IDisposable
	{
		T GetInstance<T>() where T : class;
		object GetInstance(Type type);

		IEnumerable<T> GetInstances<T>() where T : class;
		IEnumerable<object> GetInstances(Type type);
	}
}