using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Slider.WorkDone.Api.Controllers;
using Slider.WorkDone.Data;
using Slider.WorkDone.Data.Ado;
using Slider.WorkDone.Data.ElasticScale;
using Slider.WorkDone.IoC;

namespace Slider.WorkDone.Api
{
	public static class ServicesConfig
	{
		public static void Register(this SimpleIoCContainer container, HttpConfiguration config)
		{
			RegisterPersisters(container);
			RegisterControllers(container);
			config.DependencyResolver= new Resolver(container);
		}

		private static void RegisterControllers(IDepencencyInjectionStore store)
		{
			store.RegisterTransient(c=> new SubscriptionsController(c.GetInstance<ITenantPersister>()));
			store.RegisterTransient(c => new CompaniesController(c.GetInstance<ICompanyPersister>()));
		}

		private static void RegisterPersisters(IDepencencyInjectionStore store)
		{
			var conf = new MultiverseConfiguration
			{
				ShardMapManagerServerName = ConfigurationManager.AppSettings["ServerName"],
				UniverseServerName = ConfigurationManager.AppSettings["ServerName"],
				UniverseUserId = ConfigurationManager.AppSettings["Slider:UserName"],
				UniversePassword = ConfigurationManager.AppSettings["Slider:Password"]
			};
			store.RegisterSingleton(conf);
			store.RegisterSingleton(new DbFacility(conf.UniverseUserId, conf.UniversePassword));
			store.RegisterSingleton(c => new SmmFacility(conf, c.GetInstance<DbFacility>()));
			store.RegisterSingleton(c=> c.GetInstance<SmmFacility>().CreateOrGetShardMapManager());

			if (ConfigurationManager.AppSettings["Slider:BeElastic"].Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				store.RegisterSingleton<IConnectionProvider>(c => new ElasticScaleConnectionProvider(
				c.GetInstance<ShardMapManager>(),
				c.GetInstance<MultiverseConfiguration>(),
				c.GetInstance<DbFacility>())
				);
				store.RegisterSingleton<ITenantsCommonConnectionProvider>(c => new ElasticTenantsCommonConnectionProvider(
					c.GetInstance<ShardMapManager>(),
					c.GetInstance<MultiverseConfiguration>(),
					c.GetInstance<SmmFacility>(),
					c.GetInstance<DbFacility>()
					));
			}
			else
			{
				var connectionString = ConfigurationManager.ConnectionStrings["Slider:Once"].ConnectionString;
				store.RegisterSingleton<IConnectionProvider>(c => new FixedConnectionProvider(connectionString));
				store.RegisterSingleton<ITenantsCommonConnectionProvider>(c => new FixedTenantsCommonConnectionProvider(connectionString));
			}

			store.RegisterSingleton<ITenantPersister>(c=> new TenantPersister(c.GetInstance<ITenantsCommonConnectionProvider>()));
			store.RegisterSingleton<ICompanyPersister>(c => new CompanyPersister(c.GetInstance<IConnectionProvider>()));
		}

		private class Resolver : IDependencyResolver
		{
			private readonly IDepencencyInjectionContainer container;

			public Resolver(IDepencencyInjectionContainer container)
			{
				if (container == null)
				{
					throw new ArgumentNullException(nameof(container));
				}
				this.container = container;
			}

			public void Dispose()
			{
				container.Dispose();
			}

			public object GetService(Type serviceType) => container.GetInstance(serviceType);

			public IEnumerable<object> GetServices(Type serviceType) => container.GetInstances(serviceType);

			public IDependencyScope BeginScope()
			{
				return new FakeScopedContainer(container);
			}
		}

		private class FakeScopedContainer: IDependencyScope
		{
			private readonly IDepencencyInjectionContainer container;
			public FakeScopedContainer(IDepencencyInjectionContainer container)
			{
				this.container = container;
			}

			public void Dispose()
			{
				// El container tiene solo Singletons y transients.
			}

			public object GetService(Type serviceType) => container.GetInstance(serviceType);

			public IEnumerable<object> GetServices(Type serviceType) => container.GetInstances(serviceType);
		}
	}
}