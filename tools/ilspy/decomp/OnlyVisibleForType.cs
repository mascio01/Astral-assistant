using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForType : Attribute
{
	public Type Type;

	public Type Type2;

	public Type Type3;

	public OnlyVisibleForType(Type type)
	{
		Type = type;
	}

	public OnlyVisibleForType(Type type, Type type2)
	{
		Type = type;
		Type2 = type2;
	}

	public OnlyVisibleForType(Type type, Type type2, Type type3)
	{
		Type = type;
		Type2 = type2;
		Type3 = type3;
	}

	public bool Matches(Type type)
	{
		if (type == null)
		{
			return false;
		}
		if (Type3 != null && type.IsA(Type3))
		{
			return true;
		}
		if (Type2 != null && type.IsA(Type2))
		{
			return true;
		}
		return type.IsA(Type);
	}
}
