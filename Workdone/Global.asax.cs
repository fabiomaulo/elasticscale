using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Schema;
using Slider.WorkDone.Data.ElasticScale;
using Slider.WorkDone.IoC;

namespace Slider.WorkDone.Api
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		private readonly SimpleIoCContainer container= new SimpleIoCContainer();

		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configure(c => container.Register(c));
			InitializeShardMap();
		}

		private void InitializeShardMap()
		{
			var conf = container.GetInstance<MultiverseConfiguration>();
			var dataBase = container.GetInstance<DbFacility>();
			CreateShardMapDbWhereNeeded(conf, dataBase);
			var smm = container.GetInstance<ShardMapManager>();
			var shards = container.GetInstance<SmmFacility>();
			var shardMaps = smm.GetSchemaInfoCollection();
			shards.CreateOrGetListShardMap<Guid>(smm, conf.TenantsShardMapName);
			if (shardMaps.Any(x => x.Key == conf.TenantsShardMapName))
			{
				// Acá abría que efectuar modificaciones al schema del sharding
				return;
			}
			CreateSchemaInfo(smm, conf.TenantsShardMapName);
			CreateCommonShema(conf, dataBase);
		}

		private static void CreateSchemaInfo(ShardMapManager smm, string shardMapName)
		{
			SchemaInfo schemaInfo = new SchemaInfo();
			schemaInfo.Add(new ShardedTableInfo("Companies", "TenantId"));
			schemaInfo.Add(new ShardedTableInfo("Professionals", "TenantId"));
			schemaInfo.Add(new ShardedTableInfo("Worksdone", "TenantId"));

			smm.GetSchemaInfoCollection().Add(shardMapName, schemaInfo);
		}

		private void CreateCommonShema(MultiverseConfiguration conf, DbFacility database)
		{
			var connectionString = database.GetConnectionString(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName);
			var assembly = typeof(DbFacility).Assembly;
			var resourceName = "Slider.WorkDone.Data.InitializeCommon.sql";
			DbFacility.ExecuteSqlScript(connectionString, assembly.GetManifestResourceStream(resourceName)).Wait();
		}

		private static void CreateShardMapDbWhereNeeded(MultiverseConfiguration conf, DbFacility database)
		{
			if (!database.Exists(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName).Result)
			{
				var created = database.CreateDatabase(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName, waitIsOnLine: true).Result;
			}
		}
	}
}
