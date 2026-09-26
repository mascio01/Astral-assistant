using System;
using UnityEngine;

public class MoveDirectlyToTarget : Goal
{
	public float DesiredRange;

	public MovementType MovementType;

	private TimeSpan ObstacleStartTime;

	public bool Success;

	public MoveDirectlyToTarget()
	{
	}

	public MoveDirectlyToTarget(Character character, MovementType movementType, float desiredRange)
	{
		DesiredRange = desiredRange;
		MovementType = movementType;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref DesiredRange);
		reflector.Add(ref MovementType);
		reflector.Add(ref ObstacleStartTime);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveDirectlyToTarget;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.GoalTarget = Target;
		character.StartMovingDirectlyToTarget(MovementType);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.StopMovingDirectlyToTarget();
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Finished)
		{
			return;
		}
		Vector2 v = MathUtil.ToXZ(Target.Object.Pos - character.Position);
		float magnitude = v.magnitude;
		PredictedObjectManager instance = PredictedObjectManager.Instance;
		bool predicted = character.IsPredicted();
		if (magnitude <= DesiredRange && character.IsFacing(MathUtil.ToXZ(Target.Object.Pos), MathF.PI / 10f))
		{
			Success = true;
			Finished = true;
			return;
		}
		Ray ray = new Ray(character.Position, MathUtil.ToX0Y(v) / magnitude);
		if (GameTerrain.Instance.IsPassable(ray, magnitude, 2051, character, Target.Object, character.Tile, predicted))
		{
			ObstacleStartTime = TimeSpan.FromTicks(0L);
		}
		else if (ObstacleStartTime.Ticks == 0L)
		{
			ObstacleStartTime = instance.GetCurrentTime(predicted);
		}
		else if (instance.GetCurrentTime(predicted) - ObstacleStartTime >= TimeSpan.FromSeconds(1.0))
		{
			Success = false;
			Finished = true;
		}
	}
}
