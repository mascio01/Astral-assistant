using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForSpawnLocation : Attribute
{
	public TemplateSpawnLocation SpawnLocation;

	public OnlyVisibleForSpawnLocation(TemplateSpawnLocation spawnLocation)
	{
		SpawnLocation = spawnLocation;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			return template.SpawnLocation == SpawnLocation;
		}
		return false;
	}
}
