using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Slider.WorkDone.Api.Models;
using Slider.WorkDone.Data;
using Swashbuckle.Swagger.Annotations;

namespace Slider.WorkDone.Api.Controllers
{
	[RoutePrefix("api/v1/companies")]
	public class CompaniesController : ApiController
	{
		private readonly ICompanyPersister persister;

		public CompaniesController(ICompanyPersister persister)
		{
			if (persister == null)
			{
				throw new ArgumentNullException(nameof(persister));
			}
			this.persister = persister;
		}

		// GET: api/Companies
		public IEnumerable<string> Get()
		{
			return new[] {"value1", "value2"};
		}

		[SwaggerOperation("Get")]
		[SwaggerResponse(HttpStatusCode.OK)]
		[SwaggerResponse(HttpStatusCode.NotFound)]
		[ResponseType(typeof(Models.Company))]
		[Route("{tenantId:guid}/{id:guid}", Name = "CompaniesGet")]
		public string Get(Guid tenantId, Guid id)
		{
			return "value";
		}

		[SwaggerOperation("Create")]
		[SwaggerResponse(HttpStatusCode.Created)]
		[SwaggerResponse(HttpStatusCode.BadRequest)]
		[ResponseType(typeof(Models.Company))]
		[Route("{tenantId:guid}")]
		public async Task<IHttpActionResult> Post(Guid tenantId, CompanyRequest model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			try
			{
				var entity = new Company {Name = model.Name};
				await persister.Persist(tenantId, entity);
				return Created(Url.Route("CompaniesGet", new {tenantId, id= entity.Id}), new Models.Company {Id = entity.Id, Name = entity.Name});
			}
			catch (InvalidOperationException e)
			{
				return BadRequest(e.Message);
			}
		}

		// PUT: api/Companies/5
		public void Put(int id, [FromBody] string value) {}

		// DELETE: api/Companies/5
		public void Delete(int id) {}
	}
}