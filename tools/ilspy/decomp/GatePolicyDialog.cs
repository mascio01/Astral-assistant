using TMPro;
using UnityEngine.UI;

public class GatePolicyDialog : BaseDialog
{
	public Gate Gate;

	public GatePolicy GatePolicy;

	public bool ApplyToAll = true;

	private TextMeshProUGUI UnityMessage;

	private Toggle UnityOpenableByFriendsExceptWhenInsideAndUnderAttackToggle;

	private Toggle UnityOpenableByFriendsEvenWhenUnderAttackToggle;

	private Toggle UnityOpenableByCommunityExceptWhenInsideAndUnderAttackToggle;

	private Toggle UnityOpenableByCommunityEvenWhenUnderAttackToggle;

	private Toggle UnityNeverOpenableToggle;

	private Toggle UnityCompletelyOpenToggle;

	private Toggle UnityApplyToAllToggle;

	private bool SettingGatePolicy;

	public override void OnActivate()
	{
		base.OnActivate();
		UnityMessage = base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>();
		UnityOpenableByFriendsExceptWhenInsideAndUnderAttackToggle = base.gameObject.FindChild("Panel/OpenableByFriendsExceptWhenInsideAndUnderAttackToggle").GetComponent<Toggle>();
		UnityOpenableByFriendsEvenWhenUnderAttackToggle = base.gameObject.FindChild("Panel/OpenableByFriendsEvenWhenUnderAttackToggle").GetComponent<Toggle>();
		UnityOpenableByCommunityExceptWhenInsideAndUnderAttackToggle = base.gameObject.FindChild("Panel/OpenableByCommunityExceptWhenInsideAndUnderAttackToggle").GetComponent<Toggle>();
		UnityOpenableByCommunityEvenWhenUnderAttackToggle = base.gameObject.FindChild("Panel/OpenableByCommunityEvenWhenUnderAttackToggle").GetComponent<Toggle>();
		UnityNeverOpenableToggle = base.gameObject.FindChild("Panel/NeverOpenableToggle").GetComponent<Toggle>();
		UnityCompletelyOpenToggle = base.gameObject.FindChild("Panel/CompletelyOpenToggle").GetComponent<Toggle>();
		UnityApplyToAllToggle = base.gameObject.FindChild("ApplyToAllToggle").GetComponent<Toggle>();
		SetGatePolicy((Gate != null) ? Gate.GatePolicy : Session.Instance.CommunityManager.PlayerCommunity.DefaultGatePolicy);
		UnityApplyToAllToggle.isOn = ApplyToAll;
	}

	private void SetGatePolicy(GatePolicy gatePolicy)
	{
		GatePolicy = gatePolicy;
		SettingGatePolicy = true;
		UnityOpenableByFriendsExceptWhenInsideAndUnderAttackToggle.isOn = GatePolicy == GatePolicy.OpenableByFriendsExceptWhenInsideAndUnderAttack;
		UnityOpenableByFriendsEvenWhenUnderAttackToggle.isOn = GatePolicy == GatePolicy.OpenableByFriendsEvenWhenUnderAttack;
		UnityOpenableByCommunityExceptWhenInsideAndUnderAttackToggle.isOn = GatePolicy == GatePolicy.OpenableByCommunityExceptWhenInsideAndUnderAttack;
		UnityOpenableByCommunityEvenWhenUnderAttackToggle.isOn = GatePolicy == GatePolicy.OpenableByCommunityEvenWhenUnderAttack;
		UnityNeverOpenableToggle.isOn = GatePolicy == GatePolicy.NeverOpenable;
		UnityCompletelyOpenToggle.isOn = GatePolicy == GatePolicy.CompletelyOpen;
		SettingGatePolicy = false;
	}

	public override void DialogUpdate()
	{
		base.DialogUpdate();
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		_ = InputFunctionManager.Instance;
		if (!OKSelected)
		{
			return;
		}
		if (inputFrame != null)
		{
			if (ApplyToAll)
			{
				inputFrame.AddAction(InputAction.SetAllGatesPolicy(Gate, GatePolicy));
			}
			else
			{
				inputFrame.AddAction(InputAction.SetGatePolicy(Gate, GatePolicy));
			}
		}
		OKSelected = false;
		Finished = true;
	}

	public void OnOpenableByFriendsExceptWhenInsideAndUnderAttackToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.OpenableByFriendsExceptWhenInsideAndUnderAttack);
		}
	}

	public void OnOpenableByFriendsEvenWhenUnderAttackToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.OpenableByFriendsEvenWhenUnderAttack);
		}
	}

	public void OnOpenableByCommunityExceptWhenInsideAndUnderAttackToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.OpenableByCommunityExceptWhenInsideAndUnderAttack);
		}
	}

	public void OnOpenableByCommunityEvenWhenUnderAttackToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.OpenableByCommunityEvenWhenUnderAttack);
		}
	}

	public void OnNeverOpenableToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.NeverOpenable);
		}
	}

	public void OnCompletelyOpenToggled(bool on)
	{
		if (!SettingGatePolicy)
		{
			SetGatePolicy(GatePolicy.CompletelyOpen);
		}
	}

	public void OnApplyToAllToggled(bool on)
	{
		ApplyToAll = on;
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
