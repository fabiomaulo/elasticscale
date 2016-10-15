using System;
using System.ComponentModel.DataAnnotations;

namespace Slider.WorkDone.Api.Models
{
	public class CompanyRequest
	{
		[Required]
		[StringLength(100, MinimumLength = 2)]
		public string Name { get; set; }
	}

	public class Company
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}