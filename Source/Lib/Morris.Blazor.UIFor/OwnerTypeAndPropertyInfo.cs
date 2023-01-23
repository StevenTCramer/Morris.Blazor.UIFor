using System;

namespace Morris.Blazor.UIFor;

public readonly struct OwnerTypeAndPropertyInfo : IEquatable<OwnerTypeAndPropertyInfo>
{
	public readonly Type OwnerType = null!;
	public readonly string? PropertyName;

	public OwnerTypeAndPropertyInfo(Type ownerType, string propertyName) : this(ownerType)
	{
		PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
	}

	public OwnerTypeAndPropertyInfo(Type ownerType)
	{
		OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
	}

	public bool Equals(OwnerTypeAndPropertyInfo other)
	{
		return OwnerType == other.OwnerType && PropertyName == other.PropertyName;
	}

	public override bool Equals(object? obj)
	{
		if (obj is OwnerTypeAndPropertyInfo other)
			return Equals(other);
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(OwnerType, PropertyName);
	}

	public static bool operator ==(OwnerTypeAndPropertyInfo left, OwnerTypeAndPropertyInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OwnerTypeAndPropertyInfo left, OwnerTypeAndPropertyInfo right)
	{
		return !(left == right);
	}

	public static implicit operator OwnerTypeAndPropertyInfo((Type OwnerType, string PropertyName) value) =>
		new OwnerTypeAndPropertyInfo(value.OwnerType, value.PropertyName);
}
