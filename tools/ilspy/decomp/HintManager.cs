using System;

public class HintManager : IReflectable
{
	public static int HINT_EquipWeapon = StringUtil.JenkinsHash("HINT_EquipWeapon");

	public static int HINT_AimAndFire = StringUtil.JenkinsHash("HINT_AimAndFire");

	public static int HINT_TargetLegs = StringUtil.JenkinsHash("HINT_TargetLegs");

	public static int HINT_LegAttackDodged = StringUtil.JenkinsHash("HINT_LegAttackDodged");

	public static int HINT_HeadAttackDodged = StringUtil.JenkinsHash("HINT_HeadAttackDodged");

	public static int HINT_SwitchTarget = StringUtil.JenkinsHash("HINT_SwitchTarget");

	public static int HINT_FindBandage = StringUtil.JenkinsHash("HINT_FindBandage");

	public static int HINT_NeedProperBandage = StringUtil.JenkinsHash("HINT_NeedProperBandage");

	public static int HINT_SwitchCharacter = StringUtil.JenkinsHash("HINT_SwitchCharacter");

	public static int HINT_SwitchFollower = StringUtil.JenkinsHash("HINT_SwitchFollower");

	public static int HINT_CommandMode = StringUtil.JenkinsHash("HINT_CommandMode");

	public static int HINT_UseStealth = StringUtil.JenkinsHash("HINT_UseStealth");

	public static int HINT_AvoidTowns = StringUtil.JenkinsHash("HINT_AvoidTowns");

	public static int HINT_UseSaveToken = StringUtil.JenkinsHash("HINT_UseSaveToken");

	public static int HINT_OpenInventory = StringUtil.JenkinsHash("HINT_OpenInventory");

	public static int HINT_UsePauseFastForward = StringUtil.JenkinsHash("HINT_UsePauseFastForward");

	public static int HINT_SkinCorpse = StringUtil.JenkinsHash("HINT_SkinCorpse");

	public static int HINT_NeedKnifeToAssassinate = StringUtil.JenkinsHash("HINT_NeedKnifeToAssassinate");

	public static int HINT_NoWhiteStrainAntigen = StringUtil.JenkinsHash("HINT_NoWhiteStrainAntigen");

	public static int HINT_UseBrainScan = StringUtil.JenkinsHash("HINT_UseBrainScan");

	public static int HINT_Surrender = StringUtil.JenkinsHash("HINT_Surrender");

	public static int HINT_SurrenderGamepad = StringUtil.JenkinsHash("HINT_SurrenderGamepad");

	public static int HINT_SetZone = StringUtil.JenkinsHash("HINT_SetZone");

	public static int HINT_UnconsciousBleedingHint = StringUtil.JenkinsHash("HINT_UnconsciousBleedingHint");

	public static int HINT_UnconsciousNotHealingHint = StringUtil.JenkinsHash("HINT_UnconsciousNotHealingHint");

	public static int HINT_CropsDyingOfFrost = StringUtil.JenkinsHash("HINT_CropsDyingOfFrost");

	public static int HINT_CropsDiedOfFrost = StringUtil.JenkinsHash("HINT_CropsDiedOfFrost");

	public static int HINT_FastCamera = StringUtil.JenkinsHash("HINT_FastCamera");

	public static int HINT_DragToSelect = StringUtil.JenkinsHash("HINT_DragToSelect");

	public static int HINT_AddToSelection = StringUtil.JenkinsHash("HINT_AddToSelection");

	public static int HINT_AssignGroups = StringUtil.JenkinsHash("HINT_AssignGroups");

	public static int HINT_UrgentIsBadForMorale = StringUtil.JenkinsHash("HINT_UrgentIsBadForMorale");

	public static int HINT_UseMap = StringUtil.JenkinsHash("HINT_UseMap");

	public static int HINT_StayDownwind = StringUtil.JenkinsHash("HINT_StayDownwind");

	public static int HINT_WaitForCrosshairToGoRed = StringUtil.JenkinsHash("HINT_WaitForCrosshairToGoRed");

	public static int HINT_GetCloserForAccurateShot = StringUtil.JenkinsHash("HINT_GetCloserForAccurateShot");

	public static int HINT_UseArmorPiercingAmmo = StringUtil.JenkinsHash("HINT_UseArmorPiercingAmmo");

	public static int HINT_UseBluntWeaponsAgainstArmor = StringUtil.JenkinsHash("HINT_UseBluntWeaponsAgainstArmor");

	public static HintManager Instance;

	public Hint[] Hints = new Hint[39];

	public bool StartedAnyThisFrame;

	public bool HasSeenAnyZombies;

	public TimeSpan LastWarDeclaredTime = Target.Never;

	private static string HintManagerUpdateStr = "HintManagerUpdate";

	public HintManager()
	{
		Instance = this;
	}

	public void Unload()
	{
		Instance = null;
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 190 && reflector.IsDeserialising)
		{
			for (int i = 0; i < 6; i++)
			{
				Hints[i].Reflect(reflector);
				Hints[i].Dirty = true;
			}
		}
	}

	public void FixupOnLoad(Session session, Reflector reflector)
	{
		if (reflector != null && reflector.Version >= 190)
		{
			PlayerRecord localPlayerRecord = session.GetLocalPlayerRecord();
			for (int i = 0; i < 39; i++)
			{
				Hints[i] = localPlayerRecord.SyncedHint[i];
			}
		}
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		for (int i = 0; i < 39; i++)
		{
			if (Hints[i].Dirty)
			{
				inputFrame.AddAction(InputAction.SetHint((HintType)i, Hints[i]));
				Hints[i].Dirty = false;
			}
		}
	}

	public void Update()
	{
		using (new UnityProfileMarker(HintManagerUpdateStr))
		{
			Session instance = Session.Instance;
			Hud instance2 = Hud.Instance;
			InputFunctionManager instance3 = InputFunctionManager.Instance;
			if (!GameImpl.Instance.Settings.HintsEnabled || instance.HitTheRoadCount > 0)
			{
				return;
			}
			Character localControlledCharacter = instance2.LocalControlledCharacter;
			if (localControlledCharacter == null || localControlledCharacter.Community == null)
			{
				return;
			}
			PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
			if (localPlayerRecord == null || localPlayerRecord.PlayerMode != PlayerMode.Controlling || instance.CountdownToTimeJump > 0 || instance.WantSlowerTransitionIn)
			{
				return;
			}
			if (!Hints[12].Performed && Hints[12].LastShown != TimeSpan.Zero && (localControlledCharacter.Community.AreAnyMembersInCombat() || localControlledCharacter.FindActiveGoal(GoalType.Conversation) != null))
			{
				Hints[12].Hide();
			}
			if (Hints[22].CanShowHint() && localControlledCharacter.UnderAttackRefCount > 0 && localControlledCharacter.EquippedItem == null && localControlledCharacter.Inventory.HasAnyDecentWeapons() && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.None && Hud.Instance.Cursor.AvailableActions.Count == 0)
			{
				Hints[22].StartShowing(GameImpl.Translate(GameImpl.Translate(HINT_EquipWeapon)));
			}
			if (Hints[0].CanShowHint() && localControlledCharacter.UnderAttackRefCount > 0 && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.None && (Hints[22].Performed || localControlledCharacter.EquippedItem != null || !localControlledCharacter.Inventory.HasAnyDecentWeapons()))
			{
				Hints[0].StartShowing(GameImpl.Translate(GameImpl.Translate(HINT_AimAndFire)));
			}
			if (Hints[1].CanShowHint() && Hints[0].Performed && Hints[0].TimeSinceLastShown >= TimeSpan.FromSeconds(60.0) && instance2.LocalWantLockOnTarget && instance2.LocalTargetObject is Character && instance2.LocalTargetBodyLocationToAimFor == TargettableBodyLocation.Torso && localControlledCharacter.CanTargetBodyLocation(TargettableBodyLocation.Legs, instance2.LocalTargetObject) && localControlledCharacter.IsAwake && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.None && ((Character)instance2.LocalTargetObject).CurrentActionAnim != ActionAnim.HandsUp)
			{
				Hints[1].StartShowing(GameImpl.Translate(HINT_TargetLegs));
			}
			if (Hints[4].CanShowHint() && Hints[0].Performed && Hints[0].TimeSinceLastShown >= TimeSpan.FromSeconds(60.0) && instance2.LocalWantLockOnTarget && instance2.LocalTargetObject is Character && instance2.HasAnyPotentialEnemyTargetsToSwitchTo() && localControlledCharacter.IsAwake && localControlledCharacter.AIOverridesControl() == AIOverridesControlReason.None && !((Character)instance2.LocalTargetObject).Alive)
			{
				Hints[4].StartShowing(GameImpl.Translate(HINT_SwitchTarget));
			}
			if (Hints[5].CanShowHint() && localControlledCharacter.UnderAttackRefCount == 0 && localControlledCharacter.IsConscious && localControlledCharacter.HasUnbandagedInjury(0) && localControlledCharacter.Inventory.FindItemWithHighestBandageLevel() == null && Session.Instance.PlayTime - LastWarDeclaredTime >= TimeSpan.FromSeconds(1.0) && localControlledCharacter.CurrentActionAnim != ActionAnim.HandsUp && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[5].StartShowing(GameImpl.Translate(HINT_FindBandage));
			}
			if (Hints[15].CanShowHint(300f) && localControlledCharacter.UnderAttackRefCount == 0 && localControlledCharacter.IsConscious && !localControlledCharacter.HasUnbandagedInjury(0) && localControlledCharacter.HasUnbandagedInjury(1) && localControlledCharacter.Inventory.FindHighestBandageLevel() <= 0 && Session.Instance.PlayTime - LastWarDeclaredTime >= TimeSpan.FromSeconds(1.0) && localControlledCharacter.CurrentActionAnim != ActionAnim.HandsUp && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[15].StartShowing(GameImpl.Translate(HINT_NeedProperBandage));
			}
			if (Hints[23].CanShowHint() || Hints[24].CanShowHint())
			{
				foreach (Character member in localControlledCharacter.Community.Members)
				{
					if (member.Consciousness != Consciousness.Unconscious || !(member.GetBloodLoss() > 1f))
					{
						continue;
					}
					if (localControlledCharacter.Community.GetLivingNonZombieMemberCount() > 1 && !localControlledCharacter.Community.AreAnyMembersInCombat())
					{
						if (Hints[23].CanShowHint() && member.HasUnbandagedInjury(0))
						{
							string text = GameImpl.Translate(HINT_UnconsciousBleedingHint);
							text = text.Replace("%1", member.GetDisplayNameString(noStrangers: true, englishOnly: false));
							text = StringUtil.ApplyFormulae(text, null, member);
							Hints[23].StartShowing(text);
						}
						else if (Hints[24].CanShowHint() && member.HasUnbandagedInjury(1) && !member.HasUnbandagedInjury(0))
						{
							string text2 = GameImpl.Translate(HINT_UnconsciousNotHealingHint);
							text2 = text2.Replace("%1", member.GetDisplayNameString(noStrangers: true, englishOnly: false));
							text2 = StringUtil.ApplyFormulae(text2, null, member);
							Hints[24].StartShowing(text2);
						}
					}
					break;
				}
			}
			if (Hints[8].CanShowHint() && localControlledCharacter.Community.GetLivingNonZombieMemberCount() > 1 && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[8].StartShowing(GameImpl.Translate(HINT_SwitchCharacter));
			}
			if (Hints[9].CanShowHint() && Hints[8].Performed && instance.FollowerCommandsEnabled && localControlledCharacter.Followers != null && localControlledCharacter.Followers.Count > 0 && instance3.IsMapped(InputFunction.ControlFollower) && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.Community.GetLivingNonZombieMemberCount() > ((localControlledCharacter.Followers != null) ? localControlledCharacter.Followers.Count : 0) + 1 && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[9].StartShowing(GameImpl.Translate(HINT_SwitchFollower));
			}
			if (Hints[10].CanShowHint() && Hints[8].Performed && instance.FollowerCommandsEnabled && localControlledCharacter.Community.GetLivingNonZombieMemberCount() > ((localControlledCharacter.Followers != null) ? localControlledCharacter.Followers.Count : 0) + 1 && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[10].StartShowing(GameImpl.Translate(HINT_CommandMode));
			}
			if (Hints[6].CanShowHint() && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.IsConscious && MusicManager.AreAnyEnemiesNearby(localControlledCharacter, 16f) && Session.Instance.PlayTime - LastWarDeclaredTime >= TimeSpan.FromSeconds(1.0) && localControlledCharacter.CurrentActionAnim != ActionAnim.HandsUp && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Community communityThatOwnsThisArea = localControlledCharacter.GetCommunityThatOwnsThisArea();
				if (communityThatOwnsThisArea == null || communityThatOwnsThisArea.GetRelationship(localControlledCharacter.Community) == CommunityRelationshipType.Hostile)
				{
					Hints[6].StartShowing(GameImpl.Translate(HINT_UseStealth));
				}
			}
			if (Hints[7].CanShowHint() && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.IsConscious && instance.CommunityManager.GetTownForTile(localControlledCharacter.Tile, 16f) != null && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[7].ShowAndMarkPerformed(GameImpl.Translate(HINT_AvoidTowns));
			}
			if (Hints[17].CanShowHint(300f) && Hud.Instance.Pip.FocusObject is Character { Infection: InfectionType.White })
			{
				if (GameImpl.Instance.FindAntigenPrototypeForInfectionType(InfectionType.White) == null)
				{
					Hints[17].StartShowing(GameImpl.Translate(HINT_NoWhiteStrainAntigen));
				}
				else
				{
					Hints[17].MarkPerformed();
				}
			}
			if (instance.DifficultySettings.SaveTokensRequired && instance.FollowerCommandsEnabled && Hints[11].CanShowHint() && instance3.IsMapped(InputFunction.QuickSave) && localControlledCharacter.Inventory.FindItemOfClass(typeof(SavegameToken)) != null)
			{
				Hints[11].StartShowing(GameImpl.Translate(HINT_UseSaveToken));
			}
			if (Hints[12].CanShowHint() && localPlayerRecord.HasScavengedAnyEquipment && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[12].StartShowing(GameImpl.Translate(HINT_OpenInventory));
			}
			if (Hints[34].CanShowHint() && localControlledCharacter.InsideBuilding != null && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[34].StartShowing(GameImpl.Translate(HINT_OpenInventory));
			}
			if (Hints[13].CanShowHint() && Hints[34].Performed && instance.FollowerCommandsEnabled && localControlledCharacter.InsideBuilding != null)
			{
				Hints[13].StartShowing(GameImpl.Translate(HINT_UsePauseFastForward));
			}
			HasSeenAnyZombies |= instance2.LocalTargetObject is Character && ((Character)instance2.LocalTargetObject).Zombie;
			if (Hints[14].CanShowHint() && Hints[6].Performed && HasSeenAnyZombies && !localControlledCharacter.Community.AreAnyMembersInCombat() && IsACorpseNearby(localControlledCharacter) && localControlledCharacter.Inventory.FindItemOfClass(typeof(HuntingKnife)) != null && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[14].StartShowing(GameImpl.Translate(HINT_SkinCorpse));
			}
			if (Hints[25].CanShowHint() && instance.Weather.TemperatureInCelsius < 0f && instance.CropsManager.DoesCommunityHaveAnyCrops(instance.CommunityManager.PlayerCommunity.Id))
			{
				Hints[25].ShowAndMarkPerformed(GameImpl.Translate(HINT_CropsDyingOfFrost));
			}
			if (Hints[28].CanShowHint() && instance.GameCamera.FlyCam && Hints[27].Performed && Hints[27].TimeSinceLastShown > TimeSpan.FromSeconds(60.0) && Hud.Instance.CursorTargetObject is Character character2 && character2.IsControllableByPlayer() && character2.Community.GetLivingNonZombieMemberCount() > 2)
			{
				Hints[28].StartShowing(GameImpl.Translate(HINT_DragToSelect).Replace("MainAction", "AltAction"));
			}
			if (Hints[29].CanShowHint() && instance.GameCamera.FlyCam && Hud.Instance.SelectedCharacters.Count > 0 && InputFunctionManager.Instance.IsMapped(InputFunction.AddToSelection) && Hints[28].Performed && Hints[28].TimeSinceLastShown > TimeSpan.FromSeconds(60.0))
			{
				Hints[29].StartShowing(GameImpl.Translate(HINT_AddToSelection));
			}
			if (Hints[30].CanShowHint() && instance.GameCamera.FlyCam && Hud.Instance.SelectedCharacters.Count > 0 && InputFunctionManager.Instance.IsMapped(InputFunction.SelectGroup1) && Hints[29].Performed && Hints[29].TimeSinceLastShown > TimeSpan.FromSeconds(60.0))
			{
				Hints[30].StartShowing(GameImpl.Translate(HINT_AssignGroups));
			}
			if (Hints[31].CanShowHint() && !localControlledCharacter.DirectControlled && !localControlledCharacter.IsPlayerAvatar() && !instance.GameCamera.FlyCam && localControlledCharacter.GetGoal() is SurvivorGoal { SubGoal: RoleGoal subGoal } && localControlledCharacter.IsRoleUrgent(subGoal.GetRoleInfoBeingPerformed(localControlledCharacter)))
			{
				Hints[31].StartShowing(GameImpl.Translate(HINT_UrgentIsBadForMorale));
			}
			if (Hints[32].CanShowHint() && !instance.GameCamera.FlyCam && StoryManager.Instance.CurrentQuest != null && StoryManager.Instance.QuestDestinations.Count > 0 && !localControlledCharacter.Community.AreAnyMembersInCombat() && localControlledCharacter.FindActiveGoal(GoalType.Conversation) == null)
			{
				Hints[32].StartShowing(GameImpl.Translate(HINT_UseMap));
			}
			for (int i = 0; i < Hints.Length; i++)
			{
				Hints[i].Update(localControlledCharacter, (HintType)i);
			}
			StartedAnyThisFrame = false;
		}
	}

	public void HideAllHints()
	{
		for (int i = 0; i < Hints.Length; i++)
		{
			Hints[i].Hide();
		}
	}

	private bool IsACorpseNearby(Character controlledCharacter)
	{
		return GameTerrain.Instance.CharacterMapWho.GetNearestObject(controlledCharacter.Tile, 16, (Character character) => !character.Alive) != null;
	}

	public void ShowNeedKnifeToAssassinateHint()
	{
		if (Hints[16].CanShowHint())
		{
			Hints[16].StartShowing(GameImpl.Translate(HINT_NeedKnifeToAssassinate));
		}
	}

	public void ShowUseBrainScanHint(Character targetCharacter)
	{
		if (!Hints[18].CanShowHint() || targetCharacter.PersonalityKnown == 0 || targetCharacter.Skillset.SkillKnown == 0 || !targetCharacter.NameKnown || targetCharacter.Community == null || targetCharacter.Community.CommunityType == CommunityType.Player || !targetCharacter.AliveAndNotZombie)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < targetCharacter.Relationships.Count; i++)
		{
			if (targetCharacter.Relationships[i].KnownToPlayer)
			{
				num++;
			}
		}
		if (num > 0)
		{
			Hints[18].StartShowing(GameImpl.Translate(HINT_UseBrainScan));
		}
	}

	public void ShowSurrenderHint(Character targetCharacter)
	{
		if (InputFunctionManager.Instance.CurrentInputType == InputType.MouseAndKeyboard)
		{
			if (Hints[19].CanShowHint())
			{
				Hints[19].StartShowing(GameImpl.Translate(HINT_Surrender).Replace("%1", targetCharacter.Community.GetDisplayNameString()));
			}
		}
		else if (Hints[20].CanShowHint())
		{
			Hints[20].StartShowing(GameImpl.Translate(HINT_SurrenderGamepad).Replace("%1", targetCharacter.Community.GetDisplayNameString()));
		}
	}

	public void MarkSurrenderPerformed()
	{
		if (!Session.Instance.FollowerCommandsEnabled)
		{
			if (InputFunctionManager.Instance.CurrentInputType == InputType.MouseAndKeyboard)
			{
				Hints[19].Hide();
			}
			else
			{
				Hints[20].Hide();
			}
		}
		else if (InputFunctionManager.Instance.CurrentInputType == InputType.MouseAndKeyboard)
		{
			Hints[19].MarkPerformed();
		}
		else
		{
			Hints[20].MarkPerformed();
		}
	}
}
