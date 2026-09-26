using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Conversation : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	public Speech OpeningSpeech;

	public BaseObject OpeningSpeechObject;

	public MemoryParam OpeningSpeechParam;

	public Speech OpeningSpeechReplyTo;

	public BaseObject OpeningSpeechReplyToReferringTo;

	public MemoryParam OpeningSpeechReplyToParam;

	public bool ControlledByPlayer;

	public bool DontOpenOurGates;

	public bool Success;

	public bool SuccessfullyReachedListener;

	public bool GotSuccessfulResponse;

	public float SpeechRange;

	private Vector2 ShakedownStartPosXZ;

	private TimeSpan ShakedownStartPosTime = Target.Never;

	private int LastProcessedTargetId;

	private int LastProcessedTargetIdFrame;

	private Speech OldOpeningSpeech;

	public static float MaxRangeForOpener = 5f;

	public static float MaxRangeForIntroduction = 16f;

	public static float MaxRangeForKnock = 64f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("ConversationCalcBestTarget");

	private static GameProfiler GetOpenerTimer = new GameProfiler("GetOpener");

	private static GameProfiler GetOpenerToSelfTimer = new GameProfiler("GetOpenerToSelf");

	private static GameProfiler GetPayRespectsTimer = new GameProfiler("GetPayRespects");

	private static List<Character> NearbyCharacters = new List<Character>();

	public bool CanBeControlledByPlayer(Character character, Character target)
	{
		return CanBeControlledByPlayerStatic(character, target, this);
	}

	public static bool CanBeControlledByPlayerStatic(Character character, Character target)
	{
		Conversation conversation = character.FindActiveGoal(GoalType.Conversation) as Conversation;
		return CanBeControlledByPlayerStatic(character, target, conversation);
	}

	public static bool CanBeControlledByPlayerStatic(Character character, Character target, Conversation conversation)
	{
		if (character.IsControllableByPlayer() && target != null)
		{
			if (conversation != null && conversation.OpeningSpeech != null && conversation.OpeningSpeech.AIOverridesListenerControl)
			{
				return false;
			}
			if (character.Community == target.Community && target.FindActiveGoal(GoalType.Conversation) is Conversation conversation2)
			{
				if (conversation2.ControlledByPlayer)
				{
					return false;
				}
				if (conversation2.OpeningSpeech != null && conversation2.OpeningSpeech.AIOverridesListenerControl)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static bool WantReplyChoice(bool multipleOptions, Speech speech, Character listener, bool canBeControlledByPlayer)
	{
		if (!multipleOptions)
		{
			return false;
		}
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.NoReplyChoice)
		{
			return false;
		}
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.Gossip)
		{
			return false;
		}
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.NoReplyChoiceForNPC && !listener.IsPlayerAvatar())
		{
			return false;
		}
		if (!canBeControlledByPlayer)
		{
			return false;
		}
		return true;
	}

	public Conversation()
	{
	}

	public Conversation(Character character, Character target, BaseObject obj, Speech speech, bool controlledByPlayer)
	{
		OpeningSpeech = speech;
		OpeningSpeechObject = obj;
		OpeningSpeechParam = default(MemoryParam);
		OpeningSpeechReplyTo = null;
		OpeningSpeechReplyToReferringTo = null;
		OpeningSpeechReplyToParam = default(MemoryParam);
		ControlledByPlayer = controlledByPlayer && CanBeControlledByPlayer(character, target);
		SetTarget(character, null, (target != null) ? character.GetOrCreateTarget(target) : null);
		HasUserTarget = true;
	}

	public Conversation(Character character, Character target, BaseObject obj, MemoryParam param, Speech speech, bool controlledByPlayer, Speech replyTo, BaseObject replyToReferringTo, MemoryParam replyToParam)
	{
		OpeningSpeech = speech;
		OpeningSpeechObject = obj;
		OpeningSpeechParam = param;
		OpeningSpeechReplyTo = replyTo;
		OpeningSpeechReplyToReferringTo = replyToReferringTo;
		OpeningSpeechReplyToParam = replyToParam;
		ControlledByPlayer = controlledByPlayer && CanBeControlledByPlayer(character, target);
		SetTarget(character, null, (target != null) ? character.GetOrCreateTarget(target) : null);
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref OpeningSpeech);
		reflector.Add(ref OpeningSpeechObject);
		if (reflector.Version >= 245)
		{
			OpeningSpeechParam.Reflect(reflector);
		}
		reflector.AddAfter(ref OpeningSpeechReplyTo, 56);
		reflector.AddAfter(ref OpeningSpeechReplyToReferringTo, 56);
		if (reflector.Version >= 266)
		{
			OpeningSpeechReplyToParam.Reflect(reflector);
		}
		reflector.Add(ref ControlledByPlayer);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.Add(ref Success);
		reflector.AddAfter(ref SuccessfullyReachedListener, 361);
		reflector.AddAfter(ref GotSuccessfulResponse, 113);
		reflector.AddAfter(ref SpeechRange, 119);
		reflector.Add(ref ShakedownStartPosXZ);
		reflector.AddAfter(ref ShakedownStartPosTime, 112);
		reflector.Add(ref _movementType);
		reflector.AddAfter(ref LastProcessedTargetId, 304);
		reflector.AddAfter(ref LastProcessedTargetIdFrame, 305);
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		_movementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return _movementType;
	}

	public void Speak(Character character, BaseObject speechObject, MemoryParam param, Speech speech, Speech replyTo, BaseObject replyToReferringTo)
	{
		OpeningSpeech = speech;
		OpeningSpeechObject = speechObject;
		OpeningSpeechParam = param;
		OpeningSpeechReplyTo = replyTo;
		OpeningSpeechReplyToReferringTo = replyToReferringTo;
		SetSubGoal(character, this, GetOpeningSpeechGoal(character));
	}

	public override GoalType GetGoalType()
	{
		return GoalType.Conversation;
	}

	public override void OnPreActivate(Character character, Goal parent)
	{
		base.OnPreActivate(character, parent);
		OldOpeningSpeech = OpeningSpeech;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (Target == null)
		{
			GotSuccessfulResponse = false;
			Success = false;
			SuccessfullyReachedListener = true;
			if (OpeningSpeech == null)
			{
				Debug.LogWarning("Got a null opening speech in Conversation goal with no target: " + character.GetDisplayNameString() + " (" + character.GetGoalDebugString() + ") OldOpeningSpeech: " + ((OldOpeningSpeech != null) ? OldOpeningSpeech.UniqueID : ""));
				Finished = true;
			}
			else if (character.InsideBuilding != null)
			{
				SetSubGoal(character, parent, new LeaveBuilding());
			}
			else
			{
				SetSubGoal(character, parent, GetOpeningSpeechGoal(character));
			}
		}
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		if (SubGoal != null && OpeningSpeech != null && OpeningSpeech.Situation == SpeechSituation.Surrender)
		{
			return;
		}
		GotSuccessfulResponse = false;
		Success = false;
		SuccessfullyReachedListener = OpeningSpeech == null;
		if (parent is SurvivorGoal)
		{
			DontOpenOurGates = OpeningSpeech != null && OpeningSpeech.Situation == SpeechSituation.PayRespects && AlertGoal.CalcDontOpenOurGates(character);
		}
		Character targetCharacter = GetTargetCharacter();
		if (Session.Instance.GetPlayerControllingCharacter(character) != null && CanBeControlledByPlayer(character, targetCharacter))
		{
			ControlledByPlayer = true;
		}
		if (OpeningSpeech != null && (OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.Introduction || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.IntroductionShakedown) && character.Community != null && targetCharacter != null && targetCharacter.Community != null && Session.Instance.CommunityManager.GetRelationship(character.Community, targetCharacter.Community) == CommunityRelationshipType.Unknown)
		{
			Session.Instance.CommunityManager.SetRelationship(character.Community, targetCharacter.Community, CommunityRelationshipType.Introducing);
		}
		if (character.InsideBuilding != null && (targetCharacter == null || targetCharacter.CurrentActionAnim != ActionAnim.HandsUp || !character.IsOutdoors()))
		{
			int closestEntranceTo = character.InsideBuilding.GetClosestEntranceTo(Target.Object.GetCentreTile());
			SetSubGoal(character, parent, new LeaveBuilding(closestEntranceTo));
			return;
		}
		Goal firstSubGoal = GetFirstSubGoal(character, parent);
		if (firstSubGoal != null)
		{
			SetSubGoal(character, parent, firstSubGoal);
			return;
		}
		Debug.LogWarning("Conversation between " + character.GetDisplayNameString() + " and " + ((targetCharacter != null) ? targetCharacter.GetDisplayNameString() : "null") + " OnActivateTarget got no subgoal.  OpeningSpeech: " + ((OpeningSpeech != null) ? OpeningSpeech.NativeText : "null") + ((targetCharacter != null) ? (", their goal: " + targetCharacter.GetGoalDebugString()) : ""));
		Finished = true;
	}

	private float GetSpeechRange(Speech speech, bool sitting)
	{
		if (sitting)
		{
			return Math.Max(speech.MaxRange, 8f);
		}
		return Math.Max(speech.MaxRange, 1.25f);
	}

	private float GetMaxRangeForOpener(Character character, Target target, CommunityRelationshipType relationship)
	{
		float result = MaxRangeForOpener;
		if (relationship <= CommunityRelationshipType.Introducing && character.Community != target.Object.GetCommunity() && character.Community != null && character.Community.CommunityType != CommunityType.RovingRefugee)
		{
			result = ((!target.GetFlag(TargetFlags.KnockedOnGate)) ? MaxRangeForIntroduction : MaxRangeForKnock);
		}
		return result;
	}

	private Goal GetFirstSubGoal(Character character, Goal parent)
	{
		if (Target == null)
		{
			return GetOpeningSpeechGoal(character);
		}
		if (OpeningSpeech != null && (OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.Introduction || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.IntroductionShakedown || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.RepeatShakedown || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape) && Session.Instance.PlayTime - ShakedownStartPosTime >= TimeSpan.FromSeconds(120.0))
		{
			ShakedownStartPosXZ = character.PosXZ;
			ShakedownStartPosTime = Session.Instance.PlayTime;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return null;
		}
		Conversation conversation = targetCharacter.FindActiveGoal(GoalType.Conversation) as Conversation;
		if (conversation != null && conversation.GetTargetCharacter() == character && conversation.SubGoal is SpeakToTarget && !conversation.SubGoal.Finished)
		{
			SpeakToTarget speakToTarget = conversation.SubGoal as SpeakToTarget;
			SpeechRange = conversation.SpeechRange;
			if (speakToTarget.Speech.StandUp && character.IsSitting())
			{
				return new StopSittingGoal();
			}
			return new ListenToTarget(speakToTarget.Speech, speakToTarget.SpeechObject, speakToTarget.MemoryParam, speakToTarget.ReplyTo, speakToTarget.ReplyToReferringTo, speakToTarget.ReplyToParam);
		}
		if (targetCharacter.FindActiveGoal(GoalType.MoveToAndInteractGoal) is MoveToAndInteractGoal moveToAndInteractGoal && moveToAndInteractGoal.GetTargetCharacter() == character)
		{
			return new WaitForTargetToInteractWithMe();
		}
		if (OpeningSpeech != null)
		{
			bool flag = OpeningSpeech.CanTalkOutOfRange || (character.PosXZ - targetCharacter.PosXZ).sqrMagnitude <= MathUtil.Squared(GetSpeechRange(OpeningSpeech, character.IsSitting()));
			if ((OpeningSpeech.Run || OpeningSpeech.StandUp) && character.IsSitting() && !OpeningSpeech.CanTalkOutOfRange)
			{
				flag = false;
			}
			if (flag)
			{
				return GetOpeningSpeechGoal(character);
			}
			return new MoveWithinRangeAndSightOfTarget(OpeningSpeech.Run ? MovementType.Run : _movementType, aiming: false, 0f, GetSpeechRange(OpeningSpeech, sitting: false), dontOpenOurGates: false, default(StayInRangeParams), 0f)
			{
				AlwaysTrackTarget = true,
				AStarPriority = AStarPriority.Medium,
				_dontOpenOurGates = DontOpenOurGates
			};
		}
		if (conversation != null && conversation.GetTargetCharacter() == character && conversation.SubGoal is WaitForTargetToReply && !conversation.SubGoal.Finished)
		{
			WaitForTargetToReply waitForTargetToReply = conversation.SubGoal as WaitForTargetToReply;
			SpeechRange = conversation.SpeechRange;
			return GetReplyGoal(character, waitForTargetToReply.MyLastSpeech, waitForTargetToReply.MyLastSpeechObject, waitForTargetToReply.MyLastSpeechParam, null, null, default(MemoryParam));
		}
		return null;
	}

	protected override void OnDeactivateTarget(Character character, Goal parent)
	{
		if (OpeningSpeech != null && (OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.Introduction || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.IntroductionShakedown))
		{
			Character targetCharacter = GetTargetCharacter();
			if (character.Community != null && targetCharacter != null && targetCharacter.Community != null && Session.Instance.CommunityManager.GetRelationship(character.Community, targetCharacter.Community) == CommunityRelationshipType.Introducing)
			{
				StoryEvent storyEvent = new StoryEvent
				{
					Type = StoryEventType.IntroductionFinished,
					Delay = 2f,
					Subject = Specifier.Actor,
					Object = Specifier.Target
				};
				StoryManager.Instance.QueuedEvents.Add(QueuedEvent.Create(storyEvent, character, targetCharacter, OpeningSpeechObject, OpeningSpeechParam, Session.Instance.PlayTime));
			}
		}
		bool flag = false;
		if (OpeningSpeech != null)
		{
			flag = OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.TeleportIfFailed;
			flag &= character.GetQueuedSpeechIndex(OpeningSpeech, GetTargetCharacter(), OpeningSpeechObject, OpeningSpeechParam) == -1;
		}
		if (SubGoal is WaitForTargetToReply waitForTargetToReply && waitForTargetToReply.MyLastSpeech.NoReply != null && (Finished || flag || waitForTargetToReply.MyLastSpeech.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape))
		{
			Character targetCharacter2 = GetTargetCharacter();
			MemoryParam param = waitForTargetToReply.MyLastSpeechParam;
			BaseObject resultObj;
			Speech noReply = StoryManager.Instance.GetNoReply(character, targetCharacter2, waitForTargetToReply.MyLastSpeechObject, waitForTargetToReply.MyLastSpeech, out resultObj, ref param, Session.Instance.DeterministicRand);
			if (noReply != null)
			{
				character.PopQueuedSpeech(OpeningSpeech, targetCharacter2, OpeningSpeechObject, OpeningSpeechParam);
				character.PushQueuedSpeech(noReply, targetCharacter2, resultObj, param);
			}
		}
		if (SubGoal is SpeakToTarget speakToTarget && speakToTarget.Speech.NoReply != null && (Finished || flag || speakToTarget.Speech.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape))
		{
			Character targetCharacter3 = GetTargetCharacter();
			MemoryParam param2 = speakToTarget.MemoryParam;
			BaseObject resultObj2;
			Speech noReply2 = StoryManager.Instance.GetNoReply(character, targetCharacter3, speakToTarget.SpeechObject, speakToTarget.Speech, out resultObj2, ref param2, Session.Instance.DeterministicRand);
			if (noReply2 != null)
			{
				character.PopQueuedSpeech(OpeningSpeech, targetCharacter3, OpeningSpeechObject, OpeningSpeechParam);
				character.PushQueuedSpeech(noReply2, targetCharacter3, resultObj2, param2);
			}
		}
		if ((SubGoal is MoveTo || SubGoal is LeaveBuilding || SubGoal is StopSittingGoal) && OpeningSpeech != null && OpeningSpeech.NoReply != null && flag)
		{
			Character targetCharacter4 = GetTargetCharacter();
			MemoryParam param3 = OpeningSpeechParam;
			BaseObject resultObj3;
			Speech noReply3 = StoryManager.Instance.GetNoReply(character, targetCharacter4, OpeningSpeechObject, OpeningSpeech, out resultObj3, ref param3, Session.Instance.DeterministicRand);
			if (noReply3 != null)
			{
				character.PopQueuedSpeech(OpeningSpeech, targetCharacter4, OpeningSpeechObject, OpeningSpeechParam);
				character.PushQueuedSpeech(noReply3, targetCharacter4, resultObj3, param3);
			}
		}
		if (SubGoal is ListenToTarget listenToTarget)
		{
			if (listenToTarget.Finished && listenToTarget.Success)
			{
				bool multipleOptions = false;
				MemoryParam param4 = listenToTarget.SpeechParam;
				Character targetCharacter5 = GetTargetCharacter();
				BaseObject resultObj4;
				Speech reply = StoryManager.Instance.GetReply(character, targetCharacter5, listenToTarget.SpeechObject, listenToTarget.Speech, out resultObj4, ref param4, out multipleOptions, Session.Instance.DeterministicRand);
				if (reply != null && reply.MustFinish && !WantReplyChoice(multipleOptions, listenToTarget.Speech, character, CanBeControlledByPlayerStatic(targetCharacter5, character)))
				{
					character.PushQueuedSpeech(reply, targetCharacter5, resultObj4, param4);
				}
			}
			else if (listenToTarget.ReplyTo != null && listenToTarget.ReplyTo.NoReply != null && (flag || listenToTarget.ReplyTo.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape))
			{
				Character targetCharacter6 = GetTargetCharacter();
				MemoryParam param5 = listenToTarget.ReplyToParam;
				BaseObject resultObj5;
				Speech noReply4 = StoryManager.Instance.GetNoReply(character, targetCharacter6, listenToTarget.ReplyToReferringTo, listenToTarget.ReplyTo, out resultObj5, ref param5, Session.Instance.DeterministicRand);
				if (noReply4 != null)
				{
					character.PopQueuedSpeech(OpeningSpeech, targetCharacter6, OpeningSpeechObject, OpeningSpeechParam);
					character.PushQueuedSpeech(noReply4, targetCharacter6, resultObj5, param5);
				}
			}
		}
		base.OnDeactivateTarget(character, parent);
		if ((OpeningSpeech == null || OpeningSpeech.Situation != SpeechSituation.Surrender) && parent is PrioritiserGoal)
		{
			OpeningSpeech = null;
			OpeningSpeechObject = null;
			OpeningSpeechParam = default(MemoryParam);
			OpeningSpeechReplyTo = null;
			OpeningSpeechReplyToReferringTo = null;
			OpeningSpeechReplyToParam = default(MemoryParam);
			ControlledByPlayer = false;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (SuccessfullyReachedListener && !(parent is FindGoal))
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null)
			{
				character.SetRecentActivity(RecentActivityType.Conversation, targetCharacter);
			}
		}
		base.OnDeactivate(character, parent);
		if (parent is PrioritiserGoal)
		{
			OpeningSpeech = null;
			OpeningSpeechObject = null;
			OpeningSpeechParam = default(MemoryParam);
			OpeningSpeechReplyTo = null;
			OpeningSpeechReplyToReferringTo = null;
			OpeningSpeechReplyToParam = default(MemoryParam);
			ControlledByPlayer = false;
		}
		if (!HasUserTarget)
		{
			SetTarget(character, parent, null);
		}
	}

	public override void OnSpokenToStarted(Character character, Goal parent, Character speaker, Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyParam)
	{
		if (speaker == GetTargetCharacter())
		{
			if (speech.StandUp && (character.IsSitting() || SubGoal is StopSittingGoal))
			{
				if (!(SubGoal is StopSittingGoal))
				{
					SetSubGoal(character, parent, new StopSittingGoal());
				}
			}
			else if (!(SubGoal is ListenToTarget listenToTarget) || listenToTarget.Speech != speech || listenToTarget.SpeechObject != speechObject || listenToTarget.SpeechParam != speechParam || listenToTarget.ReplyTo != replyTo || listenToTarget.ReplyToReferringTo != replyReferringTo || listenToTarget.ReplyToParam != replyParam)
			{
				SetSubGoal(character, parent, new ListenToTarget(speech, speechObject, speechParam, replyTo, replyReferringTo, replyParam));
			}
		}
		else if (IsMovingToTarget() && OpeningSpeech != null && OpeningSpeech.Importance < Importance.Normal)
		{
			Finished = true;
		}
	}

	public override bool OnSpokenToFinished(Character character, Goal parent, Character speaker, Speech speech, bool interrupted, Speech cont, Speech specialBehaviourTriggered)
	{
		return base.OnSpokenToFinished(character, parent, speaker, speech, interrupted, cont, specialBehaviourTriggered);
	}

	public bool Skip(Character character)
	{
		if (SubGoal is SpeakToTarget speakToTarget)
		{
			speakToTarget.Skip(character);
			return true;
		}
		if (SubGoal is ListenToTarget listenToTarget)
		{
			listenToTarget.Skip(character);
			return true;
		}
		return false;
	}

	public bool IsSkippable(Character character)
	{
		if (SubGoal is SpeakToTarget)
		{
			return character.IsSpeechSkippable();
		}
		if (SubGoal is ListenToTarget listenToTarget)
		{
			return listenToTarget.GetTargetCharacter()?.IsSpeechSkippable() ?? false;
		}
		return false;
	}

	public bool IsSpeaking()
	{
		if (SubGoal is SpeakToTarget)
		{
			return !SubGoal.Finished;
		}
		return false;
	}

	public bool IsWaitingForPlayerToSelectAReply()
	{
		return SubGoal is PlayerChooseReply;
	}

	public bool IsWaitingForReply()
	{
		return SubGoal is WaitForTargetToReply;
	}

	public bool IsListening()
	{
		return SubGoal is ListenToTarget;
	}

	public bool IsMovingToTarget()
	{
		if (!(SubGoal is MoveAsCloseAsPossibleToTarget))
		{
			return SubGoal is MoveWithinRangeAndSightOfTarget;
		}
		return true;
	}

	public Speech GetLastSpeechToReplyTo(out BaseObject myLastSpeechObject, out MemoryParam myLastSpeechParam, out Speech myLastSpecialBehaviourTriggeredFromSpeech)
	{
		if (SubGoal is WaitForTargetToReply waitForTargetToReply)
		{
			myLastSpeechObject = waitForTargetToReply.MyLastSpeechObject;
			myLastSpeechParam = waitForTargetToReply.MyLastSpeechParam;
			myLastSpecialBehaviourTriggeredFromSpeech = waitForTargetToReply.MyLastSpecialBehaviourTriggeredFromSpeech;
			return waitForTargetToReply.MyLastSpeech;
		}
		myLastSpeechObject = null;
		myLastSpeechParam = default(MemoryParam);
		myLastSpecialBehaviourTriggeredFromSpeech = null;
		return null;
	}

	public string GetLastSpeechToReplyToText(Character character)
	{
		if (!(SubGoal is WaitForTargetToReply waitForTargetToReply))
		{
			return null;
		}
		return waitForTargetToReply.GetMyLastSpeechText(character);
	}

	public Goal GetOpeningSpeechGoal(Character character)
	{
		SuccessfullyReachedListener = true;
		SpeechRange = GetSpeechRange(OpeningSpeech, character.IsSitting());
		if (OpeningSpeech.Situation == SpeechSituation.Eulogy && OpeningSpeech.SpecialBehaviour != SpecialSpeechBehaviour.OverrideEulogy)
		{
			return new GiveEulogy(OpeningSpeech, OpeningSpeechObject);
		}
		if (OpeningSpeech.Situation == SpeechSituation.VisitGrave && OpeningSpeech.SpecialBehaviour != SpecialSpeechBehaviour.OverrideEulogy)
		{
			return new TalkToGrave(OpeningSpeech, OpeningSpeechObject);
		}
		return new SpeakToTarget(OpeningSpeech, OpeningSpeechObject, OpeningSpeechParam, OpeningSpeechReplyTo, OpeningSpeechReplyToReferringTo, OpeningSpeechReplyToParam);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (Session.Instance.GetPlayerControllingCharacter(character) != null && CanBeControlledByPlayer(character, targetCharacter))
		{
			ControlledByPlayer = true;
		}
		if ((SubGoal is MoveAsCloseAsPossibleToTarget || SubGoal is MoveWithinRangeAndSightOfTarget) && targetCharacter != null && OpeningSpeech != null)
		{
			float speechRange = GetSpeechRange(OpeningSpeech, sitting: false);
			if ((character.PosXZ - targetCharacter.PosXZ).sqrMagnitude <= speechRange * speechRange && Target.HasLineOfSightIgnoringPlantCover)
			{
				SetSubGoal(character, parent, GetOpeningSpeechGoal(character));
			}
			CommunityRelationshipType relationship = ((character.Community != targetCharacter.Community) ? Session.Instance.CommunityManager.GetRelationship(character.Community, targetCharacter.Community) : CommunityRelationshipType.Unknown);
			float maxRangeForOpener = GetMaxRangeForOpener(character, Target, relationship);
			if (OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.Introduction || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.IntroductionShakedown || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.RepeatShakedown || OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.PreventEscape)
			{
				float num = maxRangeForOpener + GetSpeechRange(OpeningSpeech, sitting: false) + 16f;
				if ((character.PosXZ - ShakedownStartPosXZ).sqrMagnitude > num * num)
				{
					Finished = true;
				}
				if (character.Community != null && character.Community.IsAISettlement() && character.Community.BaseRect != TerrainRect.Invalid && character.Community.BaseRect.Contains(GameTerrain.Instance.GetTileCoordForPosXZ(ShakedownStartPosXZ)) && character.Community.BaseRect.GetClosestDistSqTo(character.Tile) >= MathUtil.Squared(12f))
				{
					Finished = true;
				}
			}
		}
		BaseObject speechObject;
		MemoryParam param;
		Speech currentSpeech = GetCurrentSpeech(out speechObject, out param);
		if (currentSpeech != null && currentSpeech.CancelConditions != null && StoryManager.AreAnyConditionsSatisfied(currentSpeech.CancelConditions, character, targetCharacter, speechObject, param))
		{
			Finished = true;
		}
		targetCharacter?.GetOrCreateTarget(character).ForceVisible(targetCharacter);
		if (HasTargetGoneOutOfRange(character))
		{
			Finished = true;
		}
		bool flag = false;
		if (targetCharacter != null && !ControlledByPlayer && targetCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation)
		{
			if (conversation.GetTargetCharacter() != character)
			{
				if (!(SubGoal is Idle))
				{
					SetSubGoal(character, parent, new Idle());
				}
				flag = true;
			}
			if (conversation.IsListening() && IsListening())
			{
				Debug.LogWarning(character.GetDisplayNameString() + "(player: " + character.IsControllableByPlayer() + ", dc: " + character.DirectControlled + ") and " + targetCharacter.GetDisplayNameString() + "(player: " + targetCharacter.IsControllableByPlayer() + ", dc: " + targetCharacter.DirectControlled + ") are both in a listen state with each other (" + ((OpeningSpeech != null) ? OpeningSpeech.UniqueID : "none") + " / " + ((conversation.OpeningSpeech != null) ? conversation.OpeningSpeech.UniqueID : "none") + ")");
				Finished = true;
				return;
			}
		}
		if (!flag && SubGoal is Idle)
		{
			Goal firstSubGoal = GetFirstSubGoal(character, parent);
			if (firstSubGoal != null)
			{
				SetSubGoal(character, parent, firstSubGoal);
			}
			else
			{
				Finished = true;
			}
		}
		if (OpeningSpeech != null && OpeningSpeech.Situation == SpeechSituation.Surrender && character.CurrentActionAnim != ActionAnim.HandsUp && character.IsControllableByPlayer())
		{
			Finished = true;
		}
	}

	public Character GetCurrentSpeaker(Character character)
	{
		if (SubGoal is ListenToTarget listenToTarget)
		{
			return listenToTarget.GetTargetCharacter();
		}
		return character;
	}

	public Speech GetCurrentSpeech(out BaseObject speechObject, out MemoryParam param)
	{
		if (SubGoal is MoveAsCloseAsPossibleToTarget || SubGoal is MoveWithinRangeAndSightOfTarget)
		{
			speechObject = OpeningSpeechObject;
			param = OpeningSpeechParam;
			return OpeningSpeech;
		}
		if (SubGoal is SpeakToTarget speakToTarget)
		{
			speechObject = speakToTarget.SpeechObject;
			param = speakToTarget.MemoryParam;
			return speakToTarget.Speech;
		}
		if (SubGoal is WaitForTargetToReply waitForTargetToReply)
		{
			speechObject = waitForTargetToReply.MyLastSpeechObject;
			param = waitForTargetToReply.MyLastSpeechParam;
			return waitForTargetToReply.MyLastSpeech;
		}
		speechObject = null;
		param = default(MemoryParam);
		return null;
	}

	public bool HasTargetGoneOutOfRange(Character character)
	{
		if (SubGoal is MoveTo || SubGoal is LeaveBuilding || SubGoal is StopSittingGoal || SubGoal is Idle)
		{
			return false;
		}
		if (Target == null)
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.Deleted || targetCharacter.IsDisappeared())
		{
			return false;
		}
		Vector2 vector = targetCharacter.PosXZ;
		if (targetCharacter.InsideBuilding != null)
		{
			vector = GameTerrain.Instance.GetTileCentreXZ(targetCharacter.InsideBuilding.GetNearestTileTo(character.Tile));
		}
		float num = Hud.DirectControlTargetableConversationRadius;
		if (character.IsSitting() || targetCharacter.IsSitting())
		{
			num = Hud.DirectControlTargetableSittingRadius;
		}
		num = ((OpeningSpeech == null) ? (num + 2f) : Math.Max(num, GetSpeechRange(OpeningSpeech, character.IsSitting()) + 2f));
		num = Math.Max(num, SpeechRange);
		if ((character.PosXZ - vector).sqrMagnitude <= num * num)
		{
			return false;
		}
		if (SubGoal is ISpeechGoal speechGoal && speechGoal.GetSpeech().CanTalkOutOfRange)
		{
			return false;
		}
		return true;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (OpeningSpeech != null && OpeningSpeech.AIOverridesControl && !(SubGoal is PlayerChooseReply))
		{
			return AIOverridesControlReason.Scripted;
		}
		if (SubGoal is SpeakToTarget { Speech: not null } speakToTarget && speakToTarget.Speech.AIOverridesControl && !(SubGoal is PlayerChooseReply))
		{
			return AIOverridesControlReason.Scripted;
		}
		Conversation conversationWith = GameCursor.GetConversationWith(character, GetTargetObject());
		if (conversationWith != null && conversationWith.OpeningSpeech != null && conversationWith.OpeningSpeech.AIOverridesListenerControl && !(SubGoal is PlayerChooseReply))
		{
			return AIOverridesControlReason.Scripted;
		}
		if (SubGoal is ListenToTarget { Speech: not null } listenToTarget && listenToTarget.Speech.AIOverridesListenerControl && !(SubGoal is PlayerChooseReply))
		{
			return AIOverridesControlReason.Scripted;
		}
		return AIOverridesControlReason.None;
	}

	public static void SetPaidRespectsFlag(Character character, Target target)
	{
		target.SetFlag(TargetFlags.HasPaidRespects, on: true);
		Character character2 = target.Object as Character;
		foreach (Target target2 in character.Targets)
		{
			if (target2 != target && target2.Object is Character { Alive: false } character3 && character3.GetBaseObjectType() == BaseObjectType.Human && WantPayRespectsOnCorpse(character, character3) && (character2 == null || character2.Community == character.Community || character3.Community != character.Community) && !Relationship.HasAnyRelationship(character, character3))
			{
				character.CalcApprovalRating(character3, out var approval, out var _);
				if (!(Math.Abs(approval) >= 50f))
				{
					target2.SetFlag(TargetFlags.HasPaidRespects, on: true);
				}
			}
		}
	}

	private static bool WantPayRespectsOnCorpse(Character character, Character targetCharacter)
	{
		if (targetCharacter.Zombie && targetCharacter.Community != character.Community && targetCharacter.IsAmbient())
		{
			return false;
		}
		if (character.Community != targetCharacter.Community && Session.Instance.CommunityManager.GetRelationship(character.Community, targetCharacter.Community) == CommunityRelationshipType.Hostile)
		{
			return false;
		}
		if (targetCharacter.Killer == character)
		{
			return false;
		}
		return true;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is LeaveBuilding || SubGoal is StopSittingGoal)
		{
			return GetFirstSubGoal(character, parent);
		}
		if (SubGoal is MoveWithinRangeAndSightOfTarget moveWithinRangeAndSightOfTarget)
		{
			if (moveWithinRangeAndSightOfTarget.Success)
			{
				return GetOpeningSpeechGoal(character);
			}
			if (!IsTargetDeleted())
			{
				return new MoveAsCloseAsPossibleToTarget(OpeningSpeech.Run ? MovementType.Run : _movementType, GetSpeechRange(OpeningSpeech, sitting: false))
				{
					AlwaysTrackTarget = true,
					AStarPriority = AStarPriority.Medium,
					_dontOpenOurGates = DontOpenOurGates
				};
			}
		}
		if (SubGoal is MoveAsCloseAsPossibleToTarget moveAsCloseAsPossibleToTarget)
		{
			if (moveAsCloseAsPossibleToTarget.Success || OpeningSpeech.CanTalkOutOfRange)
			{
				return GetOpeningSpeechGoal(character);
			}
			Character targetCharacter = GetTargetCharacter();
			if (OpeningSpeech != null && OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.TeleportIfFailed && targetCharacter != null)
			{
				float num = Math.Min(Math.Max(moveAsCloseAsPossibleToTarget.MaxRange - 1f, 1f), moveAsCloseAsPossibleToTarget.MaxRange);
				Vector2 pos = ((targetCharacter.InsideBuilding != null) ? (MathUtil.ToXZ(targetCharacter.InsideBuilding.GetEntrancePos(0)) + MathUtil.GetDirFromAngle(targetCharacter.InsideBuilding.GetEntranceAngle(0)) * num) : (targetCharacter.PosXZ + MathUtil.ToXZ(targetCharacter.Forward) * num));
				TerrainCoord tileCoordForPosXZ = GameTerrain.Instance.GetTileCoordForPosXZ(pos);
				CustomRandom deterministicRand = Session.Instance.DeterministicRand;
				for (int i = 0; i < 100; i++)
				{
					if (!GameTerrain.Instance.IsImpassable(tileCoordForPosXZ.x, tileCoordForPosXZ.y, 2049, character, character))
					{
						break;
					}
					tileCoordForPosXZ += new TerrainCoord(deterministicRand.Next(-1, 2), deterministicRand.Next(-1, 2));
				}
				character.SetTile(GameTerrain.Instance.ClampTileWithinBounds(tileCoordForPosXZ));
				return new MoveWithinRangeAndSightOfTarget(OpeningSpeech.Run ? MovementType.Run : _movementType, aiming: false, 0f, GetSpeechRange(OpeningSpeech, sitting: false), dontOpenOurGates: false, default(StayInRangeParams), 0f)
				{
					AlwaysTrackTarget = true,
					AStarPriority = AStarPriority.Medium,
					_dontOpenOurGates = DontOpenOurGates
				};
			}
			if (OpeningSpeech != null && OpeningSpeech.Situation == SpeechSituation.PayRespects && Target != null)
			{
				SetPaidRespectsFlag(character, Target);
			}
			character.SetQueuedSpeechLastFailedAttemptTime(OpeningSpeech, targetCharacter, OpeningSpeechObject, OpeningSpeechParam, Session.Instance.PlayTime);
			return null;
		}
		if (SubGoal is SpeakToTarget speakToTarget)
		{
			if (speakToTarget.Success)
			{
				GotSuccessfulResponse |= speakToTarget.Speech.SuccessfulResponse;
				if (speakToTarget.Target == null)
				{
					Success = true;
					return null;
				}
				MemoryParam param = speakToTarget.MemoryParam;
				Character targetCharacter2 = GetTargetCharacter();
				if (StoryManager.Instance.GetReply(targetCharacter2, character, speakToTarget.SpeechObject, speakToTarget.Speech, out var _, ref param, out var _, Session.Instance.DeterministicRand) != null || speakToTarget.Speech.SpecialBehaviour == SpecialSpeechBehaviour.AskOpinion || speakToTarget.Speech.SpecialBehaviour == SpecialSpeechBehaviour.AskMorale || speakToTarget.SpecialBehaviourTriggered != null)
				{
					float timeout = ((character.CurrentActionAnim == ActionAnim.HandsUp || (targetCharacter2 != null && targetCharacter2.CurrentActionAnim == ActionAnim.HandsUp)) ? Sun.DayLengthSecs : 60f);
					return new WaitForTargetToReply(speakToTarget.Speech, speakToTarget.SpeechObject, speakToTarget.MemoryParam, speakToTarget.SpeechParamResults, speakToTarget.SpeechText_DEPRECATED, speakToTarget.SpecialBehaviourTriggered, timeout);
				}
			}
			return null;
		}
		if (SubGoal is ListenToTarget listenToTarget)
		{
			if (listenToTarget.Success)
			{
				return GetReplyGoal(character, listenToTarget.Speech, listenToTarget.SpeechObject, listenToTarget.SpeechParam, listenToTarget.MyReplyOverride, listenToTarget.MyReplyOverrideReferringTo, listenToTarget.MyReplyOverrideMemoryParam);
			}
			if (listenToTarget.ReplyTo != null && listenToTarget.ReplyTo.NoReply != null)
			{
				MemoryParam param2 = listenToTarget.ReplyToParam;
				BaseObject resultObj2;
				Speech noReply = StoryManager.Instance.GetNoReply(character, GetTargetCharacter(), listenToTarget.ReplyToReferringTo, listenToTarget.ReplyTo, out resultObj2, ref param2, Session.Instance.DeterministicRand);
				if (noReply != null)
				{
					return new SpeakToTarget(noReply, resultObj2, param2, null, null, default(MemoryParam));
				}
			}
			return null;
		}
		if (SubGoal is WaitForTargetToReply waitForTargetToReply && waitForTargetToReply.MyLastSpeech.NoReply != null)
		{
			MemoryParam param3 = waitForTargetToReply.MyLastSpeechParam;
			BaseObject resultObj3;
			Speech noReply2 = StoryManager.Instance.GetNoReply(character, GetTargetCharacter(), waitForTargetToReply.MyLastSpeechObject, waitForTargetToReply.MyLastSpeech, out resultObj3, ref param3, Session.Instance.DeterministicRand);
			if (noReply2 != null)
			{
				return new SpeakToTarget(noReply2, resultObj3, param3, null, null, default(MemoryParam));
			}
		}
		return null;
	}

	public Goal GetReplyGoal(Character character, Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyOverride, BaseObject replyOverrideReferringTo, MemoryParam replyOverrideParam)
	{
		GotSuccessfulResponse |= speech.SuccessfulResponse;
		Speech speech2 = null;
		BaseObject resultObj = null;
		MemoryParam param = speechParam;
		bool multipleOptions = false;
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.Gossip && !character.HasUnbandagedInjury(0) && Session.Instance.DeterministicRand.RandomChoice(0.2f))
		{
			speech2 = StoryManager.Instance.GetSpeechForSituation(character, GetTargetCharacter(), SpeechSituation.Morale);
		}
		if (speech2 == null && replyOverride != null)
		{
			speech2 = replyOverride;
			resultObj = replyOverrideReferringTo;
			param = replyOverrideParam;
		}
		if (speech2 == null)
		{
			speech2 = StoryManager.Instance.GetReply(character, GetTargetCharacter(), speechObject, speech, out resultObj, ref param, out multipleOptions, Session.Instance.DeterministicRand);
		}
		if (speech2 == null)
		{
			if (speech.SpecialBehaviour == SpecialSpeechBehaviour.AskOpinion)
			{
				return new GiveOpinion(GetTargetCharacter());
			}
			if (speech.SpecialBehaviour == SpecialSpeechBehaviour.AskOpinionOfReferringTo)
			{
				return new GiveOpinion(speechObject as Character);
			}
			if (speech.SpecialBehaviour == SpecialSpeechBehaviour.AskMorale)
			{
				return new GiveMorale();
			}
			if (speech.SpecialBehaviour == SpecialSpeechBehaviour.Greeting)
			{
				return new GreetingResponse();
			}
			Success = true;
			return null;
		}
		if (WantReplyChoice(multipleOptions, speech, character, ControlledByPlayer))
		{
			return new PlayerChooseReply(speech);
		}
		return new SpeakToTarget(speech2, resultObj, param, speech, speechObject, speechParam);
	}

	public override void BuildDebugExtraInfoString(Character character, Goal parent, StringBuilder str)
	{
		base.BuildDebugExtraInfoString(character, parent, str);
		if (OpeningSpeech != null)
		{
			str.Append(' ');
			str.Append('(');
			str.Append(OpeningSpeech.UniqueID);
			str.Append(')');
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (((Target != null) ? Target.Object : null) is Character character2)
		{
			if (character2.Deleted)
			{
				return false;
			}
			if (!character2.AliveAndNotZombie)
			{
				if (OpeningSpeech == null)
				{
					return false;
				}
				if (!OpeningSpeech.ListenerCanBeDead())
				{
					return false;
				}
			}
		}
		else if (OpeningSpeech == null)
		{
			return false;
		}
		if (parent is PrioritiserGoal)
		{
			if (character.IsBeingBittenOrChoked())
			{
				return false;
			}
			if (OpeningSpeech != null && !OpeningSpeech.AIOverridesControl && character.HasBeenPlayerControlledRecently(critical: true, extraCritical: false) && parent is SurvivorGoal)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (SubGoal is SpeakToTarget { Speech: not null } speakToTarget)
		{
			switch (speakToTarget.Speech.Importance)
			{
			case Importance.BlameForAttack:
				return GoalPriority.Survivor_Conversation_Blame;
			case Importance.Critical:
				return GoalPriority.Survivor_Conversation_Critical;
			case Importance.EndFisticuffs:
			case Importance.Uninterruptible:
				return GoalPriority.Survivor_Conversation_Uninterruptible;
			}
		}
		if (SubGoal is ListenToTarget { Speech: not null } listenToTarget)
		{
			switch (listenToTarget.Speech.Importance)
			{
			case Importance.BlameForAttack:
				return GoalPriority.Survivor_Conversation_Blame;
			case Importance.Critical:
				return GoalPriority.Survivor_Conversation_Critical;
			case Importance.EndFisticuffs:
			case Importance.Uninterruptible:
				return GoalPriority.Survivor_Conversation_Uninterruptible;
			}
		}
		if (OpeningSpeech != null)
		{
			switch (OpeningSpeech.Importance)
			{
			case Importance.BlameForAttack:
				return GoalPriority.Survivor_Conversation_Blame;
			case Importance.Critical:
				return GoalPriority.Survivor_Conversation_Critical;
			case Importance.EndFisticuffs:
			case Importance.Uninterruptible:
				return GoalPriority.Survivor_Conversation_Uninterruptible;
			}
			if (!Active && OpeningSpeech.Importance < Importance.Encouragement && !OpeningSpeech.AIOverridesControl)
			{
				return GoalPriority.Survivor_Conversation_Low_Inactive;
			}
		}
		return GoalPriority.Survivor_Conversation;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			Session instance = Session.Instance;
			if (HasUserTarget)
			{
				return Target;
			}
			if (Active && !IsTargetDeletedButNotNull())
			{
				Character targetCharacter = GetTargetCharacter();
				if (targetCharacter != null && instance.GetPlayerControllingCharacter(targetCharacter) == null && instance.GetPlayerControllingCharacter(character) == null && (IsWaitingForReply() || IsWaitingForPlayerToSelectAReply()))
				{
					foreach (PlayerRecord playerRecord in instance.PlayerRecords)
					{
						if (playerRecord.PlayerCharacter != null)
						{
							Conversation conversation = playerRecord.PlayerCharacter.FindActiveGoal(GoalType.Conversation) as Conversation;
							if (playerRecord.PlayerMode == PlayerMode.Controlling && conversation != null && conversation.GetTargetCharacter() == character && conversation.IsSpeaking() && targetCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation2 && (conversation2.IsWaitingForPlayerToSelectAReply() || conversation2.IsWaitingForReply()))
							{
								return character.GetOrCreateTarget(playerRecord.PlayerCharacter);
							}
						}
					}
				}
				return Target;
			}
			OpeningSpeech = null;
			OpeningSpeechObject = null;
			OpeningSpeechParam = default(MemoryParam);
			OpeningSpeechReplyTo = null;
			OpeningSpeechReplyToReferringTo = null;
			OpeningSpeechReplyToParam = default(MemoryParam);
			ControlledByPlayer = false;
			StoryManager instance2 = StoryManager.Instance;
			CommunityManager communityManager = instance.CommunityManager;
			Character mostInterestingSpeaker = instance2.GetMostInterestingSpeaker();
			bool flag = communityManager.PlayerCommunity.IsAnyMemberInCombat();
			if (instance.IsInInfoScreen(character))
			{
				return null;
			}
			for (int i = 0; i < character.QueuedSpeeches.Count; i++)
			{
				QueuedSpeech queuedSpeech = character.QueuedSpeeches[i];
				if (queuedSpeech.Speech == null)
				{
					continue;
				}
				if ((queuedSpeech.Speech.Importance != Importance.EndFisticuffs && mostInterestingSpeaker != null && mostInterestingSpeaker != character && mostInterestingSpeaker != queuedSpeech.Target && (mostInterestingSpeaker.Speaking == null || mostInterestingSpeaker.Speaking.Importance != Importance.Cheer)) || (queuedSpeech.Speech.Importance < Importance.Normal && flag))
				{
					break;
				}
				Conversation conversation3 = ((queuedSpeech.Target != null) ? (queuedSpeech.Target.FindActiveGoal(GoalType.Conversation) as Conversation) : null);
				if ((conversation3 != null && conversation3.GetTargetCharacter() != character) || (queuedSpeech.Target != null && instance.IsInInfoScreen(queuedSpeech.Target)) || ((queuedSpeech.Speech.MustFinish || queuedSpeech.Speech.Importance >= Importance.Encouragement) && instance.IsAnyoneInInfoScreen()))
				{
					break;
				}
				if (queuedSpeech.Target != null && !queuedSpeech.Target.AliveAndNotZombie && !queuedSpeech.Speech.ListenerCanBeDead())
				{
					character.PopQueuedSpeech(queuedSpeech.Speech, queuedSpeech.Target, queuedSpeech.SpeechObject, queuedSpeech.Param);
					i--;
				}
				else if (queuedSpeech.Target != null && character.IsEnemy(queuedSpeech.Target))
				{
					character.PopQueuedSpeech(queuedSpeech.Speech, queuedSpeech.Target, queuedSpeech.SpeechObject, queuedSpeech.Param);
					i--;
				}
				else if (queuedSpeech.Speech.CancelConditions != null && StoryManager.AreAnyConditionsSatisfied(queuedSpeech.Speech.CancelConditions, character, queuedSpeech.Target, queuedSpeech.SpeechObject, queuedSpeech.Param))
				{
					character.PopQueuedSpeech(queuedSpeech.Speech, queuedSpeech.Target, queuedSpeech.SpeechObject, queuedSpeech.Param);
					i--;
				}
				else
				{
					if (queuedSpeech.LastFailedAttemptTime.Ticks != 0L && !(instance.PlayTime - queuedSpeech.LastFailedAttemptTime >= TimeSpan.FromSeconds(4.0)))
					{
						continue;
					}
					if (queuedSpeech.Speech.Importance < Importance.Critical && queuedSpeech.Speech.Importance != Importance.EndFisticuffs)
					{
						if (!queuedSpeech.Speech.AIOverridesControl && !character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None))
						{
							return null;
						}
						if (character.IsControllableByPlayer() && MusicManager.AreAnyEnemiesNearby(character, 8f, checkAccessible: true))
						{
							return null;
						}
					}
					if ((queuedSpeech.Speech.AIOverridesControl || !character.HasBeenPlayerControlledRecently(critical: true, extraCritical: false) || !(parent is SurvivorGoal)) && (character.SparringPartner == null || queuedSpeech.Speech.Importance == Importance.EndFisticuffs || !character.HasQueuedSpeechOfImportance(Importance.EndFisticuffs)))
					{
						OpeningSpeech = queuedSpeech.Speech;
						OpeningSpeechObject = queuedSpeech.SpeechObject;
						OpeningSpeechParam = queuedSpeech.Param;
						return (queuedSpeech.Target != null) ? character.GetOrCreateTarget(queuedSpeech.Target) : null;
					}
				}
			}
			if (StoryManager.Instance.HasQueuedEventsForThisFrame())
			{
				return null;
			}
			Target target = null;
			float num = 1E+38f;
			bool flag2 = false;
			bool flag3 = false;
			bool wasTriggeredFromDirectControl = false;
			Speech speech = null;
			BaseObject openingSpeechObject = null;
			MemoryParam openingSpeechParam = default(MemoryParam);
			int num2 = 0;
			for (int j = 0; j < character.Targets.Count; j++)
			{
				Target target2 = character.Targets[j];
				if (target2.Object != null && target2.Object.Id == LastProcessedTargetId)
				{
					num2 = j + ((LastProcessedTargetIdFrame != Session.Instance.Frame) ? 1 : 0);
					break;
				}
			}
			bool flag4 = false;
			for (int k = 0; k < character.Targets.Count; k++)
			{
				Target target3 = character.Targets[(k + num2) % character.Targets.Count];
				if (target3.Object == null || target3.Object.Deleted || !(target3.Object is Character { Zombie: false } character2) || character2.GetBaseObjectType() != BaseObjectType.Human || character2.Consciousness == Consciousness.Unconscious || target3.GetFlag(TargetFlags.Lost) || target3.Camouflage > 0f)
				{
					continue;
				}
				float num3 = float.MaxValue;
				Speech speech2 = null;
				BaseObject baseObject = null;
				MemoryParam memoryParam = default(MemoryParam);
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				Conversation conversation4 = character2.FindActiveGoal(GoalType.Conversation) as Conversation;
				if (conversation4 != null)
				{
					if (conversation4.Target != null && conversation4.Target.Object == character)
					{
						Conversation conversationWith = GameCursor.GetConversationWith(character2, character);
						if (conversationWith != null && conversationWith != this)
						{
							continue;
						}
						if (!conversation4.IsSpeaking() && !conversation4.IsWaitingForReply())
						{
							flag5 = true;
						}
						if (flag5 && character.InsideBuilding != null && (character.PosXZ - character2.PosXZ).magnitude <= character.InsideBuilding.GetBoundingBox().extents.magnitude + 2f)
						{
							flag5 = false;
						}
						num3 = (character.PosXZ - character2.PosXZ).magnitude + (float)target3.TimeSinceLastVisible.TotalSeconds;
						flag7 = character2.CurrentActionAnim == ActionAnim.HandsUp;
						speech2 = null;
						if (conversation4.OpeningSpeech != null && conversation4.OpeningSpeech.AIOverridesListenerControl)
						{
							num3 = 0f;
							flag6 = true;
							flag5 = false;
						}
					}
				}
				else
				{
					MoveToAndInteractGoal moveToAndInteractGoal = character2.FindActiveGoal(GoalType.MoveToAndInteractGoal) as MoveToAndInteractGoal;
					if (moveToAndInteractGoal == null)
					{
						moveToAndInteractGoal = character2.GetLeaderCommandEvenIfItIsInactive() as MoveToAndInteractGoal;
					}
					if (moveToAndInteractGoal != null)
					{
						if (moveToAndInteractGoal.Target != null && moveToAndInteractGoal.Target.Object == character)
						{
							num3 = 0f;
							speech2 = null;
						}
					}
					else
					{
						if (flag4 || instance.GetPlayerControllingCharacter(character) != null || character.Speaking != null || (mostInterestingSpeaker != null && mostInterestingSpeaker != character && mostInterestingSpeaker != character2) || (character.HasMovementZone() && !character.MovementZone.Contains(character2.Tile)))
						{
							continue;
						}
						SpeechSituation speechSituation = SpeechSituation.Opener;
						if (!character2.Alive)
						{
							if (!WantPayRespectsOnCorpse(character, character2) || character2.CarriedBy != null)
							{
								continue;
							}
							speechSituation = SpeechSituation.PayRespects;
							if (target3.GetFlag(TargetFlags.HasPaidRespects))
							{
								continue;
							}
						}
						else if (character2.Zombie)
						{
							continue;
						}
						if (character2.Consciousness == Consciousness.Unconscious || character2.InCombat || !character2.IsOutdoors())
						{
							continue;
						}
						CommunityRelationshipType communityRelationshipType = ((character.Community != character2.Community) ? communityManager.GetRelationship(character.Community, character2.Community) : CommunityRelationshipType.Unknown);
						if (communityRelationshipType == CommunityRelationshipType.Hostile)
						{
							continue;
						}
						float maxRangeForOpener = GetMaxRangeForOpener(character, target3, communityRelationshipType);
						if ((character.PosXZ - character2.PosXZ).sqrMagnitude >= maxRangeForOpener * maxRangeForOpener || instance.IsInInfoScreen(character2) || character2.SparringPartner != null || character2.GetLeaderCommand() is MoveToAndInteractGoal)
						{
							continue;
						}
						BaseObject obj = null;
						MemoryParam param = default(MemoryParam);
						Speech speechForSituation;
						using (new ProfileMarker((speechSituation == SpeechSituation.PayRespects) ? GetPayRespectsTimer : GetOpenerTimer))
						{
							speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, character2, ref obj, speechSituation, ref param);
							LastProcessedTargetId = character2.Id;
							LastProcessedTargetIdFrame = Session.Instance.Frame;
							flag4 = true;
							CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
						}
						if (speechForSituation == null || ((character.IsControllableByPlayer() || character2.IsControllableByPlayer() || speechForSituation.MustFinish || speechForSituation.Importance >= Importance.Encouragement) && instance.IsAnyoneInInfoScreen()) || (flag && speechForSituation.Importance <= Importance.Normal) || character2.FindActiveGoal(GoalType.Conversation) != null)
						{
							continue;
						}
						bool flag8 = false;
						foreach (Target target4 in character.Targets)
						{
							if (target4 != target3 && target4.Object is Character character3)
							{
								character3.FindActiveGoal(GoalType.Conversation);
								if (conversation4 != null && conversation4.Target == target3)
								{
									flag8 = true;
									break;
								}
							}
						}
						if (flag8 || character2.HasQueuedSpeechWith(character))
						{
							continue;
						}
						if (character.GetCurrentActionPriority() > ActionPriority.Idle && !character.IsActionAnimLooped())
						{
							return null;
						}
						num3 = (character.PosXZ - character2.PosXZ).magnitude + (float)target3.TimeSinceLastVisible.TotalSeconds + 1000f;
						speech2 = speechForSituation;
						baseObject = obj;
						memoryParam = param;
					}
				}
				if (target3 == Target && Active)
				{
					num3 *= 0.25f;
				}
				if (num3 < num)
				{
					target = target3;
					num = num3;
					flag2 = flag5;
					flag3 = flag6;
					wasTriggeredFromDirectControl = flag7;
					speech = speech2;
					openingSpeechObject = baseObject;
					openingSpeechParam = memoryParam;
				}
			}
			if (target == null && (mostInterestingSpeaker == null || mostInterestingSpeaker == character))
			{
				BaseObject obj2 = null;
				MemoryParam param2 = default(MemoryParam);
				Speech speechForSituation2;
				using (new ProfileMarker(GetOpenerToSelfTimer))
				{
					speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, ref obj2, SpeechSituation.OpenerToSelf, ref param2);
				}
				if (speechForSituation2 != null)
				{
					if ((character.IsControllableByPlayer() || speechForSituation2.MustFinish || speechForSituation2.Importance >= Importance.Encouragement) && instance.IsAnyoneInInfoScreen())
					{
						return null;
					}
					if (character.GetCurrentActionPriority() > ActionPriority.Idle && !character.IsActionAnimLooped())
					{
						return null;
					}
					if (!character.IsOutdoors() && speechForSituation2.Importance <= Importance.Low)
					{
						return null;
					}
					speech = speechForSituation2;
					openingSpeechObject = obj2;
					openingSpeechParam = param2;
					flag3 = speechForSituation2.AIOverridesControl;
				}
			}
			if (!flag3 && !character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None, wasTriggeredFromDirectControl, isSpeechGoal: true))
			{
				return null;
			}
			if (flag2)
			{
				return null;
			}
			if (!character.IsOutdoors() && character.IsControllableByPlayer() && speech != null && AreEnemiesNearby(character))
			{
				return null;
			}
			if (speech != null && !speech.AIOverridesControl && character.HasBeenPlayerControlledRecently(critical: true, extraCritical: false) && parent is SurvivorGoal)
			{
				return null;
			}
			OpeningSpeech = speech;
			OpeningSpeechObject = openingSpeechObject;
			OpeningSpeechParam = openingSpeechParam;
			return target;
		}
	}

	private bool AreEnemiesNearby(Character character)
	{
		int num = 40;
		TileObject tileObject = ((character.InsideBuilding != null) ? ((MultiTileObject)character.InsideBuilding) : ((MultiTileObject)character));
		GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(tileObject.GetMinTile() - new TerrainCoord(num, num), tileObject.GetMaxTile() + new TerrainCoord(num, num), NearbyCharacters);
		foreach (Character nearbyCharacter in NearbyCharacters)
		{
			if (!(nearbyCharacter.Tile.GetDistSquared(tileObject.GetTileRect().GetNearestCoordWithinRectTo(nearbyCharacter.Tile)) > (float)(num * num)) && character.IsEnemy(nearbyCharacter))
			{
				NearbyCharacters.Clear();
				return true;
			}
		}
		NearbyCharacters.Clear();
		return false;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}
}
