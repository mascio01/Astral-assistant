public abstract class RoleGoal : StateMachineGoal
{
	public MovementType MovementType = MovementType.Walk;

	public static float UrgentWorkSpeed = 2f;

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		character.SetRoleUrgent(GetRoleInfoBeingPerformed(character), movementType != MovementType.Walk, null);
		if (SubGoal is AnimationGoal animationGoal && CanActionAnimSpeechChange(animationGoal.GetAnim()))
		{
			animationGoal.SetAnimSpeed(character, (MovementType == MovementType.Run) ? UrgentWorkSpeed : 1f);
		}
		base.SetMovementType(character, movementType);
	}

	public static bool CanActionAnimSpeechChange(ActionAnim anim)
	{
		switch (anim)
		{
		case ActionAnim.Build:
		case ActionAnim.RepairStart:
		case ActionAnim.Repair:
		case ActionAnim.RepairFinish:
		case ActionAnim.CraftStart:
		case ActionAnim.CraftLoop:
		case ActionAnim.CraftEnd:
		case ActionAnim.ChopTreeStart:
		case ActionAnim.ChopTreeLoop:
		case ActionAnim.ChopTreeEnd:
		case ActionAnim.ChopLogStart:
		case ActionAnim.ChopLogLoop:
		case ActionAnim.ChopLogEnd:
		case ActionAnim.PotStart:
		case ActionAnim.PotLoop:
		case ActionAnim.PotEnd:
		case ActionAnim.DigLoop:
		case ActionAnim.MineRockStart:
		case ActionAnim.MineRockLoop:
		case ActionAnim.MineRockEnd:
		case ActionAnim.MineBoulderStart:
		case ActionAnim.MineBoulderLoop:
		case ActionAnim.MineBoulderEnd:
		case ActionAnim.ForgeStart:
		case ActionAnim.ForgeLoop:
		case ActionAnim.ForgeEnd:
		case ActionAnim.CraftTableStart:
		case ActionAnim.CraftTableLoop:
		case ActionAnim.CraftTableEnd:
		case ActionAnim.CraftNonLooped:
			return true;
		default:
			return false;
		}
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		MovementType = ((!character.IsRoleUrgent(GetRoleInfoBeingPerformed(character))) ? MovementType.Walk : MovementType.Run);
		character.DirectControlled = false;
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		if (MovementType > MovementType.Walk && character.Community != null && character.Community.Leader != null && character.Community.Leader != character)
		{
			float quantityFactor = (float)(Session.Instance.PlayTime - character.LastThinkTime).TotalSeconds / Sun.DayLengthSecs;
			character.AddMemory(MemoryPrototype.LeadershipCausedOverwork, character.Community.Leader, character, quantityFactor);
		}
		base.Update(character, parent);
	}

	public abstract RoleInfo GetRoleInfoBeingPerformed(Character character);
}
