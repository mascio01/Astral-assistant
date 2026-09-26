using System;
using UnityEngine;

public class BuildAnim : AnimationGoal
{
	public TileObject Building;

	public bool Urgent;

	public BuildAnim()
	{
	}

	public BuildAnim(TileObject building, ActionAnim anim, bool urgent)
		: base(anim, urgent ? RoleGoal.UrgentWorkSpeed : 1f)
	{
		Building = building;
		Urgent = urgent;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Building);
		reflector.AddAfter(ref Urgent, 407);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BuildAnim;
	}

	private void UseResource(Character character)
	{
		UnderConstructionInfo underConstructionInfo = Building.GetUnderConstructionInfo();
		Recipe recipe = underConstructionInfo.Recipe;
		foreach (Ingredient ingredient in recipe.Ingredients)
		{
			if (underConstructionInfo.HasUsedEnoughOfIngredient(ingredient))
			{
				continue;
			}
			if (ingredient.LiquidTypes != null)
			{
				float num = underConstructionInfo.GetAmountUsedOfIngredient(ingredient);
				float num2 = num;
				for (int i = 0; i < ingredient.LiquidTypes.Count; i++)
				{
					if (!(num < ingredient.LiquidAmount))
					{
						break;
					}
					InfectionType infectedWith;
					float num3 = character.Inventory.DrainLiquidOfType(character, ingredient.LiquidTypes[i], ingredient.LiquidAmount - num, underConstructionInfo.Recipe, out infectedWith, character);
					num += num3;
					underConstructionInfo.UseLiquidIngredient(ingredient.LiquidTypes[i], num3);
				}
				if (num > num2)
				{
					if (num >= ingredient.LiquidAmount)
					{
						int val = recipe.CountIngredients();
						character.Skillset.AddProgress(character, recipe.SkillType, recipe.SkillProgression / Math.Max(1, val));
					}
					break;
				}
			}
			if (ingredient.Prototypes == null)
			{
				continue;
			}
			foreach (EquipmentPrototype prototype in ingredient.Prototypes)
			{
				Equipment equipment = character.Inventory.FindIngredient(character, character, ingredient, recipe, null, character, null);
				if (equipment != null)
				{
					NotificationManager.Instance.AddEquipmentNotification(character, null, equipment, 1);
					equipment.IncrementAmount(-1);
					character.Inventory.CacheEncumbered(character);
					if (equipment.GetAmount() == 0)
					{
						character.Inventory.Remove(character, equipment);
						equipment.Delete();
					}
					underConstructionInfo.UseIngredient(prototype, 1);
					int val2 = recipe.CountIngredients();
					character.Skillset.AddProgress(character, recipe.SkillType, recipe.SkillProgression / Math.Max(val2, 1));
					goto end_IL_01cc;
				}
			}
			continue;
			end_IL_01cc:
			break;
		}
		foreach (Character member in character.Community.Members)
		{
			if (member.IsBuildingSomething() != Building)
			{
				continue;
			}
			foreach (Ingredient ingredient2 in underConstructionInfo.Recipe.Ingredients)
			{
				if (underConstructionInfo.HasUsedEnoughOfIngredient(ingredient2))
				{
					character.Inventory.FindIngredient(character, character, ingredient2, underConstructionInfo.Recipe, null, character, character)?.SetGathered();
				}
			}
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Building == null || Building.Deleted)
		{
			Finished = true;
			return;
		}
		UnderConstructionInfo underConstructionInfo = Building.GetUnderConstructionInfo();
		if (underConstructionInfo == null)
		{
			Finished = true;
			return;
		}
		Recipe recipe = underConstructionInfo.Recipe;
		if (recipe == null)
		{
			Finished = true;
			return;
		}
		if (Anim == ActionAnim.Build || Anim == ActionAnim.DigLoop)
		{
			character.SetPosition(CalcPositionForBuilder(character, Building));
		}
		bool flag = !underConstructionInfo.ShowHalfBuiltModel();
		bool flag2 = Building.IsImpassable(character, 0, Building.GetCentreTile());
		float num = (float)(Session.Instance.PlayTime - character.LastThinkTime).TotalSeconds * (Urgent ? RoleGoal.UrgentWorkSpeed : 1f);
		underConstructionInfo.TimeTakenOnResource += num;
		float num2 = recipe.CraftingTime / (float)Math.Max(1, recipe.CountIngredients());
		if (underConstructionInfo.TimeTakenOnResource >= num2)
		{
			UseResource(character);
			if (recipe.Ingredients.Count > 0)
			{
				underConstructionInfo.TimeTakenOnResource -= num2;
			}
			if (underConstructionInfo.IsCompleted())
			{
				Finished = true;
			}
			else if (!(parent is BuildGoal buildGoal) || !buildGoal.DoesBuilderHaveNeededResources(character))
			{
				Finished = true;
			}
		}
		if (Building.GetBaseObjectType() != BaseObjectType.PitTrap)
		{
			bool flag3 = Building.IsImpassable(character, 0, Building.GetCentreTile());
			if (flag2 && !flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.RemovedFixedContents(Building));
			}
			else if (!flag2 && flag3)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddedFixedContents(Building));
			}
			if (flag2 != flag3)
			{
				GameTerrain.Instance.FixupBuiltOnTiles(Building.GetTileRect());
			}
		}
		if (!(Building is Prop prop))
		{
			return;
		}
		if (flag)
		{
			prop.UpdateWorldTransformAndBounds();
		}
		GameObject unityObject = Building.GetUnityObject();
		if (unityObject != null)
		{
			MeshPostProcessSettings component = unityObject.GetComponent<MeshPostProcessSettings>();
			if (component != null && component.ExtraInfo == MeshExtraInfo.ConstructionProgressInfo)
			{
				float progress = underConstructionInfo.GetProgress();
				Prop.UnitySetUnderConstruction(unityObject, progress);
			}
		}
	}

	public static Vector3 CalcPositionForBuilder(Character builder, TileObject building)
	{
		int num = 0;
		int num2 = 0;
		foreach (Character member in builder.Community.Members)
		{
			if (member == builder)
			{
				num2 = num;
				num++;
			}
			else if (member.IsInsideBuildingUnderConstruction(building))
			{
				num++;
			}
		}
		TerrainRect vertRect = building.GetVertRect();
		int num3 = vertRect.max.x - vertRect.min.x;
		int num4 = vertRect.max.y - vertRect.min.y;
		float num5 = (float)num3 / (float)num4;
		int num6 = Math.Max(1, Mathf.RoundToInt(Mathf.Sqrt((float)num / num5)));
		int num7 = Math.Max(1, Mathf.RoundToInt((float)num / (float)num6));
		Vector2 vertexPosXZ = GameTerrain.Instance.GetVertexPosXZ(vertRect.min.x, vertRect.min.y);
		Vector2 vertexPosXZ2 = GameTerrain.Instance.GetVertexPosXZ(vertRect.max.x, vertRect.max.y);
		int num8 = num2 % num7;
		int num9 = num2 / num7;
		float x = Mathf.Lerp(vertexPosXZ.x, vertexPosXZ2.x, (0.5f + (float)num8) / (float)num7);
		float z = Mathf.Lerp(vertexPosXZ.y, vertexPosXZ2.y, (0.5f + (float)num9) / (float)num6);
		float tileHeightAtPos = GameTerrain.Instance.GetTileHeightAtPos(x, z);
		return new Vector3(x, tileHeightAtPos, z);
	}
}
