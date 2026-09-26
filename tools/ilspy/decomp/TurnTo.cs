public class TurnTo : StateMachineGoal
{
	private TerrainCoord Tile;

	public TurnTo()
	{
	}

	public TurnTo(TerrainCoord tile)
	{
		Tile = tile;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TurnTo;
	}

	protected override bool FinishOnNullSubGoal()
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Tile);
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.IsSitting())
		{
			SetSubGoal(character, parent, new StopSittingGoal());
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal == null)
		{
			character.DesiredFacingAngle = MathUtil.GetAngleTo(character.Position, GameTerrain.Instance.GetTileCentrePos(Tile), character.DesiredFacingAngle);
			if (MathUtil.AngleDiff(character.DesiredFacingAngle, character.FacingAngle) < 0.001f)
			{
				Finished = true;
			}
		}
	}
}
