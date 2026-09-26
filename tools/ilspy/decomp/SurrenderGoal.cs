using System;
using UnityEngine;

public class SurrenderGoal : StateMachineGoal
{
	private bool ConversationFinished;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref ConversationFinished);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SurrenderGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted() || Target.GetFlag(TargetFlags.Lost))
		{
			return false;
		}
		if (character.Community == null || character.Community.CommunityType == CommunityType.Psycho)
		{
			return false;
		}
		if (character.Community != null && character.Community.Leader != null && character.Community.Leader != character && character.Community.Leader.CurrentActionAnim == ActionAnim.HandsUp)
		{
			return true;
		}
		Character targetCharacter = GetTargetCharacter();
		if (character.CanFollowPlayerIncludeNearbyAllies() && (targetCharacter == null || !targetCharacter.Zombie) && Session.Instance.IsPlayerSurrendering())
		{
			Community community = Target.Object.GetCommunity();
			if (community != null && community.CanPlayerSurrenderToMe(character, targetCharacter))
			{
				return true;
			}
		}
		if (targetCharacter == null || targetCharacter.Deleted || targetCharacter.Community == null || targetCharacter.Community.CommunityType != CommunityType.Player || targetCharacter.Zombie || targetCharacter.GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (targetCharacter.CurrentActionAnim == ActionAnim.HandsUp)
		{
			return false;
		}
		if (targetCharacter == character.SparringPartner)
		{
			return false;
		}
		if (Active && (SubGoal is LeaveBuilding || SubGoal is Conversation || SubGoal is Wait))
		{
			return true;
		}
		if (ConversationFinished)
		{
			return false;
		}
		if (character.SurrenderMode == SurrenderMode.ForceDontSurrender)
		{
			return false;
		}
		if (character.WasRevivedInCaptivity(Active))
		{
			return true;
		}
		float num = (Active ? 20f : 10f);
		if ((targetCharacter.PosXZ - character.PosXZ).sqrMagnitude >= num * num)
		{
			return false;
		}
		if (character.SurrenderMode == SurrenderMode.ForceSurrenderToPlayer && targetCharacter.IsControllableByPlayer())
		{
			return true;
		}
		int num2 = 0;
		if (character.Community != null && character.Community.Members.Count > 1)
		{
			if (Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(Active ? 60f : 10f) || character.Rank == Rank.Captive || character.HasUnbandagedInjury(0) || character.IsTooDepressedToFollowOrders())
			{
				foreach (Character member in character.Community.Members)
				{
					if (member != character && member.AliveAndNotZombie && member.Consciousness != Consciousness.Unconscious && member.GetBaseObjectType() == BaseObjectType.Human)
					{
						num2++;
					}
				}
				if (num2 <= character.Community.SurrenderThreshold)
				{
					return true;
				}
			}
		}
		else
		{
			if (character.GetBloodLoss() >= 0.25f && character.HasPersonality(CachedPersonalityType.Nervous))
			{
				return true;
			}
			if (character.GetBloodLoss() >= 0.75f && !character.HasPersonality(CachedPersonalityType.Bold))
			{
				return true;
			}
			if (character.GetBloodLoss() >= 0.4f && character.GetUniqueID() == Character.KellySalas)
			{
				return true;
			}
		}
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Attack_Surrender;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.EquippedItem = null;
		character.Surrendering = true;
		if (!character.IsOutdoors())
		{
			SetSubGoal(character, parent, new LeaveBuilding());
			return;
		}
		character.TryStartActionAnim(ActionAnim.HandsUp, GetTargetObject());
		if ((character.Community != null && character.Community.Leader != null && character.Community.Leader != character && character.Community.Leader.CurrentActionAnim == ActionAnim.HandsUp) || character.CanFollowPlayerIncludeNearbyAllies())
		{
			SetSubGoal(character, parent, new FaceTarget());
			return;
		}
		Character targetCharacter = GetTargetCharacter();
		BaseObject obj = null;
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, ref obj, SpeechSituation.Surrender, default(MemoryParam));
		if (speechForSituation != null)
		{
			SetSubGoal(character, parent, new Conversation(character, targetCharacter, obj, speechForSituation, controlledByPlayer: false));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.StopActionAnim(ActionAnim.HandsUp);
		character.GoalTarget = null;
		character.Surrendering = false;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Conversation conversation = SubGoal as Conversation;
		if (conversation != null && !conversation.IsListening())
		{
			foreach (Target target in character.Targets)
			{
				if (target.Object != null && !target.Object.Deleted && target.Object is Character { DirectControlled: not false, AliveAndNotZombie: not false } character2 && (character2.PosXZ - character.PosXZ).sqrMagnitude <= Conversation.MaxRangeForOpener * Conversation.MaxRangeForOpener)
				{
					if (target != conversation.Target)
					{
						conversation.SetTarget(character, this, target);
					}
					break;
				}
			}
		}
		if (character.CurrentActionAnim != ActionAnim.HandsUp && (SubGoal is Conversation || SubGoal is FaceTarget || SubGoal is Wait))
		{
			character.TryStartActionAnim(ActionAnim.HandsUp, GetTargetObject());
		}
		if (character.CurrentActionAnim == ActionAnim.HandsUp)
		{
			character.GoalTarget = ((conversation != null) ? conversation.Target : Target);
			if (character.CanFollowPlayerIncludeNearbyAllies() && !Session.Instance.IsPlayerSurrendering())
			{
				Finished = true;
			}
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip || SubGoal is LeaveBuilding)
		{
			character.TryStartActionAnim(ActionAnim.HandsUp, GetTargetObject());
			if ((character.Community != null && character.Community.Leader != null && character.Community.Leader != character && character.Community.Leader.CurrentActionAnim == ActionAnim.HandsUp) || character.CanFollowPlayerIncludeNearbyAllies())
			{
				return new FaceTarget();
			}
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.AliveAndNotZombie)
			{
				BaseObject obj = null;
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, ref obj, SpeechSituation.Surrender, default(MemoryParam));
				if (speechForSituation != null)
				{
					return new Conversation(character, targetCharacter, obj, speechForSituation, controlledByPlayer: false);
				}
				Debug.LogError("No Surrender speech found! " + character.GetDisplayNameString() + " -> " + targetCharacter.GetDisplayNameString());
			}
		}
		if (SubGoal is Conversation)
		{
			character.LastRevivedTime = TimeSpan.Zero;
			character.SurrenderMode = SurrenderMode.Default;
			if (character.IsAuthoritative())
			{
				character.Community.SurrenderThreshold /= 2;
			}
			ConversationFinished = true;
			return new Wait(TimeSpan.FromSeconds(1.0));
		}
		return base.GetNextSubGoal(character, parent);
	}
}
