using System;
using System.Collections.Generic;
using System.Linq;

namespace Slider.WorkDone
{
	public class Workdone:Entity
	{
		public Workdone()
		{
			Tags=new List<string>();
		}

		public Project Project { get; set; }
		public Professional Professional { get; set; }
		public string Description { get; set; }
		public DateTime Day { get; set; }
		public TimeSpan Duration { get; set; }
		public IList<string> Tags { get; }
		public bool Invoiced { get; set; }	

		public void SetTagsFromCsv(string source)
		{
			Tags.Clear();
			if (source == null)
			{
				return;
			}
			var validTags = source.Split(',').Select(t=> t.Trim()).Where(t=> !string.IsNullOrWhiteSpace(t));
			foreach (var tag in validTags)
			{
				Tags.Add(tag);
			}			
		}
	}
}