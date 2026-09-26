using System;

public struct Hint : IReflectable
{
	public TimeSpan LastShown;

	public bool Performed;

	public bool Dirty;

	public string Msg;

	private static TimeSpan HintMsgTimeout = TimeSpan.FromSeconds(HudBehaviour.MsgTimeout);

	public static int INPUT_Aim = StringUtil.JenkinsHash("INPUT_Aim");

	public static int INPUT_Attack = StringUtil.JenkinsHash("INPUT_Attack");

	public static int HUD_Equip = StringUtil.JenkinsHash("HUD_Equip");

	public TimeSpan TimeSinceLastShown => Session.Instance.PlayTime - LastShown;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref LastShown);
		reflector.Add(ref Performed);
	}

	public bool CanShowHint()
	{
		return CanShowHint(30f);
	}

	public bool CanShowHint(float timeBetweenHints)
	{
		if (Performed)
		{
			return false;
		}
		if (LastShown != TimeSpan.Zero && TimeSinceLastShown < TimeSpan.FromSeconds(timeBetweenHints))
		{
			return false;
		}
		if (HudBehaviour.Instance.IsShowingStatusBarMsg() || HintManager.Instance.StartedAnyThisFrame)
		{
			return false;
		}
		if (NotificationManager.Instance.HasAnyNotificationsDisplayedOrQueued())
		{
			return false;
		}
		if (InfoScreen.Instance.Active)
		{
			return false;
		}
		if (Session.Instance.PlayTime < TimeSpan.FromSeconds(4.0))
		{
			return false;
		}
		return true;
	}

	public void StartShowing(string msg)
	{
		LastShown = Session.Instance.PlayTime;
		Msg = StringUtil.ApplyFormulae(msg, null, Hud.Instance.LocalControlledCharacter);
		Dirty = true;
		HintManager.Instance.StartedAnyThisFrame = true;
	}

	public void ShowAndMarkPerformed(string msg)
	{
		StartShowing(msg);
		MarkPerformed();
		if (!GameImpl.Instance.Settings.HideVersionText)
		{
			HudBehaviour.Instance.SetStatusBarMsg(Msg);
		}
	}

	public void MarkPerformed()
	{
		if (!Performed)
		{
			Performed = true;
			Dirty = true;
		}
	}

	public void Hide()
	{
		LastShown = TimeSpan.Zero;
	}

	public void Update()
	{
		Update(null, HintType.Invalid);
	}

	public void Update(Character controlledCharacter, HintType hintType)
	{
		if (Performed || !(LastShown != TimeSpan.Zero) || GameImpl.Instance.Settings.HideVersionText)
		{
			return;
		}
		switch (hintType)
		{
		case HintType.AimHint:
		case HintType.EquipWeaponHint:
			if ((TimeSinceLastShown > HintMsgTimeout && controlledCharacter.UnderAttackRefCount == 0) || controlledCharacter.AIOverridesControl() != AIOverridesControlReason.None)
			{
				return;
			}
			break;
		case HintType.UseStealth:
			if (TimeSinceLastShown > HintMsgTimeout || controlledCharacter.UnderAttackRefCount > 0)
			{
				return;
			}
			break;
		case HintType.OpenBuildingInventory:
			if (TimeSinceLastShown > HintMsgTimeout || controlledCharacter.InsideBuilding == null)
			{
				return;
			}
			break;
		default:
			if (TimeSinceLastShown > HintMsgTimeout)
			{
				return;
			}
			break;
		}
		HudBehaviour.Instance.SetStatusBarMsgThisFrame(Msg);
		switch (hintType)
		{
		case HintType.EquipWeaponHint:
			if (!Hud.Instance.ParryPrompt && !Hud.Instance.ParryPromptSuccess && controlledCharacter.ConsciousAndNotZombie)
			{
				Hud.Instance.ShowBigHintPrompt(InputFunction.SelectAction, controlledCharacter, HUD_Equip);
			}
			break;
		case HintType.AimHint:
			if (!Hud.Instance.ParryPrompt && !Hud.Instance.ParryPromptSuccess && controlledCharacter.ConsciousAndNotZombie)
			{
				Hud.Instance.ShowBigHintPrompt(Hud.Instance.LocalPressingAim ? InputFunction.Attack : InputFunction.Aim, controlledCharacter, Hud.Instance.LocalPressingAim ? INPUT_Attack : INPUT_Aim);
			}
			break;
		}
	}
}
