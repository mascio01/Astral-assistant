using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunityStatBehaviour : MonoBehaviour
{
	public CommunityStatType CommunityStatType;

	public int NameHash;

	public float Current;

	public float Desired;

	public float Total;

	private int NumDecimalPlaces;

	private bool Show;

	private Texture2D Tex;

	private PropPrototype IconPropType;

	private RawImage UnityIcon;

	private TextMeshProUGUI UnityText;

	public ProgressBarBehaviour UnityProgressBar;

	private bool Dirty = true;

	private static int HUD_CommunityAggro = StringUtil.JenkinsHash("HUD_CommunityAggro");

	private static int HUD_VisitedTowns = StringUtil.JenkinsHash("HUD_VisitedTowns");

	private static int HUD_CommunitySize = StringUtil.JenkinsHash("HUD_CommunitySize");

	private static int HUD_TallStructures = StringUtil.JenkinsHash("HUD_TallStructures");

	private static int HUD_CropsPlanted = StringUtil.JenkinsHash("HUD_CropsPlanted");

	private static int HUD_StocksNeededForWinter = StringUtil.JenkinsHash("HUD_StocksNeededForWinter");

	private static int HUD_Accommodation = StringUtil.JenkinsHash("HUD_Accommodation");

	private static int HUD_Outhouses = StringUtil.JenkinsHash("HUD_Outhouses");

	public void Awake()
	{
		UnityIcon = base.transform.Find("Icon").GetComponent<RawImage>();
		UnityText = base.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		UnityProgressBar = base.transform.Find("ProgressBar").GetComponent<ProgressBarBehaviour>();
		Dirty = true;
		Populate();
	}

	public void Update()
	{
		if (Dirty)
		{
			Populate();
		}
		UpdateIcon();
	}

	private void UpdateIcon()
	{
		if (UnityIcon != null)
		{
			if (Tex != null)
			{
				UnityIcon.texture = Tex;
				UnityIcon.material = Hud.OutlineBlackMat;
			}
			else
			{
				UnityIcon.texture = IconGenerator.Instance.GetIconForObjectType(IconPropType, out var mat, highlighted: false);
				UnityIcon.material = mat;
			}
		}
	}

	public void Initialize(CommunityStatType communityStatType, Community community)
	{
		CommunityStatType = communityStatType;
		switch (CommunityStatType)
		{
		case CommunityStatType.Accomodation:
		{
			community.LacksSpace(out var current2, out var desired2);
			Initialize(HUD_Accommodation, current2, desired2, (float)desired2 * 2f, 0, PropPrototype.Shack, desired2 > 0);
			break;
		}
		case CommunityStatType.Outhouses:
		{
			community.IsUnhygienic(out var current, out var desired);
			Initialize(HUD_Outhouses, current, desired, (float)desired * 2f, 0, PropPrototype.Outhouse, desired > 0);
			break;
		}
		case CommunityStatType.CropsPlanted:
		{
			int livingNonZombieMemberCount2 = community.GetLivingNonZombieMemberCount();
			float num3 = community.GetPlantedNutritionAmount() / Sun.DayLengthSecs / (float)livingNonZombieMemberCount2;
			Initialize(HUD_CropsPlanted, num3, Community.RecommendedPlantedNutritionDays, Community.RecommendedPlantedNutritionDays * 2f, 1, (EquipmentPrototype.WateringCan != null) ? EquipmentPrototype.WateringCan.Tex : null, num3 > 0f);
			break;
		}
		case CommunityStatType.StocksNeededForWinter:
		{
			int livingNonZombieMemberCount = community.GetLivingNonZombieMemberCount();
			float num = community.GetHarvestedNutritionAmount() / Sun.DayLengthSecs / (float)livingNonZombieMemberCount;
			float num2 = Weather.CalcNutritionNeededToStoreForWinterPerPerson(Session.Instance.DayOfYear, rampUpOverPlantingSeason: false) / Sun.DayLengthSecs;
			Initialize(HUD_StocksNeededForWinter, num, num2, num2 * 2f, 1, (EquipmentPrototype.Carrot != null) ? EquipmentPrototype.Carrot.Tex : null, num > 0f);
			break;
		}
		case CommunityStatType.Aggro:
		{
			float communityAggro = Session.Instance.CommunityManager.GetCommunityAggro();
			Initialize(HUD_CommunityAggro, communityAggro, CommunityManager.MaxAggro, CommunityManager.MaxAggro, 0, PropPrototype.WatchTower, communityAggro > 0f);
			break;
		}
		}
	}

	public void Initialize(int nameHash, float current, float desired, float total, int numDecimalPlaces, PropPrototype iconPropType, bool show)
	{
		NameHash = nameHash;
		Current = current;
		Desired = desired;
		Total = total;
		NumDecimalPlaces = numDecimalPlaces;
		IconPropType = iconPropType;
		Show = show;
		Dirty = true;
		Populate();
		UpdateIcon();
	}

	public void Initialize(int nameHash, float current, float desired, float total, int numDecimalPlaces, Texture2D tex, bool show)
	{
		NameHash = nameHash;
		Current = current;
		Desired = desired;
		Total = total;
		NumDecimalPlaces = numDecimalPlaces;
		Tex = tex;
		Show = show;
		Dirty = true;
		Populate();
		UpdateIcon();
	}

	public void Populate()
	{
		if (!(UnityText != null) || NameHash == 0)
		{
			return;
		}
		string text = GameImpl.Translate(NameHash);
		if (CommunityStatType == CommunityStatType.Aggro)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			int visitedTownCount = communityManager.GetVisitedTownCount();
			int livingNonZombieMemberCount = communityManager.PlayerCommunity.GetLivingNonZombieMemberCount();
			int numTallStructures = communityManager.PlayerCommunity.GetNumTallStructures();
			if (visitedTownCount > 0 || livingNonZombieMemberCount > 1 || numTallStructures > 0)
			{
				text += " (";
				bool flag = false;
				if (visitedTownCount > 0)
				{
					if (flag)
					{
						text += ", ";
						flag = false;
					}
					text += GameImpl.Translate(HUD_VisitedTowns).Replace("%1", visitedTownCount.ToString("F" + NumDecimalPlaces));
					flag = true;
				}
				if (livingNonZombieMemberCount > 1)
				{
					if (flag)
					{
						text += ", ";
						flag = false;
					}
					text += GameImpl.Translate(HUD_CommunitySize).Replace("%2", livingNonZombieMemberCount.ToString("F" + NumDecimalPlaces));
					flag = true;
				}
				if (numTallStructures > 0)
				{
					if (flag)
					{
						text += ", ";
						flag = false;
					}
					text += GameImpl.Translate(HUD_TallStructures).Replace("%3", numTallStructures.ToString("F" + NumDecimalPlaces));
					flag = true;
				}
				text += ")";
			}
		}
		else
		{
			text = text.Replace("%1", Current.ToString("F" + NumDecimalPlaces));
			text = text.Replace("%2", Desired.ToString("F" + NumDecimalPlaces));
		}
		DummyBaseObject param = new DummyBaseObject(GenderType.Count, StringUtil.IsPlural(Current, GameImpl.Instance.Settings.Language));
		DummyBaseObject param2 = new DummyBaseObject(GenderType.Count, StringUtil.IsPlural(Desired, GameImpl.Instance.Settings.Language));
		text = StringUtil.ApplyFormulae(text, null, param, param2);
		UnityText.SetUnityText(text);
		UnityProgressBar.SetValue(Current / Total);
		UnityProgressBar.SetNotches(Desired / Total);
		Dirty = false;
	}
}
