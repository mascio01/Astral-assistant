using TMPro;
using UnityEngine;

public class GameOverMenu : BaseMenu
{
	private bool WantLatestSave;

	public override bool GetWantLatestSave()
	{
		return WantLatestSave;
	}

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		TextMeshProUGUI component = base.gameObject.FindChild("Text").GetComponent<TextMeshProUGUI>();
		GameObject gameObject = base.gameObject.FindChild("Panel/ReloadButton");
		GameObject gameObject2 = base.gameObject.FindChild("Panel/MainMenuButton");
		gameObject.SetActive(SaveGameManager.Instance.HasAnySaveGames && !OnlineParty.Instance.IsInMultiplayerGameAsFollower());
		UnityEventSystem.firstSelectedGameObject = (gameObject.activeSelf ? gameObject : gameObject2);
		Character leader = Session.Instance.CommunityManager.PlayerCommunity.Leader;
		if (leader != null)
		{
			string str = GameImpl.Translate("MENU_YouHaveDied").Replace("%1", leader.GetDisplayNameString());
			str = StringUtil.ApplyFormulae(str, null, leader);
			component.SetUnityText(str);
		}
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantLatestSave = false;
	}

	public void OnReloadLastSavegame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantLatestSave = true;
		WantFadeOut = true;
	}

	public void OnQuitGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.OnlineParty.LeaveLobby();
		WantFadeOut = true;
	}
}
