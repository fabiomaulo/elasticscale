using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Slider.WorkDone.Data.ElasticScale;

namespace Slider.WorkDone.Data.Ado
{
	public class TenantPersister: ITenantPersister
	{
		private readonly ShardMapManager smm;
		private readonly SmmFacility shards;
		private readonly DbFacility database;
		private readonly MultiverseConfiguration conf;
		private readonly string connectionString;

		public TenantPersister(ShardMapManager smm, MultiverseConfiguration conf, SmmFacility shards, DbFacility database)
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

		public async Task<Tenant> Get(Guid id)
		{
			using (var conn = new SqlConnection(connectionString))
			{
				await conn.OpenAsync();
				return await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where Id = @id", new { id });
			}
		}

		public async Task<bool> Exists(string email)
		{
			using (var conn = new SqlConnection(connectionString))
			{
				await conn.OpenAsync();
				var t= await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where [Owner] = @email", new {email});
				return t != null;
			}
		}

		public async Task<bool> IsAvailable(Guid id)
		{
			Tenant tenant;
			using (var conn = new SqlConnection(connectionString))
			{
				await conn.OpenAsync();
				tenant = await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where Id = @id", new { id });
			}
			if (tenant == null)
			{
				return false;
			}
			var dbName = GetTenantDbName(tenant.Level, tenant.Id);
			return await database.IsOnline(conf.ShardMapManagerServerName, dbName);
		}

		public async Task Create(Tenant tenant)
		{
			if (Guid.Empty.Equals(tenant.Id))
			{
				tenant.Id= Guid.NewGuid();
			}
			await CreateTenant(tenant);
			await CreateShard(tenant);
		}

		private async Task CreateTenant(Tenant tenant)
		{
			using (var conn = new SqlConnection(connectionString))
			{
				await conn.OpenAsync();
				await conn.ExecuteAsync("insert Tenants(Id,[Owner],Name,[Level]) values(@Id, @Owner, @Name, @Level)", tenant);
			}
		}

		private async Task CreateShard(Tenant tenant)
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
				await shards.CreateShard(tenantMap, tenantId, dbName, ()=> typeof(TenantPersister).Assembly.GetManifestResourceStream(conf.InitializeShardDbScriptPath), x => { }, dbEdition);
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