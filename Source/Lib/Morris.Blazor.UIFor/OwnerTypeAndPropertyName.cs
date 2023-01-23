using System;

namespace Morris.Blazor.UIFor;

public readonly struct OwnerTypeAndPropertyNameAndType : IEquatable<OwnerTypeAndPropertyNameAndType>
{
	public readonly Type OwnerType = null!;
	public readonly string PropertyName;
	public readonly Type PropertyType;

	public OwnerTypeAndPropertyNameAndType(
		Type ownerType,
		string propertyName,
		Type propertyType)
	{
		OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
		PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
		PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
	}

	public bool Equals(OwnerTypeAndPropertyNameAndType other) =>
		OwnerType == other.OwnerType && PropertyName == other.PropertyName;

	public override bool Equals(object? obj) =>
		obj is OwnerTypeAndPropertyNameAndType other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(OwnerType, PropertyName);

	public static bool operator ==(OwnerTypeAndPropertyNameAndType left, OwnerTypeAndPropertyNameAndType right) =>
		left.Equals(right);

	public static bool operator !=(OwnerTypeAndPropertyNameAndType left, OwnerTypeAndPropertyNameAndType right) =>
		!(left == right);

	//public static implicit operator OwnerTypeAndPropertyNameAndType((Type OwnerType, string PropertyName) value) =>
	//	new OwnerTypeAndPropertyNameAndType(value.OwnerType, value.PropertyName);
}
