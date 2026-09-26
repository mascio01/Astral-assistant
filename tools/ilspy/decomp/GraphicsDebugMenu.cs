using System;
using UnityEngine;

public class GraphicsDebugMenu : DebugMenu
{
	public static bool ShowHUD = true;

	public static bool ShowFPS = false;

	public static bool ShowVehicleUI = false;

	public static bool PipCommandBuffers = true;

	public static bool PipSphericalHarmonics = true;

	public GraphicsDebugMenu()
		: base(GameImpl.Translate("DEBUG_GraphicsDebug"))
	{
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Sun"), typeof(SunFiddler)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Weather"), typeof(WeatherFiddler)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Grass"), typeof(GrassDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_FogOfWarViewer"), typeof(FogOfWarViewer)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Outline"), typeof(OutlineDebugMenu)));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowFPS"), GetShowFPS, SetShowFPS));
		if (!Session.Instance.IsInMultiplayerGame())
		{
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowFogOfWar"), FogOfWar.GetDebugFogOfWarEnabled, FogOfWar.SetDebugFogOfWarEnabled, affectsGameState: true));
		}
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("MENU_SuperSample"), () => GameImpl.SuperSample, delegate(bool v)
		{
			GameImpl.Instance.SetSuperSample(v);
		}));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("MENU_TAA"), () => GameImpl.TAA, delegate(bool v)
		{
			GameImpl.Instance.SetTAA(v);
		}));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowVehicleUI"), typeof(GraphicsDebugMenu), "ShowVehicleUI"));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowHUD"), () => ShowHUD, delegate(bool v)
		{
			ShowHUD = v;
		}));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowRocks"), Rock.GetDebugShowRocks, Rock.SetDebugShowRocks));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowFlowers"), Flower.GetDebugShowFlowers, Flower.SetDebugShowFlowers));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowBushes"), Bush.GetDebugShowBushes, Bush.SetDebugShowBushes));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowTrash"), Trash.GetDebugShowTrash, Trash.SetDebugShowTrash));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowCharacters"), Character.GetDebugShowCharacters, Character.SetDebugShowCharacters));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowTerrain"), GameTerrain.Instance.GetDebugShowTerrain, GameTerrain.Instance.SetDebugShowTerrain));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowTrees"), GameTerrain.Instance.GetDebugShowTrees, GameTerrain.Instance.SetDebugShowTrees));
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_AimAngle"), -90f, 90f, typeof(Human), "AimAngleOverride"));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_TreeLODBias"), GraphicsSettings.MinTreeQuality, GraphicsSettings.MaxTreeQuality, () => GameImpl.Instance.Settings.TreeQuality, SetTreeQuality));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_LODBias"), 0f, 2f, () => QualitySettings.lodBias, delegate(float v)
		{
			QualitySettings.lodBias = v;
		}));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_RebuildMinimap"), GameTerrain.Instance.BuildEntireMinimap));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_OverrideAimAngle"), typeof(Human), "WantAimAngleOverride"));
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_AimAngle"), -90f, 90f, typeof(Human), "AimAngleOverride"));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_MaxFixedUpdatesPerFrame"), () => Math.Max(1, QualitySettings.vSyncCount), 8, GetMaxFixedUpdatesPerFrame, SetMaxFixedUpdatesPerFrame));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_VSyncCount"), 0, 4, GetVSyncCount, SetVSyncCount));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_FOV"), GraphicsSettings.MinFOV, GraphicsSettings.MaxFOV, () => HudBehaviour.Instance.UnityGameCamera.fieldOfView, SetFOV));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_FarClipDist"), 0f, 1024f, () => HudBehaviour.Instance.UnityGameCamera.farClipPlane, SetFarClipDist));
		Items.Add(new DebugMenuItemToggle("Unified Body", () => Human.JustUnifiedBody, delegate(bool v)
		{
			Human.JustUnifiedBody = v;
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				character.UnityReinit();
			}
		}));
	}

	public static bool GetShowFPS()
	{
		return ShowFPS;
	}

	public static void SetShowFPS(bool on)
	{
		ShowFPS = on;
	}

	public static int GetMaxFixedUpdatesPerFrame()
	{
		return Mathf.RoundToInt(Time.maximumDeltaTime / Time.fixedUnscaledDeltaTime);
	}

	public static void SetMaxFixedUpdatesPerFrame(int v)
	{
		Time.maximumDeltaTime = Time.fixedUnscaledDeltaTime * (float)v + 1E-07f;
	}

	public int GetVSyncCount()
	{
		return QualitySettings.vSyncCount;
	}

	public void SetVSyncCount(int v)
	{
		GameImpl.Instance.SetVSyncCount(v);
	}

	public void SetTreeQuality(float v)
	{
		GameImpl.Instance.SetTreeQuality(v);
	}

	public void SetFOV(float v)
	{
		GameImpl.Instance.SetFOV(v);
	}

	public void SetFarClipDist(float v)
	{
		HudBehaviour.Instance.UnityGameCamera.farClipPlane = (HudBehaviour.Instance.GameCameraFarClipDist = v);
	}
}
