using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Morris.Blazor.UIFor;

public static class UICatalogue
{
	private readonly struct ValueTypeToComponentTypesLookupItem
	{
		public readonly Type ComponentType;
		public readonly IsUIForDelegate IsUIForDelegate;

		public ValueTypeToComponentTypesLookupItem(
			Type componentType,
			IsUIForDelegate isUIForDelegate)
		{
			ComponentType = componentType;
			IsUIForDelegate = isUIForDelegate;
		}
	}

	private static readonly ConcurrentDictionary<Type, ImmutableList<ValueTypeToComponentTypesLookupItem?>>
		ValueTypeToComponentTypesLookup = new();

	private static readonly ConcurrentDictionary<OwnerTypeAndPropertyNameAndType, ComponentTypeAndPropertyInfo?>
		OwnerTypeAndPropertyNameToComponentTypeLookup = new();

	private static readonly ConcurrentDictionary<Assembly, bool>
		ScannedAssemblies = new();

	public static bool TryGetComponentTypeAndPropertyInfo(
		Type ownerType,
		string propertyName,
		Type propertyType,
		[NotNullWhen(true)] out ComponentTypeAndPropertyInfo? componentTypeAndPropertyInfo)
	{
		ComponentTypeAndPropertyInfo? result =
			OwnerTypeAndPropertyNameToComponentTypeLookup.GetOrAdd(
				key:
					new OwnerTypeAndPropertyNameAndType(
						ownerType: ownerType,
						propertyName: propertyName,
						propertyType: propertyType),
				valueFactory: key =>
				{
					(Type componentType, PropertyInfo? propertyInfo)? newEntry =
						FindMostApplicableUI(key);

					return newEntry is null
						? null
						: new ComponentTypeAndPropertyInfo(
								componentType: newEntry!.Value!.componentType,
								propertyInfo: newEntry!.Value!.propertyInfo,
								propertyType: propertyType);
				});

		componentTypeAndPropertyInfo = result;
		return result is not null;
	}

	private static (Type componentType, PropertyInfo? propertyInfo)? FindMostApplicableUI(
		OwnerTypeAndPropertyNameAndType ownerTypeAndPropertyNameAndType)
	{
		Type ownerType = ownerTypeAndPropertyNameAndType.OwnerType;
		string propertyName = ownerTypeAndPropertyNameAndType.PropertyName;
		Type propertyType = ownerTypeAndPropertyNameAndType.PropertyType;

		PropertyInfo? propertyInfo =
		 ownerType.GetProperty(
				 name: propertyName,
				 bindingAttr: BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		Type? mostSpecificComponentType = FindMostApplicableUI(
			propertyType: propertyType,
			ownerType: ownerType,
			propertyInfo: propertyInfo);

		return mostSpecificComponentType is null
			? null
			: (mostSpecificComponentType!, propertyInfo);
	}

	private static Type? FindMostApplicableUI(
		Type? ownerType,
		Type? propertyType,
		PropertyInfo? propertyInfo)
	{
		propertyType ??= propertyInfo?.PropertyType ??
			throw new ArgumentNullException(nameof(propertyType));

		if (
					!ValueTypeToComponentTypesLookup.TryGetValue(
						key: propertyType,
						value: out ImmutableList<ValueTypeToComponentTypesLookupItem?>? candidates))
		{
			return null;
		}

		int highestPriority = int.MinValue;
		Type? mostSpecificComponentType = null;
		PropertyInfo? mostSpecificProperty = null;

		for (int i = 0; i < candidates.Count; i++)
		{
			ValueTypeToComponentTypesLookupItem? currentCandidate = candidates![i]!;

			if (currentCandidate.HasValue)
			{

				var currentResult = currentCandidate.Value.IsUIForDelegate(
					ownerType: ownerType,
					propertyInfo: propertyInfo);

				if (currentResult.Supported && currentResult.Priority > highestPriority)
				{
					highestPriority = currentResult.Priority;
					mostSpecificComponentType = currentCandidate.Value.ComponentType;
					mostSpecificProperty = propertyInfo;
				}
			}
		}
		return mostSpecificComponentType;
	}

	public static void Scan(
		Assembly assembly,
		params Assembly[] additionalAssemblies)
	{
		ArgumentNullException.ThrowIfNull(assembly);

		additionalAssemblies ??= Array.Empty<Assembly>();
		Assembly[] allAssemblies = additionalAssemblies
			.Where(x => x is not null)
			.Append(assembly)
			.ToArray();

		foreach (Assembly currentAssembly in allAssemblies)
			Scan(currentAssembly);
	}

	public static void Scan(Assembly assembly)
	{
		ScannedAssemblies.AddOrUpdate(
			key: assembly,
			addValue: true,
			updateValueFactory: (key, existing) =>
			{
				throw new InvalidOperationException($"Assembly \"{assembly.FullName}\" has already scanned");
			});

		foreach (Type type in assembly.ExportedTypes)
			Scan(type);
	}

	private static void Scan(Type componentType)
	{
		if (
			!componentType.IsClass
			|| componentType.IsAbstract
			|| !typeof(IComponent).IsAssignableFrom(componentType))
		{
			return;
		}

		Type[] interfaces = componentType
			.GetInterfaces()
			.Where(x => x.IsGenericType)
			.Where(x => x.GetGenericTypeDefinition() == typeof(IUIFor<>))
			.ToArray();

		if (interfaces.Length == 0)
			return;

		if (interfaces.Length > 1)
			throw new InvalidOperationException(
				$"ComponentType \"{componentType.Name}\" should only implement one" +
				$" \"{typeof(IUIFor<>).Name[..^2]}<T>\"" +
				$" but {interfaces.Length} interfaces were found");

		Type currentInterface = interfaces[0];
		Type valueType = currentInterface.GetGenericArguments()[0];

		var interfaceMapping = componentType.GetInterfaceMap(currentInterface);

		MethodInfo isUISupportedMethodInfo = interfaceMapping
			.TargetMethods
			.First(IsUIForMethod);

		IsUIForDelegate isUIForFunc = (Type? ownerType, PropertyInfo? propertyInfo) =>
			(IsUIForResult)isUISupportedMethodInfo.Invoke(
				obj: null,
				parameters: new object?[] { ownerType, propertyInfo })!;

		var entry = new ValueTypeToComponentTypesLookupItem(
			componentType: componentType,
			isUIForDelegate: isUIForFunc);

		ValueTypeToComponentTypesLookup.AddOrUpdate(
			key: valueType,
			addValueFactory: _ => ImmutableList.Create<ValueTypeToComponentTypesLookupItem?>(entry),
			updateValueFactory: (_, componentTypes) => componentTypes.Add(entry));
	}

	private static bool IsUIForMethod(MethodInfo methodInfo) =>
		(
			methodInfo.IsPublic
			&& methodInfo.Name.Equals(nameof(IUIFor<object>.IsUIFor), StringComparison.Ordinal)
		)
		||
		(
			methodInfo.IsPrivate
			&& methodInfo.Name.EndsWith(
					"." + nameof(IUIFor<object>.IsUIFor),
					StringComparison.Ordinal)
		);
}
