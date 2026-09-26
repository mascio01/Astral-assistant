using UnityEngine;

public class Forge : CraftingProp
{
	private static PrefabResource LitModel = new PrefabResource("Prefabs/Props/Medieval Forge/Forge");

	private static PrefabResource UnlitModel = new PrefabResource("Prefabs/Props/Medieval Forge/Forge_Unlit");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Forge;
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
}
