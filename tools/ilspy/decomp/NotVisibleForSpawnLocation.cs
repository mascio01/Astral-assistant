using System;

[AttributeUsage(AttributeTargets.Field)]
public class NotVisibleForSpawnLocation : Attribute
{
	public TemplateSpawnLocation SpawnLocation;

	public NotVisibleForSpawnLocation(TemplateSpawnLocation spawnLocation)
	{
		SpawnLocation = spawnLocation;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			return template.SpawnLocation != SpawnLocation;
		}
		return false;
	}
}
