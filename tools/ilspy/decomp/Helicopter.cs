using System;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : TiltedBuilding
{
	public static string[] HelicopterAnimStateNames = StringUtil.GetEnumNames<HelicopterAnimState>();

	public HelicopterAnimState CurrentHelicopterAnimState;

	public float RotorSpeed;

	public TimeSpan TakeOffStartTime;

	public bool DeleteOnTakeOff;

	public static float MaxRotorSpeed = 720f;

	public static float RotorAcceleration = 180f;

	public static float TakeOffTime = 6f;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Helicopter;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref CurrentHelicopterAnimState, 462);
		reflector.AddAfter(ref RotorSpeed, 462);
		reflector.AddAfter(ref TakeOffStartTime, 462);
		reflector.AddAfter(ref DeleteOnTakeOff, 462);
	}

	public override void Init()
	{
		base.Init();
		if (CurrentHelicopterAnimState != HelicopterAnimState.Idle)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
	}

	public void SetHelicopterAnimState(HelicopterAnimState newState)
	{
		if (CurrentHelicopterAnimState == newState)
		{
			return;
		}
		CurrentHelicopterAnimState = newState;
		AudioSource component = UnityObj.GetComponent<AudioSource>();
		if (component != null)
		{
			if (component.isPlaying && CurrentHelicopterAnimState == HelicopterAnimState.Idle)
			{
				component.Stop();
			}
			else if (!component.isPlaying && CurrentHelicopterAnimState != HelicopterAnimState.Idle)
			{
				component.Play();
			}
		}
		if (CurrentHelicopterAnimState != HelicopterAnimState.Idle)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
		if (CurrentHelicopterAnimState == HelicopterAnimState.TakingOff)
		{
			TakeOffStartTime = Session.Instance.PlayTime;
		}
	}

	public override bool AddTalkToInhabitantActions(Character controlledCharacter, List<AvailableAction> actions)
	{
		if (CurrentHelicopterAnimState != HelicopterAnimState.Idle)
		{
			return false;
		}
		return base.AddTalkToInhabitantActions(controlledCharacter, actions);
	}

	public bool IsTakeOffFinished()
	{
		return Session.Instance.PlayTime - TakeOffStartTime >= TimeSpan.FromSeconds(TakeOffTime);
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		base.PropUpdate(dt, ref stillNeedUpdating);
		stillNeedUpdating = true;
		float num = (float)dt.TotalSeconds;
		RotorSpeed = Math.Min(RotorSpeed + RotorAcceleration * num, MaxRotorSpeed);
	}

	public override bool PropWantDelete()
	{
		if (DeleteOnTakeOff && CurrentHelicopterAnimState == HelicopterAnimState.TakingOff && IsTakeOffFinished())
		{
			return true;
		}
		return base.PropWantDelete();
	}

	public override void UnityInit()
	{
		base.UnityInit();
		if (UnityObj != null)
		{
			HelicopterBehaviour helicopterBehaviour = UnityObj.GetComponent<HelicopterBehaviour>();
			if (helicopterBehaviour == null)
			{
				helicopterBehaviour = UnityObj.AddComponent<HelicopterBehaviour>();
			}
			helicopterBehaviour.Init(this);
			AudioSource audioSource = UnityObj.GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = UnityObj.AddComponent<AudioSource>();
			}
			audioSource.clip = SoundManager.HelicopterSound;
			audioSource.loop = true;
			audioSource.spatialBlend = 1f;
			audioSource.minDistance = SoundManager.AudioRolloffMinDist;
			audioSource.maxDistance = SoundManager.AudioRolloffMaxDist;
			audioSource.RealisticRolloff();
			audioSource.playOnAwake = false;
			if (CurrentHelicopterAnimState != HelicopterAnimState.Idle)
			{
				audioSource.Play();
			}
		}
	}
}
