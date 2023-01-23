using System;
using System.Reflection;

namespace Morris.Blazor.UIFor;

internal delegate IsUIForResult IsUIForDelegate(
	Type? ownerType,
	PropertyInfo? propertyInfo);
