using System;

public class LeaveBuilding : Goal
{
	public int EntranceIndex;

	private const float FadeTime = 1f / 60f;

	public override GoalType GetGoalType()
	{
		return GoalType.LeaveBuilding;
	}

	public LeaveBuilding()
	{
	}

	public LeaveBuilding(int entranceIndex)
	{
		EntranceIndex = entranceIndex;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref EntranceIndex, 89);
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.InsideBuilding == null)
		{
			Finished = true;
		}
	}

	private void Leave(Character character)
	{
		EntranceIndex = MathUtil.Clamp(EntranceIndex, 0, character.InsideBuilding.GetEntranceDefs().Length - 1);
		character.InsideBuilding.OnCharacterLeave(character, Math.Min(EntranceIndex, character.InsideBuilding.GetEntranceDefs().Length - 1), fromBuildingDestroyed: false, fromRagdolled: false);
		character.SetFade(1f);
		character.StartFade(0f, 1f / 60f);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Session.Instance.PlaySpeed == PlaySpeed.Paused)
		{
			return;
		}
		if (character.InsideBuilding != null)
		{
			if (character.InsideBuilding.GetInhabitantSlotDef(character).External)
			{
				character.StartFade(1f, 1f / 60f);
			}
			else
			{
				Leave(character);
			}
		}
		if (character.IsFadeComplete())
		{
			if (character.InsideBuilding != null)
			{
				Leave(character);
			}
			else
			{
				Finished = true;
			}
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (parent is ObeyLeaderGoal obeyLeaderGoal && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player)
		{
			character.SetHangoutLocation(character.Tile);
		}
		base.OnDeactivate(character, parent);
	}
}
