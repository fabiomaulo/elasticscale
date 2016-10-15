﻿using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Slider.WorkDone.Data.ElasticScale;

namespace Slider.WorkDone.Data.Ado
{
	public class EntityTenantPersisterBase
	{
		private readonly MultiverseConfiguration conf;
		private readonly DbFacility database;
		private readonly ShardMapManager smm;

		public EntityTenantPersisterBase(ShardMapManager smm, MultiverseConfiguration conf, DbFacility database)
		{
			if (smm == null)
			{
				throw new ArgumentNullException(nameof(smm));
			}
			if (conf == null)
			{
				throw new ArgumentNullException(nameof(conf));
			}
			if (database == null)
			{
				throw new ArgumentNullException(nameof(database));
			}
			this.smm = smm;
			this.conf = conf;
			this.database = database;
		}

		protected async Task<IDbConnection> GetConnectionOrThrows(Guid tenantId)
		{
			var tenantMap = GetTenantMap(tenantId);
			if (tenantMap == null)
			{
				throw new InvalidOperationException("Tenant suspendido o inexistente.");
			}
			return await tenantMap.OpenConnectionForKeyAsync(tenantId, database.GetCredentialsConnectionString());
		}

		private ListShardMap<Guid> GetTenantMap(Guid tenantId)
		{
			ListShardMap<Guid> tenantMap;
			PointMapping<Guid> mapping;
			if (!smm.TryGetListShardMap(conf.TenantsShardMapName, out tenantMap) || !tenantMap.TryGetMappingForKey(tenantId, out mapping))
			{
				return null;
			}
			if (mapping.Status == MappingStatus.Offline)
			{
				return null;
			}
			return tenantMap;
		}
	}
}