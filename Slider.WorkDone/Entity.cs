using System;

namespace Slider.WorkDone
{
	public class Entity : IEquatable<Entity>
	{
		public Guid Id { get; set; }
		public Guid TenantId { get; set; }

		public bool Equals(Entity other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			bool otherIsTransient = other.IsTransient();
			bool thisIsTransient = IsTransient();
			if (otherIsTransient && thisIsTransient)
			{
				return TransientsEquals(this, other);
			}
			return Id.Equals(other.Id);
		}

		protected bool TransientsEquals(Entity entity, Entity other)
		{
			return ReferenceEquals(this, other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			var other = obj as Entity;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return IsTransient() ? base.GetHashCode() : Id.GetHashCode();
		}

		protected bool IsTransient()
		{
			return Equals(Id, Guid.Empty);
		}
	}
}