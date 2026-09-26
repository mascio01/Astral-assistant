using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Animal : Character
{
	public const float SmallAnimalMapDotSize = 1.5f;

	public AnimalPose CurrentPose;

	public bool Stolen;

	private static string UnityUpdateStr = "Animal.UnityUpdate";

	public override MapIconType MapIconType => MapIconType.Animal;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref CurrentPose, 274);
		reflector.AddAfter(ref Stolen, 330);
	}

	public override bool CanParry(Character attacker, AttackType attackType, out bool enabled, out ActionAnim parryAction, out bool isFacingAttacker, out float score)
	{
		enabled = false;
		parryAction = ActionAnim.None;
		isFacingAttacker = false;
		score = 0f;
		return false;
	}

	public override bool CanSetPropName()
	{
		return base.Alive;
	}

	public override float GetPipYaw()
	{
		return MathF.PI / 2f;
	}

	public virtual float GetPipXOffset()
	{
		return 0f;
	}

	public virtual float GetPipYOffset()
	{
		return 0f;
	}

	public virtual float GetPipZOffset()
	{
		return 0f;
	}

	public virtual void PlayIdleSound()
	{
	}

	public virtual void PlayDeathSound()
	{
	}

	public virtual void PlayAlertSound()
	{
	}

	public virtual void PlayFleeSound()
	{
	}

	public override bool LikesFood(EquipmentPrototype proto)
	{
		if (proto.FoodForAnimal != null)
		{
			return proto.FoodForAnimal.Contains(GetBaseObjectType());
		}
		return false;
	}

	public override void OnNewGame()
	{
		base.OnNewGame();
		SetGoal(new AnimalGoal());
	}

	public override void OnDie(float hitRadius, Vector3 nonDeterministicHitPos, Vector3 force, Bone bone, Vector3 posInBoneSpace, CauseOfDeath causeOfDeath, Character killer, SecrecyMode secret)
	{
		base.OnDie(hitRadius, nonDeterministicHitPos, force, bone, posInBoneSpace, causeOfDeath, killer, secret);
		if (CheckFrontmostPrediction(PredictedEventType.DeathSound))
		{
			PlayDeathSound();
		}
	}

	public override void SetCommunity(Community community)
	{
		base.SetCommunity(community);
		Stolen = false;
	}

	protected override void SimulateSurvivalFactors(TimeSpan curTime, float dts)
	{
		base.SimulateSurvivalFactors(curTime, dts);
		if (Stolen && CanReturnAnimalToOldCommunity(this, InitialCommunity, Community))
		{
			InitialCommunity.AddMemberWithNotifications(this);
			Stolen = false;
		}
	}

	public static bool CanReturnAnimalToOldCommunity(Character animal, Community oldCommunity, Community newCommunity)
	{
		if (oldCommunity != null && oldCommunity.BaseRect != TerrainRect.Invalid && oldCommunity.BaseRect.Expand(Target.StolenAnimalOutsideBaseRange).Contains(animal.Tile) && oldCommunity.HasAnyLivingNonZombieMembers())
		{
			if (animal.InsideBuilding != null)
			{
				return animal.InsideBuilding.Community != newCommunity;
			}
			return true;
		}
		return false;
	}

	protected void UnityInitAnimal(Resource<GameObject> prefab, RagdollSettings ragdollSettings, string prefix)
	{
		Unity.Prefab = prefab;
		Unity.Obj = UnityEngine.Object.Instantiate(prefab.GetAsset());
		Unity.Obj.SetActive(value: false);
		Unity.Obj.name = GetDisplayNameString();
		Unity.Obj.layer = Character.CharactersLayer;
		Unity.Obj.AddComponent<CharacterBehaviour>();
		Unity.Animator = Unity.Obj.GetComponent<Animator>();
		Unity.Animator.applyRootMotion = false;
		if (ragdollSettings != null)
		{
			UnitySetupRagdollBones(ragdollSettings, prefix);
		}
		Unity.SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		for (int i = 0; i < Unity.Obj.transform.childCount; i++)
		{
			SkinnedMeshRenderer component = Unity.Obj.transform.GetChild(i).gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component != null)
			{
				component.updateWhenOffscreen = Unity.WantUpdateWhenOffscreen;
				Unity.SkinnedMeshRenderers.Add(component);
			}
		}
		UnityUpdateMeshList();
		UnitySetupAppearance();
		UnitySetupAudioSource();
		UnitySetupCollisionShape();
		UnityPlayInitialAnim();
	}

	public override void UnityUpdate()
	{
		if (IsBeingPredicted())
		{
			return;
		}
		using (new UnityProfileMarker(UnityUpdateStr))
		{
			bool flag = Consciousness == Consciousness.Dead && IsProne() && Unity.CurrentVoiceSoundType == VoiceSoundType.None;
			if (!(CanSkipUnityUpdate && flag))
			{
				CanSkipUnityUpdate = flag;
				UnityEnsureAnimStateIsCorrect();
				UnityUpdatePosition();
				UpdateRecoveringFromRagdoll();
				UpdateLoopingSound();
				if (Unity.Animator != null && Unity.Animator.isInitialized)
				{
					Vector2 vector = MathUtil.GetDirFromAngle(MathUtil.SignedAngleDiff(MovementAngle, FacingAngle)) * MovementSpeed;
					Unity.Animator.SetFloat(AnimHash.VelX, vector.x);
					Unity.Animator.SetFloat(AnimHash.VelZ, vector.y);
					Unity.Animator.SetFloat(AnimHash.Speed, MovementSpeed);
					Unity.Animator.SetBool(AnimHash.Alert, CurrentPose == AnimalPose.Alert);
					Unity.Animator.SetBool(AnimHash.Grazing, CurrentPose == AnimalPose.Grazing);
					Unity.Animator.SetBool(AnimHash.Resting, CurrentPose == AnimalPose.Resting);
				}
				UnityUpdateOverheadIcons();
				if (CarriedBy != null)
				{
					UpdateCarriedBy();
				}
			}
		}
	}

	public override void SetSitting(TerrainCoord sittingAroundTile)
	{
		CurrentPose = AnimalPose.Resting;
		base.SetSitting(sittingAroundTile);
	}

	public override void ClearSitting()
	{
		base.ClearSitting();
		if (CurrentPose == AnimalPose.Resting)
		{
			CurrentPose = AnimalPose.Normal;
		}
	}
}
