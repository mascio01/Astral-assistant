using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleIfCanBeDestroyed : Attribute
{
}
