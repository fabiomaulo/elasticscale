using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.Ado
{
	public class FixedConnectionProvider: IConnectionProvider
	{
		private readonly string connectionString;
		public FixedConnectionProvider(string connectionString)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException(nameof(connectionString));
			}
			this.connectionString = connectionString;
		}

		public async Task<IDbConnection> OpenConnection(Guid tenantId)
		{
			var conn = new SqlConnection(connectionString);
			await conn.OpenAsync();
			return conn;
		}
	}
}