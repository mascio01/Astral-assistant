using TMPro;
using UnityEngine;

public class GameCompleteMenu : BaseMenu
{
	public bool WantQuit;

	public bool WantContinue;

	public bool ShowContinueButton;

	public string CompletionMessage;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GameObject gameObject = base.gameObject.FindChild("Panel/ContinueButton");
		GameObject gameObject2 = base.gameObject.FindChild("Panel/MainMenuButton");
		gameObject.SetActive(ShowContinueButton);
		UnityEventSystem.firstSelectedGameObject = (gameObject.activeSelf ? gameObject : gameObject2);
		base.gameObject.FindChild("Text").GetComponent<TextMeshProUGUI>().SetUnityText(CompletionMessage);
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantQuit = false;
		WantContinue = false;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (WantContinue && inputFrame != null)
		{
			inputFrame.AddAction(new InputAction(InputActionType.ContinueGame));
			WantFadeOut = true;
			WantContinue = false;
		}
		base.HandleInputImpl(inputFrame);
	}

	public void OnContinuePlaying()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantContinue = true;
	}

	public void OnQuitGame()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.OnlineParty.LeaveLobby();
		WantQuit = true;
		WantFadeOut = true;
	}
}
