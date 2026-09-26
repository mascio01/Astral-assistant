using System;
using UnityEngine;

public class Kiln : CraftingProp
{
	private static PrefabResource LitModel = new PrefabResource("Prefabs/Props/Charcoal Kiln/charkiln_lit");

	private static PrefabResource UnlitModel = new PrefabResource("Prefabs/Props/Charcoal Kiln/charkiln");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Kiln;
	}

	public override PrefabResource GetUnityModel()
	{
		if (!IsCrafting())
		{
			if (Prototype == null || 0 >= Prototype.Prefabs.Count)
			{
				return UnlitModel;
			}
			return Prototype.Prefabs[0];
		}
		if (Prototype == null || 1 >= Prototype.Prefabs.Count)
		{
			return LitModel;
		}
		return Prototype.Prefabs[1];
	}

	public override bool SupportsVariation()
	{
		return false;
	}

	public override bool IsBurning()
	{
		if (!base.IsBurning())
		{
			return IsCrafting();
		}
		return true;
	}

	public override bool IsBurningDueToAttack()
	{
		return base.IsBurning();
	}

	public override float GetFireEffectOnCharacter(Character character)
	{
		if (!IsCrafting())
		{
			return base.GetFireEffectOnCharacter(character);
		}
		return 1f - Mathf.Clamp01((character.PosXZ - PosXZ).magnitude / TileObject.MaxFireHeatRange);
	}

	public override void SetCraftingRecipe(Recipe recipe, Character chef, float ingredientsNutrition, InfectionType ingredientsInfectedWith, int craftingDesiredAmount)
	{
		bool num = IsBurning();
		base.SetCraftingRecipe(recipe, chef, ingredientsNutrition, ingredientsInfectedWith, craftingDesiredAmount);
		UnityReinit();
		if (!num && IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
		}
	}

	public override void OnCraftingFinished()
	{
		base.OnCraftingFinished();
		UnityReinit();
		if (!IsBurning())
		{
			GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, Tile);
		}
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (IsCrafting())
		{
			Craft((float)dt.TotalSeconds);
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}

	public override TerrainCoord GetTileToStandOn(Character character)
	{
		TerrainCoord bestTile = TerrainCoord.Invalid;
		float bestDistSq = float.MaxValue;
		TerrainCoord dirFromOrientationType = Prop.GetDirFromOrientationType((OrientationType)((int)(Orientation + 1) % 4));
		TerrainCoord dirFromOrientationType2 = Prop.GetDirFromOrientationType(Orientation);
		TerrainRect.TestTile(character.Tile, GetCentreTile() + dirFromOrientationType * 2, 2049, character, null, ref bestTile, ref bestDistSq);
		TerrainRect.TestTile(character.Tile, GetCentreTile() + dirFromOrientationType * 2 - dirFromOrientationType2, 2049, character, null, ref bestTile, ref bestDistSq);
		return bestTile;
	}

	public override TerrainRect GetStandingArea()
	{
		TerrainCoord dirFromOrientationType = Prop.GetDirFromOrientationType((OrientationType)((int)(Orientation + 1) % 4));
		TerrainCoord dirFromOrientationType2 = Prop.GetDirFromOrientationType(Orientation);
		TerrainCoord terrainCoord = GetCentreTile() + dirFromOrientationType * 2;
		return new TerrainRect(terrainCoord, terrainCoord).Include(terrainCoord - dirFromOrientationType2);
	}
}
