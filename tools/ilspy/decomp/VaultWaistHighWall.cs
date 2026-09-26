using System;
using UnityEngine;

public class VaultWaistHighWall : AnimationGoal
{
	public VaultWaistHighWall()
		: base(ActionAnim.Vault)
	{
	}

	public VaultWaistHighWall(Character character, TileObject prop)
		: base(ActionAnim.Vault)
	{
		SetTarget(character, null, character.GetOrCreateTarget(prop));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.VaultWaistHighWall;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		if (AnimState == SpeechAnimState.Started && character.CurrentActionAnim != ActionAnim.Vault)
		{
			Finished = true;
		}
		base.Update(character, parent);
	}

	public override bool CanStartAnimation(Character character, Goal parent)
	{
		if (parent is MoveTo && character.GetRouteCount() > 0)
		{
			Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(character.GetRouteNextNode());
			if (!character.IsFacing(tileCentreXZ, MathF.PI / 10f))
			{
				return false;
			}
		}
		return base.CanStartAnimation(character, parent);
	}

	public override bool WantStopAnimOnExit()
	{
		return false;
	}

	public override bool WantFaceTarget(Character character)
	{
		return false;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (parent is MoveToAndVaultWaistHighWall)
		{
			switch (AnimState)
			{
			case SpeechAnimState.NotStarted:
				return AIOverridesControlReason.Animation;
			case SpeechAnimState.Started:
				if (!character.IsActionAnimInterruptible())
				{
					return AIOverridesControlReason.Vaulting;
				}
				return AIOverridesControlReason.None;
			case SpeechAnimState.Finished:
				return AIOverridesControlReason.None;
			}
		}
		return AIOverridesControlReason.None;
	}
}
