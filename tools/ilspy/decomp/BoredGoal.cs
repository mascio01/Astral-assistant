using System;
using UnityEngine;

public class BoredGoal : StateMachineGoal
{
	private TimeSpan LastHangOutFailedTime = Target.Never;

	private TimeSpan LastSitByFireFailedTime = Target.Never;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref LastHangOutFailedTime, 109);
		reflector.AddAfter(ref LastSitByFireFailedTime, 109);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BoredGoal;
	}

	public override bool IsBored(Character character)
	{
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.DirectControlled)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_Bored;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, PickActivity(character));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		return PickActivity(character);
	}

	public Goal PickActivity(Character character)
	{
		MoveTo moveTo = SubGoal as MoveTo;
		if (moveTo != null && !moveTo.Success)
		{
			LastHangOutFailedTime = Session.Instance.PlayTime;
		}
		if (SubGoal is SitAroundFireGoal { Success: false })
		{
			LastSitByFireFailedTime = Session.Instance.PlayTime;
		}
		if (character.CarryingObject != null || character.IsCrouching())
		{
			return GetWaitGoal(character);
		}
		if (Session.Instance.GetPlayerControllingCharacter(character) != null)
		{
			return GetWaitGoal(character);
		}
		if (character.DirectControlledMajorAIDisabled)
		{
			return GetWaitGoal(character);
		}
		if (!(character.EquippedItem is Weapon))
		{
			Equipment bestWeaponForIdle = character.Inventory.GetBestWeaponForIdle(character, rangedOnly: false, allowMolotovs: false);
			if (bestWeaponForIdle != null)
			{
				return new Equip(bestWeaponForIdle);
			}
		}
		if (character.HangOutLocation != TerrainCoord.Invalid && Session.Instance.PlayTime - LastHangOutFailedTime > TimeSpan.FromSeconds(10.0) && !character.IsAmbient() && character.HangOutLocation.GetDistSquared(character.Tile) > MathUtil.Squared(character.Sitting ? TileObject.MaxFireHeatRange : 0.5f) && (!character.CanFollowPlayer || character.IsControllableByPlayer()) && (character.InsideBuilding == null || !character.HangOutLocation.IsWithinBounds(character.InsideBuilding.MinTile, character.InsideBuilding.MaxTile)) && !(SubGoal is MoveTo))
		{
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord hangOutLocation = character.HangOutLocation;
			bool flag = false;
			for (int i = 0; i < 10; i++)
			{
				if (!instance.IsTileOutsideBounds(hangOutLocation.x, hangOutLocation.y) && !instance.IsImpassable(hangOutLocation.x, hangOutLocation.y, 1, character, null))
				{
					flag = true;
					break;
				}
				hangOutLocation.x += Session.Instance.DeterministicRand.Next(-2, 3);
				hangOutLocation.y += Session.Instance.DeterministicRand.Next(-2, 3);
			}
			if (flag)
			{
				return new MoveTo(MovementType.Walk, hangOutLocation);
			}
		}
		if (character.InsideBuilding != null && character.HangOutLocation.IsWithinBounds(character.InsideBuilding.MinTile, character.InsideBuilding.MaxTile))
		{
			if (character.HasAnyEnemiesNearby())
			{
				return new Wait(TimeSpan.FromSeconds(60.0));
			}
			if (character.HasBeenPlayerControlledRecently(critical: false, extraCritical: false))
			{
				return new Wait(TimeSpan.FromSeconds(10.0));
			}
		}
		if (moveTo != null && moveTo.Success)
		{
			return GetWaitGoal(character);
		}
		Campfire nearestBurningCampfire = character.GetNearestBurningCampfire(TileObject.MaxFireHeatRange);
		if (nearestBurningCampfire != null && !character.Sitting && !Crouching && Session.Instance.PlayTime - LastSitByFireFailedTime > TimeSpan.FromSeconds(10.0))
		{
			return new SitAroundFireGoal(character, nearestBurningCampfire, MovementType.Walk);
		}
		if (nearestBurningCampfire == null && character.Sitting)
		{
			return new AnimationGoal(ActionAnim.StopSittingByFire);
		}
		return GetWaitGoal(character);
	}

	private Goal GetWaitGoal(Character character)
	{
		return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(8f, 16f, MathUtil.RandomFloat((float)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).TotalSeconds + (float)character.Id))));
	}
}
