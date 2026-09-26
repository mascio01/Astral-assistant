using UnityEngine;

public class DemoTimeoutMenu : BaseMenu
{
	public bool WantQuit;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GameObject obj = base.gameObject.FindChild("Panel/BuyButton");
		base.gameObject.FindChild("Panel/MainMenuButton").SetActive(value: true);
		obj.SetActive(value: false);
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantQuit = false;
	}

	public void OnQuitGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.OnlineParty.LeaveLobby();
		WantQuit = true;
		WantFadeOut = true;
	}

	public void OnBuyGame()
	{
	}
}
