using UnityEngine;

namespace UMA;

public class BaseSkeletonSetup : MonoBehaviour
{
	private static bool builtHashes;

	protected static int headAdjustHash;

	protected static int neckAdjustHash;

	protected static int leftOuterBreastHash;

	protected static int rightOuterBreastHash;

	protected static int leftEyeHash;

	protected static int rightEyeHash;

	protected static int leftEyeAdjustHash;

	protected static int rightEyeAdjustHash;

	protected static int spine1AdjustHash;

	protected static int spineAdjustHash;

	protected static int lowerBackBellyHash;

	protected static int lowerBackAdjustHash;

	protected static int leftTrapeziusHash;

	protected static int rightTrapeziusHash;

	protected static int leftArmAdjustHash;

	protected static int rightArmAdjustHash;

	protected static int leftForeArmAdjustHash;

	protected static int rightForeArmAdjustHash;

	protected static int leftForeArmTwistAdjustHash;

	protected static int rightForeArmTwistAdjustHash;

	protected static int leftShoulderAdjustHash;

	protected static int rightShoulderAdjustHash;

	protected static int leftUpLegAdjustHash;

	protected static int rightUpLegAdjustHash;

	protected static int leftLegAdjustHash;

	protected static int rightLegAdjustHash;

	protected static int leftGluteusHash;

	protected static int rightGluteusHash;

	protected static int leftEarAdjustHash;

	protected static int rightEarAdjustHash;

	protected static int noseBaseAdjustHash;

	protected static int noseMiddleAdjustHash;

	protected static int leftNoseAdjustHash;

	protected static int rightNoseAdjustHash;

	protected static int upperLipsAdjustHash;

	protected static int mandibleAdjustHash;

	protected static int leftLowMaxilarAdjustHash;

	protected static int rightLowMaxilarAdjustHash;

	protected static int leftCheekAdjustHash;

	protected static int rightCheekAdjustHash;

	protected static int leftLowCheekAdjustHash;

	protected static int rightLowCheekAdjustHash;

	protected static int noseTopAdjustHash;

	protected static int leftEyebrowLowAdjustHash;

	protected static int rightEyebrowLowAdjustHash;

	protected static int leftEyebrowMiddleAdjustHash;

	protected static int rightEyebrowMiddleAdjustHash;

	protected static int leftEyebrowUpAdjustHash;

	protected static int rightEyebrowUpAdjustHash;

	protected static int lipsSuperiorAdjustHash;

	protected static int lipsInferiorAdjustHash;

	protected static int leftLipsSuperiorMiddleAdjustHash;

	protected static int rightLipsSuperiorMiddleAdjustHash;

	protected static int leftLipsInferiorAdjustHash;

	protected static int rightLipsInferiorAdjustHash;

	protected static int leftLipsAdjustHash;

	protected static int rightLipsAdjustHash;

	protected static int globalHash;

	protected static int positionHash;

	protected static int lowerBackHash;

	protected static int headHash;

	protected static int leftArmHash;

	protected static int rightArmHash;

	protected static int leftForeArmHash;

	protected static int rightForeArmHash;

	protected static int leftHandHash;

	protected static int rightHandHash;

	protected static int leftFootHash;

	protected static int rightFootHash;

	protected static int leftUpLegHash;

	protected static int rightUpLegHash;

	protected static int leftShoulderHash;

	protected static int rightShoulderHash;

	protected static int mandibleHash;

	public static void Prepare()
	{
		if (!builtHashes)
		{
			headAdjustHash = Animator.StringToHash("HeadAdjust");
			neckAdjustHash = Animator.StringToHash("NeckAdjust");
			leftOuterBreastHash = Animator.StringToHash("LeftOuterBreast");
			rightOuterBreastHash = Animator.StringToHash("RightOuterBreast");
			leftEyeHash = Animator.StringToHash("LeftEye");
			rightEyeHash = Animator.StringToHash("RightEye");
			leftEyeAdjustHash = Animator.StringToHash("LeftEyeAdjust");
			rightEyeAdjustHash = Animator.StringToHash("RightEyeAdjust");
			spine1AdjustHash = Animator.StringToHash("Spine1Adjust");
			spineAdjustHash = Animator.StringToHash("SpineAdjust");
			lowerBackBellyHash = Animator.StringToHash("LowerBackBelly");
			lowerBackAdjustHash = Animator.StringToHash("LowerBackAdjust");
			leftTrapeziusHash = Animator.StringToHash("LeftTrapezius");
			rightTrapeziusHash = Animator.StringToHash("RightTrapezius");
			leftArmAdjustHash = Animator.StringToHash("LeftArmAdjust");
			rightArmAdjustHash = Animator.StringToHash("RightArmAdjust");
			leftForeArmAdjustHash = Animator.StringToHash("LeftForeArmAdjust");
			rightForeArmAdjustHash = Animator.StringToHash("RightForeArmAdjust");
			leftForeArmTwistAdjustHash = Animator.StringToHash("LeftForeArmTwistAdjust");
			rightForeArmTwistAdjustHash = Animator.StringToHash("RightForeArmTwistAdjust");
			leftShoulderAdjustHash = Animator.StringToHash("LeftShoulderAdjust");
			rightShoulderAdjustHash = Animator.StringToHash("RightShoulderAdjust");
			leftUpLegAdjustHash = Animator.StringToHash("LeftUpLegAdjust");
			rightUpLegAdjustHash = Animator.StringToHash("RightUpLegAdjust");
			leftLegAdjustHash = Animator.StringToHash("LeftLegAdjust");
			rightLegAdjustHash = Animator.StringToHash("RightLegAdjust");
			leftGluteusHash = Animator.StringToHash("LeftGluteus");
			rightGluteusHash = Animator.StringToHash("RightGluteus");
			leftEarAdjustHash = Animator.StringToHash("LeftEarAdjust");
			rightEarAdjustHash = Animator.StringToHash("RightEarAdjust");
			noseBaseAdjustHash = Animator.StringToHash("NoseBaseAdjust");
			noseMiddleAdjustHash = Animator.StringToHash("NoseMiddleAdjust");
			leftNoseAdjustHash = Animator.StringToHash("LeftNoseAdjust");
			rightNoseAdjustHash = Animator.StringToHash("RightNoseAdjust");
			upperLipsAdjustHash = Animator.StringToHash("UpperLipsAdjust");
			mandibleAdjustHash = Animator.StringToHash("MandibleAdjust");
			leftLowMaxilarAdjustHash = Animator.StringToHash("LeftLowMaxilarAdjust");
			rightLowMaxilarAdjustHash = Animator.StringToHash("RightLowMaxilarAdjust");
			leftCheekAdjustHash = Animator.StringToHash("LeftCheekAdjust");
			rightCheekAdjustHash = Animator.StringToHash("RightCheekAdjust");
			leftLowCheekAdjustHash = Animator.StringToHash("LeftLowCheekAdjust");
			rightLowCheekAdjustHash = Animator.StringToHash("RightLowCheekAdjust");
			noseTopAdjustHash = Animator.StringToHash("NoseTopAdjust");
			leftEyebrowLowAdjustHash = Animator.StringToHash("LeftEyebrowLowAdjust");
			rightEyebrowLowAdjustHash = Animator.StringToHash("RightEyebrowLowAdjust");
			leftEyebrowMiddleAdjustHash = Animator.StringToHash("LeftEyebrowMiddleAdjust");
			rightEyebrowMiddleAdjustHash = Animator.StringToHash("RightEyebrowMiddleAdjust");
			leftEyebrowUpAdjustHash = Animator.StringToHash("LeftEyebrowUpAdjust");
			rightEyebrowUpAdjustHash = Animator.StringToHash("RightEyebrowUpAdjust");
			lipsSuperiorAdjustHash = Animator.StringToHash("LipsSuperiorAdjust");
			lipsInferiorAdjustHash = Animator.StringToHash("LipsInferiorAdjust");
			leftLipsSuperiorMiddleAdjustHash = Animator.StringToHash("LeftLipsSuperiorMiddleAdjust");
			rightLipsSuperiorMiddleAdjustHash = Animator.StringToHash("RightLipsSuperiorMiddleAdjust");
			leftLipsInferiorAdjustHash = Animator.StringToHash("LeftLipsInferiorAdjust");
			rightLipsInferiorAdjustHash = Animator.StringToHash("RightLipsInferiorAdjust");
			leftLipsAdjustHash = Animator.StringToHash("LeftLipsAdjust");
			rightLipsAdjustHash = Animator.StringToHash("RightLipsAdjust");
			globalHash = Animator.StringToHash("Global");
			positionHash = Animator.StringToHash("Position");
			lowerBackHash = Animator.StringToHash("LowerBack");
			headHash = Animator.StringToHash("Head");
			leftArmHash = Animator.StringToHash("LeftArm");
			rightArmHash = Animator.StringToHash("RightArm");
			leftForeArmHash = Animator.StringToHash("LeftForeArm");
			rightForeArmHash = Animator.StringToHash("RightForeArm");
			leftHandHash = Animator.StringToHash("LeftHand");
			rightHandHash = Animator.StringToHash("RightHand");
			leftFootHash = Animator.StringToHash("LeftFoot");
			rightFootHash = Animator.StringToHash("RightFoot");
			leftUpLegHash = Animator.StringToHash("LeftUpLeg");
			rightUpLegHash = Animator.StringToHash("RightUpLeg");
			leftShoulderHash = Animator.StringToHash("LeftShoulder");
			rightShoulderHash = Animator.StringToHash("RightShoulder");
			mandibleHash = Animator.StringToHash("Mandible");
			builtHashes = true;
		}
	}
}
