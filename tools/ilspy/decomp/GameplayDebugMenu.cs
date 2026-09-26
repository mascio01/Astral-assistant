using System;
using UnityEngine;

public class GameplayDebugMenu : DebugMenu
{
	private bool WantSaveSettings;

	public static bool ShowGoals;

	public static bool DrawConstructionRecords;

	public static bool ForceDeterministicUpdate;

	public GameplayDebugMenu()
		: base(GameImpl.Translate("DEBUG_GameplayDebug"))
	{
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CameraDebug"), typeof(CameraDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CarryingDebug"), typeof(CarryDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_AStar"), typeof(AStarProfiler)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_ThreadPool"), typeof(ThreadPoolProfiler)));
		Items.Add(new DebugMenuItemToggle("Multithreaded Speech Options", () => StoryManager.EvaluateSpeechOptionsOnThreadEnabled, delegate(bool v)
		{
			StoryManager.EvaluateSpeechOptionsOnThreadEnabled = v;
			PlayerPrefs.SetInt("MultithreadedSpeechOptions", v ? 1 : 0);
		}));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_AllowViewInfoOnAnyone"), InfoScreen.GetAllowViewInfoOnAnyone, InfoScreen.SetAllowViewInfoOnAnyone, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowGoals"), GetShowGoals, SetShowGoals));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowHitBoxes"), Character.GetDebugDrawHitBoxes, Character.SetDebugDrawHitBoxes));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowInjuries"), Character.GetDebugDrawInjuries, Character.SetDebugDrawInjuries));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowBones"), Character.GetDebugDrawBones, Character.SetDebugDrawBones));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowSpawnPoints"), SpawnPoint.GetDebugShowSpawnPoints, SpawnPoint.SetDebugShowSpawnPoints));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_MinTimeBetweenAutosaves"), () => GameImpl.Instance.Settings.MinutesBetweenAutosaves.ToString(), delegate(string s)
		{
			GameImpl.Instance.Settings.MinutesBetweenAutosaves = StringUtil.ParseFloat(s);
			WantSaveSettings = true;
		}));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowTownsOnMap"), typeof(MinimapCameraBehaviour), "DrawTownsOnMap"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowCropPatches"), typeof(GameCursor), "DrawAllCropPatches"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowLookRaycasts"), typeof(Character), "DrawLookRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowLastKnownTargetPositions"), typeof(Character), "DrawLastKnownTargetPositions"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowLineOfSightRaycasts"), typeof(AStarRequester), "DrawLineOfSightRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowCanAttackRaycasts"), typeof(RangedAttack), "DrawCanAttackRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowMeleeAttackRaycasts"), typeof(Character), "DrawMeleeAttackRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowThrowingArcRaycasts"), typeof(BaseThrownProjectile), "DrawThrowingArcRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowExplosionRaycasts"), typeof(Character), "DrawExplosionRaycasts"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowConstructionRecords"), typeof(GameplayDebugMenu), "DrawConstructionRecords"));
		Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_ThrowTargetSnapAngle"), 0f, 30f, typeof(Hud), "DirectControlTargetableThrowableAngleDeg"));
		if (!Session.Instance.IsInMultiplayerGame())
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ZeroHuntersCountdown"), ResetHuntersCountdown, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ZeroExtortersCountdown"), ResetExtortersCountdown, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ZeroRepopulationCountdown"), ResetRepopulationCountdown, affectsGameState: true));
		}
		if (Session.Instance.IsInMultiplayerGame())
		{
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_PredictionEnabled"), typeof(PredictedObjectManager), "PredictionEnabled"));
		}
		else
		{
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ForceDeterministicUpdate"), typeof(GameplayDebugMenu), "ForceDeterministicUpdate"));
		}
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_FollowerCommandsEnabled"), () => Session.Instance.FollowerCommandsEnabled, delegate(bool v)
		{
			Session.Instance.FollowerCommandsEnabled = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemCustom("Update Decals", delegate
		{
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				character.WantUnityUpdateDecals = true;
				character.CanSkipUnityUpdate = false;
			}
		}));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ResetAchievements"), delegate
		{
			AchievementsManager.Instance.ResetAchievements();
		}));
	}

	public override void DeactivateImpl()
	{
		if (WantSaveSettings)
		{
			GameImpl.Instance.AutoSaveSettings();
			WantSaveSettings = false;
		}
		base.DeactivateImpl();
	}

	public static bool GetShowGoals()
	{
		return ShowGoals;
	}

	public static void SetShowGoals(bool v)
	{
		ShowGoals = v;
	}

	public static void ResetHuntersCountdown()
	{
		foreach (InvaderInstance activeInvader in StoryManager.Instance.ActiveInvaders)
		{
			activeInvader.LastSpawnedTime = Target.Never;
		}
	}

	public static void ResetExtortersCountdown()
	{
		foreach (Community community in Session.Instance.CommunityManager.Communities)
		{
			if (community.CommunityType == CommunityType.Looter)
			{
				community.NextExtortionTime = TimeSpan.Zero;
			}
			community.LastExtortedFromTime = Target.Never;
		}
	}

	public static void ResetRepopulationCountdown()
	{
		Session.Instance.CommunityManager.RepopulateCountdown = TimeSpan.Zero;
	}
}
