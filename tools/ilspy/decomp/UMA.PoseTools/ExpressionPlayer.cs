using UnityEngine;

namespace UMA.PoseTools;

public class ExpressionPlayer : MonoBehaviour
{
	public enum GazeMode
	{
		None,
		Acquiring,
		Following,
		Speaking,
		Listening
	}

	public enum MecanimJoint
	{
		None = 0,
		Head = 1,
		Neck = 2,
		Jaw = 4,
		Eye = 8
	}

	public bool enableBlinking;

	public float blinkDuration = 0.15f;

	public float minBlinkDelay = 3f;

	public float maxBlinkDelay = 15f;

	protected float blinkDelay;

	public bool enableSaccades;

	protected float saccadeDelay = 5f;

	protected float saccadeDuration;

	protected float saccadeProgress = 1f;

	protected Vector2 saccadeTarget;

	protected Vector2 saccadeTargetPrev;

	public Vector3 gazeTarget;

	public float gazeWeight;

	public GazeMode gazeMode;

	public bool overrideMecanimEyes = true;

	public bool overrideMecanimJaw = true;

	public bool overrideMecanimNeck;

	public bool overrideMecanimHead;

	public const int PoseCount = 36;

	public static readonly string[] PoseNames = new string[36]
	{
		"neckUp_Down", "neckLeft_Right", "neckTiltLeft_Right", "headUp_Down", "headLeft_Right", "headTiltLeft_Right", "jawOpen_Close", "jawForward_Back", "jawLeft_Right", "mouthLeft_Right",
		"mouthUp_Down", "mouthNarrow_Pucker", "tongueOut", "tongueCurl", "tongueUp_Down", "tongueLeft_Right", "tongueWide_Narrow", "leftMouthSmile_Frown", "rightMouthSmile_Frown", "leftLowerLipUp_Down",
		"rightLowerLipUp_Down", "leftUpperLipUp_Down", "rightUpperLipUp_Down", "leftCheekPuff_Squint", "rightCheekPuff_Squint", "noseSneer", "leftEyeOpen_Close", "rightEyeOpen_Close", "leftEyeUp_Down", "rightEyeUp_Down",
		"leftEyeIn_Out", "rightEyeIn_Out", "browsIn", "leftBrowUp_Down", "rightBrowUp_Down", "midBrowUp_Down"
	};

	public static readonly MecanimJoint[] MecanimAlternate = new MecanimJoint[36]
	{
		MecanimJoint.Neck,
		MecanimJoint.Neck,
		MecanimJoint.Neck,
		MecanimJoint.Head,
		MecanimJoint.Head,
		MecanimJoint.Head,
		MecanimJoint.Jaw,
		MecanimJoint.Jaw,
		MecanimJoint.Jaw,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.Eye,
		MecanimJoint.Eye,
		MecanimJoint.Eye,
		MecanimJoint.Eye,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None,
		MecanimJoint.None
	};

	[Range(-1f, 1f)]
	public float neckUp_Down;

	[Range(-1f, 1f)]
	public float neckLeft_Right;

	[Range(-1f, 1f)]
	public float neckTiltLeft_Right;

	[Range(-1f, 1f)]
	public float headUp_Down;

	[Range(-1f, 1f)]
	public float headLeft_Right;

	[Range(-1f, 1f)]
	public float headTiltLeft_Right;

	[Range(-1f, 1f)]
	public float jawOpen_Close;

	[Range(-1f, 1f)]
	public float jawForward_Back;

	[Range(-1f, 1f)]
	public float jawLeft_Right;

	[Range(-1f, 1f)]
	public float mouthLeft_Right;

	[Range(-1f, 1f)]
	public float mouthUp_Down;

	[Range(-1f, 1f)]
	public float mouthNarrow_Pucker;

	[Range(-1f, 1f)]
	public float tongueOut;

	[Range(0f, 1f)]
	public float tongueCurl;

	[Range(-1f, 1f)]
	public float tongueUp_Down;

	[Range(-1f, 1f)]
	public float tongueLeft_Right;

	[Range(-1f, 1f)]
	public float tongueWide_Narrow;

	[Range(-1f, 1f)]
	public float leftMouthSmile_Frown;

	[Range(-1f, 1f)]
	public float rightMouthSmile_Frown;

	[Range(-1f, 1f)]
	public float leftLowerLipUp_Down;

	[Range(-1f, 1f)]
	public float rightLowerLipUp_Down;

	[Range(-1f, 1f)]
	public float leftUpperLipUp_Down;

	[Range(-1f, 1f)]
	public float rightUpperLipUp_Down;

	[Range(-1f, 1f)]
	public float leftCheekPuff_Squint;

	[Range(-1f, 1f)]
	public float rightCheekPuff_Squint;

	[Range(0f, 1f)]
	public float noseSneer;

	[Range(-1f, 1f)]
	public float leftEyeOpen_Close;

	[Range(-1f, 1f)]
	public float rightEyeOpen_Close;

	[Range(-1f, 1f)]
	public float leftEyeUp_Down;

	[Range(-1f, 1f)]
	public float rightEyeUp_Down;

	[Range(-1f, 1f)]
	public float leftEyeIn_Out;

	[Range(-1f, 1f)]
	public float rightEyeIn_Out;

	[Range(0f, 1f)]
	public float browsIn;

	[Range(-1f, 1f)]
	public float leftBrowUp_Down;

	[Range(-1f, 1f)]
	public float rightBrowUp_Down;

	[Range(-1f, 1f)]
	public float midBrowUp_Down;

	private float[] valueArray = new float[36];

	public float[] Values
	{
		get
		{
			valueArray[0] = neckUp_Down;
			valueArray[1] = neckLeft_Right;
			valueArray[2] = neckTiltLeft_Right;
			valueArray[3] = headUp_Down;
			valueArray[4] = headLeft_Right;
			valueArray[5] = headTiltLeft_Right;
			valueArray[6] = jawOpen_Close;
			valueArray[7] = jawForward_Back;
			valueArray[8] = jawLeft_Right;
			valueArray[9] = mouthLeft_Right;
			valueArray[10] = mouthUp_Down;
			valueArray[11] = mouthNarrow_Pucker;
			valueArray[12] = tongueOut;
			valueArray[13] = tongueCurl;
			valueArray[14] = tongueUp_Down;
			valueArray[15] = tongueLeft_Right;
			valueArray[16] = tongueWide_Narrow;
			valueArray[17] = leftMouthSmile_Frown;
			valueArray[18] = rightMouthSmile_Frown;
			valueArray[19] = leftLowerLipUp_Down;
			valueArray[20] = rightLowerLipUp_Down;
			valueArray[21] = leftUpperLipUp_Down;
			valueArray[22] = rightUpperLipUp_Down;
			valueArray[23] = leftCheekPuff_Squint;
			valueArray[24] = rightCheekPuff_Squint;
			valueArray[25] = noseSneer;
			valueArray[26] = leftEyeOpen_Close;
			valueArray[27] = rightEyeOpen_Close;
			valueArray[28] = leftEyeUp_Down;
			valueArray[29] = rightEyeUp_Down;
			valueArray[30] = leftEyeIn_Out;
			valueArray[31] = rightEyeIn_Out;
			valueArray[32] = browsIn;
			valueArray[33] = leftBrowUp_Down;
			valueArray[34] = rightBrowUp_Down;
			valueArray[35] = midBrowUp_Down;
			return valueArray;
		}
		set
		{
			if (value.Length == 36)
			{
				int num = 0;
				neckUp_Down = value[num++];
				neckLeft_Right = value[num++];
				neckTiltLeft_Right = value[num++];
				headUp_Down = value[num++];
				headLeft_Right = value[num++];
				headTiltLeft_Right = value[num++];
				jawOpen_Close = value[num++];
				jawForward_Back = value[num++];
				jawLeft_Right = value[num++];
				mouthLeft_Right = value[num++];
				mouthUp_Down = value[num++];
				mouthNarrow_Pucker = value[num++];
				tongueOut = value[num++];
				tongueCurl = value[num++];
				tongueUp_Down = value[num++];
				tongueLeft_Right = value[num++];
				tongueWide_Narrow = value[num++];
				leftMouthSmile_Frown = value[num++];
				rightMouthSmile_Frown = value[num++];
				leftLowerLipUp_Down = value[num++];
				rightLowerLipUp_Down = value[num++];
				leftUpperLipUp_Down = value[num++];
				rightUpperLipUp_Down = value[num++];
				leftCheekPuff_Squint = value[num++];
				rightCheekPuff_Squint = value[num++];
				noseSneer = value[num++];
				leftEyeOpen_Close = value[num++];
				rightEyeOpen_Close = value[num++];
				leftEyeUp_Down = value[num++];
				rightEyeUp_Down = value[num++];
				leftEyeIn_Out = value[num++];
				rightEyeIn_Out = value[num++];
				browsIn = value[num++];
				leftBrowUp_Down = value[num++];
				rightBrowUp_Down = value[num++];
				midBrowUp_Down = value[num++];
			}
		}
	}
}
