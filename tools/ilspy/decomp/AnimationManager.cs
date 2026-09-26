using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager
{
	public static AnimationManager Instance;

	public Resource<RuntimeAnimatorController> UnityAnimatorController;

	public Resource<RuntimeAnimatorController> UnityBowController;

	public Resource<RuntimeAnimatorController> UnityZombieAnimatorController;

	public Resource<RuntimeAnimatorController> UnityRabbitAnimatorController;

	public Resource<RuntimeAnimatorController> UnityDeerStagAnimatorController;

	public Resource<RuntimeAnimatorController> UnityDeerDoeAnimatorController;

	public Resource<RuntimeAnimatorController> UnityRoosterAnimatorController;

	public Resource<RuntimeAnimatorController> UnityHenAnimatorController;

	public Resource<RuntimeAnimatorController> UnityChickAnimatorController;

	public List<AnimWrapper>[] Anims = new List<AnimWrapper>[164];

	public SortedList<int, AnimWrapper> AnimsByName = new SortedList<int, AnimWrapper>();

	private ActionAnim[] AllDamagedAnims = new ActionAnim[10]
	{
		ActionAnim.Damaged_Torso_FromCentre,
		ActionAnim.Damaged_Torso_FromRight,
		ActionAnim.Damaged_Torso_FromLeft,
		ActionAnim.Damaged_Torso_FromBehind,
		ActionAnim.Damaged_Head_FromCentre,
		ActionAnim.Damaged_Head_FromRight,
		ActionAnim.Damaged_Head_FromLeft,
		ActionAnim.Damaged_Head_FromBehind,
		ActionAnim.Damaged_LeftLeg,
		ActionAnim.Damaged_RightLeg
	};

	private ActionAnim[] AllGetUpAnims = new ActionAnim[2]
	{
		ActionAnim.GetUp_Front,
		ActionAnim.GetUp_Back
	};

	private static List<AnimWrapper> tmp1 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp2 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp3 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp4 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp5 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp6 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp7 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp8 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp9 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp10 = new List<AnimWrapper>();

	private static List<AnimWrapper> tmp11 = new List<AnimWrapper>();

	public AnimationManager()
	{
		Instance = this;
	}

	public void Init()
	{
	}

	public void Unload()
	{
		Instance = null;
	}

	private AnimationClip FindClip(BaseObjectType species, GenderType gender, bool youth, bool zombie, string name)
	{
		RuntimeAnimatorController runtimeAnimatorController = null;
		switch (species)
		{
		case BaseObjectType.Human:
			runtimeAnimatorController = (zombie ? UnityZombieAnimatorController : UnityAnimatorController);
			break;
		case BaseObjectType.Rabbit:
			runtimeAnimatorController = UnityRabbitAnimatorController;
			break;
		case BaseObjectType.Deer:
			runtimeAnimatorController = ((gender == GenderType.Male) ? UnityDeerStagAnimatorController : UnityDeerDoeAnimatorController);
			break;
		case BaseObjectType.Chicken:
			runtimeAnimatorController = (youth ? UnityChickAnimatorController : ((gender == GenderType.Male) ? UnityRoosterAnimatorController : UnityHenAnimatorController));
			break;
		default:
			Debug.LogError("Unknown species: " + species);
			return null;
		}
		AnimationClip[] animationClips = runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name == name)
			{
				return animationClip;
			}
		}
		return null;
	}

	private AnimWrapper AddActionAnim(ActionAnim action, string clipName, string baseStateName)
	{
		return AddActionAnim(action, clipName, baseStateName, null);
	}

	private AnimWrapper AddActionAnim(ActionAnim action, string clipName, string baseStateName, string upperBodyStateName)
	{
		int animHash = GetAnimHash(action, baseStateName);
		if (AnimsByName.ContainsKey(animHash))
		{
			Debug.LogError("There is already a " + action.ToString() + " anim named " + baseStateName);
		}
		AnimWrapper animWrapper = new AnimWrapper();
		animWrapper.Action = action;
		animWrapper.BaseStateName = baseStateName;
		animWrapper.BaseStateNameHash = Animator.StringToHash(baseStateName);
		animWrapper.UpperBodyStateName = upperBodyStateName;
		animWrapper.UpperBodyStateNameHash = ((upperBodyStateName != null) ? Animator.StringToHash(upperBodyStateName) : 0);
		animWrapper.ClipName = clipName;
		if (Anims[(int)action] == null)
		{
			Anims[(int)action] = new List<AnimWrapper>();
		}
		Anims[(int)action].Add(animWrapper);
		AnimsByName[animHash] = animWrapper;
		return animWrapper;
	}

	public void LoadContent()
	{
		UnityAnimatorController = new Resource<RuntimeAnimatorController>("Animations/AnimatorController");
		UnityBowController = new Resource<RuntimeAnimatorController>("Animations/BowAnimatorController");
		UnityZombieAnimatorController = new Resource<RuntimeAnimatorController>("Animations/ZombieAnimatorController");
		UnityRabbitAnimatorController = new Resource<RuntimeAnimatorController>("Animations/RabbitAnimatorController");
		UnityDeerStagAnimatorController = new Resource<RuntimeAnimatorController>("Animations/DeerStagAnimatorController");
		UnityDeerDoeAnimatorController = new Resource<RuntimeAnimatorController>("Animations/DeerDoeAnimatorController");
		UnityRoosterAnimatorController = new Resource<RuntimeAnimatorController>("Animations/RoosterAnimatorController");
		UnityHenAnimatorController = new Resource<RuntimeAnimatorController>("Animations/HenAnimatorController");
		UnityChickAnimatorController = new Resource<RuntimeAnimatorController>("Animations/ChickAnimatorController");
		AnimWrapper animWrapper = AddActionAnim(ActionAnim.Drunk, "Zombie_Idle_1-v2", "Drunk");
		animWrapper.Looped = true;
		animWrapper.TransitionInTime = 1f;
		animWrapper.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper2 = AddActionAnim(ActionAnim.VeryDrunk, "Zombie_Idle_4", "VeryDrunk");
		animWrapper2.HasRootMotion = true;
		animWrapper2.Looped = true;
		animWrapper2.TransitionInTime = 1f;
		animWrapper2.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper3 = AddActionAnim(ActionAnim.HandsUp, "HandsUp", "HandsUp");
		animWrapper3.Looped = true;
		animWrapper3.PipView = PipAnimView.MediumShot;
		AnimWrapper animWrapper4 = AddActionAnim(ActionAnim.PunchLeft, "Unarmed_SlowPunch_L", "UnarmedPunchLeft");
		animWrapper4.OnlyAgainstDirectControlled = true;
		animWrapper4.Aiming = true;
		animWrapper4.TargetRange = 1f;
		animWrapper4.Events.Add(new AnimEvent(AnimationEventType.PunchLeft, 0.5f));
		AnimWrapper animWrapper5 = AddActionAnim(ActionAnim.PunchLeft, "Unarmed_SlowPunchMove_L", "UnarmedMoveAndPunchLeft");
		animWrapper5.OnlyAgainstDirectControlled = true;
		animWrapper5.Aiming = true;
		animWrapper5.TargetRange = 1.25f;
		animWrapper5.Events.Add(new AnimEvent(AnimationEventType.PunchLeft, 0.5f));
		animWrapper5.HasRootMotion = true;
		AnimWrapper animWrapper6 = AddActionAnim(ActionAnim.PunchRight, "Unarmed_SlowPunch_R", "UnarmedPunchRight");
		animWrapper6.OnlyAgainstDirectControlled = true;
		animWrapper6.Aiming = true;
		animWrapper6.TargetRange = 1f;
		animWrapper6.Events.Add(new AnimEvent(AnimationEventType.PunchRight, 0.5f));
		AnimWrapper animWrapper7 = AddActionAnim(ActionAnim.PunchRight, "Unarmed_SlowPunchMove_R", "UnarmedMoveAndPunchRight");
		animWrapper7.OnlyAgainstDirectControlled = true;
		animWrapper7.Aiming = true;
		animWrapper7.TargetRange = 1.25f;
		animWrapper7.Events.Add(new AnimEvent(AnimationEventType.PunchRight, 0.5f));
		animWrapper7.HasRootMotion = true;
		AnimWrapper animWrapper8 = AddActionAnim(ActionAnim.PunchLeft, "Fists_Punch_L", "UnarmedPunchLeftFast");
		animWrapper8.NotAgainstDirectControlled = true;
		animWrapper8.Aiming = true;
		animWrapper8.TargetRange = 1f;
		animWrapper8.Events.Add(new AnimEvent(AnimationEventType.PunchLeft, 0.16f));
		AnimWrapper animWrapper9 = AddActionAnim(ActionAnim.PunchLeft, "Fists_Punch_Move_L", "UnarmedMoveAndPunchLeftFast");
		animWrapper9.NotAgainstDirectControlled = true;
		animWrapper9.Aiming = true;
		animWrapper9.TargetRange = 1.25f;
		animWrapper9.Events.Add(new AnimEvent(AnimationEventType.PunchLeft, 0.28f));
		animWrapper9.HasRootMotion = true;
		AnimWrapper animWrapper10 = AddActionAnim(ActionAnim.PunchRight, "Fists_Punch_R", "UnarmedPunchRightFast");
		animWrapper10.NotAgainstDirectControlled = true;
		animWrapper10.Aiming = true;
		animWrapper10.TargetRange = 1f;
		animWrapper10.Events.Add(new AnimEvent(AnimationEventType.PunchRight, 0.16f));
		AnimWrapper animWrapper11 = AddActionAnim(ActionAnim.PunchRight, "Fists_Punch_Move_R", "UnarmedMoveAndPunchRightFast");
		animWrapper11.NotAgainstDirectControlled = true;
		animWrapper11.Aiming = true;
		animWrapper11.TargetRange = 1.25f;
		animWrapper11.Events.Add(new AnimEvent(AnimationEventType.PunchRight, 0.3f));
		animWrapper11.HasRootMotion = true;
		AnimWrapper animWrapper12 = AddActionAnim(ActionAnim.Kick, "Unarmed_SlowKick_L", "UnarmedKick");
		animWrapper12.Aiming = true;
		animWrapper12.TargetRange = 1f;
		animWrapper12.Events.Add(new AnimEvent(AnimationEventType.KickLeft, 0.5f));
		animWrapper12.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper13 = AddActionAnim(ActionAnim.Kick, "Fists_Kick_Front_Move_R", "UnarmedMoveAndKick");
		animWrapper13.Aiming = true;
		animWrapper13.TargetRange = 1.5f;
		animWrapper13.InterruptibleFrac = 0.7f;
		animWrapper13.Speed = 0.875f;
		animWrapper13.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.3f));
		animWrapper13.HasRootMotion = true;
		animWrapper13.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper14 = AddActionAnim(ActionAnim.Kick, "Unarmed_KickOnGround", "UnarmedKickOnGround");
		animWrapper14.Aiming = true;
		animWrapper14.TargetOnGround = true;
		animWrapper14.TargetRange = 1f;
		animWrapper14.InterruptibleFrac = 0.8f;
		animWrapper14.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.42f));
		animWrapper14.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper15 = AddActionAnim(ActionAnim.Block_Punch, "Fists_Block", "UnarmedBlockPunch");
		animWrapper15.Aiming = true;
		animWrapper15.InterruptibleFrac = 0.75f;
		animWrapper15.TargetBodyLocation = TargettableBodyLocation.Torso;
		AnimWrapper animWrapper16 = AddActionAnim(ActionAnim.Block_Punch, "Fists_Block", "UnarmedBlockPunchDown");
		animWrapper16.Aiming = true;
		animWrapper16.InterruptibleFrac = 0.75f;
		animWrapper16.TargetBodyLocation = TargettableBodyLocation.Legs;
		AnimWrapper animWrapper17 = AddActionAnim(ActionAnim.Block_Punch, "Fists_BlockUp", "UnarmedBlockPunchUp");
		animWrapper17.Aiming = true;
		animWrapper17.InterruptibleFrac = 0.75f;
		animWrapper17.TargetBodyLocation = TargettableBodyLocation.Head;
		AddActionAnim(ActionAnim.Block_Kick, "Unarmed_BlockKick", "UnarmedBlockKick").Aiming = true;
		AnimWrapper animWrapper18 = AddActionAnim(ActionAnim.Dodge_Backwards, "Sword1h_Dodge", "UnarmedDodgeBackwards");
		animWrapper18.HasRootMotion = true;
		animWrapper18.Aiming = true;
		animWrapper18.InterruptibleFrac = 0.4f;
		animWrapper18.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper19 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "UnarmedDodgeForwards");
		animWrapper19.HasRootMotion = true;
		animWrapper19.Aiming = true;
		animWrapper19.CanTurnDuringAnim = false;
		animWrapper19.InterruptibleFrac = 0.7f;
		animWrapper19.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper20 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "UnarmedDodgeLeft");
		animWrapper20.HasRootMotion = true;
		animWrapper20.Aiming = true;
		animWrapper20.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper21 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "UnarmedDodgeRight");
		animWrapper21.HasRootMotion = true;
		animWrapper21.Aiming = true;
		animWrapper21.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper22 = AddActionAnim(ActionAnim.GetUp_Back, "GetUpFromBack", "GetUpBack");
		animWrapper22.CanTurnDuringAnim = false;
		animWrapper22.TransitionInTime = 0f;
		animWrapper22.Speed = 2f;
		AnimWrapper animWrapper23 = AddActionAnim(ActionAnim.GetUp_Front, "GetUpFromFace", "GetUpFront");
		animWrapper23.CanTurnDuringAnim = false;
		animWrapper23.TransitionInTime = 0f;
		animWrapper23.Speed = 2f;
		for (int i = 0; i < 2; i++)
		{
			bool flag = i == 1;
			string text = (flag ? "ZombiePlayDead" : "PlayDead");
			for (int j = 1; j <= 10; j++)
			{
				AnimWrapper animWrapper24 = AddActionAnim(ActionAnim.PlayDead, "Srv_Dead" + j, text + j);
				animWrapper24.Looped = true;
				animWrapper24.Zombie = flag;
				animWrapper24.CanTurnDuringAnim = false;
				animWrapper24.TransitionInTime = 0f;
				animWrapper24.PlayDeadOnFront = j == 3 || j == 4;
			}
		}
		ActionAnim[] array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Head_FromLeft,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action in array)
		{
			AnimWrapper animWrapper25 = AddActionAnim(action, "Idle_Hit_Strong_Right", "IdleHitHeadFromLeft");
			animWrapper25.HasRootMotion = true;
			animWrapper25.Speed = 1.5f;
			animWrapper25.InterruptibleFrac = 0.375f;
			animWrapper25.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Head_FromRight,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action2 in array)
		{
			AnimWrapper animWrapper26 = AddActionAnim(action2, "Idle_Hit_Strong_Left", "IdleHitHeadFromRight");
			animWrapper26.HasRootMotion = true;
			animWrapper26.Speed = 1.5f;
			animWrapper26.InterruptibleFrac = 0.375f;
			animWrapper26.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper27 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "IdleHitFromBehind");
		animWrapper27.HasRootMotion = true;
		animWrapper27.Speed = 1.5f;
		animWrapper27.InterruptibleFrac = 0.375f;
		animWrapper27.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper28 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "IdleHitHeadFromBehind");
		animWrapper28.HasRootMotion = true;
		animWrapper28.Speed = 1.5f;
		animWrapper28.InterruptibleFrac = 0.375f;
		animWrapper28.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Head_FromLeft,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action3 in array)
		{
			AnimWrapper animWrapper29 = AddActionAnim(action3, "Fists_Hit_Right", "UnarmedHitHeadFromLeft");
			animWrapper29.HasRootMotion = true;
			animWrapper29.Aiming = true;
			animWrapper29.Speed = 1.5f;
			animWrapper29.InterruptibleFrac = 0.375f;
			animWrapper29.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Head_FromRight,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action4 in array)
		{
			AnimWrapper animWrapper30 = AddActionAnim(action4, "Fists_Hit_Left", "UnarmedHitHeadFromRight");
			animWrapper30.HasRootMotion = true;
			animWrapper30.Aiming = true;
			animWrapper30.Speed = 1.5f;
			animWrapper30.InterruptibleFrac = 0.375f;
			animWrapper30.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper31 = AddActionAnim(ActionAnim.Damaged_Torso_FromCentre, "Sword1h_Hit_Torso_Front", "UnarmedHitTorsoFromCentre");
		animWrapper31.HasRootMotion = true;
		animWrapper31.Aiming = true;
		animWrapper31.Speed = 1.5f;
		animWrapper31.InterruptibleFrac = 0.375f;
		animWrapper31.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper32 = AddActionAnim(ActionAnim.Damaged_Torso_FromRight, "Sword1h_Hit_Torso_Left", "UnarmedHitTorsoFromRight");
		animWrapper32.HasRootMotion = true;
		animWrapper32.Aiming = true;
		animWrapper32.Speed = 1.5f;
		animWrapper32.InterruptibleFrac = 0.375f;
		animWrapper32.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper33 = AddActionAnim(ActionAnim.Damaged_Torso_FromLeft, "Sword1h_Hit_Torso_Right", "UnarmedHitTorsoFromLeft");
		animWrapper33.HasRootMotion = true;
		animWrapper33.Aiming = true;
		animWrapper33.Speed = 1.5f;
		animWrapper33.InterruptibleFrac = 0.375f;
		animWrapper33.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper34 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "UnarmedHitFromBehind");
		animWrapper34.HasRootMotion = true;
		animWrapper34.Aiming = true;
		animWrapper34.Speed = 1.5f;
		animWrapper34.InterruptibleFrac = 0.375f;
		animWrapper34.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper35 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "UnarmedHitHeadFromBehind");
		animWrapper35.HasRootMotion = true;
		animWrapper35.Aiming = true;
		animWrapper35.Speed = 1.5f;
		animWrapper35.InterruptibleFrac = 0.375f;
		animWrapper35.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper36 = AddActionAnim(ActionAnim.Panic, "Srv_OnFire_Panic1_Looping", "Panic1");
		animWrapper36.HasRootMotion = true;
		animWrapper36.Looped = true;
		animWrapper36.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper37 = AddActionAnim(ActionAnim.Panic, "Srv_OnFire_Panic2_Looping", "Panic2");
		animWrapper37.HasRootMotion = true;
		animWrapper37.Looped = true;
		animWrapper37.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper38 = AddActionAnim(ActionAnim.Slide, "SlideLoop", "Slide");
		animWrapper38.Looped = true;
		animWrapper38.TransitionInTime = 0.25f;
		animWrapper38.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper39 = AddActionAnim(ActionAnim.SlideRecover, "SlideRecover", "SlideRecover");
		animWrapper39.HasRootMotion = true;
		animWrapper39.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper40 = AddActionAnim(ActionAnim.Vault, "Vault1m", "UnarmedVault");
		animWrapper40.HasRootMotion = true;
		animWrapper40.TransitionInTime = 0.25f;
		animWrapper40.VaultLoopStartFrac = 0.2f;
		animWrapper40.VaultLoopEndFrac = 0.38f;
		animWrapper40.InterruptibleFrac = 0.5f;
		animWrapper40.Speed = 1.5f;
		animWrapper40.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper41 = AddActionAnim(ActionAnim.Scavenge, "Scavenge", "Scavenge");
		animWrapper41.Events.Add(new AnimEvent(AnimationEventType.Take, 0.5f));
		animWrapper41.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper42 = AddActionAnim(ActionAnim.Scavenge, "ScavengeCrouch", "ScavengeCrouch");
		animWrapper42.Crouching = true;
		animWrapper42.Events.Add(new AnimEvent(AnimationEventType.Take, 0.5f));
		animWrapper42.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper43 = AddActionAnim(ActionAnim.ScavengeCorpse, "ScavengeCorpse", "ScavengeCorpse");
		animWrapper43.Events.Add(new AnimEvent(AnimationEventType.Take, 0.5f));
		animWrapper43.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper44 = AddActionAnim(ActionAnim.ScavengeCorpse, "ScavengeCorpseCrouch", "ScavengeCorpseCrouch");
		animWrapper44.Crouching = true;
		animWrapper44.Events.Add(new AnimEvent(AnimationEventType.Take, 0.5f));
		animWrapper44.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper45 = AddActionAnim(ActionAnim.ScavengeStart, "ScavengeStart", "ScavengeStart");
		animWrapper45.Events.Add(new AnimEvent(AnimationEventType.Take, 0.99f));
		animWrapper45.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper46 = AddActionAnim(ActionAnim.ScavengeStart, "ScavengeCrouchStart", "ScavengeCrouchStart");
		animWrapper46.Crouching = true;
		animWrapper46.Events.Add(new AnimEvent(AnimationEventType.Take, 0.99f));
		animWrapper46.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper47 = AddActionAnim(ActionAnim.ScavengeLoop, "ScavengeLoop", "ScavengeLoop");
		animWrapper47.Looped = true;
		animWrapper47.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper48 = AddActionAnim(ActionAnim.ScavengeLoop, "ScavengeCrouchLoop", "ScavengeCrouchLoop");
		animWrapper48.Crouching = true;
		animWrapper48.Looped = true;
		animWrapper48.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.ScavengeFinish, "ScavengeEnd", "ScavengeEnd").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper49 = AddActionAnim(ActionAnim.ScavengeFinish, "ScavengeCrouchEnd", "ScavengeCrouchEnd");
		animWrapper49.Crouching = true;
		animWrapper49.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper50 = AddActionAnim(ActionAnim.ScavengeCorpseStart, "ScavengeCorpseStart", "ScavengeCorpseStart");
		animWrapper50.Events.Add(new AnimEvent(AnimationEventType.Take, 0.99f));
		animWrapper50.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper51 = AddActionAnim(ActionAnim.ScavengeCorpseStart, "ScavengeCorpseCrouchStart", "ScavengeCorpseCrouchStart");
		animWrapper51.Crouching = true;
		animWrapper51.Events.Add(new AnimEvent(AnimationEventType.Take, 0.99f));
		animWrapper51.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper52 = AddActionAnim(ActionAnim.ScavengeCorpseLoop, "ScavengeCorpseLoop", "ScavengeCorpseLoop");
		animWrapper52.Looped = true;
		animWrapper52.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper53 = AddActionAnim(ActionAnim.ScavengeCorpseLoop, "ScavengeCorpseLoop", "ScavengeCorpseCrouchLoop");
		animWrapper53.Crouching = true;
		animWrapper53.Looped = true;
		animWrapper53.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.ScavengeCorpseFinish, "ScavengeCorpseEnd", "ScavengeCorpseEnd").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper54 = AddActionAnim(ActionAnim.ScavengeCorpseFinish, "ScavengeCorpseCrouchEnd", "ScavengeCorpseCrouchEnd");
		animWrapper54.Crouching = true;
		animWrapper54.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper55 = AddActionAnim(ActionAnim.Eat, "Srv_Eat", "Eat");
		animWrapper55.Events.Add(new AnimEvent(AnimationEventType.StartEat, 0.15f));
		animWrapper55.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.65f));
		AnimWrapper animWrapper56 = AddActionAnim(ActionAnim.Eat, "CrouchEat", "CrouchEat");
		animWrapper56.Crouching = true;
		animWrapper56.Events.Add(new AnimEvent(AnimationEventType.StartEat, 0.15f));
		animWrapper56.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.65f));
		AnimWrapper animWrapper57 = AddActionAnim(ActionAnim.Eat, "Srv_SitBonfire_Eat", "EatSitting");
		animWrapper57.Sitting = true;
		animWrapper57.Events.Add(new AnimEvent(AnimationEventType.StartEat, 0.15f));
		animWrapper57.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.67f));
		AnimWrapper animWrapper58 = AddActionAnim(ActionAnim.Drink, "Srv_DrinkBottle", "Drink");
		animWrapper58.Events.Add(new AnimEvent(AnimationEventType.StartDrink, 0.3f));
		animWrapper58.Events.Add(new AnimEvent(AnimationEventType.FinishDrink, 0.55f));
		animWrapper58.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper59 = AddActionAnim(ActionAnim.Drink, "CrouchDrinkBottle", "CrouchDrink");
		animWrapper59.Crouching = true;
		animWrapper59.Events.Add(new AnimEvent(AnimationEventType.StartDrink, 0.3f));
		animWrapper59.Events.Add(new AnimEvent(AnimationEventType.FinishDrink, 0.55f));
		AnimWrapper animWrapper60 = AddActionAnim(ActionAnim.Drink, "Srv_SitBonfire_DrinkFromBottle", "DrinkSitting");
		animWrapper60.Sitting = true;
		animWrapper60.Events.Add(new AnimEvent(AnimationEventType.StartDrink, 0.28f));
		animWrapper60.Events.Add(new AnimEvent(AnimationEventType.FinishDrink, 0.55f));
		AnimWrapper animWrapper61 = AddActionAnim(ActionAnim.DrinkFromRiver, "Srv_DrinkPuddle", "DrinkFromRiver");
		animWrapper61.Events.Add(new AnimEvent(AnimationEventType.StartDrink, 0.46f));
		animWrapper61.Events.Add(new AnimEvent(AnimationEventType.FinishDrink, 0.69f));
		animWrapper61.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper62 = AddActionAnim(ActionAnim.Pee, "Pee_Male", "PeeMale");
		animWrapper62.Events.Add(new AnimEvent(AnimationEventType.PeeingSound, 0.25f));
		animWrapper62.Events.Add(new AnimEvent(AnimationEventType.FinishPeeing, 0.75f));
		animWrapper62.PipView = PipAnimView.FullBodySide;
		animWrapper62.Gender = GenderType.Male;
		AnimWrapper animWrapper63 = AddActionAnim(ActionAnim.Pee, "Pee_Female", "PeeFemale");
		animWrapper63.Events.Add(new AnimEvent(AnimationEventType.PeeingSound, 0.25f));
		animWrapper63.Events.Add(new AnimEvent(AnimationEventType.FinishPeeing, 0.75f));
		animWrapper63.PipView = PipAnimView.FullBodySide;
		animWrapper63.Gender = GenderType.Female;
		AddActionAnim(ActionAnim.ReadStart, "ReadStart", "ReadStart");
		AnimWrapper animWrapper64 = AddActionAnim(ActionAnim.ReadLoop, "ReadLoop", "ReadLoop");
		animWrapper64.Looped = true;
		animWrapper64.DontRestartIfAlreadyPLaying = true;
		AddActionAnim(ActionAnim.ReadFinish, "ReadFinish", "ReadFinish").Events.Add(new AnimEvent(AnimationEventType.FinishReading, 0.5f));
		AddActionAnim(ActionAnim.ReadStart, "SittingReadStart", "SittingReadStart").Sitting = true;
		AnimWrapper animWrapper65 = AddActionAnim(ActionAnim.ReadLoop, "SittingReadLoop", "SittingReadLoop");
		animWrapper65.Looped = true;
		animWrapper65.Sitting = true;
		animWrapper65.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper66 = AddActionAnim(ActionAnim.ReadFinish, "SittingReadFinish", "SittingReadFinish");
		animWrapper66.Sitting = true;
		animWrapper66.Events.Add(new AnimEvent(AnimationEventType.FinishReading, 0.5f));
		AnimWrapper animWrapper67 = AddActionAnim(ActionAnim.SitByFire, "Srv_SitBonfireStart", "SitByFire");
		animWrapper67.HasRootMotion = true;
		animWrapper67.Events.Add(new AnimEvent(AnimationEventType.StartSitting, 0.5f));
		animWrapper67.PipView = PipAnimView.FullBodySide;
		for (int l = 0; l < 2; l++)
		{
			bool flag2 = l == 1;
			AnimWrapper animWrapper68 = AddActionAnim(ActionAnim.StopSittingByFire, "Srv_SitBonfireStop", flag2 ? "ZombieStopSittingByFire" : "StopSittingByFire");
			animWrapper68.HasRootMotion = true;
			animWrapper68.Zombie = flag2;
			animWrapper68.Speed = 1.5f;
			animWrapper68.CanTurnDuringAnim = false;
			animWrapper68.PipView = PipAnimView.FullBodySide;
			animWrapper68.Events.Add(new AnimEvent(AnimationEventType.StopSitting, 0.5f));
		}
		AnimWrapper animWrapper69 = AddActionAnim(ActionAnim.Skin, "Srv_SkinPrey1", "Skin");
		animWrapper69.HasRootMotion = true;
		animWrapper69.Events.Add(new AnimEvent(AnimationEventType.StartSkinning, 0.25f));
		animWrapper69.Events.Add(new AnimEvent(AnimationEventType.FinishSkinning, 0.75f));
		animWrapper69.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper70 = AddActionAnim(ActionAnim.FillLiquidContainerFromWell, "Scavenge", "BottleFillFromWell");
		animWrapper70.Events.Add(new AnimEvent(AnimationEventType.FillLiquidContainer, 0.5f));
		animWrapper70.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper71 = AddActionAnim(ActionAnim.FillLiquidContainerFromRiver, "ScavengeCorpse", "BottleFillFromRiver");
		animWrapper71.Events.Add(new AnimEvent(AnimationEventType.FillLiquidContainer, 0.5f));
		animWrapper71.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper72 = AddActionAnim(ActionAnim.FillLiquidContainerFromRiver, "WateringCan_FillFromRiver", "WateringCanFillFromRiver");
		animWrapper72.EquippedType = typeof(WateringCan);
		animWrapper72.Events.Add(new AnimEvent(AnimationEventType.FillLiquidContainer, 0.5f));
		animWrapper72.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper73 = AddActionAnim(ActionAnim.ApplyBandage, "Scavenge", "Bandage");
		animWrapper73.Events.Add(new AnimEvent(AnimationEventType.ApplyBandage, 0.5f));
		animWrapper73.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper74 = AddActionAnim(ActionAnim.ApplyBandage, "ScavengeCrouch", "CrouchBandage");
		animWrapper74.Crouching = true;
		animWrapper74.Events.Add(new AnimEvent(AnimationEventType.ApplyBandage, 0.5f));
		animWrapper74.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper75 = AddActionAnim(ActionAnim.ApplyBandageToSelf, "Srv_BandageArm", "BandageSelf");
		animWrapper75.Events.Add(new AnimEvent(AnimationEventType.FinishApplyBandageToSelf, 0.72f));
		animWrapper75.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper76 = AddActionAnim(ActionAnim.ApplyBandageToSelf, "CrouchBandage", "CrouchBandageSelf");
		animWrapper76.Crouching = true;
		animWrapper76.Events.Add(new AnimEvent(AnimationEventType.FinishApplyBandageToSelf, 0.72f));
		animWrapper76.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper77 = AddActionAnim(ActionAnim.ApplyBandageToSelf, "Srv_SitBonfire_BandageArm", "BandageSelfSitting");
		animWrapper77.Sitting = true;
		animWrapper77.Events.Add(new AnimEvent(AnimationEventType.FinishApplyBandageToSelf, 0.75f));
		animWrapper77.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper78 = AddActionAnim(ActionAnim.AdministerInjection, "Scavenge", "Inject");
		animWrapper78.Events.Add(new AnimEvent(AnimationEventType.AdministerInjection, 0.5f));
		animWrapper78.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper79 = AddActionAnim(ActionAnim.AdministerInjection, "ScavengeCrouch", "CrouchInject");
		animWrapper79.Crouching = true;
		animWrapper79.Events.Add(new AnimEvent(AnimationEventType.AdministerInjection, 0.5f));
		animWrapper79.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper80 = AddActionAnim(ActionAnim.AdministerInjectionToSelf, "InjectSelf", "InjectSelf");
		animWrapper80.Events.Add(new AnimEvent(AnimationEventType.AdministerInjectionToSelf, 0.5f));
		animWrapper80.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		animWrapper80.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper81 = AddActionAnim(ActionAnim.AdministerInjectionToSelf, "CrouchInjectSelf", "CrouchInjectSelf");
		animWrapper81.Crouching = true;
		animWrapper81.Events.Add(new AnimEvent(AnimationEventType.AdministerInjectionToSelf, 0.5f));
		animWrapper81.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		animWrapper81.PipView = PipAnimView.LooseCloseUp;
		AnimWrapper animWrapper82 = AddActionAnim(ActionAnim.AdministerInjectionToSelf, "Srv_SitBonfire_Injection", "InjectSelfSitting");
		animWrapper82.Sitting = true;
		animWrapper82.Events.Add(new AnimEvent(AnimationEventType.AdministerInjectionToSelf, 0.4f));
		animWrapper82.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.55f));
		AnimWrapper animWrapper83 = AddActionAnim(ActionAnim.PlantCrops, "PickUp_RH", "PlantCrops");
		animWrapper83.Events.Add(new AnimEvent(AnimationEventType.PlantCrops, 0.4f));
		animWrapper83.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper84 = AddActionAnim(ActionAnim.WaterCrops, "PourWaterBottle", "BottleWaterCrops");
		animWrapper84.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsStart, 0.25f));
		animWrapper84.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsEnd, 0.75f));
		animWrapper84.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper85 = AddActionAnim(ActionAnim.WaterCrops, "CrouchPourWaterBottle", "CrouchBottleWaterCrops");
		animWrapper85.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsStart, 0.25f));
		animWrapper85.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsEnd, 0.75f));
		animWrapper85.PipView = PipAnimView.FullBodySide;
		animWrapper85.Crouching = true;
		AnimWrapper animWrapper86 = AddActionAnim(ActionAnim.WaterCrops, "WateringCan_Use", "WateringCanWaterCrops");
		animWrapper86.EquippedType = typeof(WateringCan);
		animWrapper86.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsStart, 0.25f));
		animWrapper86.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsEnd, 0.75f));
		animWrapper86.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper87 = AddActionAnim(ActionAnim.WaterCrops, "WateringCan_UseCrouch", "CrouchWateringCanWaterCrops");
		animWrapper87.EquippedType = typeof(WateringCan);
		animWrapper87.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsStart, 0.25f));
		animWrapper87.Events.Add(new AnimEvent(AnimationEventType.WaterPlantsEnd, 0.75f));
		animWrapper87.PipView = PipAnimView.FullBodySide;
		animWrapper87.Crouching = true;
		AnimWrapper animWrapper88 = AddActionAnim(ActionAnim.HarvestCrops, "PickUp_LH", "HarvestCrops");
		animWrapper88.Events.Add(new AnimEvent(AnimationEventType.HarvestCrops, 0.4f));
		animWrapper88.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper89 = AddActionAnim(ActionAnim.HarvestCrops, "ScavengeCorpseCrouch", "CrouchHarvestCrops");
		animWrapper89.Crouching = true;
		animWrapper89.Events.Add(new AnimEvent(AnimationEventType.HarvestCrops, 0.5f));
		animWrapper89.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper90 = AddActionAnim(ActionAnim.SpitRoast, "Scavenge", "SpitRoast");
		animWrapper90.Events.Add(new AnimEvent(AnimationEventType.StartCrafting, 0.5f));
		animWrapper90.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper91 = AddActionAnim(ActionAnim.Build, "Srv_HammeringLow_Loop", "Build");
		animWrapper91.Looped = true;
		animWrapper91.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper92 = AddActionAnim(ActionAnim.DigLoop, "Srv_Dig_Loop", "DigLoop");
		animWrapper92.Looped = true;
		animWrapper92.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper93 = AddActionAnim(ActionAnim.RepairStart, "Srv_HammeringNailHigh_Start", "RepairStart");
		animWrapper93.HasRootMotion = true;
		animWrapper93.PipView = PipAnimView.FullBodySide;
		animWrapper93.DontRestartIfAlreadyPLaying = true;
		AddActionAnim(ActionAnim.Repair, "Srv_HammeringNailHigh_Loop", "Repair").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper94 = AddActionAnim(ActionAnim.RepairFinish, "Srv_HammeringNailHigh_End", "RepairEnd");
		animWrapper94.HasRootMotion = true;
		animWrapper94.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper95 = AddActionAnim(ActionAnim.CraftStart, "Srv_CraftGround_Start", "CraftStart");
		animWrapper95.Speed = 1.5f;
		animWrapper95.HasRootMotion = true;
		animWrapper95.Events.Add(new AnimEvent(AnimationEventType.StartCrafting, 0.99f));
		animWrapper95.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper96 = AddActionAnim(ActionAnim.CraftLoop, "Srv_CraftGround_Loop", "CraftLoop");
		animWrapper96.Looped = true;
		animWrapper96.PipView = PipAnimView.FullBodySide;
		animWrapper96.DontRestartIfAlreadyPLaying = true;
		AddActionAnim(ActionAnim.CraftNonLooped, "Srv_CraftGround_Loop", "CraftLoop").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper97 = AddActionAnim(ActionAnim.CraftEnd, "Srv_CraftGround_End", "CraftEnd");
		animWrapper97.Speed = 1.5f;
		animWrapper97.HasRootMotion = true;
		animWrapper97.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper98 = AddActionAnim(ActionAnim.PotStart, "Srv_PotCooking_Start", "PotStart");
		animWrapper98.HasRootMotion = true;
		animWrapper98.IsInteractionWithObject = true;
		animWrapper98.Events.Add(new AnimEvent(AnimationEventType.StartCrafting, 0.99f));
		animWrapper98.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper99 = AddActionAnim(ActionAnim.PotLoop, "Srv_PotCooking_Loop", "PotLoop");
		animWrapper99.IsInteractionWithObject = true;
		animWrapper99.Looped = true;
		animWrapper99.PipView = PipAnimView.FullBodySide;
		animWrapper99.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper100 = AddActionAnim(ActionAnim.PotEnd, "Srv_PotCooking_End", "PotEnd");
		animWrapper100.HasRootMotion = true;
		animWrapper100.IsInteractionWithObject = true;
		animWrapper100.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper101 = AddActionAnim(ActionAnim.ForgeStart, "Srv_Forge_Start", "ForgeStart");
		animWrapper101.Speed = 1.5f;
		animWrapper101.HasRootMotion = true;
		animWrapper101.Events.Add(new AnimEvent(AnimationEventType.StartCrafting, 0.99f));
		animWrapper101.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper102 = AddActionAnim(ActionAnim.ForgeLoop, "Srv_Forge_Loop", "ForgeLoop");
		animWrapper102.Looped = true;
		animWrapper102.PipView = PipAnimView.FullBodySide;
		animWrapper102.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper103 = AddActionAnim(ActionAnim.ForgeEnd, "Srv_Forge_End", "ForgeEnd");
		animWrapper103.Speed = 1.5f;
		animWrapper103.HasRootMotion = true;
		animWrapper103.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper104 = AddActionAnim(ActionAnim.CraftTableStart, "Srv_CraftTable_Start", "CraftTableStart");
		animWrapper104.Speed = 1.5f;
		animWrapper104.HasRootMotion = true;
		animWrapper104.Events.Add(new AnimEvent(AnimationEventType.StartCrafting, 0.99f));
		animWrapper104.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper105 = AddActionAnim(ActionAnim.CraftTableLoop, "Srv_CraftTable_Loop", "CraftTableLoop");
		animWrapper105.Looped = true;
		animWrapper105.PipView = PipAnimView.FullBodySide;
		animWrapper105.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper106 = AddActionAnim(ActionAnim.CraftTableEnd, "Srv_CraftTable_End", "CraftTableEnd");
		animWrapper106.Speed = 1.5f;
		animWrapper106.HasRootMotion = true;
		animWrapper106.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.ChopTreeStart, "Srv_ChopTree_Start", "ChopTreeStart").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper107 = AddActionAnim(ActionAnim.ChopTreeLoop, "Srv_ChopTree_Loop", "ChopTreeLoop");
		animWrapper107.Events.Add(new AnimEvent(AnimationEventType.ChopTree, 0.92f));
		animWrapper107.PipView = PipAnimView.FullBodySide;
		animWrapper107.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper108 = AddActionAnim(ActionAnim.ChopTreeEnd, "Srv_ChopTree_End", "ChopTreeEnd");
		animWrapper108.Events.Add(new AnimEvent(AnimationEventType.ChopTree, 0.3f));
		animWrapper108.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.ChopLogStart, "Srv_ChopLog_Start", "ChopLogStart").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper109 = AddActionAnim(ActionAnim.ChopLogLoop, "Srv_ChopLog_Loop", "ChopLogLoop");
		animWrapper109.Events.Add(new AnimEvent(AnimationEventType.ChopLog, 0.86f));
		animWrapper109.PipView = PipAnimView.FullBodySide;
		animWrapper109.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper110 = AddActionAnim(ActionAnim.ChopLogEnd, "Srv_ChopLog_End", "ChopLogEnd");
		animWrapper110.Events.Add(new AnimEvent(AnimationEventType.ChopLogFinish, 0.3f));
		animWrapper110.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.MineBoulderStart, "Srv_PickaxeWall_Start", "MineBoulderStart").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper111 = AddActionAnim(ActionAnim.MineBoulderLoop, "Srv_PickaxeWall_Loop", "MineBoulderLoop");
		animWrapper111.Events.Add(new AnimEvent(AnimationEventType.BreakRockSound, 0.09f));
		animWrapper111.Events.Add(new AnimEvent(AnimationEventType.BreakRockSound, 0.35f));
		animWrapper111.Events.Add(new AnimEvent(AnimationEventType.BreakRockSound, 0.62f));
		animWrapper111.Events.Add(new AnimEvent(AnimationEventType.BreakRockSound, 0.86f));
		animWrapper111.Events.Add(new AnimEvent(AnimationEventType.BreakRock, 0.86f));
		animWrapper111.PipView = PipAnimView.FullBodySide;
		animWrapper111.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper112 = AddActionAnim(ActionAnim.MineBoulderEnd, "Srv_PickaxeWall_End", "MineBoulderEnd");
		animWrapper112.Events.Add(new AnimEvent(AnimationEventType.BreakRockFinish, 0.3f));
		animWrapper112.PipView = PipAnimView.FullBodySide;
		AddActionAnim(ActionAnim.MineRockStart, "Srv_ChopLog_Start", "MineRockStart").PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper113 = AddActionAnim(ActionAnim.MineRockLoop, "Srv_MineRock_Loop", "MineRockLoop");
		animWrapper113.Events.Add(new AnimEvent(AnimationEventType.BreakRock, 0.86f));
		animWrapper113.PipView = PipAnimView.FullBodySide;
		animWrapper113.DontRestartIfAlreadyPLaying = true;
		AnimWrapper animWrapper114 = AddActionAnim(ActionAnim.MineRockEnd, "Srv_MineRock_End", "MineRockEnd");
		animWrapper114.Events.Add(new AnimEvent(AnimationEventType.BreakRockFinish, 0.3f));
		animWrapper114.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper115 = AddActionAnim(ActionAnim.LightFireWithMatch, "Srv_StartBonfire_Match", "LightFireWithMatch");
		animWrapper115.HasRootMotion = true;
		animWrapper115.IsInteractionWithObject = true;
		animWrapper115.Events.Add(new AnimEvent(AnimationEventType.LightFire, 0.4f));
		animWrapper115.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper116 = AddActionAnim(ActionAnim.LightFireWithFlint, "Srv_StartBonfire_Spark", "LightFireWithFlint");
		animWrapper116.HasRootMotion = true;
		animWrapper116.IsInteractionWithObject = true;
		animWrapper116.Events.Add(new AnimEvent(AnimationEventType.EquipFlint, 0.04f));
		animWrapper116.Events.Add(new AnimEvent(AnimationEventType.LightFire, 0.75f));
		animWrapper116.Events.Add(new AnimEvent(AnimationEventType.UnequipFlint, 0.9f));
		animWrapper116.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper117 = AddActionAnim(ActionAnim.LightFireWithMatch, "Srv_StartBonfire_Match_Crouching", "CrouchLightFireWithMatch");
		animWrapper117.IsInteractionWithObject = true;
		animWrapper117.Events.Add(new AnimEvent(AnimationEventType.LightFire, 0.2f));
		animWrapper117.PipView = PipAnimView.FullBodySide;
		animWrapper117.Crouching = true;
		AnimWrapper animWrapper118 = AddActionAnim(ActionAnim.LightFireWithFlint, "Srv_StartBonfire_Spark_Crouching", "CrouchLightFireWithFlint");
		animWrapper118.IsInteractionWithObject = true;
		animWrapper118.Events.Add(new AnimEvent(AnimationEventType.EquipFlint, 0f));
		animWrapper118.Events.Add(new AnimEvent(AnimationEventType.LightFire, 0.9f));
		animWrapper118.Events.Add(new AnimEvent(AnimationEventType.UnequipFlint, 0.99f));
		animWrapper118.PipView = PipAnimView.FullBodySide;
		animWrapper118.Crouching = true;
		AnimWrapper animWrapper119 = AddActionAnim(ActionAnim.AddMaterialToFire, "Srv_AddToBonfire", "AddMaterialToFire");
		animWrapper119.HasRootMotion = true;
		animWrapper119.IsInteractionWithObject = true;
		animWrapper119.Events.Add(new AnimEvent(AnimationEventType.AddMaterialToFire, 0.5f));
		animWrapper119.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper120 = AddActionAnim(ActionAnim.AddMaterialToFire, "Srv_SitBonfire_AddWood", "AddMaterialToFireSitting");
		animWrapper120.Sitting = true;
		animWrapper120.Events.Add(new AnimEvent(AnimationEventType.AddMaterialToFire, 0.5f));
		AnimWrapper animWrapper121 = AddActionAnim(ActionAnim.PickUp, "PickUp", "PickUp");
		animWrapper121.Events.Add(new AnimEvent(AnimationEventType.PickUp, 0.5f));
		animWrapper121.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper122 = AddActionAnim(ActionAnim.PickUp, "CrouchPickUp", "CrouchPickUp");
		animWrapper122.Crouching = true;
		animWrapper122.Events.Add(new AnimEvent(AnimationEventType.PickUp, 0.5f));
		animWrapper122.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper123 = AddActionAnim(ActionAnim.Drop, "Drop", "Drop");
		animWrapper123.Events.Add(new AnimEvent(AnimationEventType.Drop, 0.5f));
		animWrapper123.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper124 = AddActionAnim(ActionAnim.Drop, "CrouchDrop", "CrouchDrop");
		animWrapper124.Crouching = true;
		animWrapper124.Events.Add(new AnimEvent(AnimationEventType.Drop, 0.5f));
		animWrapper124.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper125 = AddActionAnim(ActionAnim.OpenGate, "Scavenge", "OpenGate");
		animWrapper125.Events.Add(new AnimEvent(AnimationEventType.OpenGate, 0.5f));
		animWrapper125.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper126 = AddActionAnim(ActionAnim.CloseGate, "Scavenge", "CloseGate");
		animWrapper126.Events.Add(new AnimEvent(AnimationEventType.CloseGate, 0.5f));
		animWrapper126.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper127 = AddActionAnim(ActionAnim.LockGate, "Scavenge", "LockGate");
		animWrapper127.Events.Add(new AnimEvent(AnimationEventType.LockGate, 0.5f));
		animWrapper127.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper128 = AddActionAnim(ActionAnim.Knock, "Knock", "Knock");
		animWrapper128.Events.Add(new AnimEvent(AnimationEventType.Knock, 0.25f));
		animWrapper128.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper129 = AddActionAnim(ActionAnim.EquipUnarmed, "Unarmed_EquipIdle", "UnarmedEquip", "UnarmedEquip");
		animWrapper129.Speed = 1.5f;
		animWrapper129.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper130 = AddActionAnim(ActionAnim.Unequip, "Unarmed_UnequipIdle", "UnarmedUnequip", "UnarmedUnequip");
		animWrapper130.Speed = 1.5f;
		animWrapper130.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper131 = AddActionAnim(ActionAnim.EquipUnarmed, "Unarmed_EquipCrouch", "UnarmedCrouchEquip", "UnarmedCrouchEquip");
		animWrapper131.Speed = 1.5f;
		animWrapper131.Crouching = true;
		animWrapper131.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper132 = AddActionAnim(ActionAnim.Unequip, "Unarmed_UnequipCrouch", "UnarmedCrouchUnequip", "UnarmedCrouchUnequip");
		animWrapper132.Speed = 1.5f;
		animWrapper132.Crouching = true;
		animWrapper132.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper133 = AddActionAnim(ActionAnim.EquipUnarmed, "Srv_SitBonfire_Equip", "UnarmedSittingEquip");
		animWrapper133.Sitting = true;
		animWrapper133.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper134 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "UnarmedSittingUnequip");
		animWrapper134.Sitting = true;
		animWrapper134.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper135 = AddActionAnim(ActionAnim.HugStart, "HugStart", "HugStart");
		animWrapper135.IsInteractionWithObject = true;
		animWrapper135.CanTurnDuringAnim = false;
		animWrapper135.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper136 = AddActionAnim(ActionAnim.HugLoop, "HugLoop", "HugLoop");
		animWrapper136.IsInteractionWithObject = true;
		animWrapper136.Looped = true;
		animWrapper136.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper137 = AddActionAnim(ActionAnim.HugFinish, "HugFinish", "HugFinish");
		animWrapper137.IsInteractionWithObject = true;
		animWrapper137.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper138 = AddActionAnim(ActionAnim.Hugged, "Hugged", "Hugged");
		animWrapper138.Looped = true;
		animWrapper138.IsInteractionWithObject = true;
		animWrapper138.PipView = PipAnimView.FullBodySide;
		animWrapper138.TransitionInTime = 0.25f;
		AnimWrapper animWrapper139 = AddActionAnim(ActionAnim.HuggedFree, "HuggedFree", "HuggedFree");
		animWrapper139.InterruptibleFrac = 0.85f;
		animWrapper139.IsInteractionWithObject = true;
		animWrapper139.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper140 = AddActionAnim(ActionAnim.ChokeHoldStart, "ChokeHoldStart", "ChokeHoldStart");
		animWrapper140.IsInteractionWithObject = true;
		animWrapper140.CanTurnDuringAnim = false;
		animWrapper140.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper141 = AddActionAnim(ActionAnim.ChokeHoldLoop, "ChokeHoldLoop", "ChokeHoldLoop");
		animWrapper141.IsInteractionWithObject = true;
		animWrapper141.Looped = true;
		animWrapper141.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper142 = AddActionAnim(ActionAnim.ChokeHoldFinish, "ChokeHoldFinish", "ChokeHoldFinish");
		animWrapper142.IsInteractionWithObject = true;
		animWrapper142.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper143 = AddActionAnim(ActionAnim.ChokeHoldFail, "ChokeHoldFail", "ChokeHoldFail");
		animWrapper143.HasRootMotion = true;
		animWrapper143.IsInteractionWithObject = true;
		animWrapper143.Speed = 1.5f;
		animWrapper143.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper144 = AddActionAnim(ActionAnim.SlitThroatStart, "SlitThroatStart", "SlitThroatStart");
		animWrapper144.IsInteractionWithObject = true;
		animWrapper144.CanTurnDuringAnim = false;
		animWrapper144.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper145 = AddActionAnim(ActionAnim.SlitThroatLoop, "SlitThroatLoop", "SlitThroatLoop");
		animWrapper145.IsInteractionWithObject = true;
		animWrapper145.Looped = true;
		animWrapper145.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper146 = AddActionAnim(ActionAnim.SlitThroatFinish, "SlitThroatFinish", "SlitThroatFinish");
		animWrapper146.IsInteractionWithObject = true;
		animWrapper146.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper147 = AddActionAnim(ActionAnim.SlitThroatFail, "SlitThroatFail", "SlitThroatFail");
		animWrapper147.HasRootMotion = true;
		animWrapper147.IsInteractionWithObject = true;
		animWrapper147.Speed = 1.5f;
		animWrapper147.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper148 = AddActionAnim(ActionAnim.RestrainStart, "RestrainStart", "RestrainStart");
		animWrapper148.IsInteractionWithObject = true;
		animWrapper148.CanTurnDuringAnim = false;
		AnimWrapper animWrapper149 = AddActionAnim(ActionAnim.RestrainLoop, "RestrainLoop", "RestrainLoop");
		animWrapper149.IsInteractionWithObject = true;
		animWrapper149.Looped = true;
		AddActionAnim(ActionAnim.RestrainFinish, "RestrainFinish", "RestrainFinish").IsInteractionWithObject = true;
		AnimWrapper animWrapper150 = AddActionAnim(ActionAnim.RestrainFail, "RestrainFail", "RestrainFail");
		animWrapper150.IsInteractionWithObject = true;
		animWrapper150.Speed = 1.5f;
		AnimWrapper animWrapper151 = AddActionAnim(ActionAnim.RestrainStart, "RestrainStartLow", "RestrainStartLow");
		animWrapper151.IsInteractionWithObject = true;
		animWrapper151.CanTurnDuringAnim = false;
		animWrapper151.Gender = GenderType.Male;
		animWrapper151.TargetGender = GenderType.Female;
		AnimWrapper animWrapper152 = AddActionAnim(ActionAnim.RestrainLoop, "RestrainLoopLow", "RestrainLoopLow");
		animWrapper152.IsInteractionWithObject = true;
		animWrapper152.Gender = GenderType.Male;
		animWrapper152.TargetGender = GenderType.Female;
		animWrapper152.Looped = true;
		AnimWrapper animWrapper153 = AddActionAnim(ActionAnim.RestrainFinish, "RestrainFinishLow", "RestrainFinishLow");
		animWrapper153.IsInteractionWithObject = true;
		animWrapper153.Gender = GenderType.Male;
		animWrapper153.TargetGender = GenderType.Female;
		AnimWrapper animWrapper154 = AddActionAnim(ActionAnim.RestrainFail, "RestrainFailLow", "RestrainFailLow");
		animWrapper154.IsInteractionWithObject = true;
		animWrapper154.Gender = GenderType.Male;
		animWrapper154.TargetGender = GenderType.Female;
		animWrapper154.Speed = 1.5f;
		AnimWrapper animWrapper155 = AddActionAnim(ActionAnim.ChokeHoldStruggle, "ChokeHoldStruggle", "ChokeHoldStruggle");
		animWrapper155.Looped = true;
		animWrapper155.IsInteractionWithObject = true;
		animWrapper155.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper156 = AddActionAnim(ActionAnim.ChokeHoldFree, "ChokeHoldFree", "ChokeHoldFree");
		animWrapper156.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper156.InterruptibleFrac = 0.85f;
		animWrapper156.IsInteractionWithObject = true;
		animWrapper156.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper157 = AddActionAnim(ActionAnim.ChokeHoldStruggle, "ChokeHoldStruggleLow", "ChokeHoldStruggleLow");
		animWrapper157.Gender = GenderType.Male;
		animWrapper157.TargetGender = GenderType.Female;
		animWrapper157.Looped = true;
		animWrapper157.IsInteractionWithObject = true;
		animWrapper157.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper158 = AddActionAnim(ActionAnim.ChokeHoldFree, "ChokeHoldFreeLow", "ChokeHoldFreeLow");
		animWrapper158.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper158.Gender = GenderType.Male;
		animWrapper158.TargetGender = GenderType.Female;
		animWrapper158.InterruptibleFrac = 0.85f;
		animWrapper158.IsInteractionWithObject = true;
		animWrapper158.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper159 = AddActionAnim(ActionAnim.ChokeHoldStruggle, "ChokeHoldStruggle", "ZombieChokeHoldStruggle");
		animWrapper159.Zombie = true;
		animWrapper159.Looped = true;
		animWrapper159.IsInteractionWithObject = true;
		animWrapper159.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper160 = AddActionAnim(ActionAnim.ChokeHoldFree, "ChokeHoldFree", "ZombieChokeHoldFree");
		animWrapper160.Zombie = true;
		animWrapper160.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper160.InterruptibleFrac = 0.85f;
		animWrapper160.IsInteractionWithObject = true;
		animWrapper160.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper161 = AddActionAnim(ActionAnim.ChokeHoldStruggle, "ChokeHoldStruggleLow", "ZombieChokeHoldStruggleLow");
		animWrapper161.Gender = GenderType.Male;
		animWrapper161.TargetGender = GenderType.Female;
		animWrapper161.Zombie = true;
		animWrapper161.Looped = true;
		animWrapper161.IsInteractionWithObject = true;
		animWrapper161.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper162 = AddActionAnim(ActionAnim.ChokeHoldFree, "ChokeHoldFreeLow", "ZombieChokeHoldFreeLow");
		animWrapper162.Gender = GenderType.Male;
		animWrapper162.TargetGender = GenderType.Female;
		animWrapper162.Zombie = true;
		animWrapper162.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper162.InterruptibleFrac = 0.85f;
		animWrapper162.IsInteractionWithObject = true;
		animWrapper162.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper163 = AddActionAnim(ActionAnim.RestrainedStruggle, "RestrainedStruggle", "RestrainedStruggle");
		animWrapper163.Looped = true;
		animWrapper163.IsInteractionWithObject = true;
		AnimWrapper animWrapper164 = AddActionAnim(ActionAnim.RestrainedFree, "RestrainedFree", "RestrainedFree");
		animWrapper164.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper164.InterruptibleFrac = 0.85f;
		animWrapper164.IsInteractionWithObject = true;
		AnimWrapper animWrapper165 = AddActionAnim(ActionAnim.RestrainedStruggle, "RestrainedStruggleLow", "RestrainedStruggleLow");
		animWrapper165.Gender = GenderType.Male;
		animWrapper165.TargetGender = GenderType.Female;
		animWrapper165.Looped = true;
		animWrapper165.IsInteractionWithObject = true;
		AnimWrapper animWrapper166 = AddActionAnim(ActionAnim.RestrainedFree, "RestrainedFreeLow", "RestrainedFreeLow");
		animWrapper166.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper166.Gender = GenderType.Male;
		animWrapper166.TargetGender = GenderType.Female;
		animWrapper166.InterruptibleFrac = 0.85f;
		animWrapper166.IsInteractionWithObject = true;
		AnimWrapper animWrapper167 = AddActionAnim(ActionAnim.BludgeonUnconscious, "CrouchBludgeon", "UnarmedCrouchBludgeon");
		animWrapper167.Crouching = true;
		animWrapper167.Events.Add(new AnimEvent(AnimationEventType.BludgeonUnconscious, 0.5f));
		AnimWrapper animWrapper168 = AddActionAnim(ActionAnim.KillUnconscious, "CrouchStab", "OneHandedCrouchStab");
		animWrapper168.Crouching = true;
		animWrapper168.Events.Add(new AnimEvent(AnimationEventType.KillUnconscious, 0.5f));
		AnimWrapper animWrapper169 = AddActionAnim(ActionAnim.EquipMelee, "OneHanded_EquipIdle", "OneHandedEquip", "OneHandedEquip");
		animWrapper169.Speed = 1.5f;
		animWrapper169.EquippedType = typeof(MeleeWeapon);
		animWrapper169.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper170 = AddActionAnim(ActionAnim.Unequip, "OneHanded_UnequipIdle", "OneHandedUnequip", "OneHandedUnequip");
		animWrapper170.Speed = 1.5f;
		animWrapper170.EquippedType = typeof(MeleeWeapon);
		animWrapper170.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper171 = AddActionAnim(ActionAnim.EquipMelee, "Pistol_EquipCrouch", "OneHandedCrouchEquip", "OneHandedCrouchEquip");
		animWrapper171.Speed = 1.5f;
		animWrapper171.EquippedType = typeof(MeleeWeapon);
		animWrapper171.Crouching = true;
		animWrapper171.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper172 = AddActionAnim(ActionAnim.Unequip, "Pistol_UnequipCrouch", "OneHandedCrouchUnequip", "OneHandedCrouchUnequip");
		animWrapper172.Speed = 1.5f;
		animWrapper172.EquippedType = typeof(MeleeWeapon);
		animWrapper172.Crouching = true;
		animWrapper172.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper173 = AddActionAnim(ActionAnim.EquipMelee, "Srv_SitBonfire_Equip", "OneHandedSittingEquip");
		animWrapper173.EquippedType = typeof(MeleeWeapon);
		animWrapper173.Sitting = true;
		animWrapper173.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper174 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "OneHandedSittingUnequip");
		animWrapper174.EquippedType = typeof(MeleeWeapon);
		animWrapper174.Sitting = true;
		animWrapper174.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper175 = AddActionAnim(ActionAnim.Kick, "Attack_Move_med_Kick", "OneHandedKick");
		animWrapper175.Aiming = true;
		animWrapper175.TargetRange = 2f;
		animWrapper175.EquippedType = typeof(MeleeWeapon);
		animWrapper175.InterruptibleFrac = 0.7f;
		animWrapper175.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.35f));
		animWrapper175.HasRootMotion = true;
		animWrapper175.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper176 = AddActionAnim(ActionAnim.Kick, "OneHanded_KickOnGround", "OneHandedKickOnGround");
		animWrapper176.Aiming = true;
		animWrapper176.TargetOnGround = true;
		animWrapper176.TargetRange = 1.5f;
		animWrapper176.EquippedType = typeof(MeleeWeapon);
		animWrapper176.InterruptibleFrac = 0.8f;
		animWrapper176.Events.Add(new AnimEvent(AnimationEventType.KickLeft, 0.42f));
		animWrapper176.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper177 = AddActionAnim(ActionAnim.Vault, "Vault1m", "OneHandedVault");
		animWrapper177.EquippedType = typeof(MeleeWeapon);
		animWrapper177.HasRootMotion = true;
		animWrapper177.TransitionInTime = 0.25f;
		animWrapper177.VaultLoopStartFrac = 0.2f;
		animWrapper177.VaultLoopEndFrac = 0.38f;
		animWrapper177.InterruptibleFrac = 0.5f;
		animWrapper177.Speed = 1.5f;
		animWrapper177.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper178 = AddActionAnim(ActionAnim.Attack, "Attack_Place_med_R_2", "OneHandedAttackHigh1");
		animWrapper178.Aiming = true;
		animWrapper178.TargetRange = 1.5f;
		animWrapper178.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper178.EquippedType = typeof(MeleeWeapon);
		animWrapper178.InterruptibleFrac = 0.5f;
		animWrapper178.Speed = 0.88f;
		animWrapper178.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.25f));
		animWrapper178.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper179 = AddActionAnim(ActionAnim.Attack, "Attack_Place_fast_Rdown_1", "OneHandedAttackHigh2");
		animWrapper179.Aiming = true;
		animWrapper179.TargetRange = 1.5f;
		animWrapper179.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper179.EquippedType = typeof(MeleeWeapon);
		animWrapper179.InterruptibleFrac = 0.5f;
		animWrapper179.Speed = 0.75f;
		animWrapper179.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.23f));
		animWrapper179.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper180 = AddActionAnim(ActionAnim.Attack, "Attack_Place_fast_Rdown_2", "OneHandedAttackHigh3");
		animWrapper180.Aiming = true;
		animWrapper180.TargetRange = 1.5f;
		animWrapper180.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper180.EquippedType = typeof(MeleeWeapon);
		animWrapper180.InterruptibleFrac = 0.5f;
		animWrapper180.Speed = 1f;
		animWrapper180.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.25f));
		animWrapper180.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper181 = AddActionAnim(ActionAnim.Attack, "Attack_Place_fast_Ldown_1", "OneHandedAttackHigh4");
		animWrapper181.Aiming = true;
		animWrapper181.TargetRange = 1.5f;
		animWrapper181.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper181.EquippedType = typeof(MeleeWeapon);
		animWrapper181.InterruptibleFrac = 0.5f;
		animWrapper181.Speed = 0.75f;
		animWrapper181.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.23f));
		animWrapper181.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper182 = AddActionAnim(ActionAnim.Attack, "2xAttack_Place_med_Rdown_Rdown_1", "OneHandedAttackHigh5");
		animWrapper182.Aiming = true;
		animWrapper182.TargetRange = 1.5f;
		animWrapper182.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper182.EquippedType = typeof(MeleeWeapon);
		animWrapper182.InterruptibleFrac = 0.7f;
		animWrapper182.Speed = 0.9f;
		animWrapper182.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.2f));
		animWrapper182.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.44f));
		animWrapper182.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper183 = AddActionAnim(ActionAnim.Attack, "Attack_Move_med_Lhi_1", "OneHandedMoveAndAttackHigh1");
		animWrapper183.Aiming = true;
		animWrapper183.TargetRange = 2f;
		animWrapper183.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper183.EquippedType = typeof(MeleeWeapon);
		animWrapper183.InterruptibleFrac = 0.6f;
		animWrapper183.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.32f));
		animWrapper183.HasRootMotion = true;
		animWrapper183.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper184 = AddActionAnim(ActionAnim.Attack, "Attack_Move_med_Rhigh_1", "OneHandedMoveAndAttackHigh2");
		animWrapper184.Aiming = true;
		animWrapper184.TargetRange = 2f;
		animWrapper184.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper184.EquippedType = typeof(MeleeWeapon);
		animWrapper184.InterruptibleFrac = 0.6f;
		animWrapper184.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.3f));
		animWrapper184.HasRootMotion = true;
		animWrapper184.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper185 = AddActionAnim(ActionAnim.Attack, "Attack_Move_slow_Rdown_1", "OneHandedMoveAndAttackHigh3");
		animWrapper185.Aiming = true;
		animWrapper185.TargetRange = 2f;
		animWrapper185.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper185.EquippedType = typeof(MeleeWeapon);
		animWrapper185.InterruptibleFrac = 0.6f;
		animWrapper185.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.37f));
		animWrapper185.HasRootMotion = true;
		animWrapper185.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper186 = AddActionAnim(ActionAnim.Attack, "2xAttack_Move_med_whirl_Rhi_Rhi_1", "OneHandedMoveAndAttackHigh4");
		animWrapper186.Aiming = true;
		animWrapper186.TargetRange = 3.5f;
		animWrapper186.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper186.EquippedType = typeof(MeleeWeapon);
		animWrapper186.InterruptibleFrac = 0.75f;
		animWrapper186.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.2f));
		animWrapper186.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.5f));
		animWrapper186.HasRootMotion = true;
		animWrapper186.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper187 = AddActionAnim(ActionAnim.Attack, "Attack_Move_Achilles", "OneHandedMoveAndAttackHigh5");
		animWrapper187.Aiming = true;
		animWrapper187.TargetRange = 4f;
		animWrapper187.TargetBodyLocation = TargettableBodyLocation.Head;
		animWrapper187.EquippedType = typeof(MeleeWeapon);
		animWrapper187.InterruptibleFrac = 0.75f;
		animWrapper187.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.42f));
		animWrapper187.HasRootMotion = true;
		animWrapper187.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper188 = AddActionAnim(ActionAnim.Attack, "Attack_Place_fast_Lup_1", "OneHandedAttackMiddle1");
		animWrapper188.Aiming = true;
		animWrapper188.TargetRange = 1.5f;
		animWrapper188.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper188.EquippedType = typeof(MeleeWeapon);
		animWrapper188.InterruptibleFrac = 0.5f;
		animWrapper188.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.23f));
		animWrapper188.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper189 = AddActionAnim(ActionAnim.Attack, "Attack_Place_med_R_1", "OneHandedAttackMiddle2");
		animWrapper189.Aiming = true;
		animWrapper189.TargetRange = 1.5f;
		animWrapper189.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper189.EquippedType = typeof(MeleeWeapon);
		animWrapper189.InterruptibleFrac = 0.5f;
		animWrapper189.Speed = 0.9f;
		animWrapper189.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.3f));
		animWrapper189.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper190 = AddActionAnim(ActionAnim.Attack, "Attack_Place_med_Rup_1", "OneHandedAttackMiddle3");
		animWrapper190.Aiming = true;
		animWrapper190.TargetRange = 1.5f;
		animWrapper190.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper190.EquippedType = typeof(MeleeWeapon);
		animWrapper190.InterruptibleFrac = 0.5f;
		animWrapper190.Speed = 0.95f;
		animWrapper190.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.32f));
		animWrapper190.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper191 = AddActionAnim(ActionAnim.Attack, "Attack_Place_slow_Ldown_1", "OneHandedAttackMiddle4");
		animWrapper191.Aiming = true;
		animWrapper191.TargetRange = 1.5f;
		animWrapper191.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper191.EquippedType = typeof(MeleeWeapon);
		animWrapper191.InterruptibleFrac = 0.5f;
		animWrapper191.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.33f));
		animWrapper191.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper192 = AddActionAnim(ActionAnim.Attack, "2xAttack_Place_fast_Ldwon_Rdown_1", "OneHandedAttackMiddle5");
		animWrapper192.Aiming = true;
		animWrapper192.TargetRange = 1.5f;
		animWrapper192.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper192.EquippedType = typeof(MeleeWeapon);
		animWrapper192.InterruptibleFrac = 0.6f;
		animWrapper192.Speed = 0.8325f;
		animWrapper192.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.18f));
		animWrapper192.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.4f));
		animWrapper192.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper193 = AddActionAnim(ActionAnim.Attack, "Attack_Move_med_L_1", "OneHandedMoveAndAttackMiddle1");
		animWrapper193.Aiming = true;
		animWrapper193.TargetRange = 2f;
		animWrapper193.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper193.EquippedType = typeof(MeleeWeapon);
		animWrapper193.InterruptibleFrac = 0.6f;
		animWrapper193.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.34f));
		animWrapper193.HasRootMotion = true;
		animWrapper193.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper194 = AddActionAnim(ActionAnim.Attack, "Attack_Move_fast_Rdown_1", "OneHandedMoveAndAttackMiddle2");
		animWrapper194.Aiming = true;
		animWrapper194.TargetRange = 2f;
		animWrapper194.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper194.EquippedType = typeof(MeleeWeapon);
		animWrapper194.InterruptibleFrac = 0.6f;
		animWrapper194.Speed = 0.75f;
		animWrapper194.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.24f));
		animWrapper194.HasRootMotion = true;
		animWrapper194.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper195 = AddActionAnim(ActionAnim.Attack, "Attack_Move_slow_Lup", "OneHandedMoveAndAttackMiddle3");
		animWrapper195.Aiming = true;
		animWrapper195.TargetRange = 2f;
		animWrapper195.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper195.EquippedType = typeof(MeleeWeapon);
		animWrapper195.InterruptibleFrac = 0.6f;
		animWrapper195.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.35f));
		animWrapper195.HasRootMotion = true;
		animWrapper195.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper196 = AddActionAnim(ActionAnim.Attack, "2xAttack_Move_med_Rdown_L_1", "OneHandedMoveAndAttackMiddle4");
		animWrapper196.Aiming = true;
		animWrapper196.TargetRange = 2f;
		animWrapper196.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper196.EquippedType = typeof(MeleeWeapon);
		animWrapper196.InterruptibleFrac = 0.7f;
		animWrapper196.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.22f));
		animWrapper196.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.46f));
		animWrapper196.HasRootMotion = true;
		animWrapper196.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper197 = AddActionAnim(ActionAnim.Attack, "Attack_Move_slow_whirl_L_2", "OneHandedMoveAndAttackMiddle5");
		animWrapper197.Aiming = true;
		animWrapper197.TargetRange = 2f;
		animWrapper197.TargetBodyLocation = TargettableBodyLocation.Torso;
		animWrapper197.EquippedType = typeof(MeleeWeapon);
		animWrapper197.InterruptibleFrac = 0.8f;
		animWrapper197.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.41f));
		animWrapper197.HasRootMotion = true;
		animWrapper197.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper198 = AddActionAnim(ActionAnim.Attack, "Attack_Place_fast_Llow_1", "OneHandedAttackLow");
		animWrapper198.Aiming = true;
		animWrapper198.TargetRange = 1.5f;
		animWrapper198.TargetBodyLocation = TargettableBodyLocation.Legs;
		animWrapper198.EquippedType = typeof(MeleeWeapon);
		animWrapper198.InterruptibleFrac = 0.5f;
		animWrapper198.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.25f));
		animWrapper198.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper199 = AddActionAnim(ActionAnim.Attack, "Attack_Move_fast_Rlow_1", "OneHandedMoveAndAttackLow1");
		animWrapper199.Aiming = true;
		animWrapper199.TargetRange = 2f;
		animWrapper199.TargetBodyLocation = TargettableBodyLocation.Legs;
		animWrapper199.EquippedType = typeof(MeleeWeapon);
		animWrapper199.InterruptibleFrac = 0.7f;
		animWrapper199.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponQuickAttack, 0.23f));
		animWrapper199.HasRootMotion = true;
		animWrapper199.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper200 = AddActionAnim(ActionAnim.Attack, "Attack_Move_med_Rlow_1", "OneHandedMoveAndAttackLow2");
		animWrapper200.Aiming = true;
		animWrapper200.TargetRange = 2f;
		animWrapper200.TargetBodyLocation = TargettableBodyLocation.Legs;
		animWrapper200.EquippedType = typeof(MeleeWeapon);
		animWrapper200.InterruptibleFrac = 0.6f;
		animWrapper200.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.31f));
		animWrapper200.HasRootMotion = true;
		animWrapper200.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper201 = AddActionAnim(ActionAnim.SnapAttack, "Attack_Place_snap_T_1", "OneHandedSnapAttack1");
		animWrapper201.Aiming = true;
		animWrapper201.TargetRange = 1.5f;
		animWrapper201.EquippedType = typeof(MeleeWeapon);
		animWrapper201.InterruptibleFrac = 0.7f;
		animWrapper201.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper202 = AddActionAnim(ActionAnim.SnapAttack, "Attack_Place_snap_Ldown_1", "OneHandedSnapAttack2");
		animWrapper202.Aiming = true;
		animWrapper202.TargetRange = 1.5f;
		animWrapper202.EquippedType = typeof(MeleeWeapon);
		animWrapper202.InterruptibleFrac = 0.8f;
		animWrapper202.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper203 = AddActionAnim(ActionAnim.SnapAttack, "Attack_Place_snap_Ldown_2", "OneHandedSnapAttack3");
		animWrapper203.Aiming = true;
		animWrapper203.TargetRange = 1.5f;
		animWrapper203.EquippedType = typeof(MeleeWeapon);
		animWrapper203.InterruptibleFrac = 0.7f;
		animWrapper203.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper204 = AddActionAnim(ActionAnim.SnapAttack, "Attack_Place_snap_R_1", "OneHandedSnapAttack4");
		animWrapper204.Aiming = true;
		animWrapper204.TargetRange = 1.5f;
		animWrapper204.EquippedType = typeof(MeleeWeapon);
		animWrapper204.InterruptibleFrac = 0.8f;
		animWrapper204.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper205 = AddActionAnim(ActionAnim.AttackJumpingZombie, "OneHanded_AttackJumpingZombie", "OneHandedJumpingZombieAttack");
		animWrapper205.Aiming = true;
		animWrapper205.TargetRange = 3f;
		animWrapper205.Speed = 1f;
		animWrapper205.EquippedType = typeof(MeleeWeapon);
		animWrapper205.CanTurnDuringAnim = false;
		animWrapper205.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper206 = AddActionAnim(ActionAnim.Attack, "OneHanded_AttackOnGround", "OneHandedAttackOnGround");
		animWrapper206.Aiming = true;
		animWrapper206.TargetRange = 1.5f;
		animWrapper206.TargetOnGround = true;
		animWrapper206.EquippedType = typeof(MeleeWeapon);
		animWrapper206.NotEquippedSubType = typeof(HuntingKnife);
		animWrapper206.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.5f));
		animWrapper206.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper207 = AddActionAnim(ActionAnim.Attack, "OneHanded_AttackOnGround2", "OneHandedAttackOnGround2");
		animWrapper207.Aiming = true;
		animWrapper207.TargetRange = 1.5f;
		animWrapper207.TargetOnGround = true;
		animWrapper207.EquippedType = typeof(MeleeWeapon);
		animWrapper207.NotEquippedSubType = typeof(HuntingKnife);
		animWrapper207.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.5f));
		animWrapper207.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper208 = AddActionAnim(ActionAnim.Attack, "OneHanded_MoveAndAttackOnGround", "OneHandedMoveAndAttackOnGround");
		animWrapper208.Aiming = true;
		animWrapper208.TargetRange = 3f;
		animWrapper208.TargetOnGround = true;
		animWrapper208.HasRootMotion = true;
		animWrapper208.Speed = 1.5f;
		animWrapper208.EquippedType = typeof(MeleeWeapon);
		animWrapper208.NotEquippedSubType = typeof(HuntingKnife);
		animWrapper208.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.6f));
		animWrapper208.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper209 = AddActionAnim(ActionAnim.Attack, "OneHanded_MoveAndAttackOnGround2", "OneHandedMoveAndAttackOnGround2");
		animWrapper209.Aiming = true;
		animWrapper209.TargetRange = 3f;
		animWrapper209.TargetOnGround = true;
		animWrapper209.HasRootMotion = true;
		animWrapper209.Speed = 1.5f;
		animWrapper209.EquippedType = typeof(MeleeWeapon);
		animWrapper209.NotEquippedSubType = typeof(HuntingKnife);
		animWrapper209.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.6f));
		animWrapper209.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper210 = AddActionAnim(ActionAnim.Attack, "OneHanded_StabOnGround", "OneHandedStabOnGround");
		animWrapper210.Aiming = true;
		animWrapper210.TargetRange = 2f;
		animWrapper210.TargetOnGround = true;
		animWrapper210.EquippedType = typeof(HuntingKnife);
		animWrapper210.Events.Add(new AnimEvent(AnimationEventType.MeleeWeaponHeavyAttack, 0.5f));
		animWrapper210.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper211 = AddActionAnim(ActionAnim.Damaged_Head_FromCentre, "Sword1h_Hit_Head_Front", "OneHandedHitHeadFromCentre");
		animWrapper211.HasRootMotion = true;
		animWrapper211.Aiming = true;
		animWrapper211.EquippedType = typeof(MeleeWeapon);
		animWrapper211.Speed = 1.5f;
		animWrapper211.InterruptibleFrac = 0.375f;
		animWrapper211.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper212 = AddActionAnim(ActionAnim.Damaged_Head_FromRight, "Sword1h_Hit_Head_Left", "OneHandedHitHeadFromRight");
		animWrapper212.HasRootMotion = true;
		animWrapper212.Aiming = true;
		animWrapper212.EquippedType = typeof(MeleeWeapon);
		animWrapper212.Speed = 1.5f;
		animWrapper212.InterruptibleFrac = 0.375f;
		animWrapper212.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper213 = AddActionAnim(ActionAnim.Damaged_Head_FromLeft, "Sword1h_Hit_Head_Right", "OneHandedHitHeadFromLeft");
		animWrapper213.HasRootMotion = true;
		animWrapper213.Aiming = true;
		animWrapper213.EquippedType = typeof(MeleeWeapon);
		animWrapper213.Speed = 1.5f;
		animWrapper213.InterruptibleFrac = 0.375f;
		animWrapper213.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper214 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "OneHandedHitHeadFromBehind");
		animWrapper214.HasRootMotion = true;
		animWrapper214.Aiming = true;
		animWrapper214.EquippedType = typeof(MeleeWeapon);
		animWrapper214.Speed = 1.5f;
		animWrapper214.InterruptibleFrac = 0.375f;
		animWrapper214.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper215 = AddActionAnim(ActionAnim.Damaged_Torso_FromCentre, "Sword1h_Hit_Torso_Front", "OneHandedHitTorsoFromCentre");
		animWrapper215.HasRootMotion = true;
		animWrapper215.Aiming = true;
		animWrapper215.EquippedType = typeof(MeleeWeapon);
		animWrapper215.Speed = 1.5f;
		animWrapper215.InterruptibleFrac = 0.375f;
		animWrapper215.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper216 = AddActionAnim(ActionAnim.Damaged_Torso_FromRight, "Sword1h_Hit_Torso_Left", "OneHandedHitTorsoFromRight");
		animWrapper216.HasRootMotion = true;
		animWrapper216.Aiming = true;
		animWrapper216.EquippedType = typeof(MeleeWeapon);
		animWrapper216.Speed = 1.5f;
		animWrapper216.InterruptibleFrac = 0.375f;
		animWrapper216.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper217 = AddActionAnim(ActionAnim.Damaged_Torso_FromLeft, "Sword1h_Hit_Torso_Right", "OneHandedHitTorsoFromLeft");
		animWrapper217.HasRootMotion = true;
		animWrapper217.Aiming = true;
		animWrapper217.EquippedType = typeof(MeleeWeapon);
		animWrapper217.Speed = 1.5f;
		animWrapper217.InterruptibleFrac = 0.375f;
		animWrapper217.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper218 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "OneHandedHitTorsoFromBehind");
		animWrapper218.HasRootMotion = true;
		animWrapper218.Aiming = true;
		animWrapper218.EquippedType = typeof(MeleeWeapon);
		animWrapper218.Speed = 1.5f;
		animWrapper218.InterruptibleFrac = 0.375f;
		animWrapper218.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper219 = AddActionAnim(ActionAnim.Damaged_RightLeg, "Sword1h_Hit_Legs_Left", "OneHandedHitLegFromRight");
		animWrapper219.HasRootMotion = true;
		animWrapper219.Aiming = true;
		animWrapper219.EquippedType = typeof(MeleeWeapon);
		animWrapper219.Speed = 1.5f;
		animWrapper219.InterruptibleFrac = 0.375f;
		animWrapper219.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper220 = AddActionAnim(ActionAnim.Damaged_LeftLeg, "Sword1h_Hit_Legs_Right", "OneHandedHitLegFromLeft");
		animWrapper220.HasRootMotion = true;
		animWrapper220.Aiming = true;
		animWrapper220.EquippedType = typeof(MeleeWeapon);
		animWrapper220.Speed = 1.5f;
		animWrapper220.InterruptibleFrac = 0.375f;
		animWrapper220.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper221 = AddActionAnim(ActionAnim.Parry_High, "Sword1h_Parry_T", "OneHandedParryHigh1");
		animWrapper221.HasRootMotion = true;
		animWrapper221.Aiming = true;
		animWrapper221.EquippedType = typeof(MeleeWeapon);
		animWrapper221.InterruptibleFrac = 0.25f;
		animWrapper221.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper222 = AddActionAnim(ActionAnim.Parry_High, "Sword1h_Parry_Mid2", "OneHandedParryHigh2");
		animWrapper222.HasRootMotion = true;
		animWrapper222.Aiming = true;
		animWrapper222.EquippedType = typeof(MeleeWeapon);
		animWrapper222.InterruptibleFrac = 0.25f;
		animWrapper222.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper223 = AddActionAnim(ActionAnim.Parry_Middle, "Sword1h_Parry_Mid", "OneHandedParryMiddle1");
		animWrapper223.HasRootMotion = true;
		animWrapper223.Aiming = true;
		animWrapper223.EquippedType = typeof(MeleeWeapon);
		animWrapper223.InterruptibleFrac = 0.25f;
		animWrapper223.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper224 = AddActionAnim(ActionAnim.Parry_Middle, "Sword1h_Parry_R", "OneHandedParryMiddle2");
		animWrapper224.HasRootMotion = true;
		animWrapper224.Aiming = true;
		animWrapper224.EquippedType = typeof(MeleeWeapon);
		animWrapper224.InterruptibleFrac = 0.25f;
		animWrapper224.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper225 = AddActionAnim(ActionAnim.Parry_Low, "Sword1h_Parry_LowRight", "OneHandedParryLow");
		animWrapper225.HasRootMotion = true;
		animWrapper225.Aiming = true;
		animWrapper225.EquippedType = typeof(MeleeWeapon);
		animWrapper225.InterruptibleFrac = 0.25f;
		animWrapper225.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper226 = AddActionAnim(ActionAnim.Block_Punch, "Sword1h_Parry_L", "OneHandedBlockPunch");
		animWrapper226.HasRootMotion = true;
		animWrapper226.Aiming = true;
		animWrapper226.EquippedType = typeof(MeleeWeapon);
		animWrapper226.InterruptibleFrac = 0.75f;
		AnimWrapper animWrapper227 = AddActionAnim(ActionAnim.Block_Kick, "Sword1h_Parry_Rd", "OneHandedBlockKick");
		animWrapper227.HasRootMotion = true;
		animWrapper227.Aiming = true;
		animWrapper227.EquippedType = typeof(MeleeWeapon);
		animWrapper227.InterruptibleFrac = 0.75f;
		AnimWrapper animWrapper228 = AddActionAnim(ActionAnim.Dodge_Backwards, "Sword1h_Dodge", "OneHandedDodgeBackwards");
		animWrapper228.HasRootMotion = true;
		animWrapper228.Aiming = true;
		animWrapper228.EquippedType = typeof(MeleeWeapon);
		animWrapper228.InterruptibleFrac = 0.4f;
		animWrapper228.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper229 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "OneHandedDodgeForwards");
		animWrapper229.HasRootMotion = true;
		animWrapper229.Aiming = true;
		animWrapper229.CanTurnDuringAnim = false;
		animWrapper229.EquippedType = typeof(MeleeWeapon);
		animWrapper229.InterruptibleFrac = 0.5f;
		animWrapper229.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper230 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "OneHandedDodgeLeft");
		animWrapper230.HasRootMotion = true;
		animWrapper230.Aiming = true;
		animWrapper230.EquippedType = typeof(MeleeWeapon);
		animWrapper230.InterruptibleFrac = 0.5f;
		AnimWrapper animWrapper231 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "OneHandedDodgeRight");
		animWrapper231.HasRootMotion = true;
		animWrapper231.Aiming = true;
		animWrapper231.EquippedType = typeof(MeleeWeapon);
		animWrapper231.InterruptibleFrac = 0.5f;
		AnimWrapper animWrapper232 = AddActionAnim(ActionAnim.Fire, "ThrowEndFar", "Throw");
		animWrapper232.Aiming = true;
		animWrapper232.Speed = 1.5f;
		animWrapper232.EquippedType = typeof(Throwable);
		animWrapper232.Events.Add(new AnimEvent(AnimationEventType.Fire, 0.3f));
		AnimWrapper animWrapper233 = AddActionAnim(ActionAnim.Fire, "CrouchThrow", "ThrowCrouch");
		animWrapper233.Aiming = true;
		animWrapper233.Crouching = true;
		animWrapper233.Speed = 1.5f;
		animWrapper233.EquippedType = typeof(Throwable);
		animWrapper233.Events.Add(new AnimEvent(AnimationEventType.Fire, 0.3f));
		AnimWrapper animWrapper234 = AddActionAnim(ActionAnim.EquipThrowable, "Unarmed_EquipIdle", "ThrowableEquip", "ThrowableEquip");
		animWrapper234.Speed = 1.5f;
		animWrapper234.EquippedType = typeof(Throwable);
		animWrapper234.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper235 = AddActionAnim(ActionAnim.Unequip, "Unarmed_UnequipIdle", "ThrowableUnequip", "ThrowableUnequip");
		animWrapper235.Speed = 1.5f;
		animWrapper235.EquippedType = typeof(Throwable);
		animWrapper235.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper236 = AddActionAnim(ActionAnim.EquipThrowable, "Unarmed_EquipCrouch", "ThrowableCrouchEquip", "ThrowableCrouchEquip");
		animWrapper236.Speed = 1.5f;
		animWrapper236.EquippedType = typeof(Throwable);
		animWrapper236.Crouching = true;
		animWrapper236.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper237 = AddActionAnim(ActionAnim.Unequip, "Unarmed_UnequipCrouch", "ThrowableCrouchUnequip", "ThrowableCrouchUnequip");
		animWrapper237.Speed = 1.5f;
		animWrapper237.EquippedType = typeof(Throwable);
		animWrapper237.Crouching = true;
		animWrapper237.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper238 = AddActionAnim(ActionAnim.EquipThrowable, "Srv_SitBonfire_Equip", "ThrowableSittingEquip");
		animWrapper238.EquippedType = typeof(Throwable);
		animWrapper238.Sitting = true;
		animWrapper238.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper239 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "ThrowableSittingUnequip");
		animWrapper239.EquippedType = typeof(Throwable);
		animWrapper239.Sitting = true;
		animWrapper239.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper240 = AddActionAnim(ActionAnim.Kick, "Attack_Move_med_Kick", "ThrowableKick");
		animWrapper240.Aiming = true;
		animWrapper240.TargetRange = 2f;
		animWrapper240.EquippedType = typeof(Throwable);
		animWrapper240.InterruptibleFrac = 0.7f;
		animWrapper240.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.35f));
		animWrapper240.HasRootMotion = true;
		animWrapper240.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper241 = AddActionAnim(ActionAnim.Kick, "OneHanded_KickOnGround", "ThrowableKickOnGround");
		animWrapper241.Aiming = true;
		animWrapper241.TargetOnGround = true;
		animWrapper241.TargetRange = 1.5f;
		animWrapper241.EquippedType = typeof(Throwable);
		animWrapper241.InterruptibleFrac = 0.8f;
		animWrapper241.Events.Add(new AnimEvent(AnimationEventType.KickLeft, 0.42f));
		animWrapper241.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper242 = AddActionAnim(ActionAnim.Block_Kick, "Unarmed_BlockKick", "ThrowableBlockKick");
		animWrapper242.Aiming = true;
		animWrapper242.EquippedType = typeof(Throwable);
		AnimWrapper animWrapper243 = AddActionAnim(ActionAnim.Dodge_Backwards, "Sword1h_Dodge", "ThrowableDodgeBackwards");
		animWrapper243.HasRootMotion = true;
		animWrapper243.Aiming = true;
		animWrapper243.EquippedType = typeof(Throwable);
		animWrapper243.InterruptibleFrac = 0.4f;
		animWrapper243.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper244 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "ThrowableDodgeForwards");
		animWrapper244.HasRootMotion = true;
		animWrapper244.Aiming = true;
		animWrapper244.CanTurnDuringAnim = false;
		animWrapper244.EquippedType = typeof(Throwable);
		animWrapper244.InterruptibleFrac = 0.7f;
		animWrapper244.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper245 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "ThrowableDodgeLeft");
		animWrapper245.HasRootMotion = true;
		animWrapper245.Aiming = true;
		animWrapper245.EquippedType = typeof(Throwable);
		animWrapper245.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper246 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "ThrowableDodgeRight");
		animWrapper246.HasRootMotion = true;
		animWrapper246.Aiming = true;
		animWrapper246.EquippedType = typeof(Throwable);
		animWrapper246.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper247 = AddActionAnim(ActionAnim.Vault, "Vault1m", "ThrowableVault");
		animWrapper247.EquippedType = typeof(Throwable);
		animWrapper247.HasRootMotion = true;
		animWrapper247.TransitionInTime = 0.25f;
		animWrapper247.VaultLoopStartFrac = 0.2f;
		animWrapper247.VaultLoopEndFrac = 0.38f;
		animWrapper247.InterruptibleFrac = 0.5f;
		animWrapper247.Speed = 1.5f;
		animWrapper247.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper248 = AddActionAnim(ActionAnim.Fire, "Pistol_ShootPowerful", "PistolFire", "PistolFire");
		animWrapper248.UpperBodyIsAdditive = true;
		animWrapper248.Aiming = true;
		animWrapper248.EquippedType = typeof(Pistol);
		animWrapper248.InterruptibleFrac = 0.33f;
		animWrapper248.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper249 = AddActionAnim(ActionAnim.Fire, "Pistol_ShootCrouch", "PistolCrouchFire", "PistolCrouchFire");
		animWrapper249.UpperBodyIsAdditive = true;
		animWrapper249.Crouching = true;
		animWrapper249.Aiming = true;
		animWrapper249.EquippedType = typeof(Pistol);
		animWrapper249.InterruptibleFrac = 0.33f;
		animWrapper249.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper250 = AddActionAnim(ActionAnim.Reload, "Pistol_Reload_2", "PistolReload", "PistolReload");
		animWrapper250.UpperBodyIsAdditive = true;
		animWrapper250.Aiming = true;
		animWrapper250.EquippedType = typeof(Pistol);
		animWrapper250.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper251 = AddActionAnim(ActionAnim.Reload, "Pistol_ReloadCrouch", "PistolCrouchReload", "PistolCrouchReload");
		animWrapper251.UpperBodyIsAdditive = true;
		animWrapper251.Crouching = true;
		animWrapper251.Aiming = true;
		animWrapper251.EquippedType = typeof(Pistol);
		animWrapper251.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper252 = AddActionAnim(ActionAnim.Reload, "SittingReload", "PistolReloadSitting");
		animWrapper252.Sitting = true;
		animWrapper252.Aiming = true;
		animWrapper252.EquippedType = typeof(Pistol);
		animWrapper252.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper253 = AddActionAnim(ActionAnim.EquipPistol, "Pistol_EquipIdle", "PistolEquip", "PistolEquip");
		animWrapper253.Speed = 1.5f;
		animWrapper253.EquippedType = typeof(Pistol);
		animWrapper253.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper254 = AddActionAnim(ActionAnim.Unequip, "Pistol_UnequipIdle", "PistolUnequip", "PistolUnequip");
		animWrapper254.Speed = 1.5f;
		animWrapper254.EquippedType = typeof(Pistol);
		animWrapper254.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper255 = AddActionAnim(ActionAnim.EquipPistol, "Pistol_EquipCrouch", "PistolCrouchEquip", "PistolCrouchEquip");
		animWrapper255.Speed = 1.5f;
		animWrapper255.EquippedType = typeof(Pistol);
		animWrapper255.Crouching = true;
		animWrapper255.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper256 = AddActionAnim(ActionAnim.Unequip, "Pistol_UnequipCrouch", "PistolCrouchUnequip", "PistolCrouchUnequip");
		animWrapper256.Speed = 1.5f;
		animWrapper256.EquippedType = typeof(Pistol);
		animWrapper256.Crouching = true;
		animWrapper256.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper257 = AddActionAnim(ActionAnim.EquipPistol, "Srv_SitBonfire_Equip", "PistolSittingEquip");
		animWrapper257.EquippedType = typeof(Pistol);
		animWrapper257.Sitting = true;
		animWrapper257.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper258 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "PistolSittingUnequip");
		animWrapper258.EquippedType = typeof(Pistol);
		animWrapper258.Sitting = true;
		animWrapper258.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper259 = AddActionAnim(ActionAnim.Kick, "Pistol_Melee_Kick", "PistolKick");
		animWrapper259.Aiming = true;
		animWrapper259.TargetRange = 1.5f;
		animWrapper259.Speed = 0.833f;
		animWrapper259.EquippedType = typeof(Pistol);
		animWrapper259.InterruptibleFrac = 0.7f;
		animWrapper259.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.3f));
		animWrapper259.HasRootMotion = true;
		animWrapper259.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper260 = AddActionAnim(ActionAnim.Kick, "Pistol_KickOnGround", "PistolKickOnGround");
		animWrapper260.Aiming = true;
		animWrapper260.TargetOnGround = true;
		animWrapper260.TargetRange = 1.5f;
		animWrapper260.EquippedType = typeof(Pistol);
		animWrapper260.InterruptibleFrac = 0.8f;
		animWrapper260.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.42f));
		animWrapper260.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromRight,
			ActionAnim.Damaged_Head_FromRight
		};
		foreach (ActionAnim action5 in array)
		{
			AnimWrapper animWrapper261 = AddActionAnim(action5, "Pistol_Hit_L_2", "PistolHitHeadFromRight");
			animWrapper261.HasRootMotion = true;
			animWrapper261.Aiming = true;
			animWrapper261.EquippedType = typeof(Pistol);
			animWrapper261.Speed = 1.5f;
			animWrapper261.InterruptibleFrac = 0.375f;
			animWrapper261.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromCentre,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action6 in array)
		{
			AnimWrapper animWrapper262 = AddActionAnim(action6, "Pistol_Hit_C_1", "PistolHitHeadFromCentre");
			animWrapper262.HasRootMotion = true;
			animWrapper262.Aiming = true;
			animWrapper262.EquippedType = typeof(Pistol);
			animWrapper262.Speed = 1.5f;
			animWrapper262.InterruptibleFrac = 0.375f;
			animWrapper262.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromLeft,
			ActionAnim.Damaged_Head_FromLeft
		};
		foreach (ActionAnim action7 in array)
		{
			AnimWrapper animWrapper263 = AddActionAnim(action7, "Pistol_Hit_R_2", "PistolHitHeadFromLeft");
			animWrapper263.HasRootMotion = true;
			animWrapper263.Aiming = true;
			animWrapper263.EquippedType = typeof(Pistol);
			animWrapper263.Speed = 1.5f;
			animWrapper263.InterruptibleFrac = 0.375f;
			animWrapper263.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper264 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "PistolHitTorsoFromBehind");
		animWrapper264.HasRootMotion = true;
		animWrapper264.Aiming = true;
		animWrapper264.EquippedType = typeof(Pistol);
		animWrapper264.Speed = 1.5f;
		animWrapper264.InterruptibleFrac = 0.375f;
		animWrapper264.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper265 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "PistolHitHeadFromBehind");
		animWrapper265.HasRootMotion = true;
		animWrapper265.Aiming = true;
		animWrapper265.EquippedType = typeof(Pistol);
		animWrapper265.Speed = 1.5f;
		animWrapper265.InterruptibleFrac = 0.375f;
		animWrapper265.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper266 = AddActionAnim(ActionAnim.Block_Kick, "Pistol_BlockKick", "PistolBlockKick");
		animWrapper266.Aiming = true;
		animWrapper266.EquippedType = typeof(Pistol);
		AnimWrapper animWrapper267 = AddActionAnim(ActionAnim.Dodge_Backwards, "Sword1h_Dodge", "PistolDodgeBackwards");
		animWrapper267.HasRootMotion = true;
		animWrapper267.Aiming = true;
		animWrapper267.EquippedType = typeof(Pistol);
		animWrapper267.InterruptibleFrac = 0.4f;
		animWrapper267.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper268 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "PistolDodgeForwards");
		animWrapper268.HasRootMotion = true;
		animWrapper268.Aiming = true;
		animWrapper268.CanTurnDuringAnim = false;
		animWrapper268.EquippedType = typeof(Pistol);
		animWrapper268.InterruptibleFrac = 0.7f;
		animWrapper268.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper269 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "PistolDodgeLeft");
		animWrapper269.HasRootMotion = true;
		animWrapper269.Aiming = true;
		animWrapper269.EquippedType = typeof(Pistol);
		animWrapper269.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper270 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "PistolDodgeRight");
		animWrapper270.HasRootMotion = true;
		animWrapper270.Aiming = true;
		animWrapper270.EquippedType = typeof(Pistol);
		animWrapper270.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper271 = AddActionAnim(ActionAnim.Vault, "Pistol_Vault_1m", "PistolVault");
		animWrapper271.EquippedType = typeof(Pistol);
		animWrapper271.HasRootMotion = true;
		animWrapper271.TransitionInTime = 0.25f;
		animWrapper271.VaultLoopStartFrac = 0.22f;
		animWrapper271.VaultLoopEndFrac = 0.5f;
		animWrapper271.InterruptibleFrac = 0.7f;
		animWrapper271.Speed = 1.25f;
		animWrapper271.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper272 = AddActionAnim(ActionAnim.Fire, "Rifle_ShootOnce", "RifleFireOnce", "RifleFireOnce");
		animWrapper272.UpperBodyIsAdditive = true;
		animWrapper272.Aiming = true;
		animWrapper272.EquippedType = typeof(LongGun);
		animWrapper272.InterruptibleFrac = 0.25f;
		animWrapper272.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper273 = AddActionAnim(ActionAnim.Fire, "Rifle_Crouch_ShootOnce", "RifleCrouchFireOnce", "RifleCrouchFireOnce");
		animWrapper273.UpperBodyIsAdditive = true;
		animWrapper273.Crouching = true;
		animWrapper273.Aiming = true;
		animWrapper273.EquippedType = typeof(LongGun);
		animWrapper273.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper274 = AddActionAnim(ActionAnim.Reload, "Rifle_Reload_2", "RifleReload", "RifleReload");
		animWrapper274.UpperBodyIsAdditive = true;
		animWrapper274.Aiming = true;
		animWrapper274.EquippedType = typeof(LongGun);
		animWrapper274.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper275 = AddActionAnim(ActionAnim.Reload, "Rifle_Crouch_Reload", "RifleCrouchReload", "RifleCrouchReload");
		animWrapper275.UpperBodyIsAdditive = true;
		animWrapper275.Crouching = true;
		animWrapper275.Aiming = true;
		animWrapper275.EquippedType = typeof(LongGun);
		animWrapper275.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper276 = AddActionAnim(ActionAnim.Reload, "SittingReload", "RifleReloadSitting");
		animWrapper276.Sitting = true;
		animWrapper276.Aiming = true;
		animWrapper276.EquippedType = typeof(LongGun);
		animWrapper276.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.5f));
		AnimWrapper animWrapper277 = AddActionAnim(ActionAnim.EquipRifle, "Pistol_EquipIdle", "RifleEquip", "RifleEquip");
		animWrapper277.Speed = 1.5f;
		animWrapper277.EquippedType = typeof(LongGun);
		animWrapper277.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper278 = AddActionAnim(ActionAnim.Unequip, "Pistol_UnequipIdle", "RifleUnequip", "RifleUnequip");
		animWrapper278.Speed = 1.5f;
		animWrapper278.EquippedType = typeof(LongGun);
		animWrapper278.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper279 = AddActionAnim(ActionAnim.EquipRifle, "Pistol_EquipCrouch", "RifleCrouchEquip", "RifleCrouchEquip");
		animWrapper279.Speed = 1.5f;
		animWrapper279.EquippedType = typeof(LongGun);
		animWrapper279.Crouching = true;
		animWrapper279.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper280 = AddActionAnim(ActionAnim.Unequip, "Pistol_UnequipCrouch", "RifleCrouchUnequip", "RifleCrouchUnequip");
		animWrapper280.Speed = 1.5f;
		animWrapper280.EquippedType = typeof(LongGun);
		animWrapper280.Crouching = true;
		animWrapper280.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper281 = AddActionAnim(ActionAnim.EquipRifle, "Srv_SitBonfire_Equip", "RifleSittingEquip");
		animWrapper281.EquippedType = typeof(LongGun);
		animWrapper281.Sitting = true;
		animWrapper281.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper282 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "RifleSittingUnequip");
		animWrapper282.EquippedType = typeof(LongGun);
		animWrapper282.Sitting = true;
		animWrapper282.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper283 = AddActionAnim(ActionAnim.Kick, "Rifle_Melee_Kick", "RifleKick");
		animWrapper283.Aiming = true;
		animWrapper283.TargetRange = 1.5f;
		animWrapper283.Speed = 0.833f;
		animWrapper283.EquippedType = typeof(LongGun);
		animWrapper283.InterruptibleFrac = 0.7f;
		animWrapper283.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.3f));
		animWrapper283.HasRootMotion = true;
		animWrapper283.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper284 = AddActionAnim(ActionAnim.Kick, "Rifle_KickOnGround", "RifleKickOnGround");
		animWrapper284.Aiming = true;
		animWrapper284.TargetOnGround = true;
		animWrapper284.TargetRange = 1.5f;
		animWrapper284.EquippedType = typeof(LongGun);
		animWrapper284.InterruptibleFrac = 0.8f;
		animWrapper284.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.42f));
		animWrapper284.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromRight,
			ActionAnim.Damaged_Head_FromRight
		};
		foreach (ActionAnim action8 in array)
		{
			AnimWrapper animWrapper285 = AddActionAnim(action8, "Rifle_Hit_L_2", "RifleHitHeadFromRight");
			animWrapper285.HasRootMotion = true;
			animWrapper285.Aiming = true;
			animWrapper285.EquippedType = typeof(LongGun);
			animWrapper285.Speed = 1.5f;
			animWrapper285.InterruptibleFrac = 0.375f;
			animWrapper285.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromCentre,
			ActionAnim.Damaged_Head_FromCentre
		};
		foreach (ActionAnim action9 in array)
		{
			AnimWrapper animWrapper286 = AddActionAnim(action9, "Rifle_Hit_C_1", "RifleHitHeadFromCentre");
			animWrapper286.HasRootMotion = true;
			animWrapper286.Aiming = true;
			animWrapper286.EquippedType = typeof(LongGun);
			animWrapper286.Speed = 1.5f;
			animWrapper286.InterruptibleFrac = 0.375f;
			animWrapper286.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromLeft,
			ActionAnim.Damaged_Head_FromLeft
		};
		foreach (ActionAnim action10 in array)
		{
			AnimWrapper animWrapper287 = AddActionAnim(action10, "Rifle_Hit_R_2", "RifleHitHeadFromLeft");
			animWrapper287.HasRootMotion = true;
			animWrapper287.Aiming = true;
			animWrapper287.EquippedType = typeof(LongGun);
			animWrapper287.Speed = 1.5f;
			animWrapper287.InterruptibleFrac = 0.375f;
			animWrapper287.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper288 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "RifleHitTorsoFromBehind");
		animWrapper288.HasRootMotion = true;
		animWrapper288.Aiming = true;
		animWrapper288.EquippedType = typeof(LongGun);
		animWrapper288.Speed = 1.5f;
		animWrapper288.InterruptibleFrac = 0.375f;
		animWrapper288.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper289 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "RifleHitHeadFromBehind");
		animWrapper289.HasRootMotion = true;
		animWrapper289.Aiming = true;
		animWrapper289.EquippedType = typeof(LongGun);
		animWrapper289.Speed = 1.5f;
		animWrapper289.InterruptibleFrac = 0.375f;
		animWrapper289.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper290 = AddActionAnim(ActionAnim.Block_Kick, "Pistol_BlockKick", "RifleBlockKick");
		animWrapper290.Aiming = true;
		animWrapper290.EquippedType = typeof(LongGun);
		AnimWrapper animWrapper291 = AddActionAnim(ActionAnim.Dodge_Backwards, "Sword1h_Dodge", "RifleDodgeBackwards");
		animWrapper291.HasRootMotion = true;
		animWrapper291.Aiming = true;
		animWrapper291.EquippedType = typeof(LongGun);
		animWrapper291.InterruptibleFrac = 0.4f;
		animWrapper291.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper292 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "RifleDodgeForwards");
		animWrapper292.HasRootMotion = true;
		animWrapper292.Aiming = true;
		animWrapper292.CanTurnDuringAnim = false;
		animWrapper292.EquippedType = typeof(LongGun);
		animWrapper292.InterruptibleFrac = 0.7f;
		animWrapper292.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper293 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "RifleDodgeLeft");
		animWrapper293.HasRootMotion = true;
		animWrapper293.Aiming = true;
		animWrapper293.EquippedType = typeof(LongGun);
		animWrapper293.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper294 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "RifleDodgeRight");
		animWrapper294.HasRootMotion = true;
		animWrapper294.Aiming = true;
		animWrapper294.EquippedType = typeof(LongGun);
		animWrapper294.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper295 = AddActionAnim(ActionAnim.Vault, "Rifle_Vault_1m", "RifleVault");
		animWrapper295.EquippedType = typeof(LongGun);
		animWrapper295.HasRootMotion = true;
		animWrapper295.TransitionInTime = 0.25f;
		animWrapper295.VaultLoopStartFrac = 0.22f;
		animWrapper295.VaultLoopEndFrac = 0.5f;
		animWrapper295.InterruptibleFrac = 0.7f;
		animWrapper295.Speed = 1.25f;
		animWrapper295.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper296 = AddActionAnim(ActionAnim.Fire, "Bow_Shoot_2_Aim_02", "BowFire", "BowFire");
		animWrapper296.EquipmentStateNameHash = Animator.StringToHash("BowFire");
		animWrapper296.UpperBodyIsAdditive = true;
		animWrapper296.Aiming = true;
		animWrapper296.EquippedType = typeof(Bow);
		animWrapper296.InterruptibleFrac = 0.33f;
		animWrapper296.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		animWrapper296.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.28f));
		AnimWrapper animWrapper297 = AddActionAnim(ActionAnim.Fire, "Bow_Crouch_Shoot_2_Aim_02", "BowCrouchFire", "BowCrouchFire");
		animWrapper297.EquipmentStateNameHash = Animator.StringToHash("BowCrouchFire");
		animWrapper297.UpperBodyIsAdditive = true;
		animWrapper297.Crouching = true;
		animWrapper297.Aiming = true;
		animWrapper297.EquippedType = typeof(Bow);
		animWrapper297.InterruptibleFrac = 0.33f;
		animWrapper297.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		animWrapper297.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.28f));
		AnimWrapper animWrapper298 = AddActionAnim(ActionAnim.FireLastArrow, "Bow_Shoot_LastArrow", "BowFireLastArrow", "BowFireLastArrow");
		animWrapper298.EquipmentStateNameHash = Animator.StringToHash("BowFireLastArrow");
		animWrapper298.UpperBodyIsAdditive = true;
		animWrapper298.Aiming = true;
		animWrapper298.EquippedType = typeof(Bow);
		animWrapper298.InterruptibleFrac = 0.33f;
		animWrapper298.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper299 = AddActionAnim(ActionAnim.FireLastArrow, "Bow_Crouching_Shoot_LastArrow", "BowCrouchFireLastArrow", "BowCrouchFireLastArrow");
		animWrapper299.EquipmentStateNameHash = Animator.StringToHash("BowCrouchFireLastArrow");
		animWrapper299.UpperBodyIsAdditive = true;
		animWrapper299.Crouching = true;
		animWrapper299.Aiming = true;
		animWrapper299.EquippedType = typeof(Bow);
		animWrapper299.InterruptibleFrac = 0.33f;
		animWrapper299.Events.Add(new AnimEvent(AnimationEventType.Fire, 0f));
		AnimWrapper animWrapper300 = AddActionAnim(ActionAnim.Reload, "Bow_Reload", "BowReload", "BowReload");
		animWrapper300.EquipmentStateNameHash = Animator.StringToHash("BowReload");
		animWrapper300.UpperBodyIsAdditive = true;
		animWrapper300.EquippedType = typeof(Bow);
		animWrapper300.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.28f));
		AnimWrapper animWrapper301 = AddActionAnim(ActionAnim.Reload, "Bow_Crouching_Reload", "BowCrouchReload", "BowCrouchReload");
		animWrapper301.EquipmentStateNameHash = Animator.StringToHash("BowCrouchReload");
		animWrapper301.UpperBodyIsAdditive = true;
		animWrapper301.Crouching = true;
		animWrapper301.EquippedType = typeof(Bow);
		animWrapper301.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.28f));
		AnimWrapper animWrapper302 = AddActionAnim(ActionAnim.EquipBow, "Bow_Equip", "BowEquip", "BowEquip");
		animWrapper302.Speed = 3f;
		animWrapper302.EquippedType = typeof(Bow);
		animWrapper302.Events.Add(new AnimEvent(AnimationEventType.Equip, 0.44f));
		animWrapper302.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.45f));
		AnimWrapper animWrapper303 = AddActionAnim(ActionAnim.Unequip, "Bow_Unequip", "BowUnequip", "BowUnequip");
		animWrapper303.Speed = 3f;
		animWrapper303.EquippedType = typeof(Bow);
		animWrapper303.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.52f));
		AnimWrapper animWrapper304 = AddActionAnim(ActionAnim.EquipBow, "Bow_Crouch_Equip", "BowCrouchEquip", "BowCrouchEquip");
		animWrapper304.Speed = 3f;
		animWrapper304.EquippedType = typeof(Bow);
		animWrapper304.Crouching = true;
		animWrapper304.Events.Add(new AnimEvent(AnimationEventType.Equip, 0.44f));
		animWrapper304.Events.Add(new AnimEvent(AnimationEventType.Reload, 0.45f));
		AnimWrapper animWrapper305 = AddActionAnim(ActionAnim.Unequip, "Bow_Crouch_Unequip", "BowCrouchUnequip", "BowCrouchUnequip");
		animWrapper305.Speed = 3f;
		animWrapper305.EquippedType = typeof(Bow);
		animWrapper305.Crouching = true;
		animWrapper305.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.56f));
		AnimWrapper animWrapper306 = AddActionAnim(ActionAnim.EquipBow, "Srv_SitBonfire_Equip", "BowSittingEquip");
		animWrapper306.EquippedType = typeof(Bow);
		animWrapper306.Sitting = true;
		animWrapper306.Events.Add(new AnimEvent(AnimationEventType.Equip, 0f));
		AnimWrapper animWrapper307 = AddActionAnim(ActionAnim.Unequip, "Srv_SitBonfire_Unequip", "BowSittingUnequip");
		animWrapper307.EquippedType = typeof(Bow);
		animWrapper307.Sitting = true;
		animWrapper307.Events.Add(new AnimEvent(AnimationEventType.Unequip, 0.9f));
		AnimWrapper animWrapper308 = AddActionAnim(ActionAnim.Kick, "Pistol_Melee_Kick", "BowKick");
		animWrapper308.Aiming = true;
		animWrapper308.TargetRange = 1.5f;
		animWrapper308.Speed = 0.833f;
		animWrapper308.EquippedType = typeof(Bow);
		animWrapper308.InterruptibleFrac = 0.7f;
		animWrapper308.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.3f));
		animWrapper308.HasRootMotion = true;
		animWrapper308.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper309 = AddActionAnim(ActionAnim.Kick, "Pistol_KickOnGround", "BowKickOnGround");
		animWrapper309.Aiming = true;
		animWrapper309.TargetOnGround = true;
		animWrapper309.TargetRange = 1.5f;
		animWrapper309.EquippedType = typeof(Bow);
		animWrapper309.InterruptibleFrac = 0.8f;
		animWrapper309.Events.Add(new AnimEvent(AnimationEventType.KickRight, 0.42f));
		animWrapper309.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromRight,
			ActionAnim.Damaged_Head_FromRight
		};
		foreach (ActionAnim action11 in array)
		{
			AnimWrapper animWrapper310 = AddActionAnim(action11, "Bow_Hit_Reaction_R_01_Aim", "BowAimingHitFromRight");
			animWrapper310.EquipmentStateNameHash = Animator.StringToHash("BowAimingHitFromRight");
			animWrapper310.HasRootMotion = true;
			animWrapper310.Aiming = true;
			animWrapper310.EquippedType = typeof(Bow);
			animWrapper310.Speed = 1.5f;
			animWrapper310.InterruptibleFrac = 0.375f;
			animWrapper310.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper311 = AddActionAnim(ActionAnim.Damaged_Head_FromCentre, "Bow_Hit_Reatcion_F_03_Aim", "BowAimingHitHeadFromCentre");
		animWrapper311.EquipmentStateNameHash = Animator.StringToHash("BowAimingHitHeadFromCentre");
		animWrapper311.HasRootMotion = true;
		animWrapper311.Aiming = true;
		animWrapper311.EquippedType = typeof(Bow);
		animWrapper311.Speed = 1.5f;
		animWrapper311.InterruptibleFrac = 0.375f;
		animWrapper311.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper312 = AddActionAnim(ActionAnim.Damaged_Torso_FromCentre, "Bow_Hit_Reaction_F_04_Aim", "BowAimingHitFromCentre");
		animWrapper312.EquipmentStateNameHash = Animator.StringToHash("BowAimingHitFromCentre");
		animWrapper312.HasRootMotion = true;
		animWrapper312.Aiming = true;
		animWrapper312.EquippedType = typeof(Bow);
		animWrapper312.Speed = 1.5f;
		animWrapper312.InterruptibleFrac = 0.375f;
		animWrapper312.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromLeft,
			ActionAnim.Damaged_Head_FromLeft
		};
		foreach (ActionAnim action12 in array)
		{
			AnimWrapper animWrapper313 = AddActionAnim(action12, "Bow_Hit_Reaction_L_01_Aim", "BowAimingHitFromLeft");
			animWrapper313.EquipmentStateNameHash = Animator.StringToHash("BowAimingHitFromLeft");
			animWrapper313.HasRootMotion = true;
			animWrapper313.Aiming = true;
			animWrapper313.EquippedType = typeof(Bow);
			animWrapper313.Speed = 1.5f;
			animWrapper313.InterruptibleFrac = 0.375f;
			animWrapper313.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromBehind,
			ActionAnim.Damaged_Head_FromBehind
		};
		foreach (ActionAnim action13 in array)
		{
			AnimWrapper animWrapper314 = AddActionAnim(action13, "Bow_Hit_Reaction_B_01_Aim", "BowAimingHitFromBehind");
			animWrapper314.EquipmentStateNameHash = Animator.StringToHash("BowAimingHitFromBehind");
			animWrapper314.HasRootMotion = true;
			animWrapper314.Aiming = true;
			animWrapper314.EquippedType = typeof(Bow);
			animWrapper314.Speed = 1.5f;
			animWrapper314.InterruptibleFrac = 0.375f;
			animWrapper314.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromRight,
			ActionAnim.Damaged_Head_FromRight
		};
		foreach (ActionAnim action14 in array)
		{
			AnimWrapper animWrapper315 = AddActionAnim(action14, "Bow_Hit_Reaction_R_01", "BowIdleHitFromRight");
			animWrapper315.HasRootMotion = true;
			animWrapper315.EquippedType = typeof(Bow);
			animWrapper315.Speed = 1.5f;
			animWrapper315.InterruptibleFrac = 0.375f;
			animWrapper315.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper316 = AddActionAnim(ActionAnim.Damaged_Head_FromCentre, "Bow_Hit_Reatcion_F_03", "BowIdleHitHeadFromCentre");
		animWrapper316.HasRootMotion = true;
		animWrapper316.EquippedType = typeof(Bow);
		animWrapper316.Speed = 1.5f;
		animWrapper316.InterruptibleFrac = 0.375f;
		animWrapper316.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper317 = AddActionAnim(ActionAnim.Damaged_Torso_FromCentre, "Bow_Hit_Reaction_F_04", "BowIdleHitFromCentre");
		animWrapper317.HasRootMotion = true;
		animWrapper317.EquippedType = typeof(Bow);
		animWrapper317.Speed = 1.5f;
		animWrapper317.InterruptibleFrac = 0.375f;
		animWrapper317.PipView = PipAnimView.FullBodySide;
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromLeft,
			ActionAnim.Damaged_Head_FromLeft
		};
		foreach (ActionAnim action15 in array)
		{
			AnimWrapper animWrapper318 = AddActionAnim(action15, "Bow_Hit_Reaction_L_01", "BowIdleHitFromLeft");
			animWrapper318.HasRootMotion = true;
			animWrapper318.EquippedType = typeof(Bow);
			animWrapper318.Speed = 1.5f;
			animWrapper318.InterruptibleFrac = 0.375f;
			animWrapper318.PipView = PipAnimView.FullBodySide;
		}
		array = new ActionAnim[2]
		{
			ActionAnim.Damaged_Torso_FromBehind,
			ActionAnim.Damaged_Head_FromBehind
		};
		foreach (ActionAnim action16 in array)
		{
			AnimWrapper animWrapper319 = AddActionAnim(action16, "Bow_Hit_Reaction_B_01", "BowIdleHitFromBehind");
			animWrapper319.HasRootMotion = true;
			animWrapper319.EquippedType = typeof(Bow);
			animWrapper319.Speed = 1.5f;
			animWrapper319.InterruptibleFrac = 0.375f;
			animWrapper319.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper320 = AddActionAnim(ActionAnim.Block_Kick, "Pistol_BlockKick", "BowBlockKick");
		animWrapper320.Aiming = true;
		animWrapper320.EquippedType = typeof(Bow);
		AnimWrapper animWrapper321 = AddActionAnim(ActionAnim.Dodge_Backwards, "Bow_Dodge_B_01_Aim", "BowDodgeBackwards");
		animWrapper321.HasRootMotion = true;
		animWrapper321.Aiming = true;
		animWrapper321.EquippedType = typeof(Bow);
		animWrapper321.InterruptibleFrac = 0.7f;
		animWrapper321.Speed = 2f;
		animWrapper321.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper322 = AddActionAnim(ActionAnim.Dodge_Forwards, "Bow_Dodge_F_01_Aim", "BowDodgeForwards");
		animWrapper322.HasRootMotion = true;
		animWrapper322.Aiming = true;
		animWrapper322.CanTurnDuringAnim = false;
		animWrapper322.EquippedType = typeof(Bow);
		animWrapper322.InterruptibleFrac = 0.6f;
		animWrapper322.Speed = 2f;
		animWrapper322.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper323 = AddActionAnim(ActionAnim.Dodge_Left, "Bow_Dodge_L_01_Aim", "BowDodgeLeft");
		animWrapper323.HasRootMotion = true;
		animWrapper323.Aiming = true;
		animWrapper323.EquippedType = typeof(Bow);
		animWrapper323.InterruptibleFrac = 0.7f;
		animWrapper323.Speed = 2f;
		AnimWrapper animWrapper324 = AddActionAnim(ActionAnim.Dodge_Right, "Bow_Dodge_R_01_Aim", "BowDodgeRight");
		animWrapper324.HasRootMotion = true;
		animWrapper324.Aiming = true;
		animWrapper324.EquippedType = typeof(Bow);
		animWrapper324.InterruptibleFrac = 0.7f;
		animWrapper324.Speed = 2f;
		AnimWrapper animWrapper325 = AddActionAnim(ActionAnim.Vault, "Vault1m", "BowVault");
		animWrapper325.EquippedType = typeof(Bow);
		animWrapper325.HasRootMotion = true;
		animWrapper325.TransitionInTime = 0.25f;
		animWrapper325.VaultLoopStartFrac = 0.2f;
		animWrapper325.VaultLoopEndFrac = 0.38f;
		animWrapper325.InterruptibleFrac = 0.5f;
		animWrapper325.Speed = 1.5f;
		animWrapper325.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper326 = AddActionAnim(ActionAnim.BittenFrontStruggle, "FrontStruggle", "BittenFrontStruggle");
		animWrapper326.Looped = true;
		animWrapper326.IsInteractionWithObject = true;
		animWrapper326.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper327 = AddActionAnim(ActionAnim.BittenRearStruggle, "RearStruggle", "BittenRearStruggle");
		animWrapper327.Looped = true;
		animWrapper327.IsInteractionWithObject = true;
		animWrapper327.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper328 = AddActionAnim(ActionAnim.BittenPinnedStruggle, "PinnedStruggle", "BittenPinnedStruggle");
		animWrapper328.Looped = true;
		animWrapper328.IsInteractionWithObject = true;
		animWrapper328.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper329 = AddActionAnim(ActionAnim.BittenFrontFree, "FrontFree", "BittenFrontFree");
		animWrapper329.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper329.InterruptibleFrac = 0.85f;
		animWrapper329.IsInteractionWithObject = true;
		animWrapper329.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper330 = AddActionAnim(ActionAnim.BittenRearFree, "RearFree", "BittenRearFree");
		animWrapper330.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0.2f));
		animWrapper330.InterruptibleFrac = 0.85f;
		animWrapper330.IsInteractionWithObject = true;
		animWrapper330.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper331 = AddActionAnim(ActionAnim.BittenPinnedFree, "PinnedFree", "BittenPinnedFree");
		animWrapper331.Events.Add(new AnimEvent(AnimationEventType.StruggleFree, 0f));
		animWrapper331.InterruptibleFrac = 0.85f;
		animWrapper331.IsInteractionWithObject = true;
		animWrapper331.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper332 = AddActionAnim(ActionAnim.Frustration, "Zombie_HyperAttack_1", "ZombieFrustration");
		animWrapper332.Zombie = true;
		for (int m = 0; m < 10; m++)
		{
			animWrapper332.Events.Add(new AnimEvent(AnimationEventType.GrrArgh, (float)m / 10f));
		}
		AnimWrapper animWrapper333 = AddActionAnim(ActionAnim.ZombieEatCorpse, "ZombieEatingLoop", "ZombieEatCorpse");
		animWrapper333.PipView = PipAnimView.ZombieEating;
		animWrapper333.Zombie = true;
		animWrapper333.Looped = true;
		for (int n = 0; n < 10; n++)
		{
			animWrapper333.Events.Add(new AnimEvent(AnimationEventType.EatCorpse, (float)n / 10f));
		}
		AnimWrapper animWrapper334 = AddActionAnim(ActionAnim.Panic, "Zombie_OnFire_2B_Looping", "ZombiePanic1");
		animWrapper334.Zombie = true;
		animWrapper334.HasRootMotion = true;
		animWrapper334.Looped = true;
		for (int num = 0; num < 10; num++)
		{
			animWrapper334.Events.Add(new AnimEvent(AnimationEventType.GrrArgh, (float)num / 10f));
		}
		AnimWrapper animWrapper335 = AddActionAnim(ActionAnim.Panic, "Zombie_OnFire_4A_Looping", "ZombiePanic2");
		animWrapper335.Zombie = true;
		animWrapper335.HasRootMotion = true;
		animWrapper335.Looped = true;
		for (int num2 = 0; num2 < 3; num2++)
		{
			animWrapper335.Events.Add(new AnimEvent(AnimationEventType.GrrArgh, (float)num2 / 3f));
		}
		AnimWrapper animWrapper336 = AddActionAnim(ActionAnim.Panic, "Zombie_OnFire_3A_Looping", "ZombiePanic3");
		animWrapper336.Zombie = true;
		animWrapper336.HasRootMotion = true;
		animWrapper336.Looped = true;
		for (int num3 = 0; num3 < 10; num3++)
		{
			animWrapper336.Events.Add(new AnimEvent(AnimationEventType.GrrArgh, (float)num3 / 10f));
		}
		for (int num4 = 0; num4 < 2; num4++)
		{
			bool flag3 = num4 == 1;
			AnimWrapper animWrapper337 = AddActionAnim(ActionAnim.ZombieBiteStart, "ZombieBiteStart", flag3 ? "ZombieBiteStart" : "BiteStart");
			animWrapper337.IsInteractionWithObject = true;
			animWrapper337.Zombie = flag3;
			animWrapper337.HasRootMotion = true;
			animWrapper337.CanTurnDuringAnim = false;
			animWrapper337.Events.Add(new AnimEvent(AnimationEventType.ChanceToBlockZombieBite, 0f));
			animWrapper337.PipView = PipAnimView.FullBodySide;
			AnimWrapper animWrapper338 = AddActionAnim(ActionAnim.ZombieBitePrepare, "ZombieBitePrepare", flag3 ? "ZombieBitePrepare" : "BitePrepare");
			animWrapper338.IsInteractionWithObject = true;
			animWrapper338.Zombie = flag3;
			animWrapper338.Speed = 0.75f;
			animWrapper338.PipView = PipAnimView.FullBodySide;
			AnimWrapper animWrapper339 = AddActionAnim(ActionAnim.ZombieBiteLoop, "ZombieBiteLoop", flag3 ? "ZombieBiteLoop" : "BiteLoop");
			animWrapper339.IsInteractionWithObject = true;
			animWrapper339.Zombie = flag3;
			animWrapper339.Looped = true;
			animWrapper339.Speed = 0.75f;
			animWrapper339.Events.Add(new AnimEvent(AnimationEventType.BiteLoopStart, 0f));
			animWrapper339.Events.Add(new AnimEvent(AnimationEventType.Bite, 0.5f));
			animWrapper339.PipView = PipAnimView.FullBodySide;
			AnimWrapper animWrapper340 = AddActionAnim(ActionAnim.ZombieBiteFinish, "Zombie_Atk_End_1", flag3 ? "ZombieBiteFinish" : "BiteFinish");
			animWrapper340.IsInteractionWithObject = true;
			animWrapper340.Zombie = flag3;
			animWrapper340.HasRootMotion = true;
			animWrapper340.PipView = PipAnimView.FullBodySide;
			AnimWrapper animWrapper341 = AddActionAnim(ActionAnim.ZombieBiteFail, "Zombie_Atk_KnockBack_1", flag3 ? "ZombieBiteFail" : "BiteFail");
			animWrapper341.IsInteractionWithObject = true;
			animWrapper341.Zombie = flag3;
			animWrapper341.HasRootMotion = true;
			animWrapper341.Speed = 1.5f;
			animWrapper341.PipView = PipAnimView.FullBodySide;
			AnimWrapper animWrapper342 = AddActionAnim(ActionAnim.ZombieBiteStumble, "ZombieBiteStumble", flag3 ? "ZombieBiteStumble" : "BiteStumble");
			animWrapper342.Zombie = flag3;
			animWrapper342.HasRootMotion = true;
			animWrapper342.CanTurnDuringAnim = false;
			animWrapper342.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper343 = AddActionAnim(ActionAnim.ZombieJump, "ZombieJump", "ZombieJump");
		animWrapper343.Zombie = true;
		animWrapper343.CanTurnDuringAnim = false;
		animWrapper343.HasRootMotion = true;
		animWrapper343.Speed = 1.5f;
		animWrapper343.Events.Add(new AnimEvent(AnimationEventType.ChanceToBlockZombieJump, 0f));
		animWrapper343.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper344 = AddActionAnim(ActionAnim.ZombieJumpBiteStart, "ZombieJumpBiteStart", "ZombieJumpBiteStart");
		animWrapper344.IsInteractionWithObject = true;
		animWrapper344.Zombie = true;
		animWrapper344.CanTurnDuringAnim = false;
		animWrapper344.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper345 = AddActionAnim(ActionAnim.ZombieJumpBitePrepare, "ZombieJumpBitePrepare", "ZombieJumpBitePrepare");
		animWrapper345.IsInteractionWithObject = true;
		animWrapper345.Zombie = true;
		animWrapper345.Speed = 0.75f;
		animWrapper345.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper346 = AddActionAnim(ActionAnim.ZombieJumpBiteLoop, "ZombieJumpBiteLoop", "ZombieJumpBiteLoop");
		animWrapper346.IsInteractionWithObject = true;
		animWrapper346.Zombie = true;
		animWrapper346.Looped = true;
		animWrapper346.Speed = 0.75f;
		animWrapper346.Events.Add(new AnimEvent(AnimationEventType.BiteLoopStart, 0f));
		animWrapper346.Events.Add(new AnimEvent(AnimationEventType.Bite, 0.5f));
		animWrapper346.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper347 = AddActionAnim(ActionAnim.ZombieJumpBiteFinish, "ZombieJumpBiteFinish", "ZombieJumpBiteFinish");
		animWrapper347.IsInteractionWithObject = true;
		animWrapper347.Zombie = true;
		animWrapper347.HasRootMotion = true;
		animWrapper347.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper348 = AddActionAnim(ActionAnim.ZombieJumpBiteFail, "ZombieJumpBiteFail", "ZombieJumpBiteFail");
		animWrapper348.IsInteractionWithObject = true;
		animWrapper348.Zombie = true;
		animWrapper348.HasRootMotion = true;
		animWrapper348.Speed = 1.5f;
		animWrapper348.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper349 = AddActionAnim(ActionAnim.Slide, "SlideLoop", "ZombieSlide");
		animWrapper349.Zombie = true;
		animWrapper349.Looped = true;
		animWrapper349.TransitionInTime = 0.25f;
		AnimWrapper animWrapper350 = AddActionAnim(ActionAnim.SlideRecover, "SlideRecover", "ZombieSlideRecover");
		animWrapper350.Zombie = true;
		animWrapper350.HasRootMotion = true;
		AnimWrapper animWrapper351 = AddActionAnim(ActionAnim.Vault, "Vault1m", "ZombieVault");
		animWrapper351.IsInteractionWithObject = true;
		animWrapper351.Zombie = true;
		animWrapper351.HasRootMotion = true;
		animWrapper351.TransitionInTime = 0.25f;
		animWrapper351.VaultLoopStartFrac = 0.2f;
		animWrapper351.VaultLoopEndFrac = 0.38f;
		animWrapper351.InterruptibleFrac = 0.5f;
		animWrapper351.Speed = 1.5f;
		animWrapper351.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper352 = AddActionAnim(ActionAnim.Damaged_Head_FromCentre, "Sword1h_Hit_Head_Front", "ZombieHitHeadFromCentre");
		animWrapper352.Zombie = true;
		animWrapper352.HasRootMotion = true;
		animWrapper352.Speed = 1.5f;
		animWrapper352.InterruptibleFrac = 0.75f;
		animWrapper352.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper353 = AddActionAnim(ActionAnim.Damaged_Head_FromRight, "Sword1h_Hit_Head_Left", "ZombieHitHeadFromRight");
		animWrapper353.Zombie = true;
		animWrapper353.HasRootMotion = true;
		animWrapper353.Speed = 1.5f;
		animWrapper353.InterruptibleFrac = 0.75f;
		animWrapper353.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper354 = AddActionAnim(ActionAnim.Damaged_Head_FromLeft, "Sword1h_Hit_Head_Right", "ZombieHitHeadFromLeft");
		animWrapper354.Zombie = true;
		animWrapper354.HasRootMotion = true;
		animWrapper354.Speed = 1.5f;
		animWrapper354.InterruptibleFrac = 0.75f;
		animWrapper354.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper355 = AddActionAnim(ActionAnim.Damaged_Head_FromBehind, "Idle_Hit_Head_Behind", "ZombieHitHeadFromBehind");
		animWrapper355.Zombie = true;
		animWrapper355.HasRootMotion = true;
		animWrapper355.Speed = 1.5f;
		animWrapper355.InterruptibleFrac = 0.75f;
		animWrapper355.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper356 = AddActionAnim(ActionAnim.Damaged_Torso_FromCentre, "Sword1h_Hit_Torso_Front", "ZombieHitTorsoFromCentre");
		animWrapper356.Zombie = true;
		animWrapper356.HasRootMotion = true;
		animWrapper356.Speed = 1.5f;
		animWrapper356.InterruptibleFrac = 0.75f;
		animWrapper356.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper357 = AddActionAnim(ActionAnim.Damaged_Torso_FromRight, "Sword1h_Hit_Torso_Left", "ZombieHitTorsoFromRight");
		animWrapper357.Zombie = true;
		animWrapper357.HasRootMotion = true;
		animWrapper357.Speed = 1.5f;
		animWrapper357.InterruptibleFrac = 0.75f;
		animWrapper357.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper358 = AddActionAnim(ActionAnim.Damaged_Torso_FromLeft, "Sword1h_Hit_Torso_Right", "ZombieHitTorsoFromLeft");
		animWrapper358.Zombie = true;
		animWrapper358.HasRootMotion = true;
		animWrapper358.Speed = 1.5f;
		animWrapper358.InterruptibleFrac = 0.75f;
		animWrapper358.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper359 = AddActionAnim(ActionAnim.Damaged_Torso_FromBehind, "Idle_Hit_Behind", "ZombieHitTorsoFromBehind");
		animWrapper359.Zombie = true;
		animWrapper359.HasRootMotion = true;
		animWrapper359.Speed = 1.5f;
		animWrapper359.InterruptibleFrac = 0.75f;
		animWrapper359.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper360 = AddActionAnim(ActionAnim.Damaged_RightLeg, "Sword1h_Hit_Legs_Left", "ZombieHitLegFromRight");
		animWrapper360.Zombie = true;
		animWrapper360.HasRootMotion = true;
		animWrapper360.Speed = 1.5f;
		animWrapper360.InterruptibleFrac = 0.75f;
		animWrapper360.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper361 = AddActionAnim(ActionAnim.Damaged_LeftLeg, "Sword1h_Hit_Legs_Right", "ZombieHitLegFromLeft");
		animWrapper361.Zombie = true;
		animWrapper361.HasRootMotion = true;
		animWrapper361.Speed = 1.5f;
		animWrapper361.InterruptibleFrac = 0.75f;
		animWrapper361.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper362 = AddActionAnim(ActionAnim.Dodge_Backwards, "Zombie_Chase_2_KnockBack_Chase", "ZombieDodgeBackwards");
		animWrapper362.Zombie = true;
		animWrapper362.HasRootMotion = true;
		animWrapper362.Aiming = true;
		animWrapper362.Speed = 1.5f;
		animWrapper362.InterruptibleFrac = 0.3f;
		animWrapper362.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper363 = AddActionAnim(ActionAnim.Dodge_Forwards, "Sword1h_Dodge_Fwd", "ZombieDodgeForwards");
		animWrapper363.Zombie = true;
		animWrapper363.HasRootMotion = true;
		animWrapper363.Aiming = true;
		animWrapper363.CanTurnDuringAnim = false;
		animWrapper363.InterruptibleFrac = 0.7f;
		animWrapper363.PipView = PipAnimView.FullBodySide;
		AnimWrapper animWrapper364 = AddActionAnim(ActionAnim.Dodge_Left, "Sword1h_Dodge_Left", "ZombieDodgeLeft");
		animWrapper364.Zombie = true;
		animWrapper364.HasRootMotion = true;
		animWrapper364.Aiming = true;
		animWrapper364.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper365 = AddActionAnim(ActionAnim.Dodge_Right, "Sword1h_Dodgle_Right", "ZombieDodgeRight");
		animWrapper365.Zombie = true;
		animWrapper365.HasRootMotion = true;
		animWrapper365.Aiming = true;
		animWrapper365.InterruptibleFrac = 0.7f;
		AnimWrapper animWrapper366 = AddActionAnim(ActionAnim.GetUp_Back, "GetUpFromBack", "ZombieGetUpBack");
		animWrapper366.Zombie = true;
		animWrapper366.CanTurnDuringAnim = false;
		animWrapper366.TransitionInTime = 0f;
		animWrapper366.Speed = 2f;
		AnimWrapper animWrapper367 = AddActionAnim(ActionAnim.GetUp_Front, "GetUpFromFace", "ZombieGetUpFront");
		animWrapper367.Zombie = true;
		animWrapper367.CanTurnDuringAnim = false;
		animWrapper367.TransitionInTime = 0f;
		animWrapper367.Speed = 2f;
		AnimWrapper animWrapper368 = AddActionAnim(ActionAnim.Alert, "Zombie_Idle_2", "ZombieAlert");
		animWrapper368.Zombie = true;
		animWrapper368.CanTurnDuringAnim = false;
		AnimWrapper animWrapper369 = AddActionAnim(ActionAnim.Eat, "IdleSniffleAround", "RabbitEat");
		animWrapper369.Species = BaseObjectType.Rabbit;
		animWrapper369.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AddActionAnim(ActionAnim.LookAround, "IdleAlertLookAround", "AlertLookAround").Species = BaseObjectType.Rabbit;
		AnimWrapper animWrapper370 = AddActionAnim(ActionAnim.Eat, "GrazeOnce", "DoeEat");
		animWrapper370.Species = BaseObjectType.Deer;
		animWrapper370.Gender = GenderType.Female;
		animWrapper370.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AnimWrapper animWrapper371 = AddActionAnim(ActionAnim.LookAround, "IdleLookAround", "DoeLookAround");
		animWrapper371.Species = BaseObjectType.Deer;
		animWrapper371.Gender = GenderType.Female;
		animWrapper371.TransitionInTime = 0.5f;
		array = AllGetUpAnims;
		foreach (ActionAnim action17 in array)
		{
			AnimWrapper animWrapper372 = AddActionAnim(action17, "SleepToGoBackUp", "DoeGetUp");
			animWrapper372.Species = BaseObjectType.Deer;
			animWrapper372.Gender = GenderType.Female;
			animWrapper372.CanTurnDuringAnim = false;
			animWrapper372.TransitionInTime = 0f;
			animWrapper372.Speed = 2f;
		}
		array = AllDamagedAnims;
		foreach (ActionAnim action18 in array)
		{
			AnimWrapper animWrapper373 = AddActionAnim(action18, "GetHit", "DoeDamaged");
			animWrapper373.Species = BaseObjectType.Deer;
			animWrapper373.Gender = GenderType.Female;
			animWrapper373.Speed = 1.5f;
			animWrapper373.InterruptibleFrac = 0.75f;
			animWrapper373.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper374 = AddActionAnim(ActionAnim.Eat, "GrazeOnce", "StagEat");
		animWrapper374.Species = BaseObjectType.Deer;
		animWrapper374.Gender = GenderType.Male;
		animWrapper374.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AnimWrapper animWrapper375 = AddActionAnim(ActionAnim.LookAround, "IdleLookAround", "StagLookAround");
		animWrapper375.Species = BaseObjectType.Deer;
		animWrapper375.Gender = GenderType.Male;
		animWrapper375.TransitionInTime = 0.5f;
		array = AllGetUpAnims;
		foreach (ActionAnim action19 in array)
		{
			AnimWrapper animWrapper376 = AddActionAnim(action19, "SleepToGoBackUp", "StagGetUp");
			animWrapper376.Species = BaseObjectType.Deer;
			animWrapper376.Gender = GenderType.Male;
			animWrapper376.CanTurnDuringAnim = false;
			animWrapper376.TransitionInTime = 0f;
			animWrapper376.Speed = 2f;
		}
		array = AllDamagedAnims;
		foreach (ActionAnim action20 in array)
		{
			AnimWrapper animWrapper377 = AddActionAnim(action20, "GetHit", "StagDamaged");
			animWrapper377.Species = BaseObjectType.Deer;
			animWrapper377.Gender = GenderType.Male;
			animWrapper377.Speed = 1.5f;
			animWrapper377.InterruptibleFrac = 0.75f;
			animWrapper377.PipView = PipAnimView.FullBodySide;
		}
		AnimWrapper animWrapper378 = AddActionAnim(ActionAnim.Eat, "RoosterEat", "RoosterEat");
		animWrapper378.Species = BaseObjectType.Chicken;
		animWrapper378.Gender = GenderType.Male;
		animWrapper378.Speed = 2f;
		animWrapper378.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AnimWrapper animWrapper379 = AddActionAnim(ActionAnim.RoosterCrow, "RoosterCrow", "RoosterCrow");
		animWrapper379.Species = BaseObjectType.Chicken;
		animWrapper379.Gender = GenderType.Male;
		animWrapper379.Events.Add(new AnimEvent(AnimationEventType.RoosterCrow, 0.1f));
		AnimWrapper animWrapper380 = AddActionAnim(ActionAnim.MaleMating, "RoosterMating", "RoosterMating");
		animWrapper380.Species = BaseObjectType.Chicken;
		animWrapper380.Gender = GenderType.Male;
		animWrapper380.IsInteractionWithObject = true;
		animWrapper380.Speed = 2f;
		animWrapper380.Events.Add(new AnimEvent(AnimationEventType.MatingSound, 0f));
		animWrapper380.Events.Add(new AnimEvent(AnimationEventType.MatingSound, 0.15f));
		animWrapper380.Events.Add(new AnimEvent(AnimationEventType.MatingSound, 0.5f));
		animWrapper380.Events.Add(new AnimEvent(AnimationEventType.MatingSound, 0.83f));
		animWrapper380.Events.Add(new AnimEvent(AnimationEventType.Mate, 0.5f));
		AnimWrapper animWrapper381 = AddActionAnim(ActionAnim.LookAround, "RoosterLookAround", "RoosterLookAround");
		animWrapper381.Species = BaseObjectType.Chicken;
		animWrapper381.Gender = GenderType.Male;
		animWrapper381.Speed = 2f;
		AnimWrapper animWrapper382 = AddActionAnim(ActionAnim.SitByFire, "RoosterSit", "RoosterSit");
		animWrapper382.Species = BaseObjectType.Chicken;
		animWrapper382.Gender = GenderType.Male;
		animWrapper382.Events.Add(new AnimEvent(AnimationEventType.StartSitting, 0.5f));
		AnimWrapper animWrapper383 = AddActionAnim(ActionAnim.StopSittingByFire, "RoosterGetUp", "RoosterGetUp");
		animWrapper383.Species = BaseObjectType.Chicken;
		animWrapper383.Gender = GenderType.Male;
		animWrapper383.CanTurnDuringAnim = false;
		animWrapper383.Events.Add(new AnimEvent(AnimationEventType.StopSitting, 0.5f));
		array = AllDamagedAnims;
		foreach (ActionAnim action21 in array)
		{
			AnimWrapper animWrapper384 = AddActionAnim(action21, "RoosterHit", "RoosterHit");
			animWrapper384.Species = BaseObjectType.Chicken;
			animWrapper384.Gender = GenderType.Male;
			animWrapper384.Speed = 2f;
			animWrapper384.InterruptibleFrac = 0.75f;
		}
		array = AllGetUpAnims;
		foreach (ActionAnim action22 in array)
		{
			AnimWrapper animWrapper385 = AddActionAnim(action22, "RoosterGetUp", "RoosterGetUp");
			animWrapper385.Species = BaseObjectType.Chicken;
			animWrapper385.Gender = GenderType.Male;
			animWrapper385.CanTurnDuringAnim = false;
			animWrapper385.TransitionInTime = 0f;
			animWrapper385.Speed = 2f;
		}
		AnimWrapper animWrapper386 = AddActionAnim(ActionAnim.Carried, "RoosterResting", "RoosterCarried");
		animWrapper386.Species = BaseObjectType.Chicken;
		animWrapper386.Gender = GenderType.Male;
		animWrapper386.Looped = true;
		AnimWrapper animWrapper387 = AddActionAnim(ActionAnim.Eat, "HenEat", "HenEat");
		animWrapper387.Species = BaseObjectType.Chicken;
		animWrapper387.Gender = GenderType.Female;
		animWrapper387.Speed = 2f;
		animWrapper387.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AnimWrapper animWrapper388 = AddActionAnim(ActionAnim.LookAround, "HenLookAround", "HenLookAround");
		animWrapper388.Species = BaseObjectType.Chicken;
		animWrapper388.Speed = 2f;
		animWrapper388.Gender = GenderType.Female;
		AnimWrapper animWrapper389 = AddActionAnim(ActionAnim.FemaleMating, "HenMating", "HenMating");
		animWrapper389.Species = BaseObjectType.Chicken;
		animWrapper389.Gender = GenderType.Female;
		animWrapper389.Speed = 2f;
		animWrapper389.IsInteractionWithObject = true;
		AnimWrapper animWrapper390 = AddActionAnim(ActionAnim.SitByFire, "HenSit", "HenSit");
		animWrapper390.Species = BaseObjectType.Chicken;
		animWrapper390.Gender = GenderType.Female;
		animWrapper390.Events.Add(new AnimEvent(AnimationEventType.StartSitting, 0.5f));
		AnimWrapper animWrapper391 = AddActionAnim(ActionAnim.StopSittingByFire, "HenGetUp", "HenGetUp");
		animWrapper391.Species = BaseObjectType.Chicken;
		animWrapper391.Gender = GenderType.Female;
		animWrapper391.CanTurnDuringAnim = false;
		animWrapper391.Events.Add(new AnimEvent(AnimationEventType.StopSitting, 0.5f));
		array = AllDamagedAnims;
		foreach (ActionAnim action23 in array)
		{
			AnimWrapper animWrapper392 = AddActionAnim(action23, "HenHit", "HenHit");
			animWrapper392.Species = BaseObjectType.Chicken;
			animWrapper392.Gender = GenderType.Female;
			animWrapper392.Speed = 2f;
			animWrapper392.InterruptibleFrac = 0.75f;
		}
		array = AllGetUpAnims;
		foreach (ActionAnim action24 in array)
		{
			AnimWrapper animWrapper393 = AddActionAnim(action24, "HenGetUp", "HenGetUp");
			animWrapper393.Species = BaseObjectType.Chicken;
			animWrapper393.Gender = GenderType.Female;
			animWrapper393.CanTurnDuringAnim = false;
			animWrapper393.TransitionInTime = 0f;
			animWrapper393.Speed = 2f;
		}
		AnimWrapper animWrapper394 = AddActionAnim(ActionAnim.Carried, "HenResting", "HenCarried");
		animWrapper394.Species = BaseObjectType.Chicken;
		animWrapper394.Gender = GenderType.Female;
		animWrapper394.Looped = true;
		AnimWrapper animWrapper395 = AddActionAnim(ActionAnim.Eat, "ChickEat", "ChickEat");
		animWrapper395.Species = BaseObjectType.Chicken;
		animWrapper395.Youth = true;
		animWrapper395.Speed = 2f;
		animWrapper395.Events.Add(new AnimEvent(AnimationEventType.FinishEat, 0.99f));
		AnimWrapper animWrapper396 = AddActionAnim(ActionAnim.LookAround, "ChickIdle2", "ChickLookAround");
		animWrapper396.Species = BaseObjectType.Chicken;
		animWrapper396.Youth = true;
		animWrapper396.Speed = 2f;
		AnimWrapper animWrapper397 = AddActionAnim(ActionAnim.SitByFire, "ChickSleepStart", "ChickSit");
		animWrapper397.Species = BaseObjectType.Chicken;
		animWrapper397.Youth = true;
		animWrapper397.Speed = 2f;
		animWrapper397.Events.Add(new AnimEvent(AnimationEventType.StartSitting, 0.5f));
		AnimWrapper animWrapper398 = AddActionAnim(ActionAnim.StopSittingByFire, "ChickSleepEnd", "ChickGetUp");
		animWrapper398.Species = BaseObjectType.Chicken;
		animWrapper398.Youth = true;
		animWrapper398.CanTurnDuringAnim = false;
		animWrapper398.Speed = 2f;
		animWrapper398.Events.Add(new AnimEvent(AnimationEventType.StopSitting, 0.5f));
		array = AllDamagedAnims;
		foreach (ActionAnim action25 in array)
		{
			AnimWrapper animWrapper399 = AddActionAnim(action25, "ChickHitFront", "ChickHit");
			animWrapper399.Species = BaseObjectType.Chicken;
			animWrapper399.Youth = true;
			animWrapper399.Speed = 2f;
			animWrapper399.InterruptibleFrac = 0.75f;
		}
		array = AllGetUpAnims;
		foreach (ActionAnim action26 in array)
		{
			AnimWrapper animWrapper400 = AddActionAnim(action26, "ChickSleepEnd", "ChickGetUp");
			animWrapper400.Species = BaseObjectType.Chicken;
			animWrapper400.Youth = true;
			animWrapper400.CanTurnDuringAnim = false;
			animWrapper400.TransitionInTime = 0f;
			animWrapper400.Speed = 2f;
		}
		AnimWrapper animWrapper401 = AddActionAnim(ActionAnim.Carried, "ChickSleep", "ChickCarried");
		animWrapper401.Species = BaseObjectType.Chicken;
		animWrapper401.Youth = true;
		animWrapper401.Looped = true;
		for (int num5 = 0; num5 < Anims.Length; num5++)
		{
			if (Anims[num5] == null)
			{
				continue;
			}
			foreach (AnimWrapper item in Anims[num5])
			{
				if (item.HasRootMotion)
				{
					item.RootMotion = new Resource<SavedRootMotion>("RootMotions/" + item.ClipName);
				}
			}
		}
	}

	public void OnAllContentLoaded()
	{
		for (int i = 0; i < Anims.Length; i++)
		{
			if (Anims[i] == null)
			{
				continue;
			}
			foreach (AnimWrapper item in Anims[i])
			{
				item.Clip = FindClip(item.Species, item.Gender, item.Youth, item.Zombie, item.ClipName);
				if (item.Clip == null)
				{
					Debug.LogError("Clip not found for " + item.BaseStateName + ": " + item.ClipName);
				}
				else
				{
					if (item.Events == null)
					{
						continue;
					}
					item.ClipLength = item.Clip.length;
					foreach (AnimEvent @event in item.Events)
					{
						@event.Time = TimeSpan.FromSeconds(@event.NormalizedTime * item.Clip.length / item.Speed);
					}
					TimeSpan timeSpan = TimeSpan.FromSeconds(0.1);
					for (int num = item.Events.Count - 1; num >= 0; num--)
					{
						switch (item.Events[num].EventType)
						{
						case AnimationEventType.MeleeWeaponQuickAttack:
						case AnimationEventType.MeleeWeaponHeavyAttack:
						{
							AnimationEventType eventType = AnimationEventType.Invalid;
							switch (item.TargetBodyLocation)
							{
							case TargettableBodyLocation.Head:
								eventType = AnimationEventType.ChanceToParryHigh;
								break;
							case TargettableBodyLocation.Torso:
								eventType = AnimationEventType.ChanceToParryMiddle;
								break;
							case TargettableBodyLocation.Legs:
								eventType = AnimationEventType.ChanceToParryLow;
								break;
							}
							TimeSpan time3 = MathUtil.Max(item.Events[num].Time - timeSpan, TimeSpan.Zero);
							AnimEvent animEvent3 = new AnimEvent(eventType, (float)time3.TotalSeconds * item.Speed / item.Clip.length);
							animEvent3.Time = time3;
							item.Events.Add(animEvent3);
							break;
						}
						case AnimationEventType.KickLeft:
						case AnimationEventType.KickRight:
						{
							TimeSpan time2 = MathUtil.Max(item.Events[num].Time - timeSpan, TimeSpan.Zero);
							AnimEvent animEvent2 = new AnimEvent(AnimationEventType.ChanceToDodgeKick, (float)time2.TotalSeconds * item.Speed / item.Clip.length);
							animEvent2.Time = time2;
							item.Events.Add(animEvent2);
							break;
						}
						case AnimationEventType.PunchLeft:
						case AnimationEventType.PunchRight:
						{
							TimeSpan time = MathUtil.Max(item.Events[num].Time - timeSpan, TimeSpan.Zero);
							AnimEvent animEvent = new AnimEvent(AnimationEventType.ChanceToBlockPunch, (float)time.TotalSeconds * item.Speed / item.Clip.length);
							animEvent.Time = time;
							item.Events.Add(animEvent);
							break;
						}
						}
					}
					item.Events.Sort();
				}
			}
		}
	}

	public AnimWrapper GetAnim(Character character, TileObject targetObj, ActionAnim actionAnim, TimeSpan time)
	{
		bool flag = character.IsAiming();
		bool flag2 = character.IsCrouching();
		bool flag3 = character.IsSitting();
		bool flag4 = character.IsYouth();
		if ((int)actionAnim >= Anims.Length || actionAnim < ActionAnim.None)
		{
			return null;
		}
		List<AnimWrapper> list = Anims[(int)actionAnim];
		if (list != null && list.Count > 0)
		{
			tmp1.Clear();
			foreach (AnimWrapper item in list)
			{
				if (item.Species == character.GetBaseObjectType() && item.Zombie == character.Zombie)
				{
					tmp1.Add(item);
				}
			}
			list = tmp1;
			tmp2.Clear();
			foreach (AnimWrapper item2 in list)
			{
				if (item2.EquippedType != null && character.EquippedItem != null && item2.EquippedType.IsInstanceOfType(character.EquippedItem) && (!(item2.NotEquippedSubType != null) || !item2.NotEquippedSubType.IsInstanceOfType(character.EquippedItem)))
				{
					tmp2.Add(item2);
				}
			}
			if (tmp2.Count == 0)
			{
				foreach (AnimWrapper item3 in list)
				{
					if (item3.EquippedType == null)
					{
						tmp2.Add(item3);
					}
				}
			}
			if (tmp2.Count == 0)
			{
				list.CopyToList(tmp2);
			}
			list = tmp2;
			Character character2 = targetObj as Character;
			Vector2 vector = ((character2 != null) ? MathUtil.ToXZ(character2.GetOldVelocity() * 60f * 0.5f) : Vector2.zero);
			float num = ((targetObj != null) ? (targetObj.PosXZ + vector - character.PosXZ).magnitude : float.MaxValue);
			bool flag5 = character2 != null && (character2.IsRagdollOrProneOrRecovering() || character2.IsSmallAnimal());
			flag5 = flag5 || character2 is Rabbit;
			tmp7.Clear();
			foreach (AnimWrapper item4 in list)
			{
				if ((!(item4.TargetOnGround && flag5) || !(num > item4.TargetRange)) && item4.TargetOnGround == flag5)
				{
					tmp7.Add(item4);
				}
			}
			if (tmp7.Count == 0)
			{
				list.CopyToList(tmp7);
			}
			list = tmp7;
			TargettableBodyLocation currentTargetBodyLocation = character.GetCurrentTargetBodyLocation();
			tmp3.Clear();
			foreach (AnimWrapper item5 in list)
			{
				if (item5.TargetBodyLocation == currentTargetBodyLocation)
				{
					tmp3.Add(item5);
				}
			}
			if (tmp3.Count == 0)
			{
				list.CopyToList(tmp3);
			}
			list = tmp3;
			tmp4.Clear();
			foreach (AnimWrapper item6 in list)
			{
				if (item6.Aiming == flag)
				{
					tmp4.Add(item6);
				}
			}
			if (tmp4.Count == 0)
			{
				list.CopyToList(tmp4);
			}
			list = tmp4;
			tmp5.Clear();
			foreach (AnimWrapper item7 in list)
			{
				if (item7.Crouching == flag2)
				{
					tmp5.Add(item7);
				}
			}
			if (tmp5.Count == 0)
			{
				list.CopyToList(tmp5);
			}
			list = tmp5;
			tmp11.Clear();
			foreach (AnimWrapper item8 in list)
			{
				if (item8.Youth == flag4)
				{
					tmp11.Add(item8);
				}
			}
			if (tmp11.Count == 0)
			{
				list.CopyToList(tmp11);
			}
			list = tmp11;
			tmp6.Clear();
			float num2 = float.MaxValue;
			foreach (AnimWrapper item9 in list)
			{
				if (item9.TargetRange == 0f || num <= item9.TargetRange)
				{
					num2 = Mathf.Min(num2, item9.TargetRange);
				}
			}
			foreach (AnimWrapper item10 in list)
			{
				if (item10.TargetRange == num2)
				{
					tmp6.Add(item10);
				}
			}
			if (tmp6.Count == 0)
			{
				if (targetObj != null)
				{
					foreach (AnimWrapper item11 in list)
					{
						if (item11.RootMotion != null)
						{
							tmp6.Add(item11);
						}
					}
				}
				if (tmp6.Count == 0)
				{
					list.CopyToList(tmp6);
				}
			}
			list = tmp6;
			tmp8.Clear();
			foreach (AnimWrapper item12 in list)
			{
				if ((!item12.NotAgainstDirectControlled || character2 == null || !character2.DirectControlled) && (!item12.OnlyAgainstDirectControlled || (character2 != null && character2.DirectControlled)))
				{
					tmp8.Add(item12);
				}
			}
			list = tmp8;
			tmp9.Clear();
			foreach (AnimWrapper item13 in list)
			{
				if (item13.Sitting == flag3)
				{
					tmp9.Add(item13);
				}
			}
			if (tmp9.Count == 0)
			{
				list.CopyToList(tmp9);
			}
			list = tmp9;
			GenderType genderType = character2?.GetGender() ?? GenderType.Count;
			tmp10.Clear();
			foreach (AnimWrapper item14 in list)
			{
				if (item14.Gender == character.GetGender() && item14.TargetGender == genderType)
				{
					tmp10.Add(item14);
				}
			}
			if (tmp10.Count == 0)
			{
				foreach (AnimWrapper item15 in list)
				{
					if (item15.Gender == character.GetGender() && item15.TargetGender == GenderType.Count)
					{
						tmp10.Add(item15);
					}
				}
			}
			if (tmp10.Count == 0)
			{
				foreach (AnimWrapper item16 in list)
				{
					if (item16.Gender == GenderType.Count && item16.TargetGender == GenderType.Count)
					{
						tmp10.Add(item16);
					}
				}
			}
			if (tmp10.Count == 0)
			{
				list.CopyToList(tmp10);
			}
			list = tmp10;
			if (list.Count == 0)
			{
				Debug.LogWarning("Couldn't find anim for " + actionAnim.ToString() + " " + character.GetBaseObjectType().ToString() + (character.Zombie ? " zombie" : "") + (flag ? " aiming" : "") + (flag2 ? " crouching" : ""));
				return null;
			}
			return list[MathUtil.RandomInt((int)time.Ticks + character.Id, list.Count)];
		}
		return null;
	}

	private static int GetAnimHash(ActionAnim action, string name)
	{
		return name.GetHashCode() ^ action.GetHashCode();
	}

	public AnimWrapper GetAnimByName(ActionAnim action, string name)
	{
		if (action == ActionAnim.None)
		{
			if (!string.IsNullOrEmpty(name))
			{
				Debug.LogError("Did not expect an anim name with action " + action.ToString() + " anim: " + name);
			}
			return null;
		}
		int animHash = GetAnimHash(action, name);
		if (AnimsByName.TryGetValue(animHash, out var value))
		{
			return value;
		}
		Debug.LogError("Could not find " + action.ToString() + " anim: " + name);
		return null;
	}
}
