using System;
using UnityEngine;

public class ZombieFrustrationGoal : AnimationGoal
{
	public TimeSpan StartTime;

	public TimeSpan WaitTime;

	public bool TargetWasInaccessible;

	public ZombieFrustrationGoal()
	{
	}

	public ZombieFrustrationGoal(Character character)
		: base(ActionAnim.Frustration)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ZombieFrustrationGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartTime);
		reflector.Add(ref WaitTime);
		reflector.AddAfter(ref TargetWasInaccessible, 7);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		GameTerrain instance = GameTerrain.Instance;
		Character targetCharacter = GetTargetCharacter();
		float num = 2f;
		if (targetCharacter != null)
		{
			if (!targetCharacter.IsControllableByPlayer() && !character.IsVisibleForUpdate())
			{
				num *= 5f;
			}
			TerrainCoord tile = character.Tile;
			TerrainCoord tile2 = targetCharacter.Tile;
			if (instance.IsTileEnclosed(tile2.x, tile2.y) != instance.IsTileEnclosed(tile.x, tile.y))
			{
				num *= 5f;
			}
		}
		WaitTime = TimeSpan.FromSeconds(Mathf.Lerp(num * 0.5f, num, MathUtil.RandomFloat((float)StartTime.TotalMilliseconds)));
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime >= WaitTime)
		{
			Finished = true;
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.GrrArgh)
		{
			if (!character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
			}
			Character targetCharacter = GetTargetCharacter();
			if (character.IsAuthoritative() && !character.IsBeingChoked() && targetCharacter != null)
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, targetCharacter, character, character));
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
