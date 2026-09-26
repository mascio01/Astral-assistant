using UnityEngine;

public class SoundDebugMenu : DebugMenu
{
	public SoundDebugMenu()
		: base(GameImpl.Translate("DEBUG_SoundDebug"))
	{
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_BirdSongDebug"), typeof(BirdSongDebugMenu)));
		Items.Add(new DebugMenuItemOpenPage(GameImpl.Translate("DEBUG_MusicDebug"), typeof(MusicDebugMenu)));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_FootstepSoundDist"), 0f, 16f, () => Character.FootstepSoundDist, delegate(float v)
		{
			Character.FootstepSoundDist = v;
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				AudioSource footstepAudioSource = character.GetPredictedOrElseThisCharacter().Unity.FootstepAudioSource;
				if (footstepAudioSource != null)
				{
					footstepAudioSource.maxDistance = Character.FootstepSoundDist;
				}
			}
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_FootstepSoundVolume"), 0f, 2f, () => Character.FootstepSoundVolume, delegate(float v)
		{
			Character.FootstepSoundVolume = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_VoiceSoundVolume"), 0f, 2f, () => Character.VoiceSoundVolume, delegate(float v)
		{
			Character.VoiceSoundVolume = v;
			PlayerPrefs.SetFloat("VoiceSoundVolume", v);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WindInLeavesVolume"), 0f, 2f, () => Weather.WindInLeavesVolume, delegate(float v)
		{
			Weather.WindInLeavesVolume = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WindInGrassVolume"), 0f, 2f, () => Weather.WindInGrassVolume, delegate(float v)
		{
			Weather.WindInGrassVolume = v;
		}));
	}
}
