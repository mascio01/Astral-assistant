using System;

public class WaitAndFace : Goal
{
	public TerrainCoord Tile;

	public TimeSpan StartTime;

	public TimeSpan WaitTime;

	public WaitAndFace()
	{
	}

	public WaitAndFace(TerrainCoord tile, TimeSpan waitTime)
	{
		Tile = tile;
		WaitTime = waitTime;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.WaitAndFace;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Tile);
		reflector.Add(ref StartTime);
		reflector.Add(ref WaitTime);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		character.DesiredFacingAngle = MathUtil.GetAngleTo(character.Position, GameTerrain.Instance.GetTileCentrePos(Tile), character.DesiredFacingAngle);
		if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime >= WaitTime)
		{
			OnTimeout();
		}
	}

	public virtual void OnTimeout()
	{
		Finished = true;
	}
}
