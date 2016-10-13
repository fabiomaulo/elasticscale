namespace Slider.WorkDone
{
	public class Project: Entity
	{
		public Project()
		{
			Active = true;
		}
		public Company Owner { get; set; }
		public string Name { get; set; }
		public bool Active { get; set; }
	}
}