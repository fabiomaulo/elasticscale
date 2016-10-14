using System;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data
{
	public interface ITenantPersister
	{
		Task<bool> Exists(string email);
		Task<bool> IsAvailable(Guid id);
		Task Create(Tenant tenant);
	}
}