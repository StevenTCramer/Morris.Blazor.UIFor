namespace Morris.Blazor.UIFor;

public readonly struct IsUIForResult
{
	public readonly bool Supported;
	public readonly int Priority;

	public static readonly IsUIForResult False = new(supported: false, priority: 0);

	public static IsUIForResult True(int priority) =>
		new(supported: true, priority: priority);

	private IsUIForResult(bool supported, int priority)
	{
		Supported = supported;
		Priority = priority;
	}

}
