using System;
using UnityEngine;

public class SkinAnimalsGoal : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	private Equipment Meat;

	public static TimeSpan RememberFindFailedTime = TimeSpan.FromSeconds(300.0);

	public static float MaxSkinDist = 32f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("SkinAnimalsCalcBestTarget");

	public override GoalType GetGoalType()
	{
		return GoalType.SkinAnimalsGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorKnife;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_SkinAnimals;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.AddAfter(ref Meat, 325);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Goal nextSubGoal = GetNextSubGoal(character, parent);
		if (nextSubGoal != null)
		{
			SetSubGoal(character, parent, nextSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndSkin moveToAndSkin)
		{
			if (moveToAndSkin.Success)
			{
				Meat = moveToAndSkin.Meat;
			}
			else
			{
				character.AddFailedFindAttempt(moveToAndSkin.GetTargetObject(), 0, 0, FindType.AutoCollect, null, null, null);
			}
		}
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: not false })
		{
			Meat = null;
		}
		if (Meat != null)
		{
			if (!Meat.Deleted && character.InventoryContains(Meat))
			{
				Prop buildingToStoreSuppliesIn = GatherGoal.GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(Meat));
				if (buildingToStoreSuppliesIn != null)
				{
					return new MoveToAndDeposit(character, buildingToStoreSuppliesIn, Meat, Meat.GetAmount(), _movementType);
				}
				Meat = null;
			}
			else
			{
				Meat = null;
			}
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.SkinnedAmount < 1f)
		{
			return new MoveToAndSkin(character, targetCharacter, _movementType)
			{
				DontOpenOurGates = FindGoal.CalcDontOpenOurGates(character)
			};
		}
		return null;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.IsControllableByPlayer())
		{
			return false;
		}
		if (Active)
		{
			return true;
		}
		if (Meat != null)
		{
			return true;
		}
		return !IsTargetDeleted();
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (Active)
			{
				return Target;
			}
			if (!character.IsOutdoors())
			{
				return null;
			}
			Target result = null;
			float num = 1E+38f;
			foreach (Target target in character.Targets)
			{
				if (!target.GetFlag(TargetFlags.Dead) || target.GetFlag(TargetFlags.Lost) || target.IsInaccessible() || !(target.Object is Animal animal) || target.Object.Deleted || animal.SkinnedAmount >= 1f || !animal.IsOutdoors() || !target.FullyTracked || MathUtil.ToXZ(target.LastKnownPosition - character.Pos).sqrMagnitude >= MaxSkinDist * MaxSkinDist || GameCursor.IsStealingToTakeFrom(character, animal) || character.Inventory.GetHuntingKnife() == null)
				{
					continue;
				}
				int meatAmount = animal.GetMeatAmount(character.GetSkillLevelWithEffects(SkillType.Cooking));
				EquipmentPrototype meatType = animal.GetMeatType();
				if (meatType != null && character.HasInventorySpaceFor((float)meatAmount * meatType.Weight) && !IsAnyoneSkinningCorpse(animal, character) && !character.HasFailedFindAttempt(animal, 0, 0, FindType.AutoCollect, null, null, null, RememberFindFailedTime, out var _, out var _))
				{
					float num2 = character.Get2DDistToTargetLastKnownPos(target) / character.GetWalkSpeed();
					float num3 = (float)target.TimeSinceLastDetected.TotalSeconds + num2;
					if (target == Target && Active)
					{
						num3 *= 0.75f;
					}
					if (num3 < num)
					{
						result = target;
						num = num3;
					}
				}
			}
			return result;
		}
	}

	public static bool IsAnyoneSkinningCorpse(Character corpse, Character ignore)
	{
		foreach (Character item in corpse.CharactersTargetingMe)
		{
			if (item != ignore && item.FindActiveGoal(GoalType.MoveToAndSkin) is MoveToAndSkin moveToAndSkin && moveToAndSkin.GetTargetCharacter() == corpse)
			{
				return true;
			}
		}
		return false;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		_movementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return _movementType;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}
}
