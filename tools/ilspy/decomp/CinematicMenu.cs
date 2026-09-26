using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CinematicMenu : BaseMenu
{
	public CinematicState NextCinematicState;

	public TileObject ExfilVehicle;

	public List<Character> ExfilCharacters = new List<Character>();

	public string CompletionMessage;

	public bool ShowText;

	public float Timer;

	private static float SlideTime = 8f;

	private TextMeshProUGUI UnityTitle;

	private TextMeshProUGUI UnityText1;

	private TextMeshProUGUI UnityText2;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		UnityTitle = base.gameObject.FindChild("Panel/Text0").GetComponent<TextMeshProUGUI>();
		UnityText1 = base.gameObject.FindChild("Panel/Text1").GetComponent<TextMeshProUGUI>();
		UnityText2 = base.gameObject.FindChild("Panel/Text2").GetComponent<TextMeshProUGUI>();
	}

	public bool PopNextCinematicState()
	{
		switch (NextCinematicState)
		{
		case CinematicState.ExfilCharacter:
			if (ExfilCharacters.Count > 0)
			{
				Character character3 = ExfilCharacters[0];
				ExfilCharacters.RemoveAt(0);
				NextCinematicState = ((ExfilCharacters.Count > 0) ? CinematicState.ExfilCharacter : CinematicState.ExfilHelicopter);
				TreelinePose = PortraitPose.Treeline;
				PortraitPose = PortraitPose.InHelicopter;
				PortraitSubject = character3;
				string str6 = GameImpl.Translate("CINEMATIC_Subject").Replace("%1", character3.GetDisplayNameString());
				UnityTitle.SetUnityText(StringUtil.ApplyFormulae(str6, character3));
				UnityText1.SetUnityText(GameImpl.Translate("CINEMATIC_Stats").Replace("%1", Mathf.FloorToInt(character3.Appearance.Age).ToString()).Replace("%2", GameImpl.Translate((character3.Appearance.Gender == GenderType.Male) ? "MENU_Male" : "MENU_Female")));
				UnityText2.SetUnityText(GameImpl.Translate("CINEMATIC_Status"));
				WantFadeOut = false;
				ShowText = true;
				Timer = 0f;
				Ready = false;
				return true;
			}
			WantFadeOut = true;
			return false;
		case CinematicState.ExfilHelicopter:
			NextCinematicState = CinematicState.None;
			TreelinePose = PortraitPose.Treeline;
			PortraitPose = PortraitPose.Helicopter;
			PortraitSubject = null;
			WantFadeOut = false;
			ShowText = false;
			Timer = 0f;
			Ready = false;
			return true;
		case CinematicState.RipVehicle:
			NextCinematicState = ((ExfilCharacters.Count > 0) ? CinematicState.RipCharacter : CinematicState.None);
			PortraitPose = PortraitPose.MilitaryVehicle;
			TreelinePose = PortraitPose.TreelineHigh;
			PortraitSubject = ExfilVehicle;
			WantFadeOut = false;
			ShowText = false;
			Timer = 0f;
			Ready = false;
			return true;
		case CinematicState.RipCharacter:
			if (ExfilCharacters.Count > 0)
			{
				Character character2 = ExfilCharacters[0];
				ExfilCharacters.RemoveAt(0);
				NextCinematicState = ((ExfilCharacters.Count > 0) ? CinematicState.RipCharacter : CinematicState.None);
				if (!(character2.EquippedItem is Weapon))
				{
					character2.EquippedItem = character2.Inventory.GetBestWeaponForIdle(character2, rangedOnly: false, allowMolotovs: false);
				}
				if (character2.EquippedItem != null && character2.EquippedItem.GetEquippedAnim() == EquippedAnim.Pistol)
				{
					PortraitPose = PortraitPose.PistolAiming;
				}
				else if (character2.EquippedItem != null && character2.EquippedItem.GetEquippedAnim() == EquippedAnim.Rifle)
				{
					PortraitPose = PortraitPose.ShotgunAiming;
				}
				else if (character2.EquippedItem != null && character2.EquippedItem.GetEquippedAnim() == EquippedAnim.Bow)
				{
					PortraitPose = PortraitPose.BowAiming;
				}
				else if (character2.EquippedItem != null && character2.EquippedItem.GetEquippedAnim() == EquippedAnim.OneHanded)
				{
					PortraitPose = (MathUtil.NonDeterministicRand.RandomChoice(0.5f) ? PortraitPose.OneHandedAttack1 : PortraitPose.OneHandedAttack2);
				}
				else
				{
					PortraitPose = PortraitPose.CrouchingWithKnife;
				}
				PortraitSubject = character2;
				TreelinePose = PortraitPose.TreelineHigh;
				string str4 = GameImpl.Translate("CINEMATIC_Subject").Replace("%1", character2.GetDisplayNameString());
				UnityTitle.SetUnityText(StringUtil.ApplyFormulae(str4, character2));
				UnityText1.SetUnityText(GameImpl.Translate("CINEMATIC_Stats").Replace("%1", Mathf.FloorToInt(character2.Appearance.Age).ToString()).Replace("%2", GameImpl.Translate((character2.Appearance.Gender == GenderType.Male) ? "MENU_Male" : "MENU_Female")));
				string str5 = string.Empty;
				switch ((character2.Id + StringUtil.JenkinsHash(character2.FirstName) + StringUtil.JenkinsHash(character2.Surname)) % 3)
				{
				case 0:
					str5 = GameImpl.Translate("CINEMATIC_Status_Missing");
					break;
				case 1:
					str5 = GameImpl.Translate("CINEMATIC_Status_Captured");
					break;
				case 2:
					str5 = GameImpl.Translate("CINEMATIC_Status_Dead");
					break;
				}
				UnityText2.SetUnityText(StringUtil.ApplyFormulae(str5, null, character2));
				WantFadeOut = false;
				ShowText = true;
				Timer = 0f;
				Ready = false;
				return true;
			}
			WantFadeOut = true;
			return false;
		case CinematicState.OutbreakHelicopter:
			NextCinematicState = ((ExfilCharacters.Count > 0) ? CinematicState.OutbreakCharacter : CinematicState.None);
			TreelinePose = PortraitPose.Treeline;
			PortraitPose = PortraitPose.BigHelicopter;
			PortraitSubject = null;
			WantFadeOut = false;
			ShowText = false;
			Timer = 0f;
			Ready = false;
			return true;
		case CinematicState.OutbreakCharacter:
			if (ExfilCharacters.Count > 0)
			{
				Character character = ExfilCharacters[0];
				ExfilCharacters.RemoveAt(0);
				NextCinematicState = ((ExfilCharacters.Count > 0) ? CinematicState.OutbreakCharacter : CinematicState.None);
				TreelinePose = PortraitPose.Treeline;
				PortraitPose = PortraitPose.InHelicopter;
				PortraitSubject = character;
				string str = GameImpl.Translate("CINEMATIC_Subject").Replace("%1", character.GetDisplayNameString());
				UnityTitle.SetUnityText(StringUtil.ApplyFormulae(str, character, character));
				string str2 = GameImpl.Translate("CINEMATIC_Stats").Replace("%1", Mathf.FloorToInt(character.Appearance.Age).ToString()).Replace("%2", GameImpl.Translate((character.Appearance.Gender == GenderType.Male) ? "MENU_Male" : "MENU_Female"));
				UnityText1.SetUnityText(StringUtil.ApplyFormulae(str2, character, character, character));
				string str3 = GameImpl.Translate("CINEMATIC_Status_Outbreak");
				UnityText2.SetUnityText(StringUtil.ApplyFormulae(str3, character, character));
				WantFadeOut = false;
				ShowText = true;
				Timer = 0f;
				Ready = false;
				return true;
			}
			WantFadeOut = true;
			return false;
		default:
			return false;
		}
	}

	public override void OnActivate()
	{
		PortraitPose = PortraitPose.None;
		base.OnActivate();
		PopNextCinematicState();
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		Timer += Time.unscaledDeltaTime;
		bool activeSelf = UnityTitle.gameObject.activeSelf;
		bool activeSelf2 = UnityText1.gameObject.activeSelf;
		bool activeSelf3 = UnityText2.gameObject.activeSelf;
		UnityTitle.gameObject.SetActive(ShowText && Timer >= 1f);
		UnityText1.gameObject.SetActive(ShowText && Timer >= 2f);
		UnityText2.gameObject.SetActive(ShowText && Timer >= 3f);
		if ((!activeSelf && UnityTitle.gameObject.activeSelf) || (!activeSelf2 && UnityText1.gameObject.activeSelf) || (!activeSelf3 && UnityText2.gameObject.activeSelf))
		{
			SoundManager.PlayMenuSoundFromList(SoundManager.NotificationSounds);
		}
		if (Timer >= SlideTime)
		{
			WantFadeOut = true;
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			Timer = SlideTime;
		}
	}
}
