using System;
using System.Collections.Generic;
using UnityEngine;

public class FlankTarget : Goal
{
	public float DesiredRange;

	public float DesiredAngle;

	public MovementType MovementType;

	public bool FlankLeft;

	private TimeSpan ObstacleStartTime;

	private TimeSpan StartTime;

	private TimeSpan Timeout;

	private static List<Character> NearbyCharacters = new List<Character>();

	public FlankTarget()
	{
	}

	public FlankTarget(Character character, MovementType movementType, float desiredRange, TimeSpan timeout)
	{
		MovementType = movementType;
		DesiredRange = desiredRange;
		Timeout = timeout;
		Aiming = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref DesiredRange);
		reflector.Add(ref DesiredAngle);
		reflector.Add(ref MovementType);
		reflector.Add(ref ObstacleStartTime);
		reflector.Add(ref FlankLeft);
		reflector.Add(ref StartTime);
		reflector.Add(ref Timeout);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FlankTarget;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	private float GetClosestDistSqrToFriendlyCharacterAtAngle(Character character, float angle, List<Character> nearbyCharacters)
	{
		Vector2 vector = Target.Object.PosXZ + MathUtil.GetDirFromAngle(angle) * DesiredRange;
		float num = float.MaxValue;
		foreach (Character nearbyCharacter in nearbyCharacters)
		{
			num = Math.Min((nearbyCharacter.PosXZ - vector).sqrMagnitude, num);
		}
		return num;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		TerrainCoord centreTile = Target.Object.GetCentreTile();
		float f = DesiredRange + 8f;
		TerrainCoord terrainCoord = new TerrainCoord(Mathf.CeilToInt(f), Mathf.CeilToInt(f));
		GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(centreTile - terrainCoord, centreTile + terrainCoord, NearbyCharacters);
		for (int num = NearbyCharacters.Count - 1; num >= 0; num--)
		{
			Character character2 = NearbyCharacters[num];
			if (character2 == null || character2 == character || !character2.IsAwake || !character2.IsEnemy(Target.Object))
			{
				NearbyCharacters.RemoveAt(num);
			}
		}
		TimeSpan timeSpan = (StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()));
		float num2 = MathUtil.RandomFloat((float)timeSpan.TotalSeconds + 100f) * (MathF.PI * 2f);
		float num3 = GetClosestDistSqrToFriendlyCharacterAtAngle(character, num2, NearbyCharacters);
		for (int i = 0; i < 10; i++)
		{
			float num4 = MathUtil.RandomFloat((float)timeSpan.TotalSeconds + 1000f * (float)i) * (MathF.PI * 2f);
			float closestDistSqrToFriendlyCharacterAtAngle = GetClosestDistSqrToFriendlyCharacterAtAngle(character, num4, NearbyCharacters);
			if (closestDistSqrToFriendlyCharacterAtAngle > num3)
			{
				num2 = num4;
				num3 = closestDistSqrToFriendlyCharacterAtAngle;
			}
		}
		NearbyCharacters.Clear();
		DesiredAngle = num2;
		float angleFromDir = MathUtil.GetAngleFromDir(MathUtil.ToXZ(Target.Object.Pos - character.Position), DesiredAngle);
		FlankLeft = MathUtil.SignedAngleDiff(DesiredAngle, angleFromDir) < 0f;
		character.GoalTarget = Target;
		character.StartFlankingTarget(FlankLeft, MovementType, DesiredRange);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.StopFlankingTarget();
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (currentTime - StartTime >= Timeout || IsTargetDeleted())
		{
			Finished = true;
			return;
		}
		Vector2 vector = MathUtil.ToXZ(Target.Object.Pos - character.Position);
		float angleFromDir = MathUtil.GetAngleFromDir(vector, DesiredAngle);
		if (MathUtil.SignedAngleDiff(DesiredAngle, angleFromDir) * (FlankLeft ? (-1f) : 1f) <= 0f)
		{
			Finished = true;
			return;
		}
		Vector2 v = MathUtil.SafeNormalize(MathUtil.RightNormal(vector), MathUtil.ToXZ(character.Right)) * (FlankLeft ? (-1f) : 1f);
		Ray ray = new Ray(character.Position, MathUtil.ToX0Y(v) / 1f);
		if (GameTerrain.Instance.IsPassable(ray, 1f, 2051, character, Target.Object, character.Tile, character.IsPredicted()))
		{
			ObstacleStartTime = TimeSpan.FromTicks(0L);
		}
		else if (ObstacleStartTime.Ticks == 0L)
		{
			ObstacleStartTime = currentTime;
		}
		else if (currentTime - ObstacleStartTime >= TimeSpan.FromSeconds(1.0))
		{
			Finished = true;
		}
	}
}
