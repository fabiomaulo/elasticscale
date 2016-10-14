using System;
using System.ComponentModel.DataAnnotations;

namespace Slider.WorkDone.Api.Models
{
	public class SubscriptionRequest
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		[MaxLength(100)]
		public string Email { get; set; }
		[Required]
		[StringLength(140, MinimumLength = 10)]
		public string Name { get; set; }
		[Required]
		public string Level { get; set; }
	}

	public class SubscriptionUpdate
	{
		[Required]
		[StringLength(140, MinimumLength = 10)]
		public string Name { get; set; }
		[Required]
		public string Level { get; set; }
	}

	public class Subscription
	{
		public Guid Id { get; set; }
		public string Owner { get; set; }
		public string Name { get; set; }
		public string Level { get; set; }
		public string StateLink { get; set; }
	}

	public class SubscriptionState
	{
		public string State { get; set; }
	}
}