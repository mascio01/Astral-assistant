using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeautifyEffect;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Beautify")]
[HelpURL("https://kronnect.com/support")]
[ImageEffectAllowedInSceneView]
public class Beautify : MonoBehaviour
{
	public enum FrameStyle
	{
		Border,
		CinematicBands
	}

	public static class ShaderParams
	{
		public static int BokehData = Shader.PropertyToID("_BokehData");

		public static int BokehData3 = Shader.PropertyToID("_BokehData3");

		public static int BokehData2 = Shader.PropertyToID("_BokehData2");

		public static int Sharpen = Shader.PropertyToID("_Sharpen");

		public static int Bloom = Shader.PropertyToID("_Bloom");

		public static int BloomTexture = Shader.PropertyToID("_BloomTex");

		public static int BloomTexture1 = Shader.PropertyToID("_BloomTex1");

		public static int BloomTexture2 = Shader.PropertyToID("_BloomTex2");

		public static int BloomTexture3 = Shader.PropertyToID("_BloomTex3");

		public static int BloomTexture4 = Shader.PropertyToID("_BloomTex4");

		public static int BloomWeights2 = Shader.PropertyToID("_BloomWeights2");

		public static int BloomWeights = Shader.PropertyToID("_BloomWeights");

		public static int BloomZDepthBias = Shader.PropertyToID("_BloomLayerZBias");

		public static int BloomTint = Shader.PropertyToID("_BloomTint");

		public static int BloomTint0 = Shader.PropertyToID("_BloomTint0");

		public static int BloomTint1 = Shader.PropertyToID("_BloomTint1");

		public static int BloomTint2 = Shader.PropertyToID("_BloomTint2");

		public static int BloomTint3 = Shader.PropertyToID("_BloomTint3");

		public static int BloomTint4 = Shader.PropertyToID("_BloomTint4");

		public static int BloomTint5 = Shader.PropertyToID("_BloomTint5");

		public static int BloomDepthNearThreshold = Shader.PropertyToID("_BloomNearThreshold");

		public static int BloomDepthThreshold = Shader.PropertyToID("_BloomDepthThreshold");

		public static int BloomSourceTexture = Shader.PropertyToID("_BloomSourceTex");

		public static int BloomSourceDepthTexture = Shader.PropertyToID("_BloomSourceDepth");

		public static int BloomSourceRightEyeDepthTexture = Shader.PropertyToID("_BloomSourceDepthRightEye");

		public static int BloomSourceRightEyeTexture = Shader.PropertyToID("_BloomSourceTexRightEye");

		public static int Purkinje = Shader.PropertyToID("_Purkinje");

		public static int EyeAdaptation = Shader.PropertyToID("_EyeAdaptation");

		public static int CompareData = Shader.PropertyToID("_CompareParams");

		public static int CompareTexture = Shader.PropertyToID("_CompareTex");

		public static int DoFDepthBias = Shader.PropertyToID("_BeautifyDepthBias");

		public static int DoFExclusionCullMode = Shader.PropertyToID("_BeautifyDoFExclusionCullMode");

		public static int DoFTransparencyCullMode = Shader.PropertyToID("_BeautifyDoFTransparencyCullMode");

		public static int DoFTexture = Shader.PropertyToID("_DoFTex");

		public static int DoFExclusionTexture = Shader.PropertyToID("_DofExclusionTexture");

		public static int DoFBokehRT = Shader.PropertyToID("_DoFBokeh");

		public static int DepthTexture = Shader.PropertyToID("_DepthTexture");

		public static int AFTint = Shader.PropertyToID("_AFTint");

		public static int OverlayTexture = Shader.PropertyToID("_OverlayTex");

		public static int SFMainTexture = Shader.PropertyToID("_SFMainTex");

		public static int SFHalo = Shader.PropertyToID("_SunHalo");

		public static int SFSunTint = Shader.PropertyToID("_SunTint");

		public static int SFGhosts4 = Shader.PropertyToID("_SunGhosts4");

		public static int SFGhosts3 = Shader.PropertyToID("_SunGhosts3");

		public static int SFGhosts2 = Shader.PropertyToID("_SunGhosts2");

		public static int SFGhosts1 = Shader.PropertyToID("_SunGhosts1");

		public static int SFCoronaRays1 = Shader.PropertyToID("_SunCoronaRays1");

		public static int SFCoronaRays2 = Shader.PropertyToID("_SunCoronaRays2");

		public static int SFSunData = Shader.PropertyToID("_SunData");

		public static int SFSunPos = Shader.PropertyToID("_SunPos");

		public static int SFSunPosRightEye = Shader.PropertyToID("_SunPosRightEye");

		public static int Frame = Shader.PropertyToID("_Frame");

		public static int FrameMaskTexture = Shader.PropertyToID("_FrameMask");

		public static int FrameData = Shader.PropertyToID("_FrameData");

		public static int OutlineColor = Shader.PropertyToID("_Outline");

		public static int OutlineIntensityMultiplier = Shader.PropertyToID("_OutlineIntensityMultiplier");

		public static int OutlineMinDepthThreshold = Shader.PropertyToID("_OutlineMinDepthThreshold");

		public static int VignetteAspectRatio = Shader.PropertyToID("_VignettingAspectRatio");

		public static int Vignette = Shader.PropertyToID("_Vignetting");

		public static int VignetteMaskTexture = Shader.PropertyToID("_VignettingMask");

		public static int FXData = Shader.PropertyToID("_FXData");

		public static int FXColor = Shader.PropertyToID("_FXColor");

		public static int HardLight = Shader.PropertyToID("_HardLight");

		public static int ColorBoost = Shader.PropertyToID("_ColorBoost");

		public static int ColorBoost2 = Shader.PropertyToID("_ColorBoost2");

		public static int AntialiasData = Shader.PropertyToID("_AntialiasData");

		public static int Dither = Shader.PropertyToID("_Dither");

		public static int Dirt = Shader.PropertyToID("_Dirt");

		public static int ScreenLum = Shader.PropertyToID("_ScreenLum");

		public static int TintColor = Shader.PropertyToID("_TintColor");

		public static int LUT = Shader.PropertyToID("_LUTTex");

		public static int LUT3D = Shader.PropertyToID("_LUT3DTex");

		public static int LUT3DParams = Shader.PropertyToID("_LUT3DParams");

		public static int BlurScale = Shader.PropertyToID("_BlurScale");

		public static int EAHist = Shader.PropertyToID("_EAHist");

		public static int EALumSrc = Shader.PropertyToID("_EALumSrc");

		public static int FlareTexture = Shader.PropertyToID("_FlareTex");

		public static int ChromaticAberration = Shader.PropertyToID("_ChromaticAberrationData");

		public static int LUTPreview = Shader.PropertyToID("_LUTPreview");

		public static int LUTTex = Shader.PropertyToID("_LUTTex");

		public static int TonemapGamma = Shader.PropertyToID("_TonemapGamma");

		public const string SKW_BLOOM = "BEAUTIFY_BLOOM";

		public const string SKW_BLOOM_CONSERVATIVE_THRESHOLD = "BEAUTIFY_BLOOM_PROP_THRESHOLDING";

		public const string SKW_LUT = "BEAUTIFY_LUT";

		public const string SKW_LUT3D = "BEAUTIFY_LUT3D";

		public const string SKW_NIGHT_VISION = "BEAUTIFY_NIGHT_VISION";

		public const string SKW_THERMAL_VISION = "BEAUTIFY_THERMAL_VISION";

		public const string SKW_OUTLINE = "BEAUTIFY_OUTLINE";

		public const string SKW_FRAME = "BEAUTIFY_FRAME";

		public const string SKW_FRAME_MASK = "BEAUTIFY_FRAME_MASK";

		public const string SKW_DALTONIZE = "BEAUTIFY_DALTONIZE";

		public const string SKW_DIRT = "BEAUTIFY_DIRT";

		public const string SKW_VIGNETTING = "BEAUTIFY_VIGNETTING";

		public const string SKW_VIGNETTING_MASK = "BEAUTIFY_VIGNETTING_MASK";

		public const string SKW_DEPTH_OF_FIELD = "BEAUTIFY_DEPTH_OF_FIELD";

		public const string SKW_DEPTH_OF_FIELD_TRANSPARENT = "BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT";

		public const string SKW_EYE_ADAPTATION = "BEAUTIFY_EYE_ADAPTATION";

		public const string SKW_TONEMAP_ACES = "BEAUTIFY_TONEMAP_ACES";

		public const string SKW_TONEMAP_AGX = "BEAUTIFY_TONEMAP_AGX";

		public const string SKW_PURKINJE = "BEAUTIFY_PURKINJE";

		public const string SKW_BLOOM_USE_DEPTH = "BEAUTIFY_BLOOM_USE_DEPTH";

		public const string SKW_BLOOM_USE_LAYER = "BEAUTIFY_BLOOM_USE_LAYER";

		public const string SKW_CHROMATIC_ABERRATION = "BEAUTIFY_CHROMATIC_ABERRATION";
	}

	[SerializeField]
	private BEAUTIFY_PRESET _preset = BEAUTIFY_PRESET.Medium;

	[SerializeField]
	private BEAUTIFY_QUALITY _quality;

	[SerializeField]
	private BeautifyProfile _profile;

	[SerializeField]
	private bool _syncWithProfile = true;

	[SerializeField]
	private bool _compareMode;

	[SerializeField]
	private BEAUTIFY_COMPARE_STYLE _compareStyle;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _comparePanning = 0.25f;

	[SerializeField]
	[Range(-MathF.PI, MathF.PI)]
	private float _compareLineAngle = 1.4f;

	[SerializeField]
	[Range(0.0001f, 0.05f)]
	private float _compareLineWidth = 0.002f;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _dither = 0.02f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _ditherDepth;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sharpenMinDepth;

	[SerializeField]
	[Range(0f, 1.1f)]
	private float _sharpenMaxDepth = 0.999f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sharpenMinMaxDepthFallOff;

	[SerializeField]
	[Range(0f, 15f)]
	private float _sharpen = 2f;

	[SerializeField]
	[Range(0f, 0.05f)]
	private float _sharpenDepthThreshold = 0.035f;

	[SerializeField]
	private Color _tintColor = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _sharpenRelaxation = 0.08f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sharpenClamp = 0.45f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sharpenMotionSensibility = 0.5f;

	[SerializeField]
	[Range(0.01f, 5f)]
	private float _sharpenMotionRestoreSpeed = 0.5f;

	[SerializeField]
	[Range(-2f, 3f)]
	private float _saturate = 1f;

	[SerializeField]
	[Range(0.5f, 1.5f)]
	private float _contrast = 1.02f;

	[SerializeField]
	private float _brightness = 1.05f;

	[SerializeField]
	[Range(0f, 2f)]
	private float _daltonize;

	[SerializeField]
	[Range(0f, 1f)]
	private float _hardLightIntensity = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _hardLightBlend;

	[SerializeField]
	private bool _vignetting;

	[SerializeField]
	private Color _vignettingColor = new Color(0.3f, 0.3f, 0.3f, 0.05f);

	[SerializeField]
	[Range(0f, 1f)]
	private float _vignettingFade;

	[SerializeField]
	private bool _vignettingCircularShape;

	[SerializeField]
	private float _vignettingAspectRatio = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _vignettingBlink;

	[SerializeField]
	private BEAUTIFY_BLINK_STYLE _vignettingBlinkStyle;

	[SerializeField]
	private Vector2 _vignettingCenter = new Vector2(0.5f, 0.5f);

	[SerializeField]
	private Texture2D _vignettingMask;

	[SerializeField]
	private bool _frame;

	[SerializeField]
	private FrameStyle _frameStyle;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _frameBandHorizontalSize;

	[SerializeField]
	[Range(0f, 1f)]
	private float _frameBandHorizontalSmoothness;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float _frameBandVerticalSize = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _frameBandVerticalSmoothness;

	[SerializeField]
	private Color _frameColor = new Color(1f, 1f, 1f, 0.047f);

	[SerializeField]
	private Texture2D _frameMask;

	[SerializeField]
	private bool _lut;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lutIntensity = 1f;

	[SerializeField]
	private Texture2D _lutTexture;

	[SerializeField]
	private Texture3D _lutTexture3D;

	[SerializeField]
	private bool _nightVision;

	[SerializeField]
	private Color _nightVisionColor = new Color(0.5f, 1f, 0.5f, 0.5f);

	[SerializeField]
	private bool _outline;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _outlineColor = new Color(0f, 0f, 0f, 0.8f);

	[SerializeField]
	private bool _outlineCustomize;

	[SerializeField]
	private BEAUTIFY_OUTLINE_STAGE _outlineStage;

	[SerializeField]
	[Range(0f, 1.3f)]
	private float _outlineSpread = 1f;

	[SerializeField]
	[Range(1f, 5f)]
	private int _outlineBlurPassCount = 1;

	[SerializeField]
	[Range(0f, 8f)]
	private float _outlineIntensityMultiplier = 1f;

	[SerializeField]
	private bool _outlineBlurDownscale = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float _outlineMinDepthThreshold;

	[SerializeField]
	private bool _thermalVision;

	[SerializeField]
	private bool _lensDirt;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lensDirtThreshold = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _lensDirtIntensity = 0.9f;

	[SerializeField]
	private Texture2D _lensDirtTexture;

	[SerializeField]
	private bool _bloom;

	[SerializeField]
	private LayerMask _bloomCullingMask;

	[SerializeField]
	[Range(1f, 4f)]
	private float _bloomLayerMaskDownsampling = 1f;

	[SerializeField]
	private float _bloomIntensity = 1f;

	[SerializeField]
	private float _bloomMaxBrightness = 1000f;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost0;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost1;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost2;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost3;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost4;

	[SerializeField]
	[Range(0f, 3f)]
	private float _bloomBoost5;

	[SerializeField]
	private bool _bloomAntiflicker;

	[SerializeField]
	private float _bloomAntiflickerMaxOutput = 10f;

	[SerializeField]
	[Range(3f, 5f)]
	private int _bloomIterations = 4;

	[SerializeField]
	private bool _bloomUltra;

	[SerializeField]
	[Range(1f, 10f)]
	private int _bloomUltraResolution = 10;

	[SerializeField]
	[Range(0f, 5f)]
	private float _bloomThreshold = 0.75f;

	[SerializeField]
	private bool _bloomConservativeThreshold;

	[SerializeField]
	private Color _bloomTint = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	private bool _bloomCustomize;

	[SerializeField]
	private bool _bloomDebug;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight0 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight1 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight2 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight3 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight4 = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _bloomWeight5 = 0.5f;

	[SerializeField]
	private Color _bloomTint0 = Color.white;

	[SerializeField]
	private Color _bloomTint1 = Color.white;

	[SerializeField]
	private Color _bloomTint2 = Color.white;

	[SerializeField]
	private Color _bloomTint3 = Color.white;

	[SerializeField]
	private Color _bloomTint4 = Color.white;

	[SerializeField]
	private Color _bloomTint5 = Color.white;

	[SerializeField]
	private bool _bloomBlur = true;

	[SerializeField]
	private bool _bloomQuickerBlur;

	[SerializeField]
	private float _bloomDepthAtten;

	[SerializeField]
	private float _bloomNearAtten;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _bloomLayerZBias = 0.0001f;

	[SerializeField]
	private BEAUTIFY_PRERENDER_EVENT _preRenderCameraEvent;

	[SerializeField]
	private bool _anamorphicFlares;

	[SerializeField]
	private LayerMask _anamorphicFlaresCullingMask;

	[SerializeField]
	[Range(1f, 4f)]
	private float _anamorphicFlaresLayerMaskDownsampling = 1f;

	[SerializeField]
	private float _anamorphicFlaresIntensity = 1f;

	[SerializeField]
	private bool _anamorphicFlaresAntiflicker;

	[SerializeField]
	private float _anamorphicFlaresAntiflickerMaxOutput = 10f;

	[SerializeField]
	private bool _anamorphicFlaresUltra;

	[SerializeField]
	[Range(1f, 10f)]
	private int _anamorphicFlaresUltraResolution = 10;

	[SerializeField]
	[Range(0f, 5f)]
	private float _anamorphicFlaresThreshold = 0.75f;

	[SerializeField]
	[Range(0.1f, 2f)]
	private float _anamorphicFlaresSpread = 1f;

	[SerializeField]
	private bool _anamorphicFlaresVertical;

	[SerializeField]
	private Color _anamorphicFlaresTint = new Color(0.5f, 0.5f, 1f, 0f);

	[SerializeField]
	private bool _anamorphicFlaresBlur = true;

	[SerializeField]
	private bool _depthOfField;

	[SerializeField]
	private bool _depthOfFieldTransparencySupport;

	[SerializeField]
	private LayerMask _depthOfFieldTransparencyLayerMask = -1;

	[SerializeField]
	private CullMode _depthOfFieldTransparencyCullMode = CullMode.Back;

	[SerializeField]
	private Transform _depthOfFieldTargetFocus;

	[SerializeField]
	private bool _depthOfFieldDebug;

	[SerializeField]
	private bool _depthOfFieldAutofocus;

	[SerializeField]
	private Vector2 _depthofFieldAutofocusViewportPoint = new Vector2(0.5f, 0.5f);

	[SerializeField]
	private float _depthOfFieldAutofocusMinDistance;

	[SerializeField]
	private float _depthOfFieldAutofocusDistanceShift;

	[SerializeField]
	private float _depthOfFieldAutofocusMaxDistance = 10000f;

	[SerializeField]
	private LayerMask _depthOfFieldAutofocusLayerMask = -1;

	[SerializeField]
	private LayerMask _depthOfFieldExclusionLayerMask;

	[SerializeField]
	private CullMode _depthOfFieldExclusionCullMode = CullMode.Back;

	[SerializeField]
	[Range(1f, 4f)]
	private float _depthOfFieldExclusionLayerMaskDownsampling = 1f;

	[SerializeField]
	[Range(1f, 4f)]
	private float _depthOfFieldTransparencySupportDownsampling = 1f;

	[SerializeField]
	[Range(0.9f, 1f)]
	private float _depthOfFieldExclusionBias = 0.99f;

	[SerializeField]
	[Range(1f, 100f)]
	private float _depthOfFieldDistance = 1f;

	[SerializeField]
	[Range(0.001f, 5f)]
	private float _depthOfFieldFocusSpeed = 1f;

	[SerializeField]
	[Range(1f, 5f)]
	private int _depthOfFieldDownsampling = 2;

	[SerializeField]
	[Range(2f, 16f)]
	private int _depthOfFieldMaxSamples = 4;

	[SerializeField]
	private BEAUTIFY_DOF_CAMERA_SETTINGS _depthOfFieldCameraSettings;

	[SerializeField]
	[Range(1f, 300f)]
	private float _depthOfFieldFocalLengthReal = 50f;

	[SerializeField]
	[Range(1f, 32f)]
	private float _depthOfFieldFStop = 2f;

	[SerializeField]
	[Range(1f, 48f)]
	private float _depthOfFieldImageSensorHeight = 24f;

	[SerializeField]
	[Range(0.005f, 0.5f)]
	private float _depthOfFieldFocalLength = 0.05f;

	[SerializeField]
	private float _depthOfFieldAperture = 2.8f;

	[SerializeField]
	private bool _depthOfFieldForegroundBlur = true;

	[SerializeField]
	private bool _depthOfFieldForegroundBlurHQ;

	[SerializeField]
	[Range(0f, 32f)]
	private float _depthOfFieldForegroundBlurHQSpread = 16f;

	[SerializeField]
	private float _depthOfFieldForegroundDistance = 0.25f;

	[SerializeField]
	private bool _depthOfFieldBokeh = true;

	[SerializeField]
	private BEAUTIFY_BOKEH_COMPOSITION _depthOfFieldBokehComposition;

	[SerializeField]
	[Range(0.5f, 3f)]
	private float _depthOfFieldBokehThreshold = 1f;

	[SerializeField]
	[Range(0f, 8f)]
	private float _depthOfFieldBokehIntensity = 2f;

	[SerializeField]
	private float _depthOfFieldMaxBrightness = 1000f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _depthOfFieldMaxDistance = 1f;

	[SerializeField]
	private FilterMode _depthOfFieldFilterMode = FilterMode.Bilinear;

	[NonSerialized]
	public OnBeforeFocusEvent OnBeforeFocus;

	[SerializeField]
	private bool _eyeAdaptation;

	[SerializeField]
	[Range(0f, 1f)]
	private float _eyeAdaptationMinExposure = 0.2f;

	[SerializeField]
	[Range(1f, 100f)]
	private float _eyeAdaptationMaxExposure = 5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _eyeAdaptationSpeedToLight = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _eyeAdaptationSpeedToDark = 0.2f;

	[SerializeField]
	private bool _eyeAdaptationInEditor = true;

	[SerializeField]
	private bool _purkinje;

	[SerializeField]
	[Range(0f, 5f)]
	private float _purkinjeAmount = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _purkinjeLuminanceThreshold = 0.15f;

	[SerializeField]
	private BEAUTIFY_TMO _tonemap;

	[SerializeField]
	[Range(0f, 5f)]
	private float _tonemapGamma = 2.5f;

	[SerializeField]
	private float _tonemapExposurePre = 1f;

	[SerializeField]
	private float _tonemapBrightnessPost = 1f;

	[SerializeField]
	private bool _sunFlares;

	[SerializeField]
	private Transform _sun;

	[SerializeField]
	private LayerMask _sunFlaresLayerMask = -1;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresIntensity = 1f;

	[SerializeField]
	private float _sunFlaresRevealSpeed = 1f;

	[SerializeField]
	private float _sunFlaresHideSpeed = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresSolarWindSpeed = 0.01f;

	[SerializeField]
	private Color _sunFlaresTint = new Color(1f, 1f, 1f);

	[SerializeField]
	[Range(1f, 5f)]
	private int _sunFlaresDownsampling = 1;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresSunIntensity = 0.1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresSunDiskSize = 0.05f;

	[SerializeField]
	[Range(0f, 10f)]
	private float _sunFlaresSunRayDiffractionIntensity = 3.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresSunRayDiffractionThreshold = 0.13f;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _sunFlaresCoronaRays1Length = 0.02f;

	[SerializeField]
	[Range(2f, 30f)]
	private int _sunFlaresCoronaRays1Streaks = 12;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float _sunFlaresCoronaRays1Spread = 0.001f;

	[SerializeField]
	[Range(0f, MathF.PI * 2f)]
	private float _sunFlaresCoronaRays1AngleOffset;

	[SerializeField]
	[Range(0f, 0.2f)]
	private float _sunFlaresCoronaRays2Length = 0.05f;

	[SerializeField]
	[Range(2f, 30f)]
	private int _sunFlaresCoronaRays2Streaks = 12;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float _sunFlaresCoronaRays2Spread = 0.1f;

	[SerializeField]
	[Range(0f, MathF.PI * 2f)]
	private float _sunFlaresCoronaRays2AngleOffset;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts1Size = 0.03f;

	[SerializeField]
	[Range(-3f, 3f)]
	private float _sunFlaresGhosts1Offset = 1.04f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts1Brightness = 0.037f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts2Size = 0.1f;

	[SerializeField]
	[Range(-3f, 3f)]
	private float _sunFlaresGhosts2Offset = 0.71f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts2Brightness = 0.03f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts3Size = 0.24f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts3Brightness = 0.025f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts3Offset = 0.31f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts4Size = 0.016f;

	[SerializeField]
	[Range(-3f, 3f)]
	private float _sunFlaresGhosts4Offset;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresGhosts4Brightness = 0.017f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresHaloOffset = 0.22f;

	[SerializeField]
	[Range(0f, 50f)]
	private float _sunFlaresHaloAmplitude = 15.1415f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _sunFlaresHaloIntensity = 0.01f;

	[SerializeField]
	private float _sunFlaresRadialOffset;

	[SerializeField]
	private bool _sunFlaresRotationDeadZone;

	[SerializeField]
	private bool _blur;

	[SerializeField]
	[Range(0f, 4f)]
	private float _blurIntensity = 1f;

	[SerializeField]
	[Range(1f, 8f)]
	private float _downscale = 1f;

	[SerializeField]
	[Range(1f, 3f)]
	private int _superSampling = 1;

	[SerializeField]
	[Range(1f, 256f)]
	private int _pixelateAmount = 1;

	[SerializeField]
	private bool _pixelateDownscale;

	[SerializeField]
	[Range(0f, 20f)]
	private float _antialiasStrength = 5f;

	[SerializeField]
	[Range(0.1f, 8f)]
	private float _antialiasMaxSpread = 3f;

	[SerializeField]
	[Range(0f, 0.001f)]
	private float _antialiasDepthThreshold = 1E-06f;

	[SerializeField]
	private float _antialiasDepthAtten;

	[SerializeField]
	private bool _chromaticAberration;

	[SerializeField]
	[Range(0f, 0.05f)]
	private float _chromaticAberrationIntensity;

	[SerializeField]
	[Range(0f, 32f)]
	private float _chromaticAberrationSmoothing;

	public bool isDirty;

	private static Beautify _beautify;

	private Material bMatDesktop;

	private Material bMatMobile;

	private Material bMatBasic;

	private static Color ColorTransparent = new Color(0f, 0f, 0f, 0f);

	[SerializeField]
	private Material bMat;

	private Camera currentCamera;

	private Vector3 camPrevPos;

	private Quaternion camPrevRotation;

	private float currSens;

	private int renderPass;

	private RenderTextureFormat rtFormat;

	private RenderTexture[] rt;

	private RenderTexture[] rtAF;

	private RenderTexture[] rtEA;

	private RenderTexture rtEAacum;

	private RenderTexture rtEAHist;

	private float dofPrevDistance;

	private float dofLastAutofocusDistance;

	private Vector4 dofLastBokehData;

	private Camera depthCam;

	private GameObject depthCamObj;

	private List<string> shaderKeywords;

	private Shader depthShader;

	private Shader dofExclusionShader;

	private bool shouldUpdateMaterialProperties;

	private const string BEAUTIFY_BUILD_HINT = "BeautifyBuildHint22rc5";

	private float sunFlareCurrentIntensity;

	private bool sunIsSpotlight;

	private Vector4 sunLastScrPos;

	private float sunLastRot;

	private Texture2D flareNoise;

	private RenderTexture dofDepthTexture;

	private RenderTexture dofExclusionTexture;

	private RenderTexture bloomSourceTexture;

	private RenderTexture bloomSourceDepthTexture;

	private RenderTexture bloomSourceTextureRightEye;

	private RenderTexture bloomSourceDepthTextureRightEye;

	private RenderTexture anamorphicFlaresSourceTexture;

	private RenderTexture anamorphicFlaresSourceDepthTexture;

	private RenderTexture anamorphicFlaresSourceTextureRightEye;

	private RenderTexture anamorphicFlaresSourceDepthTextureRightEye;

	private RenderTexture pixelateTexture;

	private RenderTextureDescriptor rtDescBase;

	private float sunFlareTime;

	private int dofCurrentLayerMaskValue;

	private int bloomCurrentLayerMaskValue;

	private int anamorphicFlaresCurrentLayerMaskValue;

	private int eyeWidth;

	private int eyeHeight;

	private bool isSuperSamplingActive;

	private RenderTextureFormat rtOutlineColorFormat;

	private bool linearColorSpace;

	public BEAUTIFY_PRESET preset
	{
		get
		{
			return _preset;
		}
		set
		{
			if (_preset != value)
			{
				_preset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_QUALITY quality
	{
		get
		{
			return _quality;
		}
		set
		{
			if (_quality != value)
			{
				_quality = value;
				UpdateQualitySettings();
				UpdateMaterialProperties();
			}
		}
	}

	public BeautifyProfile profile
	{
		get
		{
			return _profile;
		}
		set
		{
			if (_profile != value)
			{
				_profile = value;
				if (_profile != null)
				{
					_profile.Load(this);
					_preset = BEAUTIFY_PRESET.Custom;
				}
			}
		}
	}

	public bool syncWithProfile
	{
		get
		{
			return _syncWithProfile;
		}
		set
		{
			_syncWithProfile = value;
		}
	}

	public bool compareMode
	{
		get
		{
			return _compareMode;
		}
		set
		{
			if (_compareMode != value)
			{
				_compareMode = value;
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_COMPARE_STYLE compareStyle
	{
		get
		{
			return _compareStyle;
		}
		set
		{
			if (_compareStyle != value)
			{
				_compareStyle = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float comparePanning
	{
		get
		{
			return _comparePanning;
		}
		set
		{
			if (_comparePanning != value)
			{
				_comparePanning = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float compareLineAngle
	{
		get
		{
			return _compareLineAngle;
		}
		set
		{
			if (_compareLineAngle != value)
			{
				_compareLineAngle = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float compareLineWidth
	{
		get
		{
			return _compareLineWidth;
		}
		set
		{
			if (_compareLineWidth != value)
			{
				_compareLineWidth = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float dither
	{
		get
		{
			return _dither;
		}
		set
		{
			if (_dither != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_dither = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float ditherDepth
	{
		get
		{
			return _ditherDepth;
		}
		set
		{
			if (_ditherDepth != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_ditherDepth = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenMinDepth
	{
		get
		{
			return _sharpenMinDepth;
		}
		set
		{
			if (_sharpenMinDepth != value)
			{
				_sharpenMinDepth = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenMaxDepth
	{
		get
		{
			return _sharpenMaxDepth;
		}
		set
		{
			if (_sharpenMaxDepth != value)
			{
				_sharpenMaxDepth = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenMinMaxDepthFallOff
	{
		get
		{
			return _sharpenMinMaxDepthFallOff;
		}
		set
		{
			if (_sharpenMinMaxDepthFallOff != value)
			{
				_sharpenMinMaxDepthFallOff = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpen
	{
		get
		{
			return _sharpen;
		}
		set
		{
			if (_sharpen != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_sharpen = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenDepthThreshold
	{
		get
		{
			return _sharpenDepthThreshold;
		}
		set
		{
			if (_sharpenDepthThreshold != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_sharpenDepthThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color tintColor
	{
		get
		{
			return _tintColor;
		}
		set
		{
			if (_tintColor != value)
			{
				_tintColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenRelaxation
	{
		get
		{
			return _sharpenRelaxation;
		}
		set
		{
			if (_sharpenRelaxation != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_sharpenRelaxation = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenClamp
	{
		get
		{
			return _sharpenClamp;
		}
		set
		{
			if (_sharpenClamp != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_sharpenClamp = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenMotionSensibility
	{
		get
		{
			return _sharpenMotionSensibility;
		}
		set
		{
			if (_sharpenMotionSensibility != value)
			{
				_sharpenMotionSensibility = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sharpenMotionRestoreSpeed
	{
		get
		{
			return _sharpenMotionRestoreSpeed;
		}
		set
		{
			if (_sharpenMotionRestoreSpeed != value)
			{
				_sharpenMotionRestoreSpeed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float saturate
	{
		get
		{
			return _saturate;
		}
		set
		{
			if (_saturate != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_saturate = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float contrast
	{
		get
		{
			return _contrast;
		}
		set
		{
			if (_contrast != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_contrast = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float brightness
	{
		get
		{
			return _brightness;
		}
		set
		{
			if (_brightness != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_brightness = Mathf.Max(0f, value);
				UpdateMaterialProperties();
			}
		}
	}

	public float daltonize
	{
		get
		{
			return _daltonize;
		}
		set
		{
			if (_daltonize != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_daltonize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float hardLightIntensity
	{
		get
		{
			return _hardLightIntensity;
		}
		set
		{
			if (_hardLightIntensity != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_hardLightIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float hardLightBlend
	{
		get
		{
			return _hardLightBlend;
		}
		set
		{
			if (_hardLightBlend != value)
			{
				_preset = BEAUTIFY_PRESET.Custom;
				_hardLightBlend = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool vignetting
	{
		get
		{
			return _vignetting;
		}
		set
		{
			if (_vignetting != value)
			{
				_vignetting = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color vignettingColor
	{
		get
		{
			return _vignettingColor;
		}
		set
		{
			if (_vignettingColor != value)
			{
				_vignettingColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float vignettingFade
	{
		get
		{
			return _vignettingFade;
		}
		set
		{
			if (_vignettingFade != value)
			{
				_vignettingFade = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool vignettingCircularShape
	{
		get
		{
			return _vignettingCircularShape;
		}
		set
		{
			if (_vignettingCircularShape != value)
			{
				_vignettingCircularShape = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float vignettingAspectRatio
	{
		get
		{
			return _vignettingAspectRatio;
		}
		set
		{
			if (_vignettingAspectRatio != value)
			{
				_vignettingAspectRatio = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float vignettingBlink
	{
		get
		{
			return _vignettingBlink;
		}
		set
		{
			if (_vignettingBlink != value)
			{
				_vignettingBlink = value;
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_BLINK_STYLE vignettingBlinkStyle
	{
		get
		{
			return _vignettingBlinkStyle;
		}
		set
		{
			if (_vignettingBlinkStyle != value)
			{
				_vignettingBlinkStyle = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector2 vignettingCenter
	{
		get
		{
			return _vignettingCenter;
		}
		set
		{
			if (_vignettingCenter != value)
			{
				_vignettingCenter = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D vignettingMask
	{
		get
		{
			return _vignettingMask;
		}
		set
		{
			if (_vignettingMask != value)
			{
				_vignettingMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool frame
	{
		get
		{
			return _frame;
		}
		set
		{
			if (_frame != value)
			{
				_frame = value;
				UpdateMaterialProperties();
			}
		}
	}

	public FrameStyle frameStyle
	{
		get
		{
			return _frameStyle;
		}
		set
		{
			if (_frameStyle != value)
			{
				_frameStyle = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float frameBandHorizontalSize
	{
		get
		{
			return _frameBandHorizontalSize;
		}
		set
		{
			if (_frameBandHorizontalSize != value)
			{
				_frameBandHorizontalSize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float frameBandHorizontalSmoothness
	{
		get
		{
			return _frameBandHorizontalSmoothness;
		}
		set
		{
			if (_frameBandHorizontalSmoothness != value)
			{
				_frameBandHorizontalSmoothness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float frameBandVerticalSize
	{
		get
		{
			return _frameBandVerticalSize;
		}
		set
		{
			if (_frameBandVerticalSize != value)
			{
				_frameBandVerticalSize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float frameBandVerticalSmoothness
	{
		get
		{
			return _frameBandVerticalSmoothness;
		}
		set
		{
			if (_frameBandVerticalSmoothness != value)
			{
				_frameBandVerticalSmoothness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color frameColor
	{
		get
		{
			return _frameColor;
		}
		set
		{
			if (_frameColor != value)
			{
				_frameColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D frameMask
	{
		get
		{
			return _frameMask;
		}
		set
		{
			if (_frameMask != value)
			{
				_frameMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool lut
	{
		get
		{
			return _lut;
		}
		set
		{
			if (_lut != value)
			{
				_lut = value;
				if (_lut)
				{
					_nightVision = false;
					_thermalVision = false;
				}
				UpdateMaterialProperties();
			}
		}
	}

	public float lutIntensity
	{
		get
		{
			return _lutIntensity;
		}
		set
		{
			if (_lutIntensity != value)
			{
				_lutIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D lutTexture
	{
		get
		{
			return _lutTexture;
		}
		set
		{
			if (_lutTexture != value)
			{
				_lutTexture = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture3D lutTexture3D
	{
		get
		{
			return _lutTexture3D;
		}
		set
		{
			if (_lutTexture3D != value)
			{
				_lutTexture3D = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool nightVision
	{
		get
		{
			return _nightVision;
		}
		set
		{
			if (_nightVision != value)
			{
				_nightVision = value;
				if (_nightVision)
				{
					_thermalVision = false;
					_lut = false;
					_vignetting = true;
					_vignettingFade = 0f;
					_vignettingColor = new Color(0f, 0f, 0f, 0.1254902f);
					_vignettingCircularShape = true;
				}
				else
				{
					_vignetting = false;
				}
				UpdateMaterialProperties();
			}
		}
	}

	public Color nightVisionColor
	{
		get
		{
			return _nightVisionColor;
		}
		set
		{
			if (_nightVisionColor != value)
			{
				_nightVisionColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool outline
	{
		get
		{
			return _outline;
		}
		set
		{
			if (_outline != value)
			{
				_outline = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color outlineColor
	{
		get
		{
			return _outlineColor;
		}
		set
		{
			if (_outlineColor != value)
			{
				_outlineColor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool outlineCustomize
	{
		get
		{
			return _outlineCustomize;
		}
		set
		{
			if (_outlineCustomize != value)
			{
				_outlineCustomize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_OUTLINE_STAGE outlineStage
	{
		get
		{
			return _outlineStage;
		}
		set
		{
			if (_outlineStage != value)
			{
				_outlineStage = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float outlineSpread
	{
		get
		{
			return _outlineSpread;
		}
		set
		{
			if (_outlineSpread != value)
			{
				_outlineSpread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int outlineBlurPassCount
	{
		get
		{
			return _outlineBlurPassCount;
		}
		set
		{
			if (_outlineBlurPassCount != value)
			{
				_outlineBlurPassCount = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float outlineIntensityMultiplier
	{
		get
		{
			return _outlineIntensityMultiplier;
		}
		set
		{
			if (_outlineIntensityMultiplier != value)
			{
				_outlineIntensityMultiplier = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool outlineBlurDownscale
	{
		get
		{
			return _outlineBlurDownscale;
		}
		set
		{
			if (_outlineBlurDownscale != value)
			{
				_outlineBlurDownscale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float outlineMinDepthThreshold
	{
		get
		{
			return _outlineMinDepthThreshold;
		}
		set
		{
			if (_outlineMinDepthThreshold != value)
			{
				_outlineMinDepthThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool thermalVision
	{
		get
		{
			return _thermalVision;
		}
		set
		{
			if (_thermalVision != value)
			{
				_thermalVision = value;
				if (_thermalVision)
				{
					_nightVision = false;
					_lut = false;
					_vignetting = true;
					_vignettingFade = 0f;
					_vignettingColor = new Color(1f, 0.0627451f, 0.0627451f, 6f / 85f);
					_vignettingCircularShape = true;
				}
				else
				{
					_vignetting = false;
				}
				UpdateMaterialProperties();
			}
		}
	}

	public bool lensDirt
	{
		get
		{
			return _lensDirt;
		}
		set
		{
			if (_lensDirt != value)
			{
				_lensDirt = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float lensDirtThreshold
	{
		get
		{
			return _lensDirtThreshold;
		}
		set
		{
			if (_lensDirtThreshold != value)
			{
				_lensDirtThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float lensDirtIntensity
	{
		get
		{
			return _lensDirtIntensity;
		}
		set
		{
			if (_lensDirtIntensity != value)
			{
				_lensDirtIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Texture2D lensDirtTexture
	{
		get
		{
			return _lensDirtTexture;
		}
		set
		{
			if (_lensDirtTexture != value)
			{
				_lensDirtTexture = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloom
	{
		get
		{
			return _bloom;
		}
		set
		{
			if (_bloom != value)
			{
				_bloom = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask bloomCullingMask
	{
		get
		{
			return _bloomCullingMask;
		}
		set
		{
			if ((int)_bloomCullingMask != (int)value)
			{
				_bloomCullingMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomLayerMaskDownsampling
	{
		get
		{
			return _bloomLayerMaskDownsampling;
		}
		set
		{
			if (_bloomLayerMaskDownsampling != value)
			{
				_bloomLayerMaskDownsampling = Mathf.Max(value, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomIntensity
	{
		get
		{
			return _bloomIntensity;
		}
		set
		{
			if (_bloomIntensity != value)
			{
				_bloomIntensity = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomMaxBrightness
	{
		get
		{
			return _bloomMaxBrightness;
		}
		set
		{
			if (_bloomMaxBrightness != value)
			{
				_bloomMaxBrightness = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost0
	{
		get
		{
			return _bloomBoost0;
		}
		set
		{
			if (_bloomBoost0 != value)
			{
				_bloomBoost0 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost1
	{
		get
		{
			return _bloomBoost1;
		}
		set
		{
			if (_bloomBoost1 != value)
			{
				_bloomBoost1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost2
	{
		get
		{
			return _bloomBoost2;
		}
		set
		{
			if (_bloomBoost2 != value)
			{
				_bloomBoost2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost3
	{
		get
		{
			return _bloomBoost3;
		}
		set
		{
			if (_bloomBoost3 != value)
			{
				_bloomBoost3 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost4
	{
		get
		{
			return _bloomBoost4;
		}
		set
		{
			if (_bloomBoost4 != value)
			{
				_bloomBoost4 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomBoost5
	{
		get
		{
			return _bloomBoost5;
		}
		set
		{
			if (_bloomBoost5 != value)
			{
				_bloomBoost5 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomAntiflicker
	{
		get
		{
			return _bloomAntiflicker;
		}
		set
		{
			if (_bloomAntiflicker != value)
			{
				_bloomAntiflicker = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomAntiflickerMaxOutput
	{
		get
		{
			return _bloomAntiflickerMaxOutput;
		}
		set
		{
			if (_bloomAntiflickerMaxOutput != value)
			{
				_bloomAntiflickerMaxOutput = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public int bloomIterations
	{
		get
		{
			return _bloomIterations;
		}
		set
		{
			if (_bloomIterations != value)
			{
				_bloomIterations = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomUltra
	{
		get
		{
			return _bloomUltra;
		}
		set
		{
			if (_bloomUltra != value)
			{
				_bloomUltra = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int bloomUltraResolution
	{
		get
		{
			return _bloomUltraResolution;
		}
		set
		{
			if (_bloomUltraResolution != value)
			{
				_bloomUltraResolution = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomThreshold
	{
		get
		{
			return _bloomThreshold;
		}
		set
		{
			if (_bloomThreshold != value)
			{
				_bloomThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomConservativeThreshold
	{
		get
		{
			return _bloomConservativeThreshold;
		}
		set
		{
			if (_bloomConservativeThreshold != value)
			{
				_bloomConservativeThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint
	{
		get
		{
			return _bloomTint;
		}
		set
		{
			if (_bloomTint != value)
			{
				_bloomTint = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomCustomize
	{
		get
		{
			return _bloomCustomize;
		}
		set
		{
			if (_bloomCustomize != value)
			{
				_bloomCustomize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomDebug
	{
		get
		{
			return _bloomDebug;
		}
		set
		{
			if (_bloomDebug != value)
			{
				_bloomDebug = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight0
	{
		get
		{
			return _bloomWeight0;
		}
		set
		{
			if (_bloomWeight0 != value)
			{
				_bloomWeight0 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight1
	{
		get
		{
			return _bloomWeight1;
		}
		set
		{
			if (_bloomWeight1 != value)
			{
				_bloomWeight1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight2
	{
		get
		{
			return _bloomWeight2;
		}
		set
		{
			if (_bloomWeight2 != value)
			{
				_bloomWeight2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight3
	{
		get
		{
			return _bloomWeight3;
		}
		set
		{
			if (_bloomWeight3 != value)
			{
				_bloomWeight3 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight4
	{
		get
		{
			return _bloomWeight4;
		}
		set
		{
			if (_bloomWeight4 != value)
			{
				_bloomWeight4 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomWeight5
	{
		get
		{
			return _bloomWeight5;
		}
		set
		{
			if (_bloomWeight5 != value)
			{
				_bloomWeight5 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint0
	{
		get
		{
			return _bloomTint0;
		}
		set
		{
			if (_bloomTint0 != value)
			{
				_bloomTint0 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint1
	{
		get
		{
			return _bloomTint1;
		}
		set
		{
			if (_bloomTint1 != value)
			{
				_bloomTint1 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint2
	{
		get
		{
			return _bloomTint2;
		}
		set
		{
			if (_bloomTint2 != value)
			{
				_bloomTint2 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint3
	{
		get
		{
			return _bloomTint3;
		}
		set
		{
			if (_bloomTint3 != value)
			{
				_bloomTint3 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint4
	{
		get
		{
			return _bloomTint4;
		}
		set
		{
			if (_bloomTint4 != value)
			{
				_bloomTint4 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color bloomTint5
	{
		get
		{
			return _bloomTint5;
		}
		set
		{
			if (_bloomTint5 != value)
			{
				_bloomTint5 = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomBlur
	{
		get
		{
			return _bloomBlur;
		}
		set
		{
			if (_bloomBlur != value)
			{
				_bloomBlur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool bloomQuickerBlur
	{
		get
		{
			return _bloomQuickerBlur;
		}
		set
		{
			if (_bloomQuickerBlur != value)
			{
				_bloomQuickerBlur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomDepthAtten
	{
		get
		{
			return _bloomDepthAtten;
		}
		set
		{
			if (_bloomDepthAtten != value)
			{
				_bloomDepthAtten = Mathf.Max(0f, value);
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomNearAtten
	{
		get
		{
			return _bloomNearAtten;
		}
		set
		{
			if (_bloomNearAtten != value)
			{
				_bloomNearAtten = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float bloomLayerZBias
	{
		get
		{
			return _bloomLayerZBias;
		}
		set
		{
			if (_bloomLayerZBias != value)
			{
				_bloomLayerZBias = Mathf.Clamp(value, -1f, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_PRERENDER_EVENT preRenderCameraEvent
	{
		get
		{
			return _preRenderCameraEvent;
		}
		set
		{
			if (_preRenderCameraEvent != value)
			{
				_preRenderCameraEvent = value;
			}
		}
	}

	public bool anamorphicFlares
	{
		get
		{
			return _anamorphicFlares;
		}
		set
		{
			if (_anamorphicFlares != value)
			{
				_anamorphicFlares = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask anamorphicFlaresCullingMask
	{
		get
		{
			return _anamorphicFlaresCullingMask;
		}
		set
		{
			if ((int)_anamorphicFlaresCullingMask != (int)value)
			{
				_anamorphicFlaresCullingMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float anamorphicFlaresLayerMaskDownsampling
	{
		get
		{
			return _anamorphicFlaresLayerMaskDownsampling;
		}
		set
		{
			if (_anamorphicFlaresLayerMaskDownsampling != value)
			{
				_anamorphicFlaresLayerMaskDownsampling = Mathf.Max(value, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public float anamorphicFlaresIntensity
	{
		get
		{
			return _anamorphicFlaresIntensity;
		}
		set
		{
			if (_anamorphicFlaresIntensity != value)
			{
				_anamorphicFlaresIntensity = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public bool anamorphicFlaresAntiflicker
	{
		get
		{
			return _anamorphicFlaresAntiflicker;
		}
		set
		{
			if (_anamorphicFlaresAntiflicker != value)
			{
				_anamorphicFlaresAntiflicker = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float anamorphicFlaresAntiflickerMaxOutput
	{
		get
		{
			return _anamorphicFlaresAntiflickerMaxOutput;
		}
		set
		{
			if (_anamorphicFlaresAntiflickerMaxOutput != value)
			{
				_anamorphicFlaresAntiflickerMaxOutput = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public bool anamorphicFlaresUltra
	{
		get
		{
			return _anamorphicFlaresUltra;
		}
		set
		{
			if (_anamorphicFlaresUltra != value)
			{
				_anamorphicFlaresUltra = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int anamorphicFlaresUltraResolution
	{
		get
		{
			return _anamorphicFlaresUltraResolution;
		}
		set
		{
			if (_anamorphicFlaresUltraResolution != value)
			{
				_anamorphicFlaresUltraResolution = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float anamorphicFlaresThreshold
	{
		get
		{
			return _anamorphicFlaresThreshold;
		}
		set
		{
			if (_anamorphicFlaresThreshold != value)
			{
				_anamorphicFlaresThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float anamorphicFlaresSpread
	{
		get
		{
			return _anamorphicFlaresSpread;
		}
		set
		{
			if (_anamorphicFlaresSpread != value)
			{
				_anamorphicFlaresSpread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool anamorphicFlaresVertical
	{
		get
		{
			return _anamorphicFlaresVertical;
		}
		set
		{
			if (_anamorphicFlaresVertical != value)
			{
				_anamorphicFlaresVertical = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color anamorphicFlaresTint
	{
		get
		{
			return _anamorphicFlaresTint;
		}
		set
		{
			if (_anamorphicFlaresTint != value)
			{
				_anamorphicFlaresTint = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool anamorphicFlaresBlur
	{
		get
		{
			return _anamorphicFlaresBlur;
		}
		set
		{
			if (_anamorphicFlaresBlur != value)
			{
				_anamorphicFlaresBlur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfField
	{
		get
		{
			return _depthOfField;
		}
		set
		{
			if (_depthOfField != value)
			{
				_depthOfField = value;
				dofPrevDistance = -1f;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldTransparencySupport
	{
		get
		{
			return _depthOfFieldTransparencySupport;
		}
		set
		{
			if (_depthOfFieldTransparencySupport != value)
			{
				_depthOfFieldTransparencySupport = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask depthOfFieldTransparencyLayerMask
	{
		get
		{
			return _depthOfFieldTransparencyLayerMask;
		}
		set
		{
			if ((int)_depthOfFieldTransparencyLayerMask != (int)value)
			{
				_depthOfFieldTransparencyLayerMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public CullMode depthOfFieldTransparencyCullMode
	{
		get
		{
			return _depthOfFieldTransparencyCullMode;
		}
		set
		{
			if (_depthOfFieldTransparencyCullMode != value)
			{
				_depthOfFieldTransparencyCullMode = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Transform depthOfFieldTargetFocus
	{
		get
		{
			return _depthOfFieldTargetFocus;
		}
		set
		{
			if (_depthOfFieldTargetFocus != value)
			{
				_depthOfFieldTargetFocus = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldDebug
	{
		get
		{
			return _depthOfFieldDebug;
		}
		set
		{
			if (_depthOfFieldDebug != value)
			{
				_depthOfFieldDebug = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldAutofocus
	{
		get
		{
			return _depthOfFieldAutofocus;
		}
		set
		{
			if (_depthOfFieldAutofocus != value)
			{
				_depthOfFieldAutofocus = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Vector2 depthofFieldAutofocusViewportPoint
	{
		get
		{
			return _depthofFieldAutofocusViewportPoint;
		}
		set
		{
			if (_depthofFieldAutofocusViewportPoint != value)
			{
				_depthofFieldAutofocusViewportPoint = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldAutofocusMinDistance
	{
		get
		{
			return _depthOfFieldAutofocusMinDistance;
		}
		set
		{
			if (_depthOfFieldAutofocusMinDistance != value)
			{
				_depthOfFieldAutofocusMinDistance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldAutofocusDistanceShift
	{
		get
		{
			return _depthOfFieldAutofocusDistanceShift;
		}
		set
		{
			if (_depthOfFieldAutofocusDistanceShift != value)
			{
				_depthOfFieldAutofocusDistanceShift = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldAutofocusMaxDistance
	{
		get
		{
			return _depthOfFieldAutofocusMaxDistance;
		}
		set
		{
			if (_depthOfFieldAutofocusMaxDistance != value)
			{
				_depthOfFieldAutofocusMaxDistance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask depthOfFieldAutofocusLayerMask
	{
		get
		{
			return _depthOfFieldAutofocusLayerMask;
		}
		set
		{
			if ((int)_depthOfFieldAutofocusLayerMask != (int)value)
			{
				_depthOfFieldAutofocusLayerMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask depthOfFieldExclusionLayerMask
	{
		get
		{
			return _depthOfFieldExclusionLayerMask;
		}
		set
		{
			if ((int)_depthOfFieldExclusionLayerMask != (int)value)
			{
				_depthOfFieldExclusionLayerMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public CullMode depthOfFieldExclusionCullMode
	{
		get
		{
			return _depthOfFieldExclusionCullMode;
		}
		set
		{
			if (_depthOfFieldExclusionCullMode != value)
			{
				_depthOfFieldExclusionCullMode = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldExclusionLayerMaskDownsampling
	{
		get
		{
			return _depthOfFieldExclusionLayerMaskDownsampling;
		}
		set
		{
			if (_depthOfFieldExclusionLayerMaskDownsampling != value)
			{
				_depthOfFieldExclusionLayerMaskDownsampling = Mathf.Max(value, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldTransparencySupportDownsampling
	{
		get
		{
			return _depthOfFieldTransparencySupportDownsampling;
		}
		set
		{
			if (_depthOfFieldTransparencySupportDownsampling != value)
			{
				_depthOfFieldTransparencySupportDownsampling = Mathf.Max(value, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldExclusionBias
	{
		get
		{
			return _depthOfFieldExclusionBias;
		}
		set
		{
			if (_depthOfFieldExclusionBias != value)
			{
				_depthOfFieldExclusionBias = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldDistance
	{
		get
		{
			return _depthOfFieldDistance;
		}
		set
		{
			if (_depthOfFieldDistance != value)
			{
				_depthOfFieldDistance = Mathf.Max(value, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldFocusSpeed
	{
		get
		{
			return _depthOfFieldFocusSpeed;
		}
		set
		{
			if (_depthOfFieldFocusSpeed != value)
			{
				_depthOfFieldFocusSpeed = Mathf.Clamp(value, 0.001f, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public int depthOfFieldDownsampling
	{
		get
		{
			return _depthOfFieldDownsampling;
		}
		set
		{
			if (_depthOfFieldDownsampling != value)
			{
				_depthOfFieldDownsampling = Mathf.Max(value, 1);
				UpdateMaterialProperties();
			}
		}
	}

	public int depthOfFieldMaxSamples
	{
		get
		{
			return _depthOfFieldMaxSamples;
		}
		set
		{
			if (_depthOfFieldMaxSamples != value)
			{
				_depthOfFieldMaxSamples = Mathf.Max(value, 2);
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_DOF_CAMERA_SETTINGS depthOfFieldCameraSettings
	{
		get
		{
			return _depthOfFieldCameraSettings;
		}
		set
		{
			if (_depthOfFieldCameraSettings != value)
			{
				_depthOfFieldCameraSettings = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldFocalLengthReal
	{
		get
		{
			return _depthOfFieldFocalLengthReal;
		}
		set
		{
			if (_depthOfFieldFocalLengthReal != value)
			{
				_depthOfFieldFocalLengthReal = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldFStop
	{
		get
		{
			return _depthOfFieldFStop;
		}
		set
		{
			if (_depthOfFieldFStop != value)
			{
				_depthOfFieldFStop = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldImageSensorHeight
	{
		get
		{
			return _depthOfFieldImageSensorHeight;
		}
		set
		{
			if (_depthOfFieldImageSensorHeight != value)
			{
				_depthOfFieldImageSensorHeight = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldFocalLength
	{
		get
		{
			return _depthOfFieldFocalLength;
		}
		set
		{
			if (_depthOfFieldFocalLength != value)
			{
				_depthOfFieldFocalLength = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldAperture
	{
		get
		{
			return _depthOfFieldAperture;
		}
		set
		{
			if (_depthOfFieldAperture != value)
			{
				_depthOfFieldAperture = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldForegroundBlur
	{
		get
		{
			return _depthOfFieldForegroundBlur;
		}
		set
		{
			if (_depthOfFieldForegroundBlur != value)
			{
				_depthOfFieldForegroundBlur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldForegroundBlurHQ
	{
		get
		{
			return _depthOfFieldForegroundBlurHQ;
		}
		set
		{
			if (_depthOfFieldForegroundBlurHQ != value)
			{
				_depthOfFieldForegroundBlurHQ = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldForegroundBlurHQSpread
	{
		get
		{
			return _depthOfFieldForegroundBlurHQSpread;
		}
		set
		{
			if (_depthOfFieldForegroundBlurHQSpread != value)
			{
				_depthOfFieldForegroundBlurHQSpread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldForegroundDistance
	{
		get
		{
			return _depthOfFieldForegroundDistance;
		}
		set
		{
			if (_depthOfFieldForegroundDistance != value)
			{
				_depthOfFieldForegroundDistance = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool depthOfFieldBokeh
	{
		get
		{
			return _depthOfFieldBokeh;
		}
		set
		{
			if (_depthOfFieldBokeh != value)
			{
				_depthOfFieldBokeh = value;
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_BOKEH_COMPOSITION depthOfFieldBokehComposition
	{
		get
		{
			return _depthOfFieldBokehComposition;
		}
		set
		{
			if (_depthOfFieldBokehComposition != value)
			{
				_depthOfFieldBokehComposition = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldBokehThreshold
	{
		get
		{
			return _depthOfFieldBokehThreshold;
		}
		set
		{
			if (_depthOfFieldBokehThreshold != value)
			{
				_depthOfFieldBokehThreshold = Mathf.Max(value, 0f);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldBokehIntensity
	{
		get
		{
			return _depthOfFieldBokehIntensity;
		}
		set
		{
			if (_depthOfFieldBokehIntensity != value)
			{
				_depthOfFieldBokehIntensity = Mathf.Max(value, 0f);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldMaxBrightness
	{
		get
		{
			return _depthOfFieldMaxBrightness;
		}
		set
		{
			if (_depthOfFieldMaxBrightness != value)
			{
				_depthOfFieldMaxBrightness = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float depthOfFieldMaxDistance
	{
		get
		{
			return _depthOfFieldMaxDistance;
		}
		set
		{
			if (_depthOfFieldMaxDistance != value)
			{
				_depthOfFieldMaxDistance = Mathf.Abs(value);
				UpdateMaterialProperties();
			}
		}
	}

	public FilterMode depthOfFieldFilterMode
	{
		get
		{
			return _depthOfFieldFilterMode;
		}
		set
		{
			if (_depthOfFieldFilterMode != value)
			{
				_depthOfFieldFilterMode = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool eyeAdaptation
	{
		get
		{
			return _eyeAdaptation;
		}
		set
		{
			if (_eyeAdaptation != value)
			{
				_eyeAdaptation = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float eyeAdaptationMinExposure
	{
		get
		{
			return _eyeAdaptationMinExposure;
		}
		set
		{
			if (_eyeAdaptationMinExposure != value)
			{
				_eyeAdaptationMinExposure = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float eyeAdaptationMaxExposure
	{
		get
		{
			return _eyeAdaptationMaxExposure;
		}
		set
		{
			if (_eyeAdaptationMaxExposure != value)
			{
				_eyeAdaptationMaxExposure = Mathf.Clamp(value, 1f, 100f);
				UpdateMaterialProperties();
			}
		}
	}

	public float eyeAdaptationSpeedToLight
	{
		get
		{
			return _eyeAdaptationSpeedToLight;
		}
		set
		{
			if (_eyeAdaptationSpeedToLight != value)
			{
				_eyeAdaptationSpeedToLight = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public float eyeAdaptationSpeedToDark
	{
		get
		{
			return _eyeAdaptationSpeedToDark;
		}
		set
		{
			if (_eyeAdaptationSpeedToDark != value)
			{
				_eyeAdaptationSpeedToDark = Mathf.Clamp01(value);
				UpdateMaterialProperties();
			}
		}
	}

	public bool eyeAdaptationInEditor
	{
		get
		{
			return _eyeAdaptationInEditor;
		}
		set
		{
			if (_eyeAdaptationInEditor != value)
			{
				_eyeAdaptationInEditor = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool purkinje
	{
		get
		{
			return _purkinje;
		}
		set
		{
			if (_purkinje != value)
			{
				_purkinje = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float purkinjeAmount
	{
		get
		{
			return _purkinjeAmount;
		}
		set
		{
			if (_purkinjeAmount != value)
			{
				_purkinjeAmount = Mathf.Clamp(value, 0f, 5f);
				UpdateMaterialProperties();
			}
		}
	}

	public float purkinjeLuminanceThreshold
	{
		get
		{
			return _purkinjeLuminanceThreshold;
		}
		set
		{
			if (purkinjeLuminanceThreshold != value)
			{
				_purkinjeLuminanceThreshold = Mathf.Clamp(value, 0f, 1f);
				UpdateMaterialProperties();
			}
		}
	}

	public BEAUTIFY_TMO tonemap
	{
		get
		{
			return _tonemap;
		}
		set
		{
			if (_tonemap != value)
			{
				_tonemap = value;
				if (_tonemap == BEAUTIFY_TMO.ACES)
				{
					_saturate = 0f;
					_contrast = 1f;
				}
				UpdateMaterialProperties();
			}
		}
	}

	public float tonemapGamma
	{
		get
		{
			return _tonemapGamma;
		}
		set
		{
			if (_tonemapGamma != value)
			{
				_tonemapGamma = Mathf.Clamp(value, 0f, 5f);
				UpdateMaterialProperties();
			}
		}
	}

	public float tonemapExposurePre
	{
		get
		{
			return _tonemapExposurePre;
		}
		set
		{
			if (_tonemapExposurePre != value)
			{
				_tonemapExposurePre = Mathf.Max(0f, value);
				UpdateMaterialProperties();
			}
		}
	}

	public float tonemapBrightnessPost
	{
		get
		{
			return _tonemapBrightnessPost;
		}
		set
		{
			if (_tonemapBrightnessPost != value)
			{
				_tonemapBrightnessPost = Mathf.Max(0f, value);
				UpdateMaterialProperties();
			}
		}
	}

	public bool sunFlares
	{
		get
		{
			return _sunFlares;
		}
		set
		{
			if (_sunFlares != value)
			{
				_sunFlares = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Transform sun
	{
		get
		{
			return _sun;
		}
		set
		{
			if (_sun != value)
			{
				_sun = value;
				UpdateMaterialProperties();
			}
		}
	}

	public LayerMask sunFlaresLayerMask
	{
		get
		{
			return _sunFlaresLayerMask;
		}
		set
		{
			if ((int)_sunFlaresLayerMask != (int)value)
			{
				_sunFlaresLayerMask = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresIntensity
	{
		get
		{
			return _sunFlaresIntensity;
		}
		set
		{
			if (_sunFlaresIntensity != value)
			{
				_sunFlaresIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresRevealSpeed
	{
		get
		{
			return _sunFlaresRevealSpeed;
		}
		set
		{
			if (_sunFlaresRevealSpeed != value)
			{
				_sunFlaresRevealSpeed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresHideSpeed
	{
		get
		{
			return _sunFlaresHideSpeed;
		}
		set
		{
			if (_sunFlaresHideSpeed != value)
			{
				_sunFlaresHideSpeed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresSolarWindSpeed
	{
		get
		{
			return _sunFlaresSolarWindSpeed;
		}
		set
		{
			if (_sunFlaresSolarWindSpeed != value)
			{
				_sunFlaresSolarWindSpeed = value;
				UpdateMaterialProperties();
			}
		}
	}

	public Color sunFlaresTint
	{
		get
		{
			return _sunFlaresTint;
		}
		set
		{
			if (_sunFlaresTint != value)
			{
				_sunFlaresTint = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int sunFlaresDownsampling
	{
		get
		{
			return _sunFlaresDownsampling;
		}
		set
		{
			if (_sunFlaresDownsampling != value)
			{
				_sunFlaresDownsampling = Mathf.Max(value, 1);
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresSunIntensity
	{
		get
		{
			return _sunFlaresSunIntensity;
		}
		set
		{
			if (_sunFlaresSunIntensity != value)
			{
				_sunFlaresSunIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresSunDiskSize
	{
		get
		{
			return _sunFlaresSunDiskSize;
		}
		set
		{
			if (_sunFlaresSunDiskSize != value)
			{
				_sunFlaresSunDiskSize = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresSunRayDiffractionIntensity
	{
		get
		{
			return _sunFlaresSunRayDiffractionIntensity;
		}
		set
		{
			if (_sunFlaresSunRayDiffractionIntensity != value)
			{
				_sunFlaresSunRayDiffractionIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresSunRayDiffractionThreshold
	{
		get
		{
			return _sunFlaresSunRayDiffractionThreshold;
		}
		set
		{
			if (_sunFlaresSunRayDiffractionThreshold != value)
			{
				_sunFlaresSunRayDiffractionThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays1Length
	{
		get
		{
			return _sunFlaresCoronaRays1Length;
		}
		set
		{
			if (_sunFlaresCoronaRays1Length != value)
			{
				_sunFlaresCoronaRays1Length = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int sunFlaresCoronaRays1Streaks
	{
		get
		{
			return _sunFlaresCoronaRays1Streaks;
		}
		set
		{
			if (_sunFlaresCoronaRays1Streaks != value)
			{
				_sunFlaresCoronaRays1Streaks = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays1Spread
	{
		get
		{
			return _sunFlaresCoronaRays1Spread;
		}
		set
		{
			if (_sunFlaresCoronaRays1Spread != value)
			{
				_sunFlaresCoronaRays1Spread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays1AngleOffset
	{
		get
		{
			return _sunFlaresCoronaRays1AngleOffset;
		}
		set
		{
			if (_sunFlaresCoronaRays1AngleOffset != value)
			{
				_sunFlaresCoronaRays1AngleOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays2Length
	{
		get
		{
			return _sunFlaresCoronaRays2Length;
		}
		set
		{
			if (_sunFlaresCoronaRays2Length != value)
			{
				_sunFlaresCoronaRays2Length = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int sunFlaresCoronaRays2Streaks
	{
		get
		{
			return _sunFlaresCoronaRays2Streaks;
		}
		set
		{
			if (_sunFlaresCoronaRays2Streaks != value)
			{
				_sunFlaresCoronaRays2Streaks = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays2Spread
	{
		get
		{
			return _sunFlaresCoronaRays2Spread;
		}
		set
		{
			if (_sunFlaresCoronaRays2Spread != value)
			{
				_sunFlaresCoronaRays2Spread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresCoronaRays2AngleOffset
	{
		get
		{
			return _sunFlaresCoronaRays2AngleOffset;
		}
		set
		{
			if (_sunFlaresCoronaRays2AngleOffset != value)
			{
				_sunFlaresCoronaRays2AngleOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts1Size
	{
		get
		{
			return _sunFlaresGhosts1Size;
		}
		set
		{
			if (_sunFlaresGhosts1Size != value)
			{
				_sunFlaresGhosts1Size = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts1Offset
	{
		get
		{
			return _sunFlaresGhosts1Offset;
		}
		set
		{
			if (_sunFlaresGhosts1Offset != value)
			{
				_sunFlaresGhosts1Offset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts1Brightness
	{
		get
		{
			return _sunFlaresGhosts1Brightness;
		}
		set
		{
			if (_sunFlaresGhosts1Brightness != value)
			{
				_sunFlaresGhosts1Brightness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts2Size
	{
		get
		{
			return _sunFlaresGhosts2Size;
		}
		set
		{
			if (_sunFlaresGhosts2Size != value)
			{
				_sunFlaresGhosts2Size = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts2Offset
	{
		get
		{
			return _sunFlaresGhosts2Offset;
		}
		set
		{
			if (_sunFlaresGhosts2Offset != value)
			{
				_sunFlaresGhosts2Offset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts2Brightness
	{
		get
		{
			return _sunFlaresGhosts2Brightness;
		}
		set
		{
			if (_sunFlaresGhosts2Brightness != value)
			{
				_sunFlaresGhosts2Brightness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts3Size
	{
		get
		{
			return _sunFlaresGhosts3Size;
		}
		set
		{
			if (_sunFlaresGhosts3Size != value)
			{
				_sunFlaresGhosts3Size = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts3Brightness
	{
		get
		{
			return _sunFlaresGhosts3Brightness;
		}
		set
		{
			if (_sunFlaresGhosts3Brightness != value)
			{
				_sunFlaresGhosts3Brightness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts3Offset
	{
		get
		{
			return _sunFlaresGhosts3Offset;
		}
		set
		{
			if (_sunFlaresGhosts3Offset != value)
			{
				_sunFlaresGhosts3Offset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts4Size
	{
		get
		{
			return _sunFlaresGhosts4Size;
		}
		set
		{
			if (_sunFlaresGhosts4Size != value)
			{
				_sunFlaresGhosts4Size = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts4Offset
	{
		get
		{
			return _sunFlaresGhosts4Offset;
		}
		set
		{
			if (_sunFlaresGhosts4Offset != value)
			{
				_sunFlaresGhosts4Offset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresGhosts4Brightness
	{
		get
		{
			return _sunFlaresGhosts4Brightness;
		}
		set
		{
			if (_sunFlaresGhosts4Brightness != value)
			{
				_sunFlaresGhosts4Brightness = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresHaloOffset
	{
		get
		{
			return _sunFlaresHaloOffset;
		}
		set
		{
			if (_sunFlaresHaloOffset != value)
			{
				_sunFlaresHaloOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresHaloAmplitude
	{
		get
		{
			return _sunFlaresHaloAmplitude;
		}
		set
		{
			if (_sunFlaresHaloAmplitude != value)
			{
				_sunFlaresHaloAmplitude = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresHaloIntensity
	{
		get
		{
			return _sunFlaresHaloIntensity;
		}
		set
		{
			if (_sunFlaresHaloIntensity != value)
			{
				_sunFlaresHaloIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float sunFlaresRadialOffset
	{
		get
		{
			return _sunFlaresRadialOffset;
		}
		set
		{
			if (_sunFlaresRadialOffset != value)
			{
				_sunFlaresRadialOffset = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool sunFlaresRotationDeadZone
	{
		get
		{
			return _sunFlaresRotationDeadZone;
		}
		set
		{
			if (_sunFlaresRotationDeadZone != value)
			{
				_sunFlaresRotationDeadZone = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool blur
	{
		get
		{
			return _blur;
		}
		set
		{
			if (_blur != value)
			{
				_blur = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float blurIntensity
	{
		get
		{
			return _blurIntensity;
		}
		set
		{
			if (_blurIntensity != value)
			{
				_blurIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float downscale
	{
		get
		{
			return _downscale;
		}
		set
		{
			if (_downscale != value)
			{
				_downscale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public int superSampling
	{
		get
		{
			return _superSampling;
		}
		set
		{
			if (_superSampling != value)
			{
				_superSampling = value;
				UpdateMaterialProperties();
			}
		}
	}

	private float renderScale
	{
		get
		{
			if (_quality == BEAUTIFY_QUALITY.BestPerformance)
			{
				return _downscale;
			}
			if (_quality == BEAUTIFY_QUALITY.BestQuality && !Application.isMobilePlatform)
			{
				return 1f / (0.5f + (float)_superSampling / 2f);
			}
			return 1f;
		}
	}

	public int pixelateAmount
	{
		get
		{
			return _pixelateAmount;
		}
		set
		{
			if (_pixelateAmount != value)
			{
				_pixelateAmount = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool pixelateDownscale
	{
		get
		{
			return _pixelateDownscale;
		}
		set
		{
			if (_pixelateDownscale != value)
			{
				_pixelateDownscale = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float antialiasStrength
	{
		get
		{
			return _antialiasStrength;
		}
		set
		{
			if (_antialiasStrength != value)
			{
				_antialiasStrength = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float antialiasMaxSpread
	{
		get
		{
			return _antialiasMaxSpread;
		}
		set
		{
			if (_antialiasMaxSpread != value)
			{
				_antialiasMaxSpread = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float antialiasDepthThreshold
	{
		get
		{
			return _antialiasDepthThreshold;
		}
		set
		{
			if (_antialiasDepthThreshold != value)
			{
				_antialiasDepthThreshold = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float antialiasDepthAtten
	{
		get
		{
			return _antialiasDepthAtten;
		}
		set
		{
			if (_antialiasDepthAtten != value)
			{
				_antialiasDepthAtten = value;
				UpdateMaterialProperties();
			}
		}
	}

	public bool chromaticAberration
	{
		get
		{
			return _chromaticAberration;
		}
		set
		{
			if (_chromaticAberration != value)
			{
				_chromaticAberration = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float chromaticAberrationIntensity
	{
		get
		{
			return _chromaticAberrationIntensity;
		}
		set
		{
			if (_chromaticAberrationIntensity != value)
			{
				_chromaticAberrationIntensity = value;
				UpdateMaterialProperties();
			}
		}
	}

	public float chromaticAberrationSmoothing
	{
		get
		{
			return _chromaticAberrationSmoothing;
		}
		set
		{
			if (_chromaticAberrationSmoothing != value)
			{
				_chromaticAberrationSmoothing = value;
				UpdateMaterialProperties();
			}
		}
	}

	public static Beautify instance
	{
		get
		{
			if (_beautify == null)
			{
				Camera[] allCameras = Camera.allCameras;
				for (int i = 0; i < allCameras.Length; i++)
				{
					_beautify = allCameras[i].GetComponent<Beautify>();
					if (_beautify != null)
					{
						break;
					}
				}
			}
			return _beautify;
		}
	}

	public Camera cameraEffect => currentCamera;

	private bool isUsingAnamorphicFlaresLayerMask
	{
		get
		{
			if ((int)_anamorphicFlaresCullingMask != 0)
			{
				return (int)_anamorphicFlaresCullingMask != -1;
			}
			return false;
		}
	}

	private bool isUsingBloomLayerMask
	{
		get
		{
			if ((int)_bloomCullingMask != 0)
			{
				return (int)_bloomCullingMask != -1;
			}
			return false;
		}
	}

	private bool isUsingDepthOfFieldExclusionLayerMask
	{
		get
		{
			if ((int)_depthOfFieldExclusionLayerMask != 0)
			{
				return (int)_depthOfFieldExclusionLayerMask != -1;
			}
			return false;
		}
	}

	public float depthOfFieldCurrentFocalPointDistance => dofLastAutofocusDistance;

	private void OnEnable()
	{
		CheckColorSpace();
		currentCamera = GetComponent<Camera>();
		VRCheck.Init();
		rtDescBase = GetDefaultRenderTextureDescriptor();
		rtOutlineColorFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : rtDescBase.colorFormat);
		if (_syncWithProfile && _profile != null)
		{
			_profile.Load(this);
		}
		if (VRCheck.isVrRunning)
		{
			_downscale = 1f;
			_pixelateDownscale = false;
		}
		UpdateMaterialPropertiesNow();
		isDirty = false;
	}

	private void OnDestroy()
	{
		CleanUpRT();
		if (depthCamObj != null)
		{
			UnityEngine.Object.DestroyImmediate(depthCamObj);
			depthCamObj = null;
		}
		if (rtEAacum != null)
		{
			rtEAacum.Release();
		}
		if (rtEAHist != null)
		{
			rtEAHist.Release();
		}
		if (bMatDesktop != null)
		{
			UnityEngine.Object.DestroyImmediate(bMatDesktop);
			bMatDesktop = null;
		}
		if (bMatMobile != null)
		{
			UnityEngine.Object.DestroyImmediate(bMatMobile);
			bMatMobile = null;
		}
		if (bMatBasic != null)
		{
			UnityEngine.Object.DestroyImmediate(bMatBasic);
			bMatBasic = null;
		}
		bMat = null;
	}

	private void Reset()
	{
		UpdateMaterialPropertiesNow();
	}

	private void OnValidate()
	{
		_bloomIntensity = Mathf.Max(0f, _bloomIntensity);
		_bloomAntiflickerMaxOutput = Mathf.Max(0f, _bloomAntiflickerMaxOutput);
		_bloomDepthAtten = Mathf.Max(0f, bloomDepthAtten);
		_anamorphicFlaresIntensity = Mathf.Max(0f, _anamorphicFlaresIntensity);
		_anamorphicFlaresAntiflickerMaxOutput = Mathf.Max(0f, _anamorphicFlaresAntiflickerMaxOutput);
		_antialiasDepthAtten = Mathf.Max(0f, antialiasDepthAtten);
		_depthofFieldAutofocusViewportPoint.x = Mathf.Clamp01(_depthofFieldAutofocusViewportPoint.x);
		_depthofFieldAutofocusViewportPoint.y = Mathf.Clamp01(_depthofFieldAutofocusViewportPoint.y);
		_sunFlaresRadialOffset = Mathf.Max(0f, sunFlaresRadialOffset);
	}

	private void LateUpdate()
	{
		if (bMat == null || !Application.isPlaying || _sharpenMotionSensibility <= 0f)
		{
			return;
		}
		Vector3 position = currentCamera.transform.position;
		Quaternion rotation = currentCamera.transform.rotation;
		float deltaTime = Time.deltaTime;
		if (position != camPrevPos || rotation.x != camPrevRotation.x || rotation.y != camPrevRotation.y || rotation.z != camPrevRotation.z || rotation.w != camPrevRotation.w)
		{
			currSens = Mathf.Lerp(currSens, _sharpen * _sharpenMotionSensibility, 30f * _sharpenMotionSensibility * deltaTime);
			camPrevPos = position;
			camPrevRotation = rotation;
		}
		else
		{
			if (currSens <= 0.001f)
			{
				return;
			}
			currSens -= 30f * _sharpenMotionRestoreSpeed * deltaTime;
		}
		currSens = Mathf.Clamp(currSens, 0f, _sharpen);
		float num = _sharpen - currSens;
		UpdateSharpenParams(num);
	}

	private void OnPreCull()
	{
		if (_preRenderCameraEvent == BEAUTIFY_PRERENDER_EVENT.OnPreCull)
		{
			DoOnPreRenderTasks();
		}
		ConfigureRenderScale();
	}

	private void DoOnPreRenderTasks()
	{
		CleanUpRT();
		if (base.enabled && base.gameObject.activeSelf && !(currentCamera == null) && !(bMat == null) && (_depthOfField || _bloom || _anamorphicFlares))
		{
			if (dofCurrentLayerMaskValue != _depthOfFieldExclusionLayerMask.value)
			{
				shouldUpdateMaterialProperties = true;
			}
			if (depthOfField && (_depthOfFieldTransparencySupport || isUsingDepthOfFieldExclusionLayerMask))
			{
				CheckDoFTransparencySupport();
				CheckDoFExclusionMask();
			}
			if (_bloomCullingMask.value != bloomCurrentLayerMaskValue || _anamorphicFlaresCullingMask.value != anamorphicFlaresCurrentLayerMaskValue)
			{
				shouldUpdateMaterialProperties = true;
			}
			if ((_bloom && isUsingBloomLayerMask) || (_anamorphicFlares && isUsingAnamorphicFlaresLayerMask))
			{
				CheckBloomAndFlaresCulling();
			}
		}
	}

	private void OnPreRender()
	{
		if (_preRenderCameraEvent == BEAUTIFY_PRERENDER_EVENT.OnPreRender)
		{
			DoOnPreRenderTasks();
		}
	}

	private void ConfigureRenderScale()
	{
		isSuperSamplingActive = false;
		float num = renderScale;
		if (Camera.current.cameraType == CameraType.SceneView)
		{
			return;
		}
		if (num != 1f && rtDescBase.width > 1)
		{
			_pixelateAmount = 1;
			RenderTextureDescriptor desc = rtDescBase;
			if (num <= 1f)
			{
				desc.msaaSamples = Mathf.Max(1, QualitySettings.antiAliasing);
				isSuperSamplingActive = true;
			}
			float num2 = (float)Screen.width / num;
			float num3 = (float)Screen.height / num;
			desc.width = Mathf.RoundToInt(Mathf.Clamp(num2, 1f, 8192f));
			float num4 = num3 / num2;
			desc.height = Mathf.RoundToInt(Mathf.Clamp(num2 * num4, 1f, 8192f));
			pixelateTexture = RenderTexture.GetTemporary(desc);
			currentCamera.targetTexture = pixelateTexture;
		}
		else if (_pixelateDownscale && _pixelateAmount > 1 && rtDescBase.width > 1 && rtDescBase.height > 1)
		{
			RenderTextureDescriptor desc2 = rtDescBase;
			desc2.width = Mathf.RoundToInt(Mathf.Max(1, currentCamera.pixelWidth / _pixelateAmount));
			float num5 = (float)currentCamera.pixelHeight / (float)currentCamera.pixelWidth;
			desc2.height = Mathf.Max(1, Mathf.RoundToInt((float)desc2.width * num5));
			pixelateTexture = RenderTexture.GetTemporary(desc2);
			currentCamera.targetTexture = pixelateTexture;
		}
	}

	private void CleanUpRT()
	{
		if (dofDepthTexture != null)
		{
			RenderTexture.ReleaseTemporary(dofDepthTexture);
			dofDepthTexture = null;
		}
		if (dofExclusionTexture != null)
		{
			RenderTexture.ReleaseTemporary(dofExclusionTexture);
			dofExclusionTexture = null;
		}
		if (bloomSourceTexture != null)
		{
			RenderTexture.ReleaseTemporary(bloomSourceTexture);
			bloomSourceTexture = null;
		}
		if (bloomSourceDepthTexture != null)
		{
			RenderTexture.ReleaseTemporary(bloomSourceDepthTexture);
			bloomSourceDepthTexture = null;
		}
		if (bloomSourceTextureRightEye != null)
		{
			RenderTexture.ReleaseTemporary(bloomSourceTextureRightEye);
			bloomSourceTextureRightEye = null;
		}
		if (bloomSourceDepthTextureRightEye != null)
		{
			RenderTexture.ReleaseTemporary(bloomSourceDepthTextureRightEye);
			bloomSourceDepthTextureRightEye = null;
		}
		if (anamorphicFlaresSourceTexture != null)
		{
			RenderTexture.ReleaseTemporary(anamorphicFlaresSourceTexture);
			anamorphicFlaresSourceTexture = null;
		}
		if (anamorphicFlaresSourceDepthTexture != null)
		{
			RenderTexture.ReleaseTemporary(anamorphicFlaresSourceDepthTexture);
			anamorphicFlaresSourceDepthTexture = null;
		}
		if (anamorphicFlaresSourceTextureRightEye != null)
		{
			RenderTexture.ReleaseTemporary(anamorphicFlaresSourceTextureRightEye);
			anamorphicFlaresSourceTextureRightEye = null;
		}
		if (anamorphicFlaresSourceDepthTextureRightEye != null)
		{
			RenderTexture.ReleaseTemporary(anamorphicFlaresSourceDepthTextureRightEye);
			anamorphicFlaresSourceDepthTextureRightEye = null;
		}
		if (pixelateTexture != null)
		{
			RenderTexture.ReleaseTemporary(pixelateTexture);
			pixelateTexture = null;
		}
	}

	private RenderTextureDescriptor GetDefaultRenderTextureDescriptor()
	{
		RenderTextureDescriptor result = new RenderTextureDescriptor(currentCamera.pixelWidth, currentCamera.pixelHeight, RenderTextureFormat.ARGB32, 24);
		result.msaaSamples = Math.Max(1, QualitySettings.antiAliasing);
		result.sRGB = !linearColorSpace;
		return result;
	}

	private void CheckDoFTransparencySupport()
	{
		if (depthCam == null)
		{
			if (depthCamObj == null)
			{
				depthCamObj = new GameObject("DepthCamera");
				depthCamObj.hideFlags = HideFlags.HideAndDontSave;
				depthCam = depthCamObj.AddComponent<Camera>();
				depthCam.enabled = false;
			}
			else
			{
				depthCam = depthCamObj.GetComponent<Camera>();
				if (depthCam == null)
				{
					UnityEngine.Object.DestroyImmediate(depthCamObj);
					depthCamObj = null;
					return;
				}
			}
		}
		depthCam.CopyFrom(currentCamera);
		depthCam.rect = new Rect(0f, 0f, 1f, 1f);
		depthCam.depthTextureMode = DepthTextureMode.None;
		depthCam.renderingPath = RenderingPath.Forward;
		float num = _depthOfFieldTransparencySupportDownsampling * (float)_depthOfFieldDownsampling;
		dofDepthTexture = RenderTexture.GetTemporary((int)((float)currentCamera.pixelWidth / num), (int)((float)currentCamera.pixelHeight / num), 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		dofDepthTexture.filterMode = FilterMode.Point;
		depthCam.backgroundColor = new Color(84f / 85f, 0.4470558f, 0.75f, 0f);
		depthCam.clearFlags = CameraClearFlags.Color;
		depthCam.targetTexture = dofDepthTexture;
		depthCam.cullingMask = _depthOfFieldTransparencyLayerMask;
		if (depthShader == null)
		{
			depthShader = Shader.Find("Hidden/Kronnect/Beautify/CopyDepth");
		}
		depthCam.RenderWithShader(depthShader, "RenderType");
		bMat.SetTexture(ShaderParams.DepthTexture, dofDepthTexture);
	}

	private void CheckDoFExclusionMask()
	{
		if (depthCam == null)
		{
			if (depthCamObj == null)
			{
				depthCamObj = new GameObject("DepthCamera");
				depthCamObj.hideFlags = HideFlags.HideAndDontSave;
				depthCam = depthCamObj.AddComponent<Camera>();
				depthCam.enabled = false;
			}
			else
			{
				depthCam = depthCamObj.GetComponent<Camera>();
				if (depthCam == null)
				{
					UnityEngine.Object.DestroyImmediate(depthCamObj);
					depthCamObj = null;
					return;
				}
			}
		}
		depthCam.CopyFrom(currentCamera);
		depthCam.rect = new Rect(0f, 0f, 1f, 1f);
		depthCam.depthTextureMode = DepthTextureMode.None;
		depthCam.renderingPath = RenderingPath.Forward;
		float num = _depthOfFieldExclusionLayerMaskDownsampling * (float)_depthOfFieldDownsampling;
		dofExclusionTexture = RenderTexture.GetTemporary((int)((float)currentCamera.pixelWidth / num), (int)((float)currentCamera.pixelHeight / num), 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		dofExclusionTexture.filterMode = FilterMode.Point;
		depthCam.backgroundColor = new Color(84f / 85f, 0.4470558f, 0.75f, 0f);
		depthCam.clearFlags = CameraClearFlags.Color;
		depthCam.targetTexture = dofExclusionTexture;
		depthCam.cullingMask = _depthOfFieldExclusionLayerMask;
		if (dofExclusionShader == null)
		{
			dofExclusionShader = Shader.Find("Hidden/Kronnect/Beautify/CopyDepthBiased");
		}
		depthCam.RenderWithShader(dofExclusionShader, null);
		bMat.SetTexture(ShaderParams.DoFExclusionTexture, dofExclusionTexture);
	}

	private void CheckBloomAndFlaresCulling()
	{
		if (rtDescBase.volumeDepth == 0)
		{
			return;
		}
		if (depthCam == null)
		{
			if (depthCamObj == null)
			{
				depthCamObj = new GameObject("DepthCamera");
				depthCamObj.hideFlags = HideFlags.HideAndDontSave;
				depthCam = depthCamObj.AddComponent<Camera>();
				depthCam.enabled = false;
			}
			else
			{
				depthCam = depthCamObj.GetComponent<Camera>();
				if (depthCam == null)
				{
					UnityEngine.Object.DestroyImmediate(depthCamObj);
					depthCamObj = null;
					return;
				}
			}
		}
		depthCam.CopyFrom(currentCamera);
		depthCam.rect = new Rect(0f, 0f, 1f, 1f);
		depthCam.depthTextureMode = DepthTextureMode.None;
		depthCam.allowMSAA = false;
		depthCam.allowHDR = false;
		depthCam.clearFlags = CameraClearFlags.Color;
		depthCam.stereoTargetEye = StereoTargetEyeMask.None;
		depthCam.renderingPath = RenderingPath.Forward;
		depthCam.backgroundColor = Color.black;
		if (_bloom && isUsingBloomLayerMask)
		{
			depthCam.cullingMask = _bloomCullingMask;
			if (_quality == BEAUTIFY_QUALITY.BestPerformance)
			{
				eyeWidth = (_bloomUltra ? ((int)(Mathf.Lerp(256f, currentCamera.pixelHeight, (float)_bloomUltraResolution / 10f) / 4f) * 4) : 256);
			}
			else
			{
				eyeWidth = (_bloomUltra ? ((int)(Mathf.Lerp(512f, currentCamera.pixelHeight, (float)_bloomUltraResolution / 10f) / 4f) * 4) : 512);
			}
			eyeWidth = (int)((float)eyeWidth * (1f / _bloomLayerMaskDownsampling) / 4f) * 4;
		}
		else
		{
			depthCam.cullingMask = _anamorphicFlaresCullingMask;
			if (_quality == BEAUTIFY_QUALITY.BestPerformance)
			{
				eyeWidth = (_anamorphicFlaresUltra ? ((int)(Mathf.Lerp(256f, currentCamera.pixelHeight, (float)_anamorphicFlaresUltraResolution / 10f) / 4f) * 4) : 256);
			}
			else
			{
				eyeWidth = (_anamorphicFlaresUltra ? ((int)(Mathf.Lerp(512f, currentCamera.pixelHeight, (float)_anamorphicFlaresUltraResolution / 10f) / 4f) * 4) : 512);
			}
			eyeWidth = (int)((float)eyeWidth * (1f / _anamorphicFlaresLayerMaskDownsampling) / 4f) * 4;
		}
		float num = (float)currentCamera.pixelHeight / (float)currentCamera.pixelWidth;
		eyeHeight = Mathf.Max(1, (int)((float)eyeWidth * num));
		if (VRCheck.isVrRunning)
		{
			depthCam.projectionMatrix = currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
		}
		RenderLeftEyeDepth();
		if (VRCheck.isVrRunning)
		{
			depthCam.projectionMatrix = currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
			RenderRightEyeDepth();
		}
		if (_bloom && _anamorphicFlares && (int)_anamorphicFlaresCullingMask != (int)_bloomCullingMask)
		{
			depthCam.cullingMask = _anamorphicFlaresCullingMask;
			if (VRCheck.isVrRunning)
			{
				depthCam.projectionMatrix = currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
			}
			if (_quality == BEAUTIFY_QUALITY.BestPerformance)
			{
				eyeWidth = 256;
			}
			else
			{
				eyeWidth = (_anamorphicFlaresUltra ? ((int)(Mathf.Lerp(512f, currentCamera.pixelHeight, (float)_anamorphicFlaresUltraResolution / 10f) / 4f) * 4) : 512);
				eyeWidth = (int)((float)eyeWidth * (1f / _anamorphicFlaresLayerMaskDownsampling) / 4f) * 4;
			}
			num = (float)currentCamera.pixelHeight / (float)currentCamera.pixelWidth;
			eyeHeight = Mathf.Max(1, (int)((float)eyeWidth * num));
			RenderLeftEyeDepthAF();
			if (VRCheck.isVrRunning)
			{
				depthCam.projectionMatrix = currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
				RenderRightEyeDepthAF();
			}
		}
	}

	private void RenderLeftEyeDepth()
	{
		RenderTextureDescriptor desc = rtDescBase;
		desc.width = eyeWidth;
		desc.height = eyeHeight;
		desc.depthBufferBits = 24;
		desc.colorFormat = RenderTextureFormat.Depth;
		bloomSourceDepthTexture = RenderTexture.GetTemporary(desc);
		desc.depthBufferBits = 0;
		desc.colorFormat = rtFormat;
		bloomSourceTexture = RenderTexture.GetTemporary(desc);
		depthCam.SetTargetBuffers(bloomSourceTexture.colorBuffer, bloomSourceDepthTexture.depthBuffer);
		depthCam.Render();
		bMat.SetTexture(ShaderParams.BloomSourceTexture, bloomSourceTexture);
		bMat.SetTexture(ShaderParams.BloomSourceDepthTexture, bloomSourceDepthTexture);
	}

	private void RenderRightEyeDepth()
	{
		RenderTextureDescriptor desc = rtDescBase;
		desc.width = eyeWidth;
		desc.height = eyeHeight;
		desc.depthBufferBits = 24;
		desc.colorFormat = RenderTextureFormat.Depth;
		bloomSourceDepthTextureRightEye = RenderTexture.GetTemporary(desc);
		desc.depthBufferBits = 0;
		desc.colorFormat = rtFormat;
		bloomSourceTextureRightEye = RenderTexture.GetTemporary(desc);
		depthCam.SetTargetBuffers(bloomSourceTextureRightEye.colorBuffer, bloomSourceDepthTextureRightEye.depthBuffer);
		depthCam.Render();
		bMat.SetTexture(ShaderParams.BloomSourceRightEyeTexture, bloomSourceTextureRightEye);
		bMat.SetTexture(ShaderParams.BloomSourceRightEyeDepthTexture, bloomSourceDepthTextureRightEye);
	}

	private void RenderLeftEyeDepthAF()
	{
		RenderTextureDescriptor desc = rtDescBase;
		desc.width = eyeWidth;
		desc.height = eyeHeight;
		desc.depthBufferBits = 24;
		desc.colorFormat = RenderTextureFormat.Depth;
		anamorphicFlaresSourceDepthTexture = RenderTexture.GetTemporary(desc);
		desc.depthBufferBits = 0;
		desc.colorFormat = rtFormat;
		anamorphicFlaresSourceTexture = RenderTexture.GetTemporary(desc);
		depthCam.SetTargetBuffers(anamorphicFlaresSourceTexture.colorBuffer, anamorphicFlaresSourceDepthTexture.depthBuffer);
		depthCam.Render();
		bMat.SetTexture(ShaderParams.BloomSourceTexture, bloomSourceTexture);
		bMat.SetTexture(ShaderParams.BloomSourceDepthTexture, bloomSourceDepthTexture);
	}

	private void RenderRightEyeDepthAF()
	{
		RenderTextureDescriptor desc = rtDescBase;
		desc.width = eyeWidth;
		desc.height = eyeHeight;
		desc.depthBufferBits = 24;
		desc.colorFormat = RenderTextureFormat.Depth;
		anamorphicFlaresSourceDepthTextureRightEye = RenderTexture.GetTemporary(desc);
		desc.depthBufferBits = 0;
		desc.colorFormat = rtFormat;
		anamorphicFlaresSourceTextureRightEye = RenderTexture.GetTemporary(desc);
		depthCam.SetTargetBuffers(anamorphicFlaresSourceTextureRightEye.colorBuffer, anamorphicFlaresSourceDepthTextureRightEye.depthBuffer);
		depthCam.Render();
		bMat.SetTexture(ShaderParams.BloomSourceRightEyeTexture, bloomSourceTextureRightEye);
		bMat.SetTexture(ShaderParams.BloomSourceRightEyeDepthTexture, bloomSourceDepthTextureRightEye);
	}

	private int GetRawCopyPass()
	{
		if (_quality != BEAUTIFY_QUALITY.BestQuality)
		{
			return 18;
		}
		return 22;
	}

	protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_ = Camera.current;
		if (bMat == null || !base.enabled)
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (shouldUpdateMaterialProperties)
		{
			UpdateMaterialPropertiesNow();
		}
		bool flag = _quality != BEAUTIFY_QUALITY.Basic;
		rtDescBase = source.descriptor;
		if (isSuperSamplingActive)
		{
			rtDescBase.width = currentCamera.pixelWidth;
			rtDescBase.height = currentCamera.pixelHeight;
		}
		rtDescBase.msaaSamples = 1;
		rtDescBase.colorFormat = rtFormat;
		rtDescBase.depthBufferBits = 0;
		RenderTexture renderTexture = null;
		if (_quality == BEAUTIFY_QUALITY.BestQuality && (float)_superSampling > 1f)
		{
			RenderTextureDescriptor desc = rtDescBase;
			desc.width = currentCamera.pixelWidth;
			desc.height = currentCamera.pixelHeight;
			renderTexture = RenderTexture.GetTemporary(desc);
			Graphics.Blit(source, renderTexture, bMat, 22);
			source = renderTexture;
		}
		RenderTexture renderTexture2 = null;
		RenderTexture renderTexture3 = null;
		RenderTexture renderTexture4 = null;
		float num = (float)source.height / (float)source.width;
		bool flag2 = _blur && _blurIntensity > 0f && flag;
		if (renderPass == 0 || flag2)
		{
			if (flag2)
			{
				int num2;
				if (_blurIntensity < 1f)
				{
					num2 = (int)Mathf.Lerp(currentCamera.pixelWidth, 512f, _blurIntensity);
					if (_quality == BEAUTIFY_QUALITY.BestPerformance)
					{
						num2 /= 2;
					}
				}
				else
				{
					num2 = ((_quality == BEAUTIFY_QUALITY.BestQuality) ? 512 : 256);
					num2 = (int)((float)num2 / _blurIntensity);
				}
				RenderTextureDescriptor desc2 = rtDescBase;
				desc2.width = num2;
				desc2.height = Mathf.Max(1, (int)((float)num2 * num));
				renderTexture3 = RenderTexture.GetTemporary(desc2);
				if (renderPass == 0)
				{
					renderTexture2 = RenderTexture.GetTemporary(desc2);
				}
			}
			else
			{
				renderTexture2 = RenderTexture.GetTemporary(rtDescBase);
			}
		}
		RenderTexture renderTexture5 = null;
		RenderTexture renderTexture6 = null;
		RenderTexture renderTexture7 = null;
		if (flag)
		{
			if (_pixelateAmount > 1)
			{
				source.filterMode = FilterMode.Point;
				if (!_pixelateDownscale)
				{
					RenderTextureDescriptor desc3 = rtDescBase;
					desc3.width = Mathf.RoundToInt(Mathf.Max(1, source.width / _pixelateAmount));
					desc3.height = Mathf.Max(1, Mathf.RoundToInt((float)desc3.width * num));
					renderTexture5 = RenderTexture.GetTemporary(desc3);
					renderTexture5.filterMode = FilterMode.Point;
					Graphics.Blit(source, renderTexture5, bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 22 : 18);
					source = renderTexture5;
				}
			}
			if (_depthOfField)
			{
				UpdateDepthOfFieldData();
				int pass = ((_quality == BEAUTIFY_QUALITY.BestQuality) ? 12 : 6);
				RenderTextureDescriptor desc4 = rtDescBase;
				desc4.width = source.width / _depthOfFieldDownsampling;
				desc4.height = source.height / _depthOfFieldDownsampling;
				renderTexture6 = RenderTexture.GetTemporary(desc4);
				renderTexture6.filterMode = _depthOfFieldFilterMode;
				Graphics.Blit(source, renderTexture6, bMat, pass);
				if (_quality == BEAUTIFY_QUALITY.BestQuality && _depthOfFieldForegroundBlur && _depthOfFieldForegroundBlurHQ)
				{
					BlurThisAlpha(renderTexture6, _depthOfFieldForegroundBlurHQSpread);
				}
				if (depthOfFieldBokehComposition == BEAUTIFY_BOKEH_COMPOSITION.Integrated || _quality != BEAUTIFY_QUALITY.BestQuality || !depthOfFieldBokeh)
				{
					pass = ((_quality != BEAUTIFY_QUALITY.BestQuality) ? (_depthOfFieldBokeh ? 8 : 15) : (_depthOfFieldBokeh ? 14 : 19));
					BlurThisDoF(renderTexture6, pass);
				}
				else
				{
					BlurThisDoF(renderTexture6, 19);
					RenderTexture temporary = RenderTexture.GetTemporary(desc4);
					Graphics.Blit(source, temporary, bMat, 30);
					BlurThisDoF(temporary, 32);
					Graphics.Blit(temporary, renderTexture6, bMat, 31);
					RenderTexture.ReleaseTemporary(temporary);
				}
				if (_depthOfFieldDebug)
				{
					pass = ((_quality == BEAUTIFY_QUALITY.BestQuality) ? 13 : 7);
					Graphics.Blit(renderTexture6, destination, bMat, pass);
					RenderTexture.ReleaseTemporary(renderTexture6);
					return;
				}
				Shader.SetGlobalTexture(ShaderParams.DoFTexture, renderTexture6);
			}
			if (_outline && _outlineCustomize && _quality == BEAUTIFY_QUALITY.BestQuality && _outlineStage == BEAUTIFY_OUTLINE_STAGE.BeforeBloom)
			{
				SeparateOutlinePass(source);
			}
		}
		bool flag3 = _sunFlares && _sun != null;
		if (flag && (_lensDirt || _bloom || _anamorphicFlares || flag3))
		{
			RenderTexture renderTexture8 = null;
			int num3;
			int num4;
			int num5;
			if (_quality == BEAUTIFY_QUALITY.BestPerformance)
			{
				num3 = _bloomIterations;
				num4 = 4;
				num5 = 256;
			}
			else
			{
				num3 = (num4 = 5);
				num5 = 512;
			}
			int a;
			if (_bloomUltra)
			{
				a = (int)((float)(source.width * _bloomUltraResolution) / 10f) / 4 * 4;
				a = Mathf.Max(a, num5);
			}
			else
			{
				a = num5;
			}
			if (rt == null || rt.Length != num3 + 1)
			{
				rt = new RenderTexture[num3 + 1];
			}
			if (rtAF == null || rtAF.Length != num4 + 1)
			{
				rtAF = new RenderTexture[num4 + 1];
			}
			if (_bloom || (_lensDirt && !_anamorphicFlares))
			{
				UpdateMaterialBloomIntensityAndThreshold();
				RenderTextureDescriptor desc5 = rtDescBase;
				for (int i = 0; i <= num3; i++)
				{
					desc5.width = Mathf.Max(1, a);
					desc5.height = Mathf.Max(1, (int)((float)a * num));
					rt[i] = RenderTexture.GetTemporary(desc5);
					a /= 2;
				}
				renderTexture8 = rt[0];
				if (_bloomAntiflicker)
				{
					Graphics.Blit(source, rt[0], bMat, (_quality == BEAUTIFY_QUALITY.BestPerformance) ? 22 : 9);
				}
				else
				{
					Graphics.Blit(source, rt[0], bMat, 2);
				}
				if (!_bloomUltra || _bloomUltraResolution < 6)
				{
					BlurThis(rt[0]);
				}
				for (int j = 0; j < num3; j++)
				{
					if (_quality == BEAUTIFY_QUALITY.BestPerformance)
					{
						if (_bloomBlur)
						{
							BlurThisDownscaling(rt[j], rt[j + 1]);
						}
						else
						{
							Graphics.Blit(rt[j], rt[j + 1], bMat, 20);
						}
					}
					else if (_bloomQuickerBlur)
					{
						BlurThisDownscaling(rt[j], rt[j + 1]);
					}
					else
					{
						Graphics.Blit(rt[j], rt[j + 1], bMat, 7);
						BlurThis(rt[j + 1]);
					}
				}
				if (_bloom)
				{
					bMat.SetColor(ShaderParams.BloomTint, ColorTransparent);
					bool flag4 = quality == BEAUTIFY_QUALITY.BestQuality && _bloomCustomize;
					for (int num6 = num3; num6 > 0; num6--)
					{
						if (num6 == 1 && !flag4)
						{
							bMat.SetColor(ShaderParams.BloomTint, _bloomTint);
						}
						Graphics.Blit(rt[num6], rt[num6 - 1], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 8 : 21);
					}
					if (flag4)
					{
						bMat.SetColor(ShaderParams.BloomTint, _bloomTint);
						bMat.SetTexture(ShaderParams.BloomTexture4, rt[4]);
						bMat.SetTexture(ShaderParams.BloomTexture3, rt[3]);
						bMat.SetTexture(ShaderParams.BloomTexture2, rt[2]);
						bMat.SetTexture(ShaderParams.BloomTexture1, rt[1]);
						Shader.SetGlobalTexture(ShaderParams.BloomTexture, rt[0]);
						renderTexture4 = RenderTexture.GetTemporary(rt[0].descriptor);
						renderTexture8 = renderTexture4;
						Graphics.Blit(rt[num3], renderTexture8, bMat, 6);
					}
					bMat.SetColor(ShaderParams.BloomTint, ColorTransparent);
				}
			}
			if (_anamorphicFlares)
			{
				UpdateMaterialAnamorphicIntensityAndThreshold();
				int a2;
				if (_anamorphicFlaresUltra)
				{
					a2 = (int)((float)(source.width * _anamorphicFlaresUltraResolution) / 10f) / 4 * 4;
					a2 = Mathf.Max(a2, num5);
				}
				else
				{
					a2 = num5;
				}
				RenderTextureDescriptor desc6 = rtDescBase;
				int num7 = a2;
				for (int k = 0; k <= num4; k++)
				{
					int b = (int)((float)a2 / _anamorphicFlaresSpread);
					if (_anamorphicFlaresVertical)
					{
						desc6.width = num7;
						desc6.height = Mathf.Max(1, b);
					}
					else
					{
						desc6.width = Mathf.Max(1, b);
						desc6.height = num7;
					}
					EnsureSafeDimensions(ref desc6);
					rtAF[k] = RenderTexture.GetTemporary(desc6);
					a2 /= 2;
				}
				if (isUsingAnamorphicFlaresLayerMask)
				{
					if (_bloom)
					{
						if ((int)_anamorphicFlaresCullingMask != (int)_bloomCullingMask)
						{
							bMat.SetTexture(ShaderParams.BloomSourceTexture, anamorphicFlaresSourceTexture);
							bMat.SetTexture(ShaderParams.BloomSourceDepthTexture, anamorphicFlaresSourceDepthTexture);
							bMat.SetTexture(ShaderParams.BloomSourceRightEyeTexture, anamorphicFlaresSourceTextureRightEye);
							bMat.SetTexture(ShaderParams.BloomSourceRightEyeDepthTexture, anamorphicFlaresSourceDepthTextureRightEye);
						}
					}
					else
					{
						bMat.SetTexture(ShaderParams.BloomSourceTexture, bloomSourceTexture);
						bMat.SetTexture(ShaderParams.BloomSourceDepthTexture, bloomSourceDepthTexture);
						bMat.SetTexture(ShaderParams.BloomSourceRightEyeTexture, bloomSourceTextureRightEye);
						bMat.SetTexture(ShaderParams.BloomSourceRightEyeDepthTexture, bloomSourceDepthTextureRightEye);
					}
				}
				if (_anamorphicFlaresAntiflicker)
				{
					Graphics.Blit(source, rtAF[0], bMat, (_quality == BEAUTIFY_QUALITY.BestPerformance) ? 22 : 9);
				}
				else
				{
					Graphics.Blit(source, rtAF[0], bMat, 2);
				}
				rtAF[0] = BlurThisOneDirection(rtAF[0], _anamorphicFlaresVertical);
				for (int l = 0; l < num4; l++)
				{
					if (_quality == BEAUTIFY_QUALITY.BestPerformance)
					{
						Graphics.Blit(rtAF[l], rtAF[l + 1], bMat, 18);
						if (_anamorphicFlaresBlur)
						{
							rtAF[l + 1] = BlurThisOneDirection(rtAF[l + 1], _anamorphicFlaresVertical);
						}
					}
					else
					{
						Graphics.Blit(rtAF[l], rtAF[l + 1], bMat, 7);
						rtAF[l + 1] = BlurThisOneDirection(rtAF[l + 1], _anamorphicFlaresVertical);
					}
				}
				for (int num8 = num4; num8 > 0; num8--)
				{
					if (num8 == 1)
					{
						Graphics.Blit(rtAF[num8], rtAF[num8 - 1], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 10 : 14);
					}
					else
					{
						Graphics.Blit(rtAF[num8], rtAF[num8 - 1], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 8 : 13);
					}
				}
				if (_bloom)
				{
					if (_lensDirt)
					{
						Graphics.Blit(rtAF[3], rt[3], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 11 : 13);
					}
					Graphics.Blit(rtAF[0], renderTexture8, bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 11 : 13);
				}
				else
				{
					renderTexture8 = rtAF[0];
				}
				UpdateMaterialBloomIntensityAndThreshold();
			}
			if (flag3)
			{
				Vector3 vector;
				Vector3 direction;
				float num9;
				if (sunIsSpotlight)
				{
					vector = _sun.transform.position;
					direction = vector - currentCamera.transform.position;
					num9 = direction.magnitude;
					direction /= num9;
				}
				else
				{
					vector = currentCamera.transform.position - _sun.transform.forward * 1000f;
					num9 = currentCamera.farClipPlane;
					direction = -_sun.transform.forward;
				}
				Vector3 vector2 = currentCamera.WorldToViewportPoint(vector, (!VRCheck.isVrRunning) ? Camera.MonoOrStereoscopicEye.Mono : Camera.MonoOrStereoscopicEye.Left);
				float num10 = 0f;
				if (vector2.z > 0f && vector2.x >= -0.1f && vector2.x < 1.1f && vector2.y >= -0.1f && vector2.y < 1.1f && !Physics.Raycast(new Ray(currentCamera.transform.position, direction), num9, _sunFlaresLayerMask))
				{
					Vector2 vector3 = vector2 - Vector3.one * 0.5f;
					num10 = _sunFlaresIntensity * Mathf.Clamp01((0.6f - Mathf.Max(Mathf.Abs(vector3.x), Mathf.Abs(vector3.y))) / 0.6f);
				}
				if (num10 > sunFlareCurrentIntensity)
				{
					sunFlareCurrentIntensity = Mathf.Lerp(sunFlareCurrentIntensity, num10, Application.isPlaying ? (30f * Time.unscaledDeltaTime * _sunFlaresRevealSpeed) : 1f);
				}
				else
				{
					sunFlareCurrentIntensity = Mathf.Lerp(sunFlareCurrentIntensity, num10, Application.isPlaying ? (30f * Time.unscaledDeltaTime * _sunFlaresHideSpeed) : 1f);
				}
				if (sunFlareCurrentIntensity > 0f)
				{
					sunLastScrPos = vector2;
					bMat.SetColor(ShaderParams.SFSunTint, _sunFlaresTint * sunFlareCurrentIntensity);
					sunLastScrPos.z = 0.5f + sunFlareTime * _sunFlaresSolarWindSpeed;
					Vector2 vector4 = new Vector2(0.5f - sunLastScrPos.y, sunLastScrPos.x - 0.5f);
					if (!_sunFlaresRotationDeadZone || vector4.sqrMagnitude > 0.00025f)
					{
						sunLastRot = Mathf.Atan2(vector4.x, vector4.y);
					}
					sunLastScrPos.w = sunLastRot;
					sunFlareTime += Time.deltaTime;
					bMat.SetVector(ShaderParams.SFSunPos, sunLastScrPos);
					if (VRCheck.isVrRunning)
					{
						Vector3 vector5 = currentCamera.WorldToViewportPoint(vector, Camera.MonoOrStereoscopicEye.Right);
						bMat.SetVector(ShaderParams.SFSunPosRightEye, vector5);
					}
					RenderTextureDescriptor desc7 = rtDescBase;
					desc7.width /= _sunFlaresDownsampling;
					desc7.height /= _sunFlaresDownsampling;
					renderTexture7 = RenderTexture.GetTemporary(desc7);
					int pass2 = ((_quality != BEAUTIFY_QUALITY.BestQuality) ? ((renderTexture8 != null) ? 17 : 16) : ((renderTexture8 != null) ? 21 : 20));
					bMat.SetTexture(ShaderParams.SFMainTexture, source);
					Graphics.Blit((renderTexture8 != null) ? renderTexture8 : source, renderTexture7, bMat, pass2);
					if (_lensDirt && _bloom)
					{
						Graphics.Blit(renderTexture7, rt[3], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 11 : 13);
					}
					renderTexture8 = renderTexture7;
					if (!_bloom && !_anamorphicFlares)
					{
						bMat.SetVector(ShaderParams.Bloom, Vector4.one);
						if (!bMat.IsKeywordEnabled("BEAUTIFY_BLOOM"))
						{
							bMat.EnableKeyword("BEAUTIFY_BLOOM");
						}
					}
				}
			}
			if (renderTexture8 != null)
			{
				Shader.SetGlobalTexture(ShaderParams.BloomTexture, renderTexture8);
			}
			else
			{
				if (bMat.IsKeywordEnabled("BEAUTIFY_BLOOM"))
				{
					bMat.DisableKeyword("BEAUTIFY_BLOOM");
				}
				bMat.SetVector(ShaderParams.Bloom, Vector4.zero);
			}
			if (_lensDirt)
			{
				Shader.SetGlobalTexture(ShaderParams.ScreenLum, (_anamorphicFlares && !_bloom) ? rtAF[3] : rt[3]);
			}
		}
		if (_lensDirt)
		{
			Vector4 value = new Vector4(1f, 1f / (1.01f - _lensDirtIntensity), _lensDirtThreshold, Mathf.Max(_bloomIntensity, 1f));
			bMat.SetVector(ShaderParams.Dirt, value);
		}
		if (flag && (_eyeAdaptation || _purkinje))
		{
			int num11 = ((_quality == BEAUTIFY_QUALITY.BestQuality) ? 9 : 8);
			int num12 = (int)Mathf.Pow(2f, num11);
			if (rtEA == null || rtEA.Length < num11)
			{
				rtEA = new RenderTexture[num11];
			}
			RenderTextureDescriptor desc8 = rtDescBase;
			for (int m = 0; m < num11; m++)
			{
				desc8.width = num12;
				desc8.height = num12;
				rtEA[m] = RenderTexture.GetTemporary(desc8);
				num12 /= 2;
			}
			Graphics.Blit(source, rtEA[0], bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 22 : 18);
			int num13 = num11 - 1;
			int num14 = ((_quality == BEAUTIFY_QUALITY.BestQuality) ? 15 : 9);
			for (int n = 0; n < num13; n++)
			{
				Graphics.Blit(rtEA[n], rtEA[n + 1], bMat, (n == 0) ? num14 : (num14 + 1));
			}
			bMat.SetTexture(ShaderParams.EALumSrc, rtEA[num13]);
			if (rtEAacum == null)
			{
				int rawCopyPass = GetRawCopyPass();
				RenderTextureDescriptor desc9 = rtDescBase;
				desc9.width = 2;
				desc9.height = 2;
				rtEAacum = new RenderTexture(desc9);
				Graphics.Blit(rtEA[num13], rtEAacum, bMat, rawCopyPass);
				rtEAHist = new RenderTexture(desc9);
				Graphics.Blit(rtEAacum, rtEAHist, bMat, rawCopyPass);
			}
			else
			{
				Graphics.Blit(rtEA[num13], rtEAacum, bMat, num14 + 2);
				Graphics.Blit(rtEAacum, rtEAHist, bMat, num14 + 3);
			}
			bMat.SetTexture(ShaderParams.EAHist, rtEAHist);
		}
		if (_outline && _outlineCustomize && _quality == BEAUTIFY_QUALITY.BestQuality && _outlineStage == BEAUTIFY_OUTLINE_STAGE.AfterBloom)
		{
			SeparateOutlinePass(source);
		}
		RenderTexture renderTexture9 = destination;
		RenderTexture renderTexture10 = null;
		if (renderTexture2 != null)
		{
			Graphics.Blit(source, renderTexture2, bMat, 1);
			Shader.SetGlobalTexture(ShaderParams.CompareTexture, renderTexture2);
			if (_chromaticAberration && _depthOfField)
			{
				renderTexture10 = RenderTexture.GetTemporary(rtDescBase);
				Graphics.Blit(renderTexture2, renderTexture10, bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 29 : 19);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = renderTexture10;
			}
		}
		else if (_chromaticAberration && _depthOfField)
		{
			renderTexture9 = RenderTexture.GetTemporary(rtDescBase);
		}
		if (renderTexture3 != null)
		{
			float blurScale = ((_blurIntensity > 1f) ? 1f : _blurIntensity);
			if (renderTexture2 != null)
			{
				Graphics.Blit(renderTexture2, renderTexture3, bMat, renderPass);
				BlurThis(renderTexture3, blurScale);
			}
			else
			{
				BlurThisDownscaling(source, renderTexture3, blurScale);
			}
			BlurThis(renderTexture3, blurScale);
			if (_quality == BEAUTIFY_QUALITY.BestQuality)
			{
				BlurThis(renderTexture3, blurScale);
			}
			if (renderTexture2 != null)
			{
				Shader.SetGlobalTexture(ShaderParams.CompareTexture, renderTexture3);
				Graphics.Blit(source, renderTexture9, bMat, renderPass);
			}
			else
			{
				Graphics.Blit(renderTexture3, renderTexture9, bMat, renderPass);
			}
			RenderTexture.ReleaseTemporary(renderTexture3);
		}
		else
		{
			Graphics.Blit(source, renderTexture9, bMat, renderPass);
		}
		if (renderTexture9 != destination)
		{
			Graphics.Blit(renderTexture9, destination, bMat, (_quality == BEAUTIFY_QUALITY.BestQuality) ? 29 : 19);
			RenderTexture.ReleaseTemporary(renderTexture9);
		}
		if (rtEA != null)
		{
			for (int num15 = 0; num15 < rtEA.Length; num15++)
			{
				if (rtEA[num15] != null)
				{
					RenderTexture.ReleaseTemporary(rtEA[num15]);
					rtEA[num15] = null;
				}
			}
		}
		if (rt != null)
		{
			for (int num16 = 0; num16 < rt.Length; num16++)
			{
				if (rt[num16] != null)
				{
					RenderTexture.ReleaseTemporary(rt[num16]);
					rt[num16] = null;
				}
			}
		}
		if (rtAF != null)
		{
			for (int num17 = 0; num17 < rtAF.Length; num17++)
			{
				if (rtAF[num17] != null)
				{
					RenderTexture.ReleaseTemporary(rtAF[num17]);
					rtAF[num17] = null;
				}
			}
		}
		if (renderTexture4 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture4);
		}
		if (renderTexture6 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture6);
		}
		if (renderTexture2 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		if (renderTexture5 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture5);
		}
		if (renderTexture7 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture7);
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	private void EnsureSafeDimensions(ref RenderTextureDescriptor desc)
	{
		desc.width = Mathf.Clamp(desc.width, 1, 8192);
		desc.height = Mathf.Clamp(desc.height, 1, 8192);
	}

	private void SeparateOutlinePass(RenderTexture source)
	{
		RenderTextureDescriptor desc = rtDescBase;
		desc.colorFormat = rtOutlineColorFormat;
		RenderTexture temporary = RenderTexture.GetTemporary(desc);
		Graphics.Blit(source, temporary, bMat, 25);
		for (int i = 1; i <= _outlineBlurPassCount; i++)
		{
			BlurThisOutline(temporary, _outlineSpread, (!_outlineBlurDownscale) ? 1 : i);
		}
		Graphics.Blit(temporary, source, bMat, 28);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void OnPostRender()
	{
		if (Camera.current.cameraType != CameraType.SceneView)
		{
			if (renderScale != 1f && pixelateTexture != null)
			{
				currentCamera.targetTexture = null;
				RenderTexture.active = null;
			}
			else if (_pixelateDownscale && _pixelateAmount > 1 && pixelateTexture != null)
			{
				RenderTexture.active = null;
				currentCamera.targetTexture = null;
				pixelateTexture.filterMode = FilterMode.Point;
			}
		}
	}

	private void BlurThis(RenderTexture rt, float blurScale = 1f)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(rt.descriptor);
		temporary.filterMode = FilterMode.Bilinear;
		bMat.SetFloat(ShaderParams.BlurScale, blurScale);
		Graphics.Blit(rt, temporary, bMat, 4);
		Graphics.Blit(temporary, rt, bMat, 5);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void BlurThisOutline(RenderTexture rt, float blurScale = 1f, int downscale = 1)
	{
		RenderTextureDescriptor descriptor = rt.descriptor;
		descriptor.colorFormat = rtOutlineColorFormat;
		descriptor.width /= downscale;
		descriptor.height /= downscale;
		RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
		temporary.filterMode = FilterMode.Bilinear;
		float num = rt.width / descriptor.width;
		bMat.SetFloat(ShaderParams.BlurScale, blurScale * num);
		Graphics.Blit(rt, temporary, bMat, 26);
		bMat.SetFloat(ShaderParams.BlurScale, blurScale);
		Graphics.Blit(temporary, rt, bMat, 27);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void BlurThisDownscaling(RenderTexture rt, RenderTexture downscaled, float blurScale = 1f)
	{
		RenderTextureDescriptor descriptor = rt.descriptor;
		descriptor.width = downscaled.width;
		descriptor.height = downscaled.height;
		RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
		temporary.filterMode = FilterMode.Bilinear;
		float num = rt.width / descriptor.width;
		bMat.SetFloat(ShaderParams.BlurScale, blurScale * num);
		Graphics.Blit(rt, temporary, bMat, 4);
		bMat.SetFloat(ShaderParams.BlurScale, blurScale);
		Graphics.Blit(temporary, downscaled, bMat, 5);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private RenderTexture BlurThisOneDirection(RenderTexture rt, bool vertical, float blurScale = 1f)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(rt.descriptor);
		temporary.filterMode = FilterMode.Bilinear;
		bMat.SetFloat(ShaderParams.BlurScale, blurScale);
		Graphics.Blit(rt, temporary, bMat, vertical ? 5 : 4);
		RenderTexture.ReleaseTemporary(rt);
		return temporary;
	}

	private void BlurThisDoF(RenderTexture rt, int renderPass)
	{
		RenderTextureDescriptor descriptor = rt.descriptor;
		RenderTexture temporary = RenderTexture.GetTemporary(descriptor);
		RenderTexture temporary2 = RenderTexture.GetTemporary(descriptor);
		temporary.filterMode = _depthOfFieldFilterMode;
		temporary2.filterMode = _depthOfFieldFilterMode;
		UpdateDepthOfFieldBlurData(new Vector2(0.44721f, -0.89443f));
		Graphics.Blit(rt, temporary, bMat, renderPass);
		UpdateDepthOfFieldBlurData(new Vector2(-1f, 0f));
		Graphics.Blit(temporary, temporary2, bMat, renderPass);
		UpdateDepthOfFieldBlurData(new Vector2(0.44721f, 0.89443f));
		Graphics.Blit(temporary2, rt, bMat, renderPass);
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void BlurThisAlpha(RenderTexture rt, float blurScale = 1f)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(rt.descriptor);
		temporary.filterMode = FilterMode.Bilinear;
		bMat.SetFloat(ShaderParams.BlurScale, blurScale);
		Graphics.Blit(rt, temporary, bMat, 23);
		Graphics.Blit(temporary, rt, bMat, 24);
		RenderTexture.ReleaseTemporary(temporary);
	}

	public void OnDidApplyAnimationProperties()
	{
		shouldUpdateMaterialProperties = true;
	}

	public void UpdateQualitySettings()
	{
		switch (_quality)
		{
		case BEAUTIFY_QUALITY.BestPerformance:
			_depthOfFieldDownsampling = 2;
			_depthOfFieldMaxSamples = 4;
			_sunFlaresDownsampling = 2;
			break;
		case BEAUTIFY_QUALITY.BestQuality:
			_depthOfFieldDownsampling = 1;
			_depthOfFieldMaxSamples = 8;
			_sunFlaresDownsampling = 1;
			break;
		}
		isDirty = true;
	}

	public void UpdateMaterialProperties()
	{
		if (Application.isPlaying)
		{
			shouldUpdateMaterialProperties = true;
		}
		else
		{
			UpdateMaterialPropertiesNow();
		}
	}

	private void CheckColorSpace()
	{
		linearColorSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;
	}

	public void UpdateMaterialPropertiesNow()
	{
		shouldUpdateMaterialProperties = false;
		CheckColorSpace();
		if (currentCamera != null && currentCamera.depthTextureMode == DepthTextureMode.None && _quality != BEAUTIFY_QUALITY.Basic)
		{
			currentCamera.depthTextureMode = DepthTextureMode.Depth;
		}
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		if (graphicsDeviceName != null && graphicsDeviceName.ToUpper().Contains("MALI-T720"))
		{
			rtFormat = RenderTextureFormat.Default;
			_bloomBlur = false;
			_anamorphicFlaresBlur = false;
		}
		else
		{
			rtFormat = (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
		}
		switch (_quality)
		{
		case BEAUTIFY_QUALITY.BestQuality:
			if (bMatDesktop == null)
			{
				bMatDesktop = new Material(Shader.Find("Hidden/Kronnect/Beautify/Beautify"));
				bMatDesktop.hideFlags = HideFlags.DontSave;
			}
			bMat = bMatDesktop;
			break;
		case BEAUTIFY_QUALITY.BestPerformance:
			if (bMatMobile == null)
			{
				bMatMobile = new Material(Shader.Find("Hidden/Kronnect/Beautify/BeautifyMobile"));
				bMatMobile.hideFlags = HideFlags.DontSave;
			}
			bMat = bMatMobile;
			break;
		case BEAUTIFY_QUALITY.Basic:
			if (bMatBasic == null)
			{
				bMatBasic = new Material(Shader.Find("Hidden/Kronnect/Beautify/BeautifyBasic"));
				bMatBasic.hideFlags = HideFlags.DontSave;
			}
			bMat = bMatBasic;
			break;
		}
		switch (_preset)
		{
		case BEAUTIFY_PRESET.Soft:
			_sharpen = 2f;
			if (linearColorSpace)
			{
				_sharpen *= 2f;
			}
			_sharpenDepthThreshold = 0.035f;
			_sharpenRelaxation = 0.065f;
			_sharpenClamp = 0.4f;
			_saturate = 0.5f;
			_contrast = 1.005f;
			_brightness = 1.05f;
			_dither = 0.02f;
			_ditherDepth = 0f;
			_daltonize = 0f;
			break;
		case BEAUTIFY_PRESET.Medium:
			_sharpen = 3f;
			if (linearColorSpace)
			{
				_sharpen *= 2f;
			}
			_sharpenDepthThreshold = 0.035f;
			_sharpenRelaxation = 0.07f;
			_sharpenClamp = 0.45f;
			_saturate = 1f;
			_contrast = 1.02f;
			_brightness = 1.05f;
			_dither = 0.02f;
			_ditherDepth = 0f;
			_daltonize = 0f;
			break;
		case BEAUTIFY_PRESET.Strong:
			_sharpen = 4.75f;
			if (linearColorSpace)
			{
				_sharpen *= 2f;
			}
			_sharpenDepthThreshold = 0.035f;
			_sharpenRelaxation = 0.075f;
			_sharpenClamp = 0.5f;
			_saturate = 1.5f;
			_contrast = 1.03f;
			_brightness = 1.05f;
			_dither = 0.022f;
			_ditherDepth = 0f;
			_daltonize = 0f;
			break;
		case BEAUTIFY_PRESET.Exaggerated:
			_sharpen = 6f;
			if (linearColorSpace)
			{
				_sharpen *= 2f;
			}
			_sharpenDepthThreshold = 0.035f;
			_sharpenRelaxation = 0.08f;
			_sharpenClamp = 0.55f;
			_saturate = 2.25f;
			_contrast = 1.035f;
			_brightness = 1.05f;
			_dither = 0.025f;
			_ditherDepth = 0f;
			_daltonize = 0f;
			break;
		}
		isDirty = true;
		if (bMat == null)
		{
			return;
		}
		renderPass = 1;
		if (_pixelateAmount > 1)
		{
			if (QualitySettings.antiAliasing > 1)
			{
				QualitySettings.antiAliasing = 1;
			}
			if (_pixelateDownscale)
			{
				_dither = 0f;
			}
		}
		if (shaderKeywords == null)
		{
			shaderKeywords = new List<string>();
		}
		else
		{
			shaderKeywords.Clear();
		}
		UpdateSharpenParams(_sharpen);
		bool flag = currentCamera != null && currentCamera.orthographic;
		bMat.SetVector(ShaderParams.Dither, new Vector4(_dither, flag ? 0f : _ditherDepth, (_sharpenMaxDepth + _sharpenMinDepth) * 0.5f, Mathf.Abs(_sharpenMaxDepth - _sharpenMinDepth) * 0.5f + (flag ? 1000f : 0f)));
		bMat.SetVector(ShaderParams.AntialiasData, new Vector4(_antialiasStrength, _antialiasDepthThreshold, _antialiasDepthAtten * 10f, _antialiasMaxSpread));
		float y = (linearColorSpace ? (1f + (_contrast - 1f) / 2.2f) : _contrast);
		bMat.SetVector(ShaderParams.ColorBoost, new Vector4(_brightness, y, _saturate, _daltonize * 10f));
		bMat.SetVector(ShaderParams.HardLight, new Vector3(_hardLightBlend, _hardLightIntensity));
		bMat.SetVector(ShaderParams.ColorBoost2, new Vector4(_tonemapExposurePre, _tonemapBrightnessPost, 0f, 0f));
		Color value = _vignettingColor;
		value.a *= (_vignetting ? 32f : 0f);
		float num = 1f - _vignettingBlink * 2f;
		if (num < 0f)
		{
			num = 0f;
		}
		value.r *= num;
		value.g *= num;
		value.b *= num;
		bMat.SetColor(ShaderParams.Vignette, value);
		if (currentCamera != null)
		{
			bMat.SetFloat(ShaderParams.VignetteAspectRatio, (_vignettingCircularShape && _vignettingBlink <= 0f) ? (1f / currentCamera.aspect) : (_vignettingAspectRatio + 1.001f / (1.001f - _vignettingBlink) - 1f));
		}
		float num2 = _vignettingCenter.y;
		if (_vignettingBlinkStyle == BEAUTIFY_BLINK_STYLE.Human)
		{
			num2 -= _vignettingBlink * 0.5f;
		}
		bMat.SetVector(ShaderParams.FXData, new Vector4(_vignettingCenter.x, num2, _sharpenMinMaxDepthFallOff));
		if (_frame)
		{
			if (_frameMask != null)
			{
				bMat.SetTexture(ShaderParams.FrameMaskTexture, _frameMask);
				shaderKeywords.Add("BEAUTIFY_FRAME_MASK");
			}
			else
			{
				shaderKeywords.Add("BEAUTIFY_FRAME");
			}
			if (_frameStyle == FrameStyle.Border)
			{
				Vector4 value2 = new Vector4(_frameColor.r, _frameColor.g, _frameColor.b, _frameColor.a);
				bMat.SetVector(ShaderParams.Frame, value2);
				float num3 = ((_frameMask != null) ? _frameColor.a : ((1.00001f - _frameColor.a) * 0.5f));
				bMat.SetVector(ShaderParams.FrameData, new Vector4(num3, 50f, num3, 50f));
			}
			else
			{
				bMat.SetVector(ShaderParams.Frame, Color.black);
				bMat.SetVector(ShaderParams.FrameData, new Vector4(0.5f - _frameBandHorizontalSize, 1f / (0.0001f + _frameBandHorizontalSmoothness), 0.5f - _frameBandVerticalSize, 1f / (0.0001f + _frameBandVerticalSmoothness)));
			}
		}
		bMat.SetFloat(ShaderParams.OutlineIntensityMultiplier, _outlineIntensityMultiplier);
		bMat.SetFloat(ShaderParams.OutlineMinDepthThreshold, _outlineMinDepthThreshold);
		bMat.SetColor(ShaderParams.OutlineColor, _outlineColor);
		float num4 = 1E-05f + _bloomWeight0 + _bloomWeight1 + _bloomWeight2 + _bloomWeight3 + _bloomWeight4 + _bloomWeight5;
		bMat.SetVector(ShaderParams.BloomWeights2, new Vector4(_bloomWeight4 / num4 + _bloomBoost4, _bloomWeight5 / num4 + _bloomBoost5, _bloomMaxBrightness, num4));
		if (_bloomCustomize)
		{
			bMat.SetVector(ShaderParams.BloomWeights, new Vector4(_bloomWeight0 / num4 + _bloomBoost0, _bloomWeight1 / num4 + _bloomBoost1, _bloomWeight2 / num4 + _bloomBoost2, _bloomWeight3 / num4 + _bloomBoost3));
			bMat.SetColor(ShaderParams.BloomTint0, _bloomTint0);
			bMat.SetColor(ShaderParams.BloomTint1, _bloomTint1);
			bMat.SetColor(ShaderParams.BloomTint2, _bloomTint2);
			bMat.SetColor(ShaderParams.BloomTint3, _bloomTint3);
			bMat.SetColor(ShaderParams.BloomTint4, _bloomTint4);
			bMat.SetColor(ShaderParams.BloomTint5, _bloomTint5);
		}
		if (_bloomDebug && (_bloom || _anamorphicFlares || _sunFlares))
		{
			renderPass = 3;
		}
		bloomCurrentLayerMaskValue = _bloomCullingMask;
		anamorphicFlaresCurrentLayerMaskValue = _anamorphicFlaresCullingMask;
		if (_sunFlares)
		{
			_sunFlaresRevealSpeed = Mathf.Max(0.001f, _sunFlaresRevealSpeed);
			_sunFlaresHideSpeed = Mathf.Max(0.001f, _sunFlaresHideSpeed);
			bMat.SetVector(ShaderParams.SFSunData, new Vector4(_sunFlaresSunIntensity, _sunFlaresSunDiskSize, _sunFlaresSunRayDiffractionIntensity, _sunFlaresSunRayDiffractionThreshold));
			bMat.SetVector(ShaderParams.SFCoronaRays1, new Vector4(_sunFlaresCoronaRays1Length, Mathf.Max((float)_sunFlaresCoronaRays1Streaks / 2f, 1f), Mathf.Max(_sunFlaresCoronaRays1Spread, 0.0001f), _sunFlaresCoronaRays1AngleOffset));
			bMat.SetVector(ShaderParams.SFCoronaRays2, new Vector4(_sunFlaresCoronaRays2Length, Mathf.Max((float)_sunFlaresCoronaRays2Streaks / 2f, 1f), Mathf.Max(_sunFlaresCoronaRays2Spread + 0.0001f), _sunFlaresCoronaRays2AngleOffset));
			bMat.SetVector(ShaderParams.SFGhosts1, new Vector4(0f, _sunFlaresGhosts1Size, _sunFlaresGhosts1Offset, _sunFlaresGhosts1Brightness));
			bMat.SetVector(ShaderParams.SFGhosts2, new Vector4(0f, _sunFlaresGhosts2Size, _sunFlaresGhosts2Offset, _sunFlaresGhosts2Brightness));
			bMat.SetVector(ShaderParams.SFGhosts3, new Vector4(0f, _sunFlaresGhosts3Size, _sunFlaresGhosts3Offset, _sunFlaresGhosts3Brightness));
			bMat.SetVector(ShaderParams.SFGhosts4, new Vector4(0f, _sunFlaresGhosts4Size, _sunFlaresGhosts4Offset, _sunFlaresGhosts4Brightness));
			bMat.SetVector(ShaderParams.SFHalo, new Vector4(_sunFlaresHaloOffset, _sunFlaresHaloAmplitude, _sunFlaresHaloIntensity * 100f, _sunFlaresRadialOffset));
		}
		if (_lensDirtTexture == null)
		{
			_lensDirtTexture = Resources.Load<Texture2D>("Textures/dirt2");
		}
		bMat.SetTexture(ShaderParams.OverlayTexture, _lensDirtTexture);
		bMat.SetColor(ShaderParams.AFTint, _anamorphicFlaresTint);
		if (_depthOfField)
		{
			if ((int)_depthOfFieldAutofocusLayerMask != 0)
			{
				Shader.SetGlobalFloat(ShaderParams.DoFDepthBias, _depthOfFieldExclusionBias);
			}
			if ((int)_depthOfFieldExclusionLayerMask != 0)
			{
				Shader.SetGlobalInt(ShaderParams.DoFExclusionCullMode, (int)_depthOfFieldExclusionCullMode);
			}
			if (_depthOfFieldTransparencySupport)
			{
				Shader.SetGlobalInt(ShaderParams.DoFTransparencyCullMode, (int)_depthOfFieldTransparencyCullMode);
			}
		}
		dofCurrentLayerMaskValue = _depthOfFieldExclusionLayerMask.value;
		if (_compareMode)
		{
			renderPass = 0;
			float f;
			float z;
			switch (_compareStyle)
			{
			case BEAUTIFY_COMPARE_STYLE.FreeAngle:
				f = _compareLineAngle;
				z = -10f;
				break;
			case BEAUTIFY_COMPARE_STYLE.SameSide:
				f = MathF.PI / 2f;
				z = _comparePanning;
				break;
			default:
				f = MathF.PI / 2f;
				z = -20f + _comparePanning * 2f;
				break;
			}
			bMat.SetVector(ShaderParams.CompareData, new Vector4(Mathf.Cos(f), Mathf.Sin(f), z, _compareLineWidth));
		}
		if (_quality != BEAUTIFY_QUALITY.Basic)
		{
			if (_lut && (_lutTexture != null || _lutTexture3D != null))
			{
				if (_lutTexture != null)
				{
					shaderKeywords.Add("BEAUTIFY_LUT");
					bMat.SetTexture(ShaderParams.LUT, _lutTexture);
				}
				else
				{
					shaderKeywords.Add("BEAUTIFY_LUT3D");
					bMat.SetTexture(ShaderParams.LUT3D, _lutTexture3D);
					float num5 = 1f / (float)_lutTexture3D.width;
					float num6 = (float)_lutTexture3D.width - 1f;
					bMat.SetVector(ShaderParams.LUT3DParams, new Vector4(num5 * 0.5f, num5 * num6, 0f, 0f));
				}
				bMat.SetColor(ShaderParams.FXColor, new Color(0f, 0f, 0f, _lutIntensity));
			}
			else if (_nightVision)
			{
				shaderKeywords.Add("BEAUTIFY_NIGHT_VISION");
				Color value3 = _nightVisionColor;
				if (linearColorSpace)
				{
					value3.a *= 5f * value3.a;
				}
				else
				{
					value3.a *= 3f * value3.a;
				}
				value3.r *= value3.a;
				value3.g *= value3.a;
				value3.b *= value3.a;
				bMat.SetColor(ShaderParams.FXColor, value3);
			}
			else if (_thermalVision)
			{
				shaderKeywords.Add("BEAUTIFY_THERMAL_VISION");
			}
			else if (_daltonize > 0f)
			{
				shaderKeywords.Add("BEAUTIFY_DALTONIZE");
			}
			else
			{
				bMat.SetColor(ShaderParams.FXColor, new Color(0f, 0f, 0f, _lutIntensity));
			}
			bMat.SetColor(ShaderParams.TintColor, _tintColor);
			if (_sunFlares)
			{
				if (flareNoise == null)
				{
					flareNoise = Resources.Load<Texture2D>("Textures/flareNoise");
				}
				flareNoise.wrapMode = TextureWrapMode.Repeat;
				bMat.SetTexture(ShaderParams.FlareTexture, flareNoise);
				if (_sun == null)
				{
					Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
					foreach (Light light in array)
					{
						if (light.type == LightType.Directional && light.enabled && light.gameObject.activeSelf)
						{
							_sun = light.transform;
							break;
						}
					}
				}
				if (_sun != null)
				{
					Light componentInChildren = _sun.GetComponentInChildren<Light>();
					sunIsSpotlight = componentInChildren.type == LightType.Spot;
				}
			}
			if (_vignetting)
			{
				if (_vignettingMask != null)
				{
					bMat.SetTexture(ShaderParams.VignetteMaskTexture, _vignettingMask);
					shaderKeywords.Add("BEAUTIFY_VIGNETTING_MASK");
				}
				else
				{
					shaderKeywords.Add("BEAUTIFY_VIGNETTING");
				}
			}
			if (_outline && !_outlineCustomize)
			{
				shaderKeywords.Add("BEAUTIFY_OUTLINE");
			}
			if (_lensDirt)
			{
				shaderKeywords.Add("BEAUTIFY_DIRT");
			}
			if (_bloom || _anamorphicFlares || _sunFlares)
			{
				shaderKeywords.Add("BEAUTIFY_BLOOM");
				if (_bloomDepthAtten > 0f || _bloomNearAtten > 0f)
				{
					bMat.SetFloat(ShaderParams.BloomDepthThreshold, _bloomDepthAtten);
					bMat.SetFloat(ShaderParams.BloomDepthNearThreshold, _bloomNearAtten);
					shaderKeywords.Add("BEAUTIFY_BLOOM_USE_DEPTH");
				}
				if ((_bloom && isUsingBloomLayerMask) || (_anamorphicFlares && isUsingAnamorphicFlaresLayerMask))
				{
					bMat.SetFloat(ShaderParams.BloomZDepthBias, _bloomLayerZBias);
				}
				if (_bloom && _bloomConservativeThreshold)
				{
					shaderKeywords.Add("BEAUTIFY_BLOOM_PROP_THRESHOLDING");
				}
			}
			if (_depthOfField)
			{
				if (_depthOfFieldTransparencySupport || isUsingDepthOfFieldExclusionLayerMask)
				{
					shaderKeywords.Add("BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT");
				}
				else
				{
					shaderKeywords.Add("BEAUTIFY_DEPTH_OF_FIELD");
				}
			}
			if (_eyeAdaptation)
			{
				Vector4 value4 = new Vector4(_eyeAdaptationMinExposure, _eyeAdaptationMaxExposure, _eyeAdaptationSpeedToDark, _eyeAdaptationSpeedToLight);
				bMat.SetVector(ShaderParams.EyeAdaptation, value4);
				shaderKeywords.Add("BEAUTIFY_EYE_ADAPTATION");
			}
			if (_tonemap == BEAUTIFY_TMO.ACES)
			{
				shaderKeywords.Add("BEAUTIFY_TONEMAP_ACES");
			}
			else if (_tonemap == BEAUTIFY_TMO.AGX)
			{
				bMat.SetFloat(ShaderParams.TonemapGamma, linearColorSpace ? _tonemapGamma : (_tonemapGamma * 0.5f));
				shaderKeywords.Add("BEAUTIFY_TONEMAP_AGX");
			}
			if (_purkinje || _vignetting)
			{
				float z2 = _vignettingFade + _vignettingBlink * 0.5f;
				if (_vignettingBlink > 0.99f)
				{
					z2 = 1f;
				}
				Vector3 vector = new Vector3(_purkinjeAmount, _purkinjeLuminanceThreshold, z2);
				bMat.SetVector(ShaderParams.Purkinje, vector);
				shaderKeywords.Add("BEAUTIFY_PURKINJE");
			}
			if (_chromaticAberration)
			{
				bMat.SetVector(ShaderParams.ChromaticAberration, new Vector4(_chromaticAberrationIntensity, _chromaticAberrationSmoothing, 0f, 0f));
				if (!_depthOfField)
				{
					shaderKeywords.Add("BEAUTIFY_CHROMATIC_ABERRATION");
				}
			}
		}
		bMat.enabledKeywords = null;
		bMat.shaderKeywords = shaderKeywords.ToArray();
	}

	private void UpdateMaterialBloomIntensityAndThreshold()
	{
		float num = _bloomThreshold;
		if (linearColorSpace)
		{
			num *= num;
		}
		bMat.SetVector(ShaderParams.Bloom, new Vector4(_bloomIntensity + (_anamorphicFlares ? 0.0001f : 0f), _bloomAntiflickerMaxOutput, 0f, num));
		if (isUsingBloomLayerMask)
		{
			bMat.EnableKeyword("BEAUTIFY_BLOOM_USE_LAYER");
		}
		else
		{
			bMat.DisableKeyword("BEAUTIFY_BLOOM_USE_LAYER");
		}
	}

	private void UpdateMaterialAnamorphicIntensityAndThreshold()
	{
		float num = _anamorphicFlaresThreshold;
		if (linearColorSpace)
		{
			num *= num;
		}
		float x = _anamorphicFlaresIntensity / (_bloomIntensity + 0.0001f);
		bMat.SetVector(ShaderParams.Bloom, new Vector4(x, _anamorphicFlaresAntiflickerMaxOutput, 0f, num));
		if (_anamorphicFlares && isUsingAnamorphicFlaresLayerMask)
		{
			bMat.EnableKeyword("BEAUTIFY_BLOOM_USE_LAYER");
		}
		else
		{
			bMat.DisableKeyword("BEAUTIFY_BLOOM_USE_LAYER");
		}
	}

	private void UpdateSharpenParams(float sharpen)
	{
		bMat.SetVector(ShaderParams.Sharpen, new Vector4(sharpen, _sharpenDepthThreshold, _sharpenClamp, _sharpenRelaxation));
	}

	private void UpdateDepthOfFieldData()
	{
		float num;
		if (!_depthOfFieldAutofocus)
		{
			num = ((!(_depthOfFieldTargetFocus != null)) ? _depthOfFieldDistance : ((!(currentCamera.WorldToScreenPoint(_depthOfFieldTargetFocus.position).z < 0f)) ? Vector3.Distance(currentCamera.transform.position, _depthOfFieldTargetFocus.position) : currentCamera.farClipPlane));
		}
		else
		{
			UpdateDoFAutofocusDistance();
			num = ((dofLastAutofocusDistance > 0f) ? dofLastAutofocusDistance : currentCamera.farClipPlane);
		}
		if (OnBeforeFocus != null)
		{
			num = OnBeforeFocus(num);
		}
		if (dofPrevDistance < 0f)
		{
			dofPrevDistance = num;
		}
		else
		{
			dofPrevDistance = Mathf.Lerp(dofPrevDistance, num, Application.isPlaying ? (_depthOfFieldFocusSpeed * Time.unscaledDeltaTime * 30f) : 1f);
		}
		float y;
		if (depthOfFieldCameraSettings == BEAUTIFY_DOF_CAMERA_SETTINGS.Real)
		{
			float num2 = depthOfFieldFocalLengthReal;
			y = num2 / depthOfFieldFStop * (num2 / Mathf.Max(dofPrevDistance * 1000f - num2, 0.001f)) * (1f / depthOfFieldImageSensorHeight) * (float)currentCamera.pixelHeight;
		}
		else
		{
			y = _depthOfFieldAperture * (_depthOfFieldFocalLength / Mathf.Max(dofPrevDistance - _depthOfFieldFocalLength, 0.001f)) * 41.666668f;
		}
		dofLastBokehData = new Vector4(dofPrevDistance, y, 0f, 0f);
		bMat.SetVector(ShaderParams.BokehData, dofLastBokehData);
		float num3 = _depthOfFieldBokehThreshold;
		if (!linearColorSpace)
		{
			num3 = Mathf.LinearToGammaSpace(num3);
		}
		bMat.SetVector(ShaderParams.BokehData2, new Vector4(_depthOfFieldForegroundBlur ? _depthOfFieldForegroundDistance : currentCamera.farClipPlane, _depthOfFieldMaxSamples, num3, _depthOfFieldBokehIntensity * _depthOfFieldBokehIntensity));
		bMat.SetVector(ShaderParams.BokehData3, new Vector3(_depthOfFieldMaxBrightness, _depthOfFieldMaxDistance * (currentCamera.farClipPlane + 1f), 0f));
	}

	private void UpdateDepthOfFieldBlurData(Vector2 blurDir)
	{
		float num = 1f / (float)_depthOfFieldDownsampling;
		blurDir *= num;
		dofLastBokehData.z = blurDir.x;
		dofLastBokehData.w = blurDir.y;
		bMat.SetVector(ShaderParams.BokehData, dofLastBokehData);
	}

	private void UpdateDoFAutofocusDistance()
	{
		Vector3 pos = _depthofFieldAutofocusViewportPoint;
		pos.z = 10f;
		if (Physics.Raycast(currentCamera.ViewportPointToRay(pos), out var hitInfo, currentCamera.farClipPlane, _depthOfFieldAutofocusLayerMask))
		{
			float num = Vector3.Distance(currentCamera.transform.position, hitInfo.point);
			num += _depthOfFieldAutofocusDistanceShift;
			dofLastAutofocusDistance = Mathf.Clamp(num, _depthOfFieldAutofocusMinDistance, _depthOfFieldAutofocusMaxDistance);
		}
		else
		{
			dofLastAutofocusDistance = currentCamera.farClipPlane;
		}
	}

	public void Blink(float duration, float maxValue = 1f)
	{
		if (!(duration <= 0f))
		{
			StartCoroutine(DoBlink(duration, maxValue));
		}
	}

	private IEnumerator DoBlink(float duration, float maxValue)
	{
		float start = Time.time;
		WaitForEndOfFrame w = new WaitForEndOfFrame();
		float t;
		do
		{
			t = (Time.time - start) / duration;
			if (t > 1f)
			{
				t = 1f;
			}
			float num = t * (2f - t);
			vignettingBlink = num * maxValue;
			yield return w;
		}
		while (t < 1f);
		start = Time.time;
		do
		{
			t = (Time.time - start) / duration;
			if (t > 1f)
			{
				t = 1f;
			}
			float num2 = t * t;
			vignettingBlink = (1f - num2) * maxValue;
			yield return w;
		}
		while (t < 1f);
	}
}
