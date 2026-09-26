using System;

[AttributeUsage(AttributeTargets.Field)]
public class ProcedurallyGeneratedAttribute : Attribute
{
	public bool ProcedurallyGenerated;

	public ProcedurallyGeneratedAttribute(bool procedurallyGenerated)
	{
		ProcedurallyGenerated = procedurallyGenerated;
	}
}
