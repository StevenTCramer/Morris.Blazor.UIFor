using System;
using System.Reflection;

namespace Morris.Blazor.UIFor;

public readonly struct ComponentTypeAndPropertyInfo : IEquatable<ComponentTypeAndPropertyInfo>
{
	public readonly Type ComponentType = null!;
	public readonly PropertyInfo? PropertyInfo;
	public readonly Type PropertyType = null!;

	public ComponentTypeAndPropertyInfo(
		Type componentType,
		PropertyInfo? propertyInfo,
		Type propertyType) 
	{
		PropertyType = propertyType ?? throw new ArgumentNullException();
		ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
		PropertyInfo = propertyInfo;
	}

	public bool Equals(ComponentTypeAndPropertyInfo other)
	{
		return ComponentType == other.ComponentType
			&& PropertyInfo == other.PropertyInfo
			&& PropertyType == other.PropertyType;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ComponentTypeAndPropertyInfo other)
			return Equals(other);
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ComponentType, PropertyInfo, PropertyType);
	}

	public static bool operator ==(ComponentTypeAndPropertyInfo left, ComponentTypeAndPropertyInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ComponentTypeAndPropertyInfo left, ComponentTypeAndPropertyInfo right)
	{
		return !(left == right);
	}
}
