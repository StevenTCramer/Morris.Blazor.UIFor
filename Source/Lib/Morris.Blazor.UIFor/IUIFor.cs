using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Morris.Blazor.UIFor;

public interface IUIFor<TValue>
{
	static abstract IsUIForResult IsUIFor(Type? ownerType, PropertyInfo? propertyInfo);

	TValue Value { get; set; }
	EventCallback<TValue> ValueChanged { get; set; }
	Expression<Func<TValue>>? ValueExpression { get; set; }
	PropertyInfo? ValuePropertyInfo { get; set; }
}
