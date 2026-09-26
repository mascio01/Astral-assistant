using UnityEngine;

public class ObeyLeaderGoal : StateMachineGoal
{
	public enum SourceType
	{
		Player,
		SquadLeader_LowPrio,
		Scripted,
		SquadLeader,
		SpeakTo,
		Squad_HighPrio
	}

	private Goal Command;

	private TileObject CommandTarget;

	private SourceType Source;

	public bool WasCommandSuccessful;

	public override bool CanShowDialogOptions(Character character)
	{
		if (Active && Command is DrinkGoal)
		{
			return false;
		}
		return true;
	}

	public void SetCommand(Character character, Goal parent, Goal command, TileObject targetObject, SourceType source)
	{
		Target target = ((targetObject != null) ? character.GetOrCreateTarget(targetObject) : null);
		if (Command == command && Target == target)
		{
			return;
		}
		if (Active)
		{
			SetSubGoal(character, parent, null);
		}
		if (Command != null && Command != command)
		{
			Command.SetTarget(character, this, null);
		}
		Command = command;
		CommandTarget = targetObject;
		WasCommandSuccessful = false;
		Source = source;
		Finished = false;
		SetTarget(character, parent, target);
		if (Active)
		{
			if (Command != null)
			{
				SetSubGoal(character, parent, Command);
			}
			else
			{
				Finished = true;
			}
		}
		else if (Command != null)
		{
			Command.SetTarget(character, this, Command.CalcBestTarget(character, this));
		}
	}

	public override void OnPrioritiserDeactivated(Character character, Goal parent)
	{
		base.OnPrioritiserDeactivated(character, parent);
		SetCommand(character, parent, null, null, SourceType.Player);
	}

	public Goal GetCommand()
	{
		return Command;
	}

	public SourceType GetSource()
	{
		return Source;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (Active)
		{
			if (reflector.IsDeserialising)
			{
				Command = SubGoal;
			}
		}
		else
		{
			reflector.Add(ref Command, character);
		}
		if (reflector.IsDeserialising && reflector.Version < 46)
		{
			bool value = false;
			reflector.Add(ref value);
			Source = ((!value) ? SourceType.Scripted : SourceType.Player);
		}
		else
		{
			reflector.Add(ref Source);
			if (reflector.Version < 363 && Source == SourceType.SquadLeader_LowPrio && character.Community != null && character.Community.CommunityType == CommunityType.Looter)
			{
				Source = SourceType.SquadLeader;
			}
		}
		reflector.AddAfter(ref CommandTarget, 94);
		reflector.AddAfter(ref WasCommandSuccessful, 375);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ObeyLeaderGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Command == null && !character.IsPredicted())
		{
			return false;
		}
		if (character.DirectControlled)
		{
			if (Command == null)
			{
				return false;
			}
			if (character.IsBeingBittenOrChoked())
			{
				return false;
			}
			if (Command.GetGoalType() != GoalType.Conversation && Command.GetGoalType() != GoalType.MoveToAndVaultWaistHighWall && Command.GetGoalType() != GoalType.MoveToAndChokeHold)
			{
				return false;
			}
		}
		else if (Command != null && Command.GetGoalType() == GoalType.MoveToAndVaultWaistHighWall && character.IsBeingBitten())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		return Target;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Command != null)
		{
			if (Command.GetMovementType() <= MovementType.Walk && Command.AIOverridesControl(character, this) == AIOverridesControlReason.None)
			{
				if (character.Zombie)
				{
					return GoalPriority.Zombie_ObeyLeader;
				}
				switch (Source)
				{
				case SourceType.SquadLeader_LowPrio:
					return GoalPriority.Survivor_ObeyLeader_Low;
				case SourceType.SquadLeader:
					return GoalPriority.Survivor_ObeyLeader_Medium;
				case SourceType.Scripted:
					return GoalPriority.Survivor_ObeyLeader_Scripted;
				case SourceType.SpeakTo:
					return GoalPriority.Survivor_ObeyLeader_SpeakTo;
				case SourceType.Squad_HighPrio:
					return GoalPriority.Survivor_ObeyLeader;
				default:
					if (Command.GetGoalType() == GoalType.LeaveBuilding)
					{
						return GoalPriority.Survivor_ObeyLeader_Urgent;
					}
					if (Active && character.CurrentActionAnim != ActionAnim.None)
					{
						return GoalPriority.Survivor_ObeyLeader_Animation;
					}
					return GoalPriority.Survivor_ObeyLeader;
				}
			}
			if (Command is Attack { PreferredTarget: not null } attack && character.IsTargetObjectSurrendering(attack.PreferredTarget))
			{
				return GoalPriority.Survivor_Attack_EnemySurrendering;
			}
		}
		if (!character.Zombie)
		{
			if (!character.IsControllableByPlayer())
			{
				return GoalPriority.Survivor_ObeyLeader_UrgentAI;
			}
			return GoalPriority.Survivor_ObeyLeader_Urgent;
		}
		return GoalPriority.Zombie_ObeyLeader_Urgent;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		if (CommandTarget != null)
		{
			Target orCreateTarget = character.GetOrCreateTarget(CommandTarget);
			SetTarget(character, parent, orCreateTarget);
			if (Command != null && Command.HasUserTarget)
			{
				Command.SetTarget(character, parent, orCreateTarget);
			}
		}
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, Command);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal == Command && Command != null)
		{
			WasCommandSuccessful = Command.WasSuccessful();
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		bool flag = SubGoal != null && SubGoal.IsSubstantiallyFinished(character);
		base.OnDeactivate(character, parent);
		if (character.DirectControlledMajorAIDisabled)
		{
			character.SetDirectControlled(value: true, wantSetLastInputTime: false, wantClearLeaderCommand: false);
		}
		if (Command != null && (Finished || flag))
		{
			Command.SetTarget(character, this, null);
			Command = null;
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Source == SourceType.Player)
		{
			character.MarkPlayerControlled();
		}
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal == null)
		{
			return null;
		}
		return SubGoal.GetOverheadActionIcon(character);
	}
}
