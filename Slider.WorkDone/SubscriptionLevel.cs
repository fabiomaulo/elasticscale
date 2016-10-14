using System;
using System.Collections.Generic;
using System.Linq;

namespace Slider.WorkDone
{
	public class SubscriptionLevel
	{
		public string Code { get; private set; }

		public static readonly SubscriptionLevel Free = new SubscriptionLevel { Code = "Free" };
		public static readonly SubscriptionLevel Basic = new SubscriptionLevel { Code = "Basic" };
		public static readonly SubscriptionLevel Standard = new SubscriptionLevel { Code = "Standard" };
		public static IEnumerable<SubscriptionLevel> All { get; } = new[] { Free, Basic, Standard };
		public static SubscriptionLevel GetSupported(string level) => All.FirstOrDefault(x => string.Equals(level, x.Code, StringComparison.OrdinalIgnoreCase));
	}
}