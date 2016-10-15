using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Slider.WorkDone.Api.Models;
using Slider.WorkDone.Data;
using Swashbuckle.Swagger.Annotations;

namespace Slider.WorkDone.Api.Controllers
{
	[RoutePrefix("api/v1/subscriptions")]
	public class SubscriptionsController : ApiController
	{
		private readonly ITenantPersister tenantPersister;
		public SubscriptionsController(ITenantPersister tenantPersister)
		{
			if (tenantPersister == null)
			{
				throw new ArgumentNullException(nameof(tenantPersister));
			}
			this.tenantPersister = tenantPersister;
		}

		[SwaggerOperation("Get")]
		[SwaggerResponse(HttpStatusCode.NotFound)]
		[SwaggerResponse(HttpStatusCode.OK)]
		[ResponseType(typeof(Subscription))]
		[Route("{id:guid}")]
		public async Task<IHttpActionResult> Get(Guid id)
		{
			var tenant = await tenantPersister.Get(id);
			if (tenant == null)
			{
				return NotFound();
			}
			var subscription = new Subscription
			{
				Id = tenant.Id,
				Owner = tenant.Owner,
				Name = tenant.Name,
				Level = tenant.Level,
				StateLink = Url.Route("SubscriptionState", new { id = tenant.Id })
			};
			return Ok(subscription);
		}

		[SwaggerOperation("State")]
		[SwaggerResponse(HttpStatusCode.OK)]
		[ResponseType(typeof(SubscriptionState))]
		[Route("{id:guid}/state", Name = "SubscriptionState")]
		[HttpGet]
		public async Task<IHttpActionResult> State(Guid id)
		{
			var isavailable = await tenantPersister.IsAvailable(id);
			return Ok(new SubscriptionState {State = isavailable ? "Available":"NotAvailable"});
		}

		[SwaggerOperation("Create")]
		[SwaggerResponse(HttpStatusCode.Accepted)]
		[SwaggerResponse(HttpStatusCode.BadRequest)]
		[SwaggerResponse(HttpStatusCode.Conflict)]
		[ResponseType(typeof(Subscription))]
		[Route("")]
		public async Task<IHttpActionResult> Post(SubscriptionRequest subscriptionRequest)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (await tenantPersister.Exists(subscriptionRequest.Email))
			{
				return Conflict();
			}
			var tenant = new Tenant
			{
				Owner = subscriptionRequest.Email,
				Name = subscriptionRequest.Name,
				Level = (SubscriptionLevel.GetSupported(subscriptionRequest.Level) ?? SubscriptionLevel.Free).Code,
			};
			await tenantPersister.Create(tenant);
			var subscription = new Subscription
			{
				Id = tenant.Id,
				Owner = tenant.Owner,
				Name = tenant.Name,
				Level = tenant.Level,
				StateLink = Url.Route("SubscriptionState", new {id= tenant.Id})
			};
			return Content(HttpStatusCode.Accepted, subscription);
		}

		public void Put(Guid id, SubscriptionUpdate value) {}

		public void Delete(Guid id) {}
	}
}