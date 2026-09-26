using System;
using UnityEngine;

namespace UMA.PoseTools;

public class UMAExpressionPlayer : ExpressionPlayer
{
	public UMAExpressionSet expressionSet;

	public UMASkeleton Skeleton;

	public float minWeight;

	public float HeadSize;

	private int jawHash;

	private int neckHash;

	private int headHash;

	private bool initialized;

	private Animator UnityAnimator;

	[NonSerialized]
	public int SlotUpdateVsCharacterUpdate;

	public bool logResetErrors;

	public bool WantExpensiveUpdate;

	private const float eyeMovementRange = 30f;

	private const float mutualGazeRange = 0.1f;

	private const float MinSaccadeDelay = 0.25f;

	private static float SaccadeRangeX = 1f;

	private static float SaccadeRangeY = 0.25f;

	public void Initialize(UMASkeleton skeleton, float headSize)
	{
		Skeleton = skeleton;
		HeadSize = headSize;
		blinkDelay = Mathf.Lerp(minBlinkDelay, maxBlinkDelay, MathUtil.NonDeterministicRand.RandomFloat());
		if (expressionSet != null)
		{
			UnityAnimator = GetComponent<Animator>();
			jawHash = Animator.StringToHash("Mandible");
			neckHash = Animator.StringToHash("Neck");
			headHash = Animator.StringToHash("Head");
			initialized = true;
		}
	}

	public void Update()
	{
		if (!initialized || Skeleton == null)
		{
			return;
		}
		if (WantExpensiveUpdate)
		{
			Quaternion rotation = Skeleton.GetRotation(headHash);
			Quaternion rotation2 = Skeleton.GetRotation(neckHash);
			expressionSet.RestoreBones(Skeleton, logResetErrors);
			if (!overrideMecanimNeck)
			{
				Skeleton.SetRotation(neckHash, rotation2);
			}
			if (!overrideMecanimHead)
			{
				Skeleton.SetRotation(headHash, rotation);
			}
		}
		Skeleton.SetScale(headHash, new Vector3(Mathf.Clamp(1f + (HeadSize - 0.5f) * 2f, 0.5f, 2f), Mathf.Clamp(1f + (HeadSize - 0.5f) * 2f, 0.5f, 2f), Mathf.Clamp(1f + (HeadSize - 0.5f) * 2f, 0.5f, 2f)));
		if (gazeWeight > 0f && UnityAnimator != null)
		{
			UnityAnimator.SetLookAtPosition(gazeTarget);
			UnityAnimator.SetLookAtWeight(gazeWeight);
		}
	}

	public void LateUpdate()
	{
		if (!initialized || Skeleton == null)
		{
			return;
		}
		if (enableSaccades)
		{
			UpdateSaccades();
		}
		if (enableBlinking)
		{
			UpdateBlinking();
		}
		float[] values = base.Values;
		MecanimJoint mecanimJoint = MecanimJoint.None;
		if (!overrideMecanimNeck)
		{
			mecanimJoint |= MecanimJoint.Neck;
		}
		if (!overrideMecanimHead)
		{
			mecanimJoint |= MecanimJoint.Head;
		}
		if (!overrideMecanimJaw)
		{
			mecanimJoint |= MecanimJoint.Jaw;
		}
		if (!overrideMecanimEyes)
		{
			mecanimJoint |= MecanimJoint.Eye;
		}
		if (overrideMecanimJaw)
		{
			Skeleton.Restore(jawHash);
		}
		if (!WantExpensiveUpdate)
		{
			return;
		}
		WantExpensiveUpdate = false;
		for (int i = 0; i < values.Length; i++)
		{
			if ((ExpressionPlayer.MecanimAlternate[i] & mecanimJoint) != MecanimJoint.None)
			{
				continue;
			}
			float num = values[i];
			if (num != 0f)
			{
				UMABonePose uMABonePose = null;
				if (num > 0f)
				{
					uMABonePose = expressionSet.posePairs[i].primary;
				}
				else
				{
					num = 0f - num;
					uMABonePose = expressionSet.posePairs[i].inverse;
				}
				if (num > minWeight && uMABonePose != null)
				{
					uMABonePose.ApplyPose(Skeleton, num);
				}
			}
		}
	}

	protected void UpdateSaccades()
	{
		saccadeDelay -= Time.deltaTime;
		if (saccadeDelay < 0f)
		{
			saccadeTargetPrev = saccadeTarget;
			switch (MathUtil.NonDeterministicRand.Next(0, 4))
			{
			case 0:
				saccadeTarget.Set(MathUtil.NonDeterministicRand.GaussianRandom(0f, SaccadeRangeX), 0f);
				break;
			case 1:
				saccadeTarget.Set(0f - MathUtil.NonDeterministicRand.GaussianRandom(0f, SaccadeRangeX), 0f);
				break;
			case 2:
				saccadeTarget.Set(0f, MathUtil.NonDeterministicRand.GaussianRandom(0f, SaccadeRangeY));
				break;
			default:
				saccadeTarget.Set(0f, 0f - MathUtil.NonDeterministicRand.GaussianRandom(0f, SaccadeRangeY));
				break;
			}
			float num = Mathf.Lerp(0.01f, 15f, MathUtil.NonDeterministicRand.RandomFloat());
			float num2 = -0.23f * Mathf.Log(num / 15.7f);
			saccadeDuration = 0.021f + 0.0022f * num2 * 30f;
			saccadeProgress = 0f;
			if (gazeMode == GazeMode.Listening)
			{
				if (Mathf.Abs(num2) < 0.1f)
				{
					saccadeDelay = MathUtil.NonDeterministicRand.GaussianRandom(7.9166665f, 1.5699999f);
				}
				else
				{
					saccadeDelay = MathUtil.NonDeterministicRand.GaussianRandom(13f / 30f, 0.23666666f);
				}
			}
			else if (Mathf.Abs(num2) < 0.1f)
			{
				saccadeDelay = MathUtil.NonDeterministicRand.GaussianRandom(3.13f, 3.1633334f);
			}
			else
			{
				saccadeDelay = MathUtil.NonDeterministicRand.GaussianRandom(0.9266666f, 0.8f);
			}
			if (saccadeDelay < 0.25f)
			{
				saccadeDelay = 0.25f;
			}
			saccadeTarget *= num2;
		}
		if (saccadeProgress < 1f)
		{
			float num3 = Time.deltaTime / saccadeDuration;
			float num4 = 1.5f - 3f * Mathf.Pow(saccadeProgress - 0.5f, 2f);
			saccadeProgress += num3 * num4;
			leftEyeIn_Out = Mathf.Lerp(saccadeTargetPrev.x, saccadeTarget.x, saccadeProgress);
			leftEyeUp_Down = Mathf.Lerp(saccadeTargetPrev.y, saccadeTarget.y, saccadeProgress);
			rightEyeIn_Out = Mathf.Lerp(0f - saccadeTargetPrev.x, 0f - saccadeTarget.x, saccadeProgress);
			rightEyeUp_Down = Mathf.Lerp(saccadeTargetPrev.y, saccadeTarget.y, saccadeProgress);
		}
		else
		{
			leftEyeIn_Out = saccadeTarget.x;
			leftEyeUp_Down = saccadeTarget.y;
			rightEyeIn_Out = 0f - saccadeTarget.x;
			rightEyeUp_Down = saccadeTarget.y;
		}
	}

	protected void UpdateBlinking()
	{
		if (leftEyeOpen_Close < -1f)
		{
			leftEyeOpen_Close = 0f;
		}
		if (rightEyeOpen_Close < -1f)
		{
			rightEyeOpen_Close = 0f;
		}
		blinkDelay -= Time.deltaTime;
		if (!(blinkDelay < blinkDuration))
		{
			return;
		}
		if (blinkDelay < 0f)
		{
			switch (gazeMode)
			{
			case GazeMode.Speaking:
			case GazeMode.Listening:
				blinkDelay = MathUtil.NonDeterministicRand.GaussianRandom(2.3f, 1.1f);
				break;
			case GazeMode.Following:
				blinkDelay = MathUtil.NonDeterministicRand.GaussianRandom(15.4f, 8.2f);
				break;
			default:
				blinkDelay = MathUtil.NonDeterministicRand.GaussianRandom(3.8f, 1.2f);
				break;
			}
			if (blinkDelay < blinkDuration)
			{
				blinkDelay = blinkDuration;
			}
		}
		else
		{
			leftEyeOpen_Close = -1.01f;
			rightEyeOpen_Close = -1.01f;
		}
	}
}
