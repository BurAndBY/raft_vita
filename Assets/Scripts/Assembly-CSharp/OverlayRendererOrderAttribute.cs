using System;

public class OverlayRendererOrderAttribute : Attribute
{
	private int priority;

	public int Priority
	{
		get
		{
			return priority;
		}
	}

	public OverlayRendererOrderAttribute(int priority)
	{
		this.priority = priority;
	}
}
