using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Slider.WorkDone.IoC
{
	public class SimpleIoCContainer : IDepencencyInjectionContainer, IDepencencyInjectionStore
	{
		private enum LifeStyle
		{
			Singleton,
			Transient
		}

		private readonly ConcurrentDictionary<string, LifeStyle> lifeStyles = new ConcurrentDictionary<string, LifeStyle>();
		private readonly ConcurrentDictionary<string, List<Func<IDepencencyInjectionContainer, object>>> ctors = new ConcurrentDictionary<string, List<Func<IDepencencyInjectionContainer, object>>>();
		private readonly ConcurrentDictionary<string, ArrayList> singletons = new ConcurrentDictionary<string, ArrayList>();
		private readonly List<IDisposable> singletonsDisposableInstancesRegistered = new List<IDisposable>();

		public void Dispose()
		{
			var allSingletons = singletons.SelectMany(x => x.Value.Cast<object>()).Select(x => x as IDisposable)
				.Where(x => x != null)
				.Concat(singletonsDisposableInstancesRegistered)
				.Distinct(new ReferenceEqualityComparar());
			foreach (var disposableSingleton in allSingletons)
			{
				disposableSingleton.Dispose();
			}
			singletonsDisposableInstancesRegistered.Clear();
			singletons.Clear();
		}

		private class ReferenceEqualityComparar : IEqualityComparer<IDisposable>
		{
			public bool Equals(IDisposable x, IDisposable y)
			{
				return ReferenceEquals(x,y);
			}

			public int GetHashCode(IDisposable obj)
			{
				return obj?.GetHashCode() ?? 0;
			}
		}

		public T GetInstance<T>() where T : class
		{
			return (T)GetInstances(typeof(T)).FirstOrDefault();
		}

		public object GetInstance(Type type)
		{
			return GetInstances(type).FirstOrDefault();
		}

		public IEnumerable<T> GetInstances<T>() where T : class
		{
			return GetInstances(typeof(T)).Cast<T>();
		}

		public IEnumerable<object> GetInstances(Type type)
		{
			LifeStyle lifeStyle;
			var keyForType = type.FullName;
			if (!lifeStyles.TryGetValue(keyForType, out lifeStyle))
			{
				yield break;
			}
			if (LifeStyle.Transient == lifeStyle)
			{
				var ctorFuncs = ctors[keyForType];
				foreach (var ctorFunc in ctorFuncs)
				{
					yield return ctorFunc(this);
				}
			}
			else
			{
				ArrayList sigleInstances;
				if (!singletons.TryGetValue(keyForType, out sigleInstances))
				{
					var ctorFuncs = ctors[keyForType];
					sigleInstances = new ArrayList(ctorFuncs.Select(ctorFunc => ctorFunc(this)).ToArray());
					singletons[keyForType] = sigleInstances;
				}
				foreach (var o in sigleInstances) { yield return o; }
			}
		}

		public void RegisterSingleton<T>(Func<IDepencencyInjectionContainer, T> ctor) where T : class
		{
			var keyForType = KeyForType<T>();
			LifeStyle registeredLifeStyle;
			if (lifeStyles.TryGetValue(keyForType, out registeredLifeStyle) && registeredLifeStyle == LifeStyle.Transient)
			{
				throw new InvalidOperationException($"Mixing lifestyle for '{keyForType}' not allowed.");
			}
			lifeStyles[keyForType] = LifeStyle.Singleton;
			RegisterCtor(keyForType, ctor);
		}

		public void RegisterSingleton<T>(T instance) where T : class
		{
			var keyForType = KeyForType<T>();
			LifeStyle registeredLifeStyle;
			if (lifeStyles.TryGetValue(keyForType, out registeredLifeStyle) && registeredLifeStyle == LifeStyle.Transient)
			{
				throw new InvalidOperationException($"Mixing lifestyle for '{keyForType}' not allowed.");
			}
			lifeStyles[keyForType] = LifeStyle.Singleton;
			var disposable = instance as IDisposable;
			if (disposable != null)
			{
				singletonsDisposableInstancesRegistered.Add(disposable);
			}
			
			RegisterCtor(keyForType, x=> instance);
		}

		public void RegisterTransient<T>(Func<IDepencencyInjectionContainer, T> ctor) where T : class
		{
			var keyForType = KeyForType<T>();
			LifeStyle registeredLifeStyle;
			if (lifeStyles.TryGetValue(keyForType, out registeredLifeStyle) && registeredLifeStyle == LifeStyle.Singleton)
			{
				throw new InvalidOperationException($"Mixing lifestyle for '{keyForType}' not allowed.");
			}

			lifeStyles[keyForType] = LifeStyle.Transient;
			RegisterCtor(keyForType, ctor);
		}

		private void RegisterCtor<T>(string keyForType, Func<IDepencencyInjectionContainer, T> ctor) where T : class
		{
			List<Func<IDepencencyInjectionContainer, object>> ctorFuncs;
			if (!ctors.TryGetValue(keyForType, out ctorFuncs))
			{
				ctorFuncs = new List<Func<IDepencencyInjectionContainer, object>>();
				ctors[keyForType] = ctorFuncs;
			}
			ctorFuncs.Add(ctor);
		}

		private string KeyForType<T>()
		{
			return typeof(T).FullName;
		}
	}
}