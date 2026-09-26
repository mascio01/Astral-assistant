using System;

[AttributeUsage(AttributeTargets.Field)]
public class IsModAttribute : Attribute
{
	public bool IsMod;

	public IsModAttribute(bool isMod)
	{
		IsMod = isMod;
	}
}
