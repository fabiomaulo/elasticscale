using System;
using System.Data;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.Ado
{
	public interface ITenantsCommonConnectionProvider
	{
		Task<IDbConnection> OpenConnection();
		Task<bool> IsAvailable(Tenant tenant);
		Task CreateShard(Tenant tenant);
	}
}