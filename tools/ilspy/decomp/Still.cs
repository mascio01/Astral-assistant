using System;

public class Still : CraftingProp
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Still");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Still;
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
