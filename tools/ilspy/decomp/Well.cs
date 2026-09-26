using UnityEngine;

public class Well : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Water well_Prefab");

	public override Color32 MapColor => GameTerrain.MinimapSettings.PropCol;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Well;
	}

	public override bool IsTargetable()
	{
		if (Destroyed)
		{
			return false;
		}
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		return GameTerrain.Instance.FogOfWar.IsAnyTileInRectExplored(MinTile, MaxTile);
	}
}
