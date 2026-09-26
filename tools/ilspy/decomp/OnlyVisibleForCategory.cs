using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForCategory : Attribute
{
	public string Cat;

	public OnlyVisibleForCategory(string cat)
	{
		Cat = cat;
	}

	public bool Matches(string cat)
	{
		return cat.StartsWith(Cat);
	}
}
