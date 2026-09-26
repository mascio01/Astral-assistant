using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class CharacterEditor : DebugMenu
{
	private Character CharacterToEdit;

	private bool WantUpdate;

	private static string TimeFormat = "dd\\.hh\\:mm\\:ss";

	public CharacterEditor()
		: base("CharacterEditor")
	{
	}

	public static TileObject GetCurrentObject()
	{
		if (Session.Instance.Editor)
		{
			return Hud.Instance.EditorSelectedObject;
		}
		if (InfoScreen.Instance.Active && InfoScreen.Instance.CurrentObject != null)
		{
			return InfoScreen.Instance.CurrentObject;
		}
		TileObject localTargetObject = Hud.Instance.GetLocalTargetObject();
		if (localTargetObject == null)
		{
			return Hud.Instance.LocalControlledCharacter;
		}
		return localTargetObject;
	}

	public static Character GetCurrentCharacter()
	{
		TileObject currentObject = GetCurrentObject();
		if (currentObject is Grave grave)
		{
			return grave.Corpse;
		}
		return currentObject as Character;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Character character = GetCurrentCharacter();
		if (CharacterToEdit == character && !WantUpdate)
		{
			return;
		}
		CharacterToEdit = character;
		Items.Clear();
		if (character == null)
		{
			return;
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Id"), character.Id.ToString()));
		Items.Add(new DebugMenuUniqueID(GameImpl.Translate("DEBUG_UniqueID"), character));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_FirstName"), () => character.FirstName, delegate(string v)
		{
			character.SetFirstName(v);
			character.FirstNameVerified = StringStatus.Unverified;
			Session.Instance.VerifiedStrings = false;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Surname"), () => character.Surname, delegate(string v)
		{
			character.Surname = v;
			character.SurnameVerified = StringStatus.Unverified;
			Session.Instance.VerifiedStrings = false;
		}));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_RandomizeName"), delegate
		{
			character.RandomizeName(MathUtil.NonDeterministicRand);
			WantUpdate = true;
		}));
		if (character.Community == null || character.Community.CommunityType != CommunityType.Player || character != character.Community.Leader)
		{
			Items.Add(new DebugMenuAllegianceAdjuster(GameImpl.Translate("DEBUG_Community"), character));
		}
		Items.Add(new DebugMenuAllegianceAdjuster(GameImpl.Translate("DEBUG_InitialCommunity"), character, initialCommunity: true));
		Items.Add(new DebugMenuItemEnum<Consciousness>(GameImpl.Translate("DEBUG_Consciousness"), Consciousness.Dead, () => character.Consciousness, delegate(Consciousness consciousness)
		{
			character.SetConsciousness(consciousness, canSpeak: false);
			if (!Session.Instance.Editor && consciousness != Consciousness.Dead && character.GetGoal() == null)
			{
				character.SetGoal((character.GetBaseObjectType() != BaseObjectType.Human) ? new AnimalGoal() : (character.Zombie ? ((PrioritiserGoal)new ZombieGoal()) : ((PrioritiserGoal)new SurvivorGoal())));
			}
			Session.Instance.AchievementsEnabled = false;
		}));
		if (!character.AliveAndNotZombie)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_KilledBy"), ((character.Killer != null) ? character.Killer.GetDisplayNameString() : "") + ((character.CauseOfDeath != CauseOfDeath.Other) ? (" " + character.CauseOfDeath) : "")));
		}
		if (character.HangOutLocation != TerrainCoord.Invalid)
		{
			List<DebugMenuItem> items = Items;
			string name = GameImpl.Translate("DEBUG_HangOutLocation");
			string text = character.HangOutLocation.ToString();
			object obj;
			if (!(character.InitialHangOutLocation != TerrainCoord.Invalid))
			{
				obj = "";
			}
			else
			{
				TerrainCoord initialHangOutLocation = character.InitialHangOutLocation;
				obj = " (" + initialHangOutLocation.ToString() + ")";
			}
			items.Add(new DebugMenuItemText(name, text + (string)obj));
		}
		if (character.VotedFor != null)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_VotedFor"), (character.VotedFor != null) ? (character.VotedFor.GetDisplayNameString() + ", ") : ""));
		}
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_SurvivalFactorsEditor"), typeof(CharacterSurvivalFactorsEditor)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_MemoryEditor"), typeof(CharacterMemoryEditor)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_SkillEditor"), typeof(CharacterSkillEditor)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_PersonalityEditor"), typeof(CharacterPersonalityEditor)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_RelationshipEditor"), typeof(CharacterRelationshipEditor)));
		Human human = CharacterToEdit as Human;
		if (human != null)
		{
			HumanAppearance appearance = human.GetAppearance();
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Randomize"), delegate
			{
				human.RandomizeAppearance(MathUtil.NonDeterministicRand);
				character.RandomizeClothing(MathUtil.NonDeterministicRand, seasonallyAppropriate: true);
				WantUpdate = true;
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_RandomizeAppearance"), delegate
			{
				human.RandomizeAppearance(MathUtil.NonDeterministicRand);
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_RandomizeClothing"), delegate
			{
				human.RandomizeClothing(MathUtil.NonDeterministicRand, seasonallyAppropriate: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemEnum<GenderType>(GameImpl.Translate("MENU_Gender"), GenderType.Female, () => human.Appearance.Gender, delegate(GenderType v)
			{
				human.SetGender(v);
				human.SetFirstName(GameImpl.Instance.PickRandomName((v == GenderType.Female) ? NameType.FemaleFirstName : NameType.MaleFirstName, MathUtil.NonDeterministicRand));
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Teleporter"), typeof(CharacterTeleporter)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_HairEditor"), typeof(CharacterHairEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_BoneEditor"), typeof(CharacterBoneEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_RawBoneEditor"), typeof(CharacterRawBoneEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_FaceEditor"), typeof(CharacterFaceEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_BodyTypeEditor"), typeof(CharacterBodyTypeEditor)));
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Age"), 18f, 80f, () => appearance.Age, delegate(float v)
			{
				character.SetAge(v);
			}, affectsGameState: true));
			if (Session.Instance.Editor)
			{
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Angle"), -180f, 180f, () => character.FacingAngle * 57.29578f, delegate(float v)
				{
					character.SetFacingAngle(v * (MathF.PI / 180f));
				}, affectsGameState: true));
			}
			if (character.Zombie)
			{
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_ZombieDecayAmount"), 0f, 1f, () => appearance.ZombieDecayAmount, delegate(float v)
				{
					appearance.ZombieDecayAmount = v;
					character.UnityOnChangedAppearance();
				}));
			}
			Items.Add(new DebugMenuColorSpectrumAdjuster(GameImpl.Translate("DEBUG_SkinTone"), HumanAppearance.SkinColorSpectrum, () => appearance.SkinColorIndex, delegate(float v)
			{
				appearance.SkinColorIndex = v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_SkinRed"), 0, 255, () => HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].r, delegate(int v)
			{
				HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].r = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_SkinGreen"), 0, 255, () => HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].g, delegate(int v)
			{
				HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].g = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_SkinBlue"), 0, 255, () => HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].b, delegate(int v)
			{
				HumanAppearance.SkinColorSpectrum[(int)(appearance.SkinColorIndex + 0.5f)].b = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuColorSpectrumAdjuster(GameImpl.Translate("DEBUG_EyeColor"), HumanAppearance.EyeColorSpectrum, () => appearance.EyeColorIndex, delegate(float v)
			{
				appearance.EyeColorIndex = v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_UnderwearRed"), 0, 255, () => appearance.UnderwearColor.r, delegate(int v)
			{
				appearance.UnderwearColor.r = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_UnderwearGreen"), 0, 255, () => appearance.UnderwearColor.g, delegate(int v)
			{
				appearance.UnderwearColor.g = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_UnderwearBlue"), 0, 255, () => appearance.UnderwearColor.b, delegate(int v)
			{
				appearance.UnderwearColor.b = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_FrecklesAlpha"), 0, 255, () => appearance.FrecklesColor.a, delegate(int v)
			{
				appearance.FrecklesColor.a = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_FrecklesRed"), 0, 255, () => appearance.FrecklesColor.r, delegate(int v)
			{
				appearance.FrecklesColor.r = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_FrecklesGreen"), 0, 255, () => appearance.FrecklesColor.g, delegate(int v)
			{
				appearance.FrecklesColor.g = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_FrecklesBlue"), 0, 255, () => appearance.FrecklesColor.b, delegate(int v)
			{
				appearance.FrecklesColor.b = (byte)v;
				character.UnityOnChangedAppearance();
			}));
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_Boxer"), character, "Boxer", affectsGameState: true));
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_God"), character, "God", affectsGameState: true));
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_CanFollowPlayer"), character, "CanFollowPlayer", affectsGameState: true));
			for (int num = 0; num < character.Roles.Count; num++)
			{
				int localIndex = num;
				Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Role"), delegate
				{
					if (localIndex < character.Roles.Count)
					{
						RoleInfo value = character.Roles[localIndex];
						value.Role = (Role)((int)(value.Role + 18 - 1) % 18);
						character.Roles[localIndex] = value;
					}
				}, delegate
				{
					if (localIndex < character.Roles.Count)
					{
						RoleInfo value = character.Roles[localIndex];
						value.Role = (Role)((int)(value.Role + 1) % 18);
						character.Roles[localIndex] = value;
					}
				}, delegate(ref StringBuilder value)
				{
					if (localIndex < character.Roles.Count)
					{
						value.Append(Character.RoleNames[(int)character.Roles[localIndex].Role]);
					}
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_DeleteRole"), delegate
				{
					if (localIndex < character.Roles.Count)
					{
						character.RemoveRole(localIndex);
						WantUpdate = true;
					}
				}, affectsGameState: true));
			}
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_AddRole"), delegate
			{
				character.Roles.Add(new RoleInfo(Role.None));
				WantUpdate = true;
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Rank"), delegate
			{
				character.SetRank((Rank)((int)(character.Rank + 3 - 1) % 3));
			}, delegate
			{
				character.SetRank((Rank)((int)(character.Rank + 1) % 3));
			}, delegate(ref StringBuilder value)
			{
				value.Append(Character.RankNames[(int)character.Rank]);
			}, affectsGameState: true));
			if (Session.Instance.Editor)
			{
				Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Infection"), delegate
				{
					character.Infection = (InfectionType)((int)(character.Infection + 6 - 1) % 6);
				}, delegate
				{
					character.Infection = (InfectionType)((int)(character.Infection + 1) % 6);
					character.InfectionProgression = ((character.Infection == InfectionType.None) ? 0f : 2f);
				}, delegate(ref StringBuilder value)
				{
					value.Append(Injury.InfectionTypeNames[(int)character.Infection]);
				}, affectsGameState: true));
			}
			if (Session.Instance.Editor || !Session.Instance.AchievementsEnabled)
			{
				Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_InvisibleStrain"), delegate
				{
					character.InvisibleStrain = (InvisibleStrainType)((int)(character.InvisibleStrain + 3 - 1) % 3);
				}, delegate
				{
					character.InvisibleStrain = (InvisibleStrainType)((int)(character.InvisibleStrain + 1) % 3);
				}, delegate(ref StringBuilder value)
				{
					value.Append(Injury.InvisibleStrainTypeNames[(int)character.InvisibleStrain]);
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_HadInvisibleStrainFromStart"), character, "HadInvisibleStrainFromStart", affectsGameState: true));
				Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_AlwaysActivateInvisibleStrain"), character, "AlwaysActivateInvisibleStrain", affectsGameState: true));
			}
			else
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ShowInvisibleStrain"), delegate
				{
					Session.Instance.AchievementsEnabled = false;
					WantUpdate = true;
				}));
			}
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_PlayDead"), character, "PlayDead", affectsGameState: true));
			Items.Add(new DebugMenuLootLocationAdjuster(GameImpl.Translate("EDITOR_LootLocation"), character));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_GenerateLoot"), delegate
			{
				character.Inventory.GeneratedLoot = false;
				character.Inventory.DeleteAll(character, carrierBeingDeleted: false);
				GameTerrain.GenerateCharacterEquipment(character, character.IsLooter() ? PersonalityGroup.LooterFaction : PersonalityGroup.NormalFaction, MathUtil.NonDeterministicRand);
				GameTerrain.GenerateLoot(character, MathUtil.NonDeterministicRand);
				Session.Instance.AchievementsEnabled = false;
			}));
			if (!character.IsControllableByPlayer())
			{
				if (character.RestockTime != TimeSpan.Zero)
				{
					TimeSpan timeSpan = character.RestockTime - Session.Instance.PlayTime;
					Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_TimeTillRestock"), timeSpan.ToString(TimeFormat)));
				}
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Restock"), delegate
				{
					character.RestockTime = Session.Instance.PlayTime;
					Session.Instance.AchievementsEnabled = false;
				}));
			}
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_DontSellItemTypeToPlayer"), () => (character.DontSellItemTypeToPlayer == null) ? string.Empty : character.DontSellItemTypeToPlayer.Name, delegate(string v)
			{
				character.DontSellItemTypeToPlayer = GameImpl.Instance.FindEquipmentPrototypeByName(v);
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_SaveAsFavourite"), delegate
			{
				human.SaveFavouriteCharacter(delegate(string text2)
				{
					GameImpl.Instance.ShowMessageBox("Saved successfully as: " + text2);
				});
			}));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_LoadFavourite"), delegate
			{
				OpenFileName openFileName = new OpenFileName();
				openFileName.structSize = Marshal.SizeOf(openFileName);
				openFileName.filter = "character files (*.char)\0*.char\0All files (*.*)\0*.*\0\0";
				openFileName.file = new string(new char[256]);
				openFileName.maxFile = openFileName.file.Length;
				openFileName.fileTitle = new string(new char[64]);
				openFileName.maxFileTitle = openFileName.fileTitle.Length;
				openFileName.initialDir = GameImpl.Instance.SaveGamePath + "\\Characters";
				openFileName.title = "Open Character";
				openFileName.defExt = "CHAR";
				openFileName.flags = 530440;
				if (DllTest.GetOpenFileName(openFileName))
				{
					SavedCharacter savedCharacter = new SavedCharacter();
					if (savedCharacter.Load(openFileName.file))
					{
						savedCharacter.ApplyToCharacter(human);
						human.UnityOnChangedAppearance();
						human.GetAppearance().SetupDNA();
						human.UnityOnChangedBones();
						human.UnityUpdateAppearance();
						Session.Instance.AchievementsEnabled = false;
						Session.Instance.VerifiedStrings = false;
					}
				}
			}));
			if (character.FindAttempts.Count > 0)
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_ClearFindAttempts"), delegate
				{
					character.FindAttempts.Clear();
					Session.Instance.AchievementsEnabled = false;
				}));
			}
			for (int num2 = 0; num2 < character.EmpathyOverrides.Count; num2++)
			{
				int scopedIndex = num2;
				Items.Add(new DebugMenuBaseObjectID(GameImpl.Translate("DEBUG_EmpathyDisabled") + " " + num2, character.EmpathyOverrides[scopedIndex].OverrideObject, delegate(BaseObject v)
				{
					if (scopedIndex < character.EmpathyOverrides.Count)
					{
						EmpathyOverride value = character.EmpathyOverrides[scopedIndex];
						value.OverrideObject = v;
						character.EmpathyOverrides[scopedIndex] = value;
						Session.Instance.AchievementsEnabled = false;
					}
				}));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Amount"), -100f, 100f, () => (scopedIndex >= character.EmpathyOverrides.Count) ? 0f : character.EmpathyOverrides[scopedIndex].OverrideValue, delegate(float v)
				{
					if (scopedIndex < character.EmpathyOverrides.Count)
					{
						EmpathyOverride value = character.EmpathyOverrides[scopedIndex];
						value.OverrideValue = v;
						character.EmpathyOverrides[scopedIndex] = value;
						Session.Instance.AchievementsEnabled = false;
					}
				}));
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), delegate
				{
					if (scopedIndex < character.EmpathyOverrides.Count)
					{
						character.EmpathyOverrides.RemoveAt(scopedIndex);
						WantUpdate = true;
						Session.Instance.AchievementsEnabled = false;
					}
				}));
			}
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_AddEmpathyDisabled"), delegate
			{
				character.EmpathyOverrides.Add(default(EmpathyOverride));
				WantUpdate = true;
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuItemEnum<SurrenderMode>(GameImpl.Translate("DEBUG_SurrenderMode"), SurrenderMode.ForceDontSurrender, () => character.SurrenderMode, delegate(SurrenderMode v)
			{
				character.SurrenderMode = v;
			}));
		}
		Chicken chicken = CharacterToEdit as Chicken;
		if (chicken != null)
		{
			ChickenAppearance appearance2 = chicken.GetAppearance();
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Age"), 0f, 15f, () => appearance2.Age, delegate(float v)
			{
				character.SetAge(v);
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuItemEnum<ChickenColor>(GameImpl.Translate("EDITOR_Color"), ChickenColor.White, () => appearance2.ChickenColor, delegate(ChickenColor v)
			{
				appearance2.ChickenColor = v;
				chicken.UnityOnChangedAppearance();
			}));
		}
		if (Session.Instance.CommunityManager.PlayerCommunity == null || character != Session.Instance.CommunityManager.PlayerCommunity.Leader)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), character.Delete, affectsGameState: true));
		}
	}
}
