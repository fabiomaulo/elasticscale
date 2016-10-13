using System;
using System.Threading.Tasks;

namespace Slider.WorkDone
{
	public interface IEntityTenantPersister<TEntity> where TEntity: Entity
	{
		Task Persist(Guid tenantId, TEntity entity);
		Task<TEntity> Get(Guid tenantId, Guid id);
		Task Remove(Guid tenantId, Guid id);
		Task Remove(Guid tenantId, TEntity entity);
	}
}