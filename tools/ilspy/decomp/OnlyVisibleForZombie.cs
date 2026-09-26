using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForZombie : Attribute
{
	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			return template.Infection != InfectionType.None;
		}
		return false;
	}
}
