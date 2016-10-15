using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Slider.WorkDone.Data.ElasticScale;

namespace Slider.WorkDone.Data.Ado
{
	public class CompanyPersister : EntityTenantPersisterBase, ICompanyPersister
	{
		public CompanyPersister(ShardMapManager smm, MultiverseConfiguration conf, DbFacility database) : base(smm, conf, database) {}

		public async Task Persist(Guid tenantId, Company entity)
		{
			entity.TenantId = tenantId;
			using (var conn = await GetConnectionOrThrows(tenantId))
			{
				if (Guid.Empty.Equals(entity.Id))
				{
					entity.Id = Guid.NewGuid();
					await conn.ExecuteAsync("insert Companies(Id, TenantId, Name) values (@Id,@TenantId, @Name)", entity);
				}
				else
				{
					await conn.ExecuteAsync("update Companies SET Name=@Name", entity);
				}
			}
		}

		public Task<Company> Get(Guid tenantId, Guid id)
		{
			return null;
		}

		public Task Remove(Guid tenantId, Guid id)
		{
			return null;
		}

		public Task Remove(Guid tenantId, Company entity)
		{
			return null;
		}

		public Task<IEnumerable<Company>> Get(Guid tenantId, int skip = 0, int take = 24)
		{
			return null;
		}
	}
}