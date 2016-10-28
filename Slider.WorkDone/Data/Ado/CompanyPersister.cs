using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;

namespace Slider.WorkDone.Data.Ado
{
	public class CompanyPersister : ICompanyPersister
	{
		private readonly IConnectionProvider connectionProvider;
		public CompanyPersister(IConnectionProvider connectionProvider)
		{
			if (connectionProvider == null)
			{
				throw new ArgumentNullException(nameof(connectionProvider));
			}
			this.connectionProvider = connectionProvider;
		}

		public async Task Persist(Guid tenantId, Company entity)
		{
			entity.TenantId = tenantId;
			using (var conn = await connectionProvider.OpenConnection(tenantId))
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