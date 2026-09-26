using System.Collections.Generic;
using System.Xml.Serialization;

public class SettingsData
{
	public int Version;

	public float SoundFXVolume = 1f;

	public float BackgroundSoundVolume = 1f;

	public float MusicSoundVolume = 1f;

	public float MenuSoundVolume = 1f;

	public Language Language = Language.Invalid;

	public AllowJoinMode AllowJoinMode;

	public bool ShowAllowJoinDialog = true;

	public bool OnlineCoopPlayersCanUseCheats;

	public bool HideVersionText;

	public int MaxPartySize = 2;

	[XmlIgnore]
	public List<PlayerID> BannedPlayers;

	public List<ulong> BannedPlayerIDs;

	public string Greeting;

	public bool VoiceChatEnabled = true;

	public bool PushBtnToTalkEnabled = true;

	public bool MouseLookAcceleration = true;

	public float MouseLookSensitivity = Hud.DefaultMouseLookSensitivity;

	public float MouseTargetSensitivity = Hud.DefaultMouseTargetThreshold;

	public float MouseBodyLocationSensitivity = Hud.DefaultMouseBodyLocationThreshold;

	public List<MappedInput> OverriddenControls;

	public WorkshopSortOrder WorkshopSortOrder = WorkshopSortOrder.Trending;

	public bool SidebarLayoutEnabled;

	public bool PiPBackgroundEnabled;

	public bool HintsEnabled = true;

	public bool HighQualityEffects = true;

	public float GrassDensity = 1f;

	public float TreeQuality = 1f;

	public bool UseMetricWeights;

	public bool UseCelsius;

	public bool MinimapRotationEnabled = true;

	public float MinutesBetweenAutosaves = 10f;

	public NetworkProtocol NetworkProtocol;

	public SettingsData Copy()
	{
		SettingsData settingsData = new SettingsData();
		settingsData.Version = Version;
		settingsData.SoundFXVolume = SoundFXVolume;
		settingsData.BackgroundSoundVolume = BackgroundSoundVolume;
		settingsData.MusicSoundVolume = MusicSoundVolume;
		settingsData.MenuSoundVolume = MenuSoundVolume;
		settingsData.Language = Language;
		settingsData.AllowJoinMode = AllowJoinMode;
		settingsData.ShowAllowJoinDialog = ShowAllowJoinDialog;
		settingsData.OnlineCoopPlayersCanUseCheats = OnlineCoopPlayersCanUseCheats;
		settingsData.HideVersionText = HideVersionText;
		settingsData.MaxPartySize = MaxPartySize;
		settingsData.Greeting = Greeting;
		settingsData.VoiceChatEnabled = VoiceChatEnabled;
		settingsData.PushBtnToTalkEnabled = PushBtnToTalkEnabled;
		settingsData.MouseLookAcceleration = MouseLookAcceleration;
		settingsData.MouseLookSensitivity = MouseLookSensitivity;
		settingsData.MouseTargetSensitivity = MouseTargetSensitivity;
		settingsData.MouseBodyLocationSensitivity = MouseBodyLocationSensitivity;
		settingsData.WorkshopSortOrder = WorkshopSortOrder;
		settingsData.SidebarLayoutEnabled = SidebarLayoutEnabled;
		settingsData.PiPBackgroundEnabled = PiPBackgroundEnabled;
		settingsData.HintsEnabled = HintsEnabled;
		settingsData.HighQualityEffects = HighQualityEffects;
		settingsData.GrassDensity = GrassDensity;
		settingsData.TreeQuality = TreeQuality;
		settingsData.UseMetricWeights = UseMetricWeights;
		settingsData.UseCelsius = UseCelsius;
		settingsData.MinimapRotationEnabled = MinimapRotationEnabled;
		settingsData.MinutesBetweenAutosaves = MinutesBetweenAutosaves;
		settingsData.NetworkProtocol = NetworkProtocol;
		if (BannedPlayers != null)
		{
			settingsData.BannedPlayers = new List<PlayerID>();
			for (int i = 0; i < BannedPlayers.Count; i++)
			{
				settingsData.BannedPlayers.Add(BannedPlayers[i]);
			}
		}
		if (BannedPlayerIDs != null)
		{
			settingsData.BannedPlayerIDs = new List<ulong>();
			for (int j = 0; j < BannedPlayerIDs.Count; j++)
			{
				settingsData.BannedPlayerIDs.Add(BannedPlayerIDs[j]);
			}
		}
		if (OverriddenControls != null)
		{
			settingsData.OverriddenControls = new List<MappedInput>();
			for (int k = 0; k < OverriddenControls.Count; k++)
			{
				settingsData.OverriddenControls.Add(OverriddenControls[k].Copy());
			}
		}
		return settingsData;
	}
}
