using System.Collections.Generic;

namespace Slider.WorkDone
{
	public class Company: Entity
	{
		private readonly ISet<Project> projects;

		public Company()
		{
			projects= new HashSet<Project>();
		}

		public string Name { get; set; }

		public IEnumerable<Project> Projects => projects;

		public void Add(Project project)
		{
			if (project == null)
			{
				return;
			}
			var existingOwner = project.Owner;
			if (existingOwner != null && !Equals(existingOwner, this))
			{
				existingOwner.Remove(project);
			}
			project.Owner = this;
			projects.Add(project);
		}

		public void Remove(Project project)
		{
			if (project == null || !Equals(this, project.Owner))
			{
				return;
			}
			projects.Remove(project);
		}
	}
}