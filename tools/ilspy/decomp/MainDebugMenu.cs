using System.Collections.Generic;
using System.IO;

public class MainDebugMenu : DebugMenu
{
	public MainDebugMenu()
		: base("Debug Options")
	{
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		Items.Clear();
		if (!Session.Instance.IsInMultiplayerGame())
		{
			if (Session.Instance.Editor)
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Save"), Save));
			}
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_TerrainEditor"), typeof(TerrainEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CharacterSpawner"), typeof(CharacterSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CharacterEditor"), typeof(CharacterEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_PropSpawner"), typeof(PropSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_PropEditor"), typeof(PropEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_EquipmentSpawner"), typeof(EquipmentSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_LiquidSpawner"), typeof(LiquidSpawner)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_EquippedModelEditor"), typeof(EquippedModelEditor)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_CommunityList"), typeof(CommunityList)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Quests"), typeof(QuestList)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Variables"), typeof(VariableList)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Triggers"), typeof(TriggerList)));
			Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Invaders"), typeof(InvaderList)));
			if (!Session.Instance.Editor)
			{
				Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_DifficultySettings"), typeof(DifficultyDebugMenu)));
			}
		}
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_GraphicsDebug"), typeof(GraphicsDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_GameplayDebug"), typeof(GameplayDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_SoundDebug"), typeof(SoundDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_Utils"), typeof(UtilsDebugMenu)));
		if (Session.Instance.IsInMultiplayerGameAsLeader())
		{
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_Snapshot"), typeof(Session), "ForceFullSnapshotStatic"));
		}
		if (!Session.Instance.Editor)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_LoneWolf"), Session.Instance.LoneWolf.ToString()));
			bool flag = false;
			if (!AchievementsManager.AllowAchievementsWithMods && !GameImpl.Instance.IsCurrentStoryListEqualTo(AchievementsManager.SandboxMode) && !GameImpl.Instance.IsCurrentStoryListEqualTo(AchievementsManager.StoryMode))
			{
				flag = true;
			}
			if (flag)
			{
				Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_AchievementsEnabled"), false + " - " + GameImpl.Translate("DEBUG_DisabledBecauseOfMods")));
			}
			else
			{
				Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_AchievementsEnabled"), Session.Instance.AchievementsEnabled.ToString()));
			}
		}
	}

	private void Save()
	{
		GameTerrain.Instance.OnEditorSave();
		foreach (Character character in Session.Instance.CharacterManager.Characters)
		{
			character.OnEditorSave();
		}
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		string text = (File.Exists(currentlyEditingStory.Path + "/Terrain.map") ? (currentlyEditingStory.Path + "/Terrain.map") : (currentlyEditingStory.Path + "/Terrain.mapx"));
		string text2 = (File.Exists(currentlyEditingStory.Path + "/FixedTerrain.map") ? (currentlyEditingStory.Path + "/FixedTerrain.map") : (currentlyEditingStory.Path + "/FixedTerrain.mapx"));
		string text3 = currentlyEditingStory.Path + "/TerrainText.tsv";
		using (FileStream stream = new FileStream(text, FileMode.Create))
		{
			using CustomBinaryWriter reflector = new CustomBinaryWriter(stream);
			Session.Instance.Reflect(reflector);
		}
		using (FileStream stream2 = new FileStream(text2, FileMode.Create))
		{
			using CustomBinaryWriter customBinaryWriter = new CustomBinaryWriter(stream2);
			customBinaryWriter.Write(629);
			GameTerrain.Instance.ReflectFixedTerrain(customBinaryWriter);
		}
		using (StreamWriter streamWriter = File.CreateText(text3))
		{
			Session instance = Session.Instance;
			GameImpl instance2 = GameImpl.Instance;
			streamWriter.WriteLine(Translation.Tag + "\tNative");
			bool wantNewLine = false;
			foreach (Community community in instance.CommunityManager.Communities)
			{
				if (community.CommunityName.Type == GangNameType.TranslatedString)
				{
					PrintTSVLine(streamWriter, ref wantNewLine, community.CommunityName.TranslatedStringKey, community.CommunityName.CustomString);
					wantNewLine = true;
				}
			}
			PrintSpacerLine(streamWriter, ref wantNewLine);
			foreach (Town town in instance.CommunityManager.Towns)
			{
				if (town.TownName.Type == TownNameType.TranslatedString)
				{
					PrintTSVLine(streamWriter, ref wantNewLine, town.TownName.TranslatedStringKey, town.TownName.CustomString);
				}
			}
			PrintSpacerLine(streamWriter, ref wantNewLine);
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			foreach (Character character2 in instance.CharacterManager.Characters)
			{
				if (character2.Appearance.Gender == GenderType.Male)
				{
					if (!string.IsNullOrEmpty(character2.FirstName) && !list.Contains(character2.FirstName) && !instance2.IsNameInAnyNamesList(character2.FirstName))
					{
						list.Add(character2.FirstName);
					}
				}
				else if (!string.IsNullOrEmpty(character2.FirstName) && !list2.Contains(character2.FirstName) && !instance2.IsNameInAnyNamesList(character2.FirstName))
				{
					list2.Add(character2.FirstName);
				}
				if (!string.IsNullOrEmpty(character2.Surname) && !list3.Contains(character2.Surname) && !instance2.IsNameInAnyNamesList(character2.Surname))
				{
					list3.Add(character2.Surname);
				}
			}
			list.Sort();
			list2.Sort();
			list3.Sort();
			foreach (string item in list2)
			{
				PrintTSVLine(streamWriter, ref wantNewLine, item, "");
			}
			PrintSpacerLine(streamWriter, ref wantNewLine);
			foreach (string item2 in list)
			{
				PrintTSVLine(streamWriter, ref wantNewLine, item2, "");
			}
			PrintSpacerLine(streamWriter, ref wantNewLine);
			foreach (string item3 in list3)
			{
				PrintTSVLine(streamWriter, ref wantNewLine, item3, "");
			}
			PrintSpacerLine(streamWriter, ref wantNewLine);
		}
		Script.CopyFileBackToUnityFolder(text);
		Script.CopyFileBackToUnityFolder(text2);
		Script.CopyFileBackToUnityFolder(text3);
		currentlyEditingStory.BuildTSVFile();
	}

	private static void PrintTSVLine(StreamWriter stream, ref bool wantNewLine, string key, string native)
	{
		if (native == null)
		{
			native = "";
		}
		native = native.Replace('\t', ' ');
		native = native.Replace('\n', ' ');
		native = native.Replace('"', '\'');
		stream.WriteLine(key + "\t" + native);
	}

	private static void PrintSpacerLine(StreamWriter stream, ref bool wantNewLine)
	{
		if (wantNewLine)
		{
			stream.WriteLine();
			wantNewLine = false;
		}
	}
}
