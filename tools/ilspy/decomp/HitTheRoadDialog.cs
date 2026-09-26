using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HitTheRoadDialog : BaseDialog
{
	public delegate void ConfirmFunction(InputFrame inputFrame);

	public ConfirmFunction OnConfirm;

	public RoadDestinationTraits Traits;

	private TextMeshProUGUI UnityMessageText;

	private TextMeshProUGUI UnityRouteText;

	private TextMeshProUGUI UnityAreaText;

	private TextMeshProUGUI UnitySkillCapBonusText;

	private GameObject UnityTraitsPanel;

	public override void OnActivate()
	{
		base.OnActivate();
		UnityMessageText = base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>();
		UnityRouteText = base.gameObject.FindChild("RouteText").GetComponent<TextMeshProUGUI>();
		UnityAreaText = base.gameObject.FindChild("Panel/AreaTypeText").GetComponent<TextMeshProUGUI>();
		UnitySkillCapBonusText = base.gameObject.FindChild("Panel/SkillCapBonusPanel/Text").GetComponent<TextMeshProUGUI>();
		UnityTraitsPanel = base.gameObject.FindChild("Panel");
		UnityMessageText.SetUnityText(GameImpl.Translate("HUD_AreYouSureHitTheRoad"));
		UnityRouteText.SetUnityText(GameImpl.Translate("HUD_WhatsOnRoute"));
		UnitySkillCapBonusText.SetUnityText(GameImpl.Translate("HUD_SkillCapBonus") + ":");
		GameObject gameObject = UnitySkillCapBonusText.transform.parent.gameObject;
		for (int i = 1; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).gameObject.SetActive(i <= Traits.SkillCapBonus);
		}
		switch (Traits.Type)
		{
		case RoadDestinationType.Random:
			UnityAreaText.gameObject.SetActive(value: false);
			SetupTrait("UrbanizationPanel", "HUD_Urbanization", Traits.Urbanized);
			SetupTrait("PopulationPanel", "HUD_Population", Traits.Populated);
			SetupTrait("InfectionPanel", "HUD_Infection", Traits.Infected);
			SetupTrait("InvisibleStrainPanel", "HUD_InvisibleStrain", Traits.InvisibleStrain);
			break;
		case RoadDestinationType.KeepCurrentSettings:
		case RoadDestinationType.HarderSettings:
			UnityAreaText.gameObject.SetActive(value: true);
			UnityAreaText.SetUnityText((Traits.Type == RoadDestinationType.KeepCurrentSettings) ? GameImpl.Translate("HUD_SimilarArea") : GameImpl.Translate("HUD_HarderArea"));
			HideTrait("UrbanizationPanel");
			HideTrait("PopulationPanel");
			HideTrait("InfectionPanel");
			HideTrait("InvisibleStrainPanel");
			break;
		}
	}

	private void SetupTrait(string path, string titleTag, float amount)
	{
		GameObject obj = base.gameObject.FindChild("Panel/" + path);
		obj.SetActive(value: true);
		string key = ((amount > 0.8f) ? "HUD_VeryHigh" : ((amount > 0.6f) ? "HUD_High" : ((amount > 0.4f) ? "HUD_Medium" : ((amount > 0.2f) ? "HUD_Low" : ((!(amount > 0f)) ? "HUD_None" : "HUD_VeryLow")))));
		obj.FindChild("Text").GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate(titleTag) + ": " + GameImpl.Translate(key));
		obj.FindChild("Icon").GetComponent<RawImage>().color = HudBehaviour.GetTraitColor(amount);
	}

	private void HideTrait(string path)
	{
		base.gameObject.FindChild("Panel/" + path).SetActive(value: false);
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (OKSelected)
		{
			Finished = true;
			OnConfirm(inputFrame);
			OKSelected = false;
		}
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
