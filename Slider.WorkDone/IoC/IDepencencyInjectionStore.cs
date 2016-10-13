using System;

namespace Slider.WorkDone.IoC
{
	public interface IDepencencyInjectionStore
	{
		void RegisterSingleton<T>(Func<IDepencencyInjectionContainer, T> ctor) where T : class;
		void RegisterSingleton<T>(T instance) where T : class;
		void RegisterTransient<T>(Func<IDepencencyInjectionContainer, T> ctor) where T : class;
	}
}