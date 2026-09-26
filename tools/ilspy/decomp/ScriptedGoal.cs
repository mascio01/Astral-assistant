using System;

public class ScriptedGoal : StateMachineGoal
{
	public static string[] ScriptedMoveImportanceNames = StringUtil.GetEnumNames<ScriptedMoveImportance>();

	private MovementType _movementType;

	private bool _disableWhenReachedMarker;

	private ScriptedMoveImportance Importance;

	private bool TeleportIfMoveFailed;

	private bool Sit;

	private bool AvoidHostileBases;

	public bool Success;

	private static string MartinSteele = "MartinSteele";

	public void SetMarker(Character character, Goal parent, TileObject dest, MovementType movementType, bool disableWhenReachedMarker, ScriptedMoveImportance importance, bool teleportIfMoveFailed, bool sit, bool avoidHostileBases)
	{
		_movementType = movementType;
		_disableWhenReachedMarker = disableWhenReachedMarker;
		Importance = importance;
		TeleportIfMoveFailed = teleportIfMoveFailed;
		AvoidHostileBases = avoidHostileBases;
		Sit = sit;
		Success = false;
		if (Active)
		{
			if (dest != null)
			{
				Finished = false;
				SetSubGoal(character, parent, GetMoveToGoal(character));
			}
			else
			{
				Finished = true;
			}
		}
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref _disableWhenReachedMarker);
		reflector.AddAfter(ref Sit, 491);
		reflector.AddAfter(ref AvoidHostileBases, 517);
		reflector.AddAfter(ref TeleportIfMoveFailed, 605);
		reflector.Add(ref Success);
		if (reflector.Version < 496)
		{
			bool value = false;
			reflector.Add(ref value);
			Importance = (value ? ScriptedMoveImportance.Medium : ScriptedMoveImportance.Low);
		}
		else
		{
			reflector.Add(ref Importance);
		}
		if (reflector.Version < 510 && Importance == ScriptedMoveImportance.High && character.GetUniqueID() == MartinSteele)
		{
			Importance = ScriptedMoveImportance.Low;
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ScriptedGoal;
	}

	public bool IsMoveFinished()
	{
		return SubGoal is Idle;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (character.ScriptedGoalMarker != null)
		{
			switch (Importance)
			{
			case ScriptedMoveImportance.High:
				return GoalPriority.Survivor_Scripted_HighImportance;
			case ScriptedMoveImportance.Medium:
				return GoalPriority.Survivor_Scripted_MediumImportance;
			}
		}
		return GoalPriority.Survivor_Scripted;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.ScriptedGoalMarker == null || character.ScriptedGoalMarker.Deleted)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, GetMoveToGoal(character));
	}

	public Goal GetMoveToGoal(Character character)
	{
		if (character.ScriptedGoalMarker is Marker marker)
		{
			return new MoveTo(_movementType, marker.Tile)
			{
				AvoidHostileBases = AvoidHostileBases
			};
		}
		if (character.ScriptedGoalMarker is Building targetBuilding)
		{
			return new MoveToAndEnterBuilding(character, targetBuilding, _movementType)
			{
				ForceEnter = true,
				AvoidHostileBases = AvoidHostileBases
			};
		}
		return new MoveAdjacentToTarget(character, character.ScriptedGoalMarker, _movementType, canBeOnTile: true);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAsCloseAsPossibleTo)
		{
			return new Wait(TimeSpan.FromSeconds(1.0));
		}
		if (SubGoal is MoveTo moveTo)
		{
			Marker marker = character.ScriptedGoalMarker as Marker;
			if (moveTo.Success)
			{
				if (marker != null)
				{
					return new TurnToAngle(marker.Angle);
				}
				if (Sit)
				{
					return new SitGoal();
				}
				return GetIdleGoal(character);
			}
			if (marker != null)
			{
				if (TeleportIfMoveFailed)
				{
					character.SetPosition(marker.Pos);
					character.SetFacingAngle(marker.Angle);
					return GetIdleGoal(character);
				}
				return new MoveAsCloseAsPossibleTo(_movementType, marker.Tile);
			}
			return new Wait(TimeSpan.FromSeconds(1.0));
		}
		if (SubGoal is TurnToAngle)
		{
			if (Sit)
			{
				return new SitGoal();
			}
			return GetIdleGoal(character);
		}
		if (SubGoal is SitGoal)
		{
			return GetIdleGoal(character);
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			Success = true;
			StoryManager.Instance.SetConditionsDirty();
			if (_disableWhenReachedMarker)
			{
				character.ScriptedGoalMarker = null;
			}
			if (moveToAndEnterBuilding.Success)
			{
				return new Idle();
			}
			return new Wait(TimeSpan.FromSeconds(3.0));
		}
		if (SubGoal is Wait)
		{
			return GetMoveToGoal(character);
		}
		return base.GetNextSubGoal(character, parent);
	}

	private Goal GetIdleGoal(Character character)
	{
		Success = true;
		StoryManager.Instance.SetConditionsDirty();
		if (_disableWhenReachedMarker)
		{
			character.ScriptedGoalMarker = null;
		}
		return new Idle();
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		return AIOverridesControlReason.Scripted;
	}
}
