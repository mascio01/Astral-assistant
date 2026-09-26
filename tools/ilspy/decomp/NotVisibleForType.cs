using System;

[AttributeUsage(AttributeTargets.Field)]
public class NotVisibleForType : Attribute
{
	public Type Type;

	public NotVisibleForType(Type type)
	{
		Type = type;
	}

	public NotVisibleForType(Type type, Type type2)
	{
		Type = type;
	}

	public bool Matches(Type type)
	{
		if (type != null)
		{
			return type.IsA(Type);
		}
		return false;
	}
}
