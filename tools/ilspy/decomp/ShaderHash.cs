using UnityEngine;

public static class ShaderHash
{
	public static int _Sensitivity = Shader.PropertyToID("_Sensitivity");

	public static int _BgFade = Shader.PropertyToID("_BgFade");

	public static int _SampleDistance = Shader.PropertyToID("_SampleDistance");

	public static int _BgColor = Shader.PropertyToID("_BgColor");

	public static int _Exponent = Shader.PropertyToID("_Exponent");

	public static int _Threshold = Shader.PropertyToID("_Threshold");

	public static int _EdgeTex = Shader.PropertyToID("_EdgeTex");

	public static int _GridSize = Shader.PropertyToID("_GridSize");

	public static int _Spread = Shader.PropertyToID("_Spread");

	public static int _EdgeWidth = Shader.PropertyToID("_EdgeWidth");

	public static int _EdgeIntensity = Shader.PropertyToID("_EdgeIntensity");

	public static int _NormalThreshold = Shader.PropertyToID("_NormalThreshold");

	public static int _DepthThreshold = Shader.PropertyToID("_DepthThreshold");

	public static int _NormalSensitivity = Shader.PropertyToID("_NormalSensitivity");

	public static int _DepthSensitivity = Shader.PropertyToID("_DepthSensitivity");

	public static int _OutlineSource = Shader.PropertyToID("_OutlineSource");

	public static int _LineColor = Shader.PropertyToID("_LineColor");

	public static int _LineThicknessX = Shader.PropertyToID("_LineThicknessX");

	public static int _LineThicknessY = Shader.PropertyToID("_LineThicknessY");

	public static int _SnowAmount = Shader.PropertyToID("_SnowAmount");

	public static int _SnowTiling = Shader.PropertyToID("_SnowTiling");

	public static int _SnowNoiseTiling = Shader.PropertyToID("_SnowNoiseTiling");

	public static int _SnowTex = Shader.PropertyToID("_SnowTex");

	public static int _SnowNormals = Shader.PropertyToID("_SnowNormals");

	public static int _SnowNoiseCover = Shader.PropertyToID("_SnowNoiseCover");

	public static int _SnowFade = Shader.PropertyToID("_SnowFade");

	public static int _Season = Shader.PropertyToID("_Season");

	public static int _UnderConstruction = Shader.PropertyToID("_UnderConstruction");

	public static int _MossTex = Shader.PropertyToID("_MossTex");

	public static int _MossNormals = Shader.PropertyToID("_MossNormals");

	public static int _MossRoughness = Shader.PropertyToID("_MossRoughness");

	public static int _MossAO = Shader.PropertyToID("_MossAO");

	public static int _MossAmount = Shader.PropertyToID("_MossAmount");

	public static int _MossOnSidesAmount = Shader.PropertyToID("_MossOnSidesAmount");

	public static int _Opacity = Shader.PropertyToID("_Opacity");

	public static int _GrassDryness = Shader.PropertyToID("_GrassDryness");

	public static int _GrassDensityFactor = Shader.PropertyToID("_GrassDensityFactor");

	public static int _PlayerPosSpeedTerrainSize = Shader.PropertyToID("_PlayerPosSpeedTerrainSize");

	public static int _WindDirStrength = Shader.PropertyToID("_WindDirStrength");

	public static int _GrassType = Shader.PropertyToID("_GrassType");

	public static int _GrassRangeMinMax = Shader.PropertyToID("_GrassRangeMinMax");

	public static int _GrassMap = Shader.PropertyToID("_GrassMap");

	public static int _WorldPosition = Shader.PropertyToID("_WorldPosition");

	public static int _CloudPosXZDensityTime = Shader.PropertyToID("_CloudPosXZDensityTime");

	public static int _SunColor = Shader.PropertyToID("_SunColor");

	public static int _MoonColor = Shader.PropertyToID("_MoonColor");

	public static int _FrustumFarCornersWS = Shader.PropertyToID("_FrustumFarCornersWS");

	public static int _CameraWS = Shader.PropertyToID("_CameraWS");

	public static int _FogOfWarTex = Shader.PropertyToID("_FogOfWarTex");

	public static int _HeightMapTex = Shader.PropertyToID("_HeightMapTex");

	public static int _TerrainHalfSize_HeightRange = Shader.PropertyToID("_TerrainHalfSize_HeightRange");

	public static int _FogColTop = Shader.PropertyToID("_FogColTop");

	public static int _FogColBottom = Shader.PropertyToID("_FogColBottom");

	public static int _FogStart = Shader.PropertyToID("_FogStart");

	public static int _FogEnd = Shader.PropertyToID("_FogEnd");

	public static int _FarFogStart = Shader.PropertyToID("_FarFogStart");

	public static int _FarFogEnd = Shader.PropertyToID("_FarFogEnd");

	public static int _HeightFogStart = Shader.PropertyToID("_HeightFogStart");

	public static int _HeightFogEnd = Shader.PropertyToID("_HeightFogEnd");

	public static int _SkyHeightFogStart = Shader.PropertyToID("_SkyHeightFogStart");

	public static int _SkyHeightFogEnd = Shader.PropertyToID("_SkyHeightFogEnd");

	public static int _FogAmount = Shader.PropertyToID("_FogAmount");

	public static int _SunDir = Shader.PropertyToID("_SunDir");

	public static int _PlayerPos = Shader.PropertyToID("_PlayerPos");

	public static int _WaveTime = Shader.PropertyToID("_WaveTime");

	public static int _IceCover = Shader.PropertyToID("_IceCover");

	public static int _IceCoverSoftness = Shader.PropertyToID("_IceCoverSoftness");

	public static int _RoadMiddleLine = Shader.PropertyToID("_RoadMiddleLine");

	public static int _RoadEdgeLine = Shader.PropertyToID("_RoadEdgeLine");

	public static int _DotColor = Shader.PropertyToID("_DotColor");

	public static int _FrecklesColor = Shader.PropertyToID("_FrecklesColor");

	public static int _WrinklesAmount = Shader.PropertyToID("_WrinklesAmount");

	public static int _ZombieAmount = Shader.PropertyToID("_ZombieAmount");

	public static int _HairColor = Shader.PropertyToID("_HairColor");

	public static int _HairTex = Shader.PropertyToID("_HairTex");

	public static int _HairBumpMap = Shader.PropertyToID("_HairBumpMap");

	public static int _HairMetallicGlossMap = Shader.PropertyToID("_HairMetallicGlossMap");

	public static int _BeardTex = Shader.PropertyToID("_BeardTex");

	public static int _BeardBumpMap = Shader.PropertyToID("_BeardBumpMap");

	public static int _BeardMetallicGlossMap = Shader.PropertyToID("_BeardMetallicGlossMap");

	public static int _BeardTex_ST = Shader.PropertyToID("_BeardTex_ST");

	public static int _BeardBumpMap_ST = Shader.PropertyToID("_BeardBumpMap_ST");

	public static int _BeardMetallicGlossMap_ST = Shader.PropertyToID("_BeardMetallicGlossMap_ST");

	public static int _Color2 = Shader.PropertyToID("_Color2");

	public static int _Color3 = Shader.PropertyToID("_Color3");

	public static int _UnderwearColor = Shader.PropertyToID("_UnderwearColor");

	public static int _NumDecals = Shader.PropertyToID("_NumDecals");

	public static int _DecalInvTransform = Shader.PropertyToID("_DecalInvTransform");

	public static int _DecalTex = Shader.PropertyToID("_DecalTex");

	public static int _DecalNormals = Shader.PropertyToID("_DecalNormals");

	public static int _DecalTexIndex = Shader.PropertyToID("_DecalTexIndex");

	public static int _GrungeOffset = Shader.PropertyToID("_GrungeOffset");

	public static int _OverlayTex = Shader.PropertyToID("_OverlayTex");

	public static int _OverlayNormal = Shader.PropertyToID("_OverlayNormal");

	public static int _OverlayOffsetAndAmount = Shader.PropertyToID("_OverlayOffsetAndAmount");

	public static int _Radius = Shader.PropertyToID("_Radius");

	public static int _FillAmount = Shader.PropertyToID("_FillAmount");

	public static int _FillAlpha = Shader.PropertyToID("_FillAlpha");

	public static int _Bendiness = Shader.PropertyToID("_Bendiness");

	public static int _CookedAmount = Shader.PropertyToID("_CookedAmount");

	public static int _NumConditions = Shader.PropertyToID("_NumConditions");

	public static int _ConditionType = Shader.PropertyToID("_ConditionType");

	public static int _ConditionData = Shader.PropertyToID("_ConditionData");

	public static int _ApprovalRespect = Shader.PropertyToID("_ApprovalRespect");

	public static int _OutlineColor = Shader.PropertyToID("_OutlineColor");

	public static int _BlendTex = Shader.PropertyToID("_BlendTex");

	public static int _BumpMap = Shader.PropertyToID("_BumpMap");

	public static int _BlendAmount = Shader.PropertyToID("_BlendAmount");

	public static int _EdgeSharpness = Shader.PropertyToID("_EdgeSharpness");

	public static int _SeeThroughness = Shader.PropertyToID("_SeeThroughness");

	public static int _Distortion = Shader.PropertyToID("_Distortion");

	public static int _ProjectionParams = Shader.PropertyToID("_ProjectionParams");

	public static int _ScreenParams = Shader.PropertyToID("_ScreenParams");

	public static int _ZBufferParams = Shader.PropertyToID("_ZBufferParams");

	public static int _WorldSpaceLightPos0 = Shader.PropertyToID("_WorldSpaceLightPos0");

	public static int _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");

	public static int _LightPositionRange = Shader.PropertyToID("_LightPositionRange");

	public static int _LightProjectionParams = Shader.PropertyToID("_LightProjectionParams");

	public static int _LightShadowData = Shader.PropertyToID("_LightShadowData");

	public static int _LightColor0 = Shader.PropertyToID("_LightColor0");

	public static int unity_WorldToLight = Shader.PropertyToID("unity_WorldToLight ");

	public static int unity_WorldToShadow = Shader.PropertyToID("unity_WorldToShadow");

	public static int unity_LightColor = Shader.PropertyToID("unity_LightColor");

	public static int unity_ShadowColor = Shader.PropertyToID("unity_ShadowColor");

	public static int unity_4LightAtten0 = Shader.PropertyToID("unity_4LightAtten0");

	public static int unity_4LightPosX0 = Shader.PropertyToID("unity_4LightPosX0");

	public static int unity_4LightPosY0 = Shader.PropertyToID("unity_4LightPosY0");

	public static int unity_4LightPosZ0 = Shader.PropertyToID("unity_4LightPosZ0");

	public static int unity_AmbientSky = Shader.PropertyToID("unity_AmbientSky");

	public static int unity_AmbientEquator = Shader.PropertyToID("unity_AmbientEquator");

	public static int unity_AmbientGround = Shader.PropertyToID("unity_AmbientGround");

	public static int unity_IndirectSpecColor = Shader.PropertyToID("unity_IndirectSpecColor");

	public static int unity_SHAr = Shader.PropertyToID("unity_SHAr");

	public static int unity_SHAg = Shader.PropertyToID("unity_SHAg");

	public static int unity_SHAb = Shader.PropertyToID("unity_SHAb");

	public static int unity_SHBr = Shader.PropertyToID("unity_SHBr");

	public static int unity_SHBg = Shader.PropertyToID("unity_SHBg");

	public static int unity_SHBb = Shader.PropertyToID("unity_SHBb");

	public static int unity_SHC = Shader.PropertyToID("unity_SHC");

	public static int unity_OcclusionMaskSelector = Shader.PropertyToID("unity_OcclusionMaskSelector");

	public static int unity_ProbesOcclusion = Shader.PropertyToID("unity_ProbesOcclusion");

	public static int unity_SpecCube0 = Shader.PropertyToID("unity_SpecCube0");

	public static int unity_SpecCube1 = Shader.PropertyToID("unity_SpecCube1");

	public static int unity_SpecCube0_ProbePosition = Shader.PropertyToID("unity_SpecCube0_ProbePosition");

	public static int unity_SpecCube1_ProbePosition = Shader.PropertyToID("unity_SpecCube1_ProbePosition");

	public static int unity_SpecCube0_BoxMax = Shader.PropertyToID("unity_SpecCube0_BoxMax");

	public static int unity_SpecCube1_BoxMax = Shader.PropertyToID("unity_SpecCube1_BoxMax");

	public static int unity_SpecCube0_BoxMin = Shader.PropertyToID("unity_SpecCube0_BoxMin");

	public static int unity_SpecCube1_BoxMin = Shader.PropertyToID("unity_SpecCube1_BoxMin");

	public static int unity_SpecCube0_HDR = Shader.PropertyToID("unity_SpecCube0_HDR");

	public static int unity_SpecCube1_HDR = Shader.PropertyToID("unity_SpecCube1_HDR");

	public static int UnityReflectionProbes = Shader.PropertyToID("UnityReflectionProbes");

	public static int _CameraDepthNormalsTexture = Shader.PropertyToID("_CameraDepthNormalsTexture");

	public static int _MainTex = Shader.PropertyToID("_MainTex");
}
