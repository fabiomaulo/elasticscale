using System;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data.Ado
{
	public class TenantPersister: ITenantPersister
	{
		public Task<bool> Exists(string email)
		{
			return null;
		}

		public Task<bool> IsAvailable(Guid id)
		{
			return Task.FromResult(false);
		}

		public Task Create(Tenant tenant)
		{
			return null;
		}
	}
}