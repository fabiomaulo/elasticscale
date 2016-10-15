using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slider.WorkDone.Data
{
	public interface ICompanyPersister: IEntityTenantPersister<Company>
	{
		Task<IEnumerable<Company>> Get(Guid tenantId, int skip = 0, int take = 24);
	}
}