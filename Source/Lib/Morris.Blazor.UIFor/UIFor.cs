using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Morris.Blazor.UIFor;

public partial class UIFor<TValue> : ComponentBase
{
	[Parameter]
	public Expression<Func<TValue>> OneWay { get; set; } = null!;

	[Parameter]
	public TValue TwoWay { get; set; } = default!;

	[Parameter]
	public Expression<Func<TValue>> TwoWayExpression { get; set; } = null!;

	[Parameter]
	public EventCallback<TValue> TwoWayChanged { get; set; }

	private Type? ComponentType;
	private Func<TValue> GetValue = null!;
	private PropertyInfo? ValuePropertyInfo;
	private Expression<Func<TValue>>? EffectiveValueExpression => 
		OneWay ?? TwoWayExpression;

	public override async Task SetParametersAsync(ParameterView parameters)
	{
		EnsureValidInputParameters(parameters);
		
		Expression<Func<TValue>>? previousEffectiveValueExpression = EffectiveValueExpression;
		
		await base.SetParametersAsync(parameters);
		
		if (EffectiveValueExpression != previousEffectiveValueExpression)
			CreatePrivateState();
	}

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenComponent(0, ComponentType!);
		builder.AddAttribute(1, nameof(IUIFor<object>.Value), GetValue());
		builder.AddAttribute(2, nameof(IUIFor<object>.ValueChanged), TwoWayChanged);
		builder.AddAttribute(3, nameof(IUIFor<object>.ValueExpression), EffectiveValueExpression);
		builder.AddAttribute(4, nameof(IUIFor<object>.ValuePropertyInfo), ValuePropertyInfo);
		builder.CloseComponent();
	}

	private void CreatePrivateState()
	{
		GetValue = EffectiveValueExpression!.Compile();
		var fieldIdentifier = FieldIdentifier.Create(EffectiveValueExpression);
		Type ownerType = fieldIdentifier.Model.GetType();
		string propertyName = fieldIdentifier.FieldName;

		if (!UICatalogue.TryGetComponentTypeAndPropertyInfo(
			ownerType: ownerType,
			propertyName: propertyName,
			propertyType: typeof(TValue),
			out ComponentTypeAndPropertyInfo? componentTypeAndPropertyInfo))
		{
			throw new InvalidOperationException(
				$"No IUIFor<T> registered that matches \"{ownerType.Name}.{propertyName}\"");
		}

		ComponentType = componentTypeAndPropertyInfo.Value.ComponentType;
		ValuePropertyInfo = componentTypeAndPropertyInfo.Value.PropertyInfo;
	}

	private static void EnsureValidInputParameters(ParameterView parameters)
	{
		const string WrongInputMessage =
			$"You must only use the @bind-{nameof(TwoWay)} directive" +
			$" or set the \"{nameof(OneWay)}\" parameter.";

		const string CannotSetTwoWayParametersIndependently =
			$"{WrongInputMessage} You cannot set the \"{nameof(TwoWay)}\"," +
			$" \"{nameof(TwoWayChanged)}\", or \"{nameof(TwoWayExpression)}\"" +
			$" parameters directly.";

		const string CannotSetTwoInputsMessage =
			WrongInputMessage + " You cannot use both.";

		bool hasOneWayParam =
			parameters.TryGetValue<Expression<Func<TValue>>>(nameof(OneWay), out _);

		bool hasTwoWayParam = parameters.TryGetValue<TValue>(nameof(TwoWay), out _);

		bool hasTwoWayExpressionParam =
			parameters.TryGetValue<Expression<Func<TValue>>>(nameof(TwoWayExpression), out _);

		bool hasTwoWayChangedParam =
			parameters
				.TryGetValue<EventCallback<TValue>>(
					parameterName: nameof(TwoWayChanged),
					out var twoWayChanged)
				&& twoWayChanged.HasDelegate;

		bool hasAnyTwoWayParams =
			hasTwoWayParam
			|| hasTwoWayChangedParam
			|| hasTwoWayExpressionParam;

		bool hasAllTwoWayParams =
			hasAnyTwoWayParams
			&& hasTwoWayParam
			&& hasTwoWayChangedParam
			&& hasTwoWayExpressionParam;

		if (hasOneWayParam && hasAllTwoWayParams)
			throw new InvalidOperationException(CannotSetTwoInputsMessage);

		if (hasAnyTwoWayParams && !hasAllTwoWayParams)
			throw new InvalidOperationException(CannotSetTwoWayParametersIndependently);

		if (!hasOneWayParam && !hasAnyTwoWayParams)
			throw new InvalidOperationException(CannotSetTwoInputsMessage);
	}


}