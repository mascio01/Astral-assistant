using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForRole : Attribute
{
	public Role Role;

	public OnlyVisibleForRole(Role role)
	{
		Role = role;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			return template.Role == Role;
		}
		return false;
	}
}
