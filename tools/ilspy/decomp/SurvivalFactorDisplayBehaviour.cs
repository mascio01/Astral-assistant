using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalFactorDisplayBehaviour : MonoBehaviour
{
	public int NameHash;

	public float CriticalTime;

	public float DeathTime = 1f;

	public float Notches;

	public float Amount;

	public bool WantPowerNapButton;

	public bool WantSnap;

	private TextMeshProUGUI UnityText;

	private ProgressBarBehaviour UnityProgressBar;

	private Button UnityButton;

	private bool WantPowerNap;

	public void Awake()
	{
		UnityText = base.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		if (NameHash != 0)
		{
			UnityText.SetUnityText(GameImpl.Translate(NameHash));
		}
		UnityProgressBar = base.transform.Find("ProgressBar").GetComponent<ProgressBarBehaviour>();
		UnityButton = base.transform.Find("Button").GetComponent<Button>();
		UnityButton.gameObject.SetActive(value: false);
		Update();
	}

	public void Initialize(int nameHash, float criticalTime, float deathTime, float notches, bool wantPowerNapButton)
	{
		NameHash = nameHash;
		CriticalTime = criticalTime;
		DeathTime = deathTime;
		Notches = notches;
		WantPowerNapButton = wantPowerNapButton;
		WantSnap = true;
		if (UnityText != null)
		{
			UnityText.SetUnityText(GameImpl.Translate(NameHash));
		}
	}

	public void SetAmount(float amount)
	{
		Amount = amount;
	}

	public void Update()
	{
		UnityProgressBar.SetNotches(Notches / DeathTime);
		UnityProgressBar.SetValue(WantSnap ? (Amount / DeathTime) : MathUtil.Delt(UnityProgressBar.GetValue(), Amount / DeathTime, Time.unscaledDeltaTime));
		UnityProgressBar.SetFlashing(Amount >= CriticalTime);
		WantSnap = false;
		if (WantPowerNapButton)
		{
			CharacterPage characterPage = InfoScreen.Instance.GetCurrentPage() as CharacterPage;
			if (characterPage != null)
			{
				CursorActionDisabledReason cursorActionDisabledReason = GameCursor.CanPowerNap(characterPage.CurrentCharacter);
				UnityButton.gameObject.SetActive(cursorActionDisabledReason != CursorActionDisabledReason.Disabled);
				UnityButton.interactable = cursorActionDisabledReason == CursorActionDisabledReason.Enabled;
			}
		}
	}

	public void OnPowerNap()
	{
		CharacterPage characterPage = InfoScreen.Instance.GetCurrentPage() as CharacterPage;
		if (characterPage != null)
		{
			WantPowerNap = true;
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
			SoundManager.PlayMenuSound((characterPage.CurrentCharacter.Appearance.Gender == GenderType.Male) ? SoundManager.YawnMaleSound : SoundManager.YawnFemaleSound);
		}
	}

	public void HandleInput(InputFrame inputFrame)
	{
		if (WantPowerNap)
		{
			CharacterPage characterPage = InfoScreen.Instance.GetCurrentPage() as CharacterPage;
			if (characterPage != null)
			{
				inputFrame.AddAction(InputAction.PowerNap(characterPage.CurrentCharacter));
			}
			WantPowerNap = false;
		}
	}
}
