using System;

public class Nitrary : CraftingProp
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Nitrary");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Nitrary;
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (IsCrafting())
		{
			Craft((float)dt.TotalSeconds);
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}
}
