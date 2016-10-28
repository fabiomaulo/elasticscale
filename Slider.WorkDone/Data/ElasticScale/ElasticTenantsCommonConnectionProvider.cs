using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Slider.WorkDone.Data.Ado;

namespace Slider.WorkDone.Data.ElasticScale
{
	public class ElasticTenantsCommonConnectionProvider: ITenantsCommonConnectionProvider
	{
		private readonly ShardMapManager smm;
		private readonly SmmFacility shards;
		private readonly DbFacility database;
		private readonly MultiverseConfiguration conf;
		private readonly string connectionString;

		public ElasticTenantsCommonConnectionProvider(ShardMapManager smm, MultiverseConfiguration conf, SmmFacility shards, DbFacility database)
		{
			if (smm == null)
			{
				throw new ArgumentNullException(nameof(smm));
			}
			if (shards == null)
			{
				throw new ArgumentNullException(nameof(shards));
			}
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}
			if (conf == null)
			{
				throw new ArgumentNullException(nameof(conf));
			}
			connectionString = database.GetConnectionString(conf.ShardMapManagerServerName, conf.ShardMapManagerDatabaseName);
			this.smm = smm;
			this.shards = shards;
			this.database = database;
			this.conf = conf;
		}

		public async Task<IDbConnection> OpenConnection()
		{
			var conn = new SqlConnection(connectionString);
			await conn.OpenAsync();
			return conn;
		}

		public Task<bool> IsAvailable(Tenant tenant)
		{
			var dbName = GetTenantDbName(tenant.Level, tenant.Id);
			return database.IsOnline(conf.ShardMapManagerServerName, dbName);
		}

		public async Task CreateShard(Tenant tenant)
		{
			ListShardMap<Guid> tenantMap;
			if (!smm.TryGetListShardMap(conf.TenantsShardMapName, out tenantMap))
			{
				// No está registrado el shard map
				return;
			}

			var tenantId = tenant.Id;
			var dbName = GetTenantDbName(tenant.Level, tenantId);
			var dbEdition = GetTenantDbEdition(tenant.Level);
			PointMapping<Guid> mapping;
			if (!tenantMap.TryGetMappingForKey(tenantId, out mapping))
			{
				await shards.CreateShard(tenantMap, tenantId, dbName, () => typeof(TenantPersister).Assembly.GetManifestResourceStream(conf.InitializeShardDbScriptPath), x => { }, dbEdition);
			}
		}

		private string GetTenantDbName(string level, Guid tenantId)
		{
			var realLevel = SubscriptionLevel.GetSupported(level);
			if (SubscriptionLevel.Free.Equals(realLevel))
			{
				return "FreeShared";
			}
			if (SubscriptionLevel.Basic.Equals(realLevel))
			{
				return "StandardShared";
			}
			if (SubscriptionLevel.Standard.Equals(realLevel))
			{
				return string.Concat("t", tenantId.ToString("N"));
			}
			return "FreeShared";
		}

		private string GetTenantDbEdition(string level)
		{
			var realLevel = SubscriptionLevel.GetSupported(level);
			if (SubscriptionLevel.Free.Equals(realLevel))
			{
				return "Basic";
			}
			if (SubscriptionLevel.Basic.Equals(realLevel))
			{
				return "S0";
			}
			if (SubscriptionLevel.Standard.Equals(realLevel))
			{
				return "Basic";
			}
			return "Basic";
		}
	}
}