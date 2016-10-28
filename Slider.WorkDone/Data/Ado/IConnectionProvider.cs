using System;
using System.Data;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.Ado
{
	public interface IConnectionProvider
	{
		Task<IDbConnection> OpenConnection(Guid tenantId);
	}
}