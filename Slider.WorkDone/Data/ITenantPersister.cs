using System;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data
{
	public interface ITenantPersister
	{
		Task<Tenant> Get(Guid id);
		Task<bool> Exists(string email);
		Task<bool> IsAvailable(Guid id);
		Task Create(Tenant tenant);
	}
}