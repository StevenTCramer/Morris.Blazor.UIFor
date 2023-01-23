# Morris.Blazor.UIFor - documentation
![](./../Images/small-logo.png)

## Component decoupling

***Morris.Blazor.UIFor*** allows Blazor pages/components to render
data by convention, allowing the develop to not concern themselves with
knowing which specific UI to use.

A one way binding to something like a complex object `Person` would
look like this.

```html
<UIFor OneWay="() => Person"/>
```

To a simple type like a string, it would look like this

```html
<UIFor OneWay="() => Person.Salutation"/>
```

A two-way binding would look like this

```html
<UIFor @bind-TwoWay=Person.Salutation/>
```

## Component reuse
By making the `<UIFor/>` component syntax so simple, it makes developers
more likely to write a reusable component than repeating mark-up.

For example, instead of repeating the following markup

```html
<div class="form-group">
  <label for="EmailAddress">Email address</label>
  <input type="email" class="form-control" id="EmailAddress">
</div>
```

The user would simply write `<UIFor @bind-TwoWay=Person.EmailAddress/>` and the library would
automatically choose a component that looks something like the following

```html
<div class="form-group">
  <label for="@UniqueControlId">@LabelText</label>
  <input type="email" class="form-control" id="@UniqueControlId">
</div>
```

## Flexible
When creating components that the `<UIFor/>` component can use, your components can specify whether or
not they provide UI for the specific item being rendered (the `Type` of the owner, and the property's
`PropertyInfo`).

Components can return a true/false result, and also indicate a priority so that ***UIFor*** can determine
which of the components claims to be the most specific for the given scenario.

* Any string (MyStringUI.razor)
* Any string decorated with `[Salutation]` (MySalutationDropDownUI.razor)
* Specifically `ProfessionalWorker.Salutation` (MySalutationEditableUI.razor)


## Tutorials (not yet implemented)

* [UI for simple values](../Source/Tutorials/01-SimpleTypes/)
* [UI for two-way binding simple values](../Source/Tutorials/02-TwoWayBinding/)
* [UI for complex models](../Source/Tutorials/03-ComplexModels/)
* [Overriding UI for attribute decorated properties](../Source/Tutorials/04-AttributeDecoratedProperties/)
* [Overriding UI for corner cases](../Source/Tutorials/05-CornerCases/)
