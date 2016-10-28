using System;
using System.Threading.Tasks;
using Dapper;

namespace Slider.WorkDone.Data.Ado
{
	public class TenantPersister: ITenantPersister
	{
		private readonly ITenantsCommonConnectionProvider connectionProvider;

		public TenantPersister(ITenantsCommonConnectionProvider connectionProvider)
		{
			if (connectionProvider == null)
			{
				throw new ArgumentNullException(nameof(connectionProvider));
			}
			this.connectionProvider = connectionProvider;
		}

		public async Task<Tenant> Get(Guid id)
		{
			using (var conn = await connectionProvider.OpenConnection())
			{
				return await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where Id = @id", new { id });
			}
		}

		public async Task<bool> Exists(string email)
		{
			using (var conn = await connectionProvider.OpenConnection())
			{
				var t= await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where [Owner] = @email", new {email});
				return t != null;
			}
		}

		public async Task<bool> IsAvailable(Guid id)
		{
			Tenant tenant;
			using (var conn = await connectionProvider.OpenConnection())
			{
				tenant = await conn.QueryFirstOrDefaultAsync<Tenant>("select * From Tenants where Id = @id", new { id });
			}
			if (tenant == null)
			{
				return false;
			}
			return await connectionProvider.IsAvailable(tenant);
		}

		public async Task Create(Tenant tenant)
		{
			if (Guid.Empty.Equals(tenant.Id))
			{
				tenant.Id= Guid.NewGuid();
			}
			await CreateTenant(tenant);
			await connectionProvider.CreateShard(tenant);
		}

		private async Task CreateTenant(Tenant tenant)
		{
			using (var conn = await connectionProvider.OpenConnection())
			{
				await conn.ExecuteAsync("insert Tenants(Id,[Owner],Name,[Level]) values(@Id, @Owner, @Name, @Level)", tenant);
			}
		}
	}
}