using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.Ado
{
	public class FixedTenantsCommonConnectionProvider: ITenantsCommonConnectionProvider
	{
		private readonly string connectionString;
		public FixedTenantsCommonConnectionProvider(string connectionString)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException(nameof(connectionString));
			}
			this.connectionString = connectionString;
		}

		public async Task<IDbConnection> OpenConnection()
		{
			var conn = new SqlConnection(connectionString);
			await conn.OpenAsync();
			return conn;
		}

		public Task<bool> IsAvailable(Tenant tenant)
		{
			return Task.FromResult(true);
		}

		public Task CreateShard(Tenant tenant)
		{
			return Task.FromResult(true);
		}
	}
}