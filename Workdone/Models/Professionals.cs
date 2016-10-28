using System;
using System.ComponentModel.DataAnnotations;

namespace Slider.WorkDone.Api.Models
{
	public class ProfessionalRequest
	{
		[Required]
		[StringLength(100, MinimumLength = 2)]
		[DataType(DataType.EmailAddress)]
		public string EMail { get; set; }
		[Required]
		[StringLength(100, MinimumLength = 2)]
		public string First { get; set; }
		[Required]
		[StringLength(100, MinimumLength = 2)]
		public string Last { get; set; }
		[MaxLength(100)]
		public string Middle { get; set; }
	}

	public class Professional
	{
		public Guid Id { get; set; }
		public string EMail { get; set; }
		public string First { get; set; }
		public string Last { get; set; }
		public string Middle { get; set; }
	}
}