public class EnterBuilding : Idle
{
	public bool ForceEnter;

	public bool Success;

	public const float FadeTime = 1f / 60f;

	public override GoalType GetGoalType()
	{
		return GoalType.EnterBuilding;
	}

	public EnterBuilding()
	{
	}

	public EnterBuilding(bool forceEnter)
	{
		ForceEnter = forceEnter;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.AddAfter(ref ForceEnter, 504);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.StartFade(1f, 1f / 60f);
		if (character.CarryingObject != null)
		{
			if (character.IsAuthoritative())
			{
				character.DropAuthoritative();
			}
			else
			{
				character.Drop();
			}
		}
		GetTargetBuilding()?.PlayEnterSound(character);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		Building targetBuilding = GetTargetBuilding();
		if (targetBuilding == null || targetBuilding.IsDestroyed())
		{
			return false;
		}
		return true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!character.IsFadeComplete() || !character.IsAuthoritative())
		{
			return;
		}
		if (character.InsideBuilding == null && character.InteractionObject == null)
		{
			bool wasOrderedInsideBuilding = parent is ObeyLeaderGoal obeyLeaderGoal && obeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player;
			Building targetBuilding = GetTargetBuilding();
			Success = targetBuilding?.OnCharacterEnter(character, wasOrderedInsideBuilding, ForceEnter) ?? false;
			if (Success && targetBuilding.GetInhabitantSlotDef(character).External)
			{
				character.StartFade(0f, 1f / 60f);
			}
			else
			{
				Finished = true;
			}
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.SetFade(0f);
		base.OnDeactivate(character, parent);
	}
}
