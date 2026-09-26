using System;

public class TakeCoverAndReload : StateMachineGoal
{
	public bool Success;

	public bool _dontOpenOurGates;

	public StayInRangeParams StayInRangeParams;

	public EquipmentPrototype AmmoType;

	public InfectionType InfectedWith;

	public TakeCoverAndReload()
	{
	}

	public TakeCoverAndReload(bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public TakeCoverAndReload(bool dontOpenOurGates, StayInRangeParams stayInRangeParams, EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
		AmmoType = ammoType;
		InfectedWith = infectedWith;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref _dontOpenOurGates);
		StayInRangeParams.Reflect(reflector);
		reflector.AddAfter(ref AmmoType, 548);
		reflector.AddAfter(ref InfectedWith, 548);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeCoverAndReload;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new TakeCoverGoal(_dontOpenOurGates, StayInRangeParams));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if ((SubGoal as TakeCoverFromTargetWhileFiring).Success && GetTargetCharacter() != null)
		{
			bool crouching = false;
			if (TakeCoverGoal.CheckCover(character, Target, out crouching))
			{
				Success = true;
				if (crouching)
				{
					SetCrouching(character, parent, crouching: true);
					return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.6666666865348816), aiming: true, crouching: true);
				}
			}
			return new ReloadAnim(aiming: true, crouching: false, AmmoType, InfectedWith);
		}
		if (SubGoal is WaitAndFaceTarget)
		{
			return new ReloadAnim(aiming: true, crouching: true, AmmoType, InfectedWith);
		}
		if (SubGoal is ReloadAnim)
		{
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}
