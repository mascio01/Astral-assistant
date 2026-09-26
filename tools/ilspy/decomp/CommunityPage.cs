using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunityPage : InfoPage
{
	public struct CharacterScore
	{
		public Character Character;

		public float Score;

		public string Name;

		public CharacterScore(Character character)
		{
			Character = character;
			if (GameImpl.WantNamesReversed(character.Surname, englishOnly: false))
			{
				Name = GameImpl.TranslateSurname(character.Surname, character.Appearance.Gender) + GameImpl.TranslateName(character.FirstName);
			}
			else
			{
				Name = GameImpl.TranslateName(character.FirstName) + " " + GameImpl.TranslateSurname(character.Surname, character.Appearance.Gender);
			}
			switch (Session.Instance.CharactersSortBy)
			{
			case SortCharactersBy.Strength:
				Score = character.GetSkillLevelWithEffects(SkillType.Strength);
				break;
			case SortCharactersBy.HandToHand:
				Score = character.GetSkillLevelWithEffects(SkillType.HandToHand);
				break;
			case SortCharactersBy.Archery:
				Score = character.GetSkillLevelWithEffects(SkillType.Archery);
				break;
			case SortCharactersBy.Firearms:
				Score = character.GetSkillLevelWithEffects(SkillType.Firearms);
				break;
			case SortCharactersBy.Stealth:
				Score = character.GetSkillLevelWithEffects(SkillType.Stealth);
				break;
			case SortCharactersBy.Construction:
				Score = character.GetSkillLevelWithEffects(SkillType.Construction);
				break;
			case SortCharactersBy.Farming:
				Score = character.GetSkillLevelWithEffects(SkillType.Farming);
				break;
			case SortCharactersBy.Medicine:
				Score = character.GetSkillLevelWithEffects(SkillType.Medicine);
				break;
			case SortCharactersBy.Cooking:
				Score = character.GetSkillLevelWithEffects(SkillType.Cooking);
				break;
			case SortCharactersBy.Constitution:
				Score = character.GetSkillLevelWithEffects(SkillType.Constitution);
				break;
			default:
				Score = 0f;
				break;
			}
		}
	}

	public class SortCharactersByScore : IComparer<CharacterScore>
	{
		int IComparer<CharacterScore>.Compare(CharacterScore a, CharacterScore b)
		{
			if (a.Score < b.Score)
			{
				return 1;
			}
			if (a.Score > b.Score)
			{
				return -1;
			}
			return string.CompareOrdinal(a.Name, b.Name);
		}
	}

	public struct EquipmentScore
	{
		public EquipmentPrototype Proto;

		public LiquidPrototype Liquid;

		public float Score;

		public string Name;

		public float Amount;

		public EquipmentScore(EquipmentPrototype proto, LiquidPrototype liquid, float amount)
		{
			Proto = proto;
			Liquid = liquid;
			Name = ((proto != null) ? GameImpl.Translate(proto.NameHash) : ((liquid != null) ? GameImpl.Translate(liquid.NameHash) : ""));
			Amount = amount;
			switch (Session.Instance.InventorySortBy)
			{
			case SortBy.Weight:
				Score = proto?.Weight ?? LbsPerFlOzOfWater;
				break;
			case SortBy.WeightTotal:
				Score = ((proto != null) ? (proto.Weight * amount) : (LbsPerFlOzOfWater * amount));
				break;
			case SortBy.Value:
				Score = proto?.BasePrice ?? liquid?.BasePricePerFlOz ?? 0f;
				break;
			case SortBy.ValueTotal:
				Score = ((proto != null) ? (proto.BasePrice * amount) : ((liquid != null) ? (liquid.BasePricePerFlOz * amount) : 0f));
				break;
			default:
				Score = 0f;
				break;
			}
		}

		public EquipmentScore(EquipmentPrototype proto, LiquidPrototype liquid, float amount, float score)
		{
			Proto = proto;
			Liquid = liquid;
			Name = ((proto != null) ? GameImpl.Translate(proto.NameHash) : ((liquid != null) ? GameImpl.Translate(liquid.NameHash) : ""));
			Amount = amount;
			Score = score;
		}
	}

	public class SortEquipmentByScore : IComparer<EquipmentScore>
	{
		int IComparer<EquipmentScore>.Compare(EquipmentScore a, EquipmentScore b)
		{
			if (a.Score < b.Score)
			{
				return 1;
			}
			if (a.Score > b.Score)
			{
				return -1;
			}
			return string.CompareOrdinal(a.Name, b.Name);
		}
	}

	public class SortEquipmentByCategory : IComparer<EquipmentScore>
	{
		int IComparer<EquipmentScore>.Compare(EquipmentScore a, EquipmentScore b)
		{
			string strA = ((a.Proto != null) ? a.Proto.Category : ((a.Liquid != null) ? a.Liquid.Category : string.Empty));
			string strB = ((b.Proto != null) ? b.Proto.Category : ((b.Liquid != null) ? b.Liquid.Category : string.Empty));
			int num = string.CompareOrdinal(strA, strB);
			if (num == 0)
			{
				num = string.CompareOrdinal(a.Name, b.Name);
			}
			return num;
		}
	}

	public static CommunityPage Instance;

	public Community CurrentCommunity;

	private TextMeshProUGUI UnityDayText;

	public GameObject UnityMembersPanel;

	public GridLayoutGroup UnityMembersContents;

	public CharacterIconBehaviour LastSelectedMember;

	public List<CharacterIconBehaviour> UnityMembers = new List<CharacterIconBehaviour>();

	public GameObject UnityStatsPanel;

	public List<CommunityStatBehaviour> CommunityStatBehaviours = new List<CommunityStatBehaviour>();

	public GameObject UnityEquipmentTotalsPanel;

	public GridLayoutGroup UnityEquipmentTotalsContents;

	public EquipmentTotalBehaviour LastSelectedEquipmentTotal;

	public List<EquipmentTotalBehaviour> EquipmentTotalBehaviours = new List<EquipmentTotalBehaviour>();

	private CloseIconBehaviour[] UnityCharactersSortByButton = new CloseIconBehaviour[12];

	private GameObject[] UnityCharactersSortOrderButton = new GameObject[2];

	private CloseIconBehaviour[] UnityEquipmentSortByButton = new CloseIconBehaviour[6];

	private GameObject[] UnityEquipmentSortOrderButton = new GameObject[2];

	private static int INFOPAGE_Community = StringUtil.JenkinsHash("INFOPAGE_Community");

	private static SortCharactersByScore CharacterSorterByScore = new SortCharactersByScore();

	public static List<CharacterScore> Temp = new List<CharacterScore>();

	private static string BuildListStr = "BuildList";

	private static float LbsPerFlOzOfWater = 0.065f;

	public static SortEquipmentByScore EquipmentSorterByScore = new SortEquipmentByScore();

	public static SortEquipmentByCategory EquipmentSorterByCategory = new SortEquipmentByCategory();

	public static List<EquipmentScore> Temp2 = new List<EquipmentScore>();

	private int CurrentDay = -1;

	private int CurrentHour = -1;

	private int CurrentMinute = -1;

	private static string MembersStr = "Members";

	private static string EquipmentStr = "Equipment";

	private int WantUpdateOnScreen;

	private static string CommunityPagePopulateStr = "CommunityPagePopulate";

	private List<bool> WasOnScreen = new List<bool>();

	public override void OnAwake()
	{
		Instance = this;
		UnityDayText = base.gameObject.FindChild("DayText").GetComponent<TextMeshProUGUI>();
		UnityStatsPanel = base.gameObject.FindChild("StatsPanel");
		UnityMembersPanel = base.gameObject.FindChild("Members");
		UnityMembersContents = base.gameObject.FindChild("Members/Viewport/Content").GetComponent<GridLayoutGroup>();
		UnityEquipmentTotalsPanel = base.gameObject.FindChild("EquipmentTotalsPanel");
		UnityEquipmentTotalsContents = base.gameObject.FindChild("EquipmentTotalsPanel/Viewport/Content").GetComponent<GridLayoutGroup>();
		for (int i = 0; i < UnityCharactersSortByButton.Length; i++)
		{
			CloseIconBehaviour[] unityCharactersSortByButton = UnityCharactersSortByButton;
			int num = i;
			Transform obj = base.transform;
			SortCharactersBy sortCharactersBy = (SortCharactersBy)i;
			unityCharactersSortByButton[num] = obj.Find("MembersPanel/SortBy" + sortCharactersBy.ToString() + "Button").GetComponent<CloseIconBehaviour>();
		}
		for (int j = 0; j < UnityCharactersSortOrderButton.Length; j++)
		{
			GameObject[] unityCharactersSortOrderButton = UnityCharactersSortOrderButton;
			int num2 = j;
			Transform obj2 = base.transform;
			SortOrder sortOrder = (SortOrder)j;
			unityCharactersSortOrderButton[num2] = obj2.Find("MembersPanel/SortOrder" + sortOrder.ToString() + "Button").gameObject;
		}
		for (int k = 0; k < UnityEquipmentSortByButton.Length; k++)
		{
			CloseIconBehaviour[] unityEquipmentSortByButton = UnityEquipmentSortByButton;
			int num3 = k;
			Transform obj3 = base.transform;
			SortBy sortBy = (SortBy)k;
			unityEquipmentSortByButton[num3] = obj3.Find("EquipmentTitlePanel/SortBy" + sortBy.ToString() + "Button").GetComponent<CloseIconBehaviour>();
		}
		for (int l = 0; l < UnityEquipmentSortOrderButton.Length; l++)
		{
			GameObject[] unityEquipmentSortOrderButton = UnityEquipmentSortOrderButton;
			int num4 = l;
			Transform obj4 = base.transform;
			SortOrder sortOrder = (SortOrder)l;
			unityEquipmentSortOrderButton[num4] = obj4.Find("EquipmentTitlePanel/SortOrder" + sortOrder.ToString() + "Button").gameObject;
		}
	}

	public CommunityPage Initialize(Community community)
	{
		CurrentCommunity = community;
		return this;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Community));
	}

	public override bool IsShowingInventoryFor(TileObject obj)
	{
		if (obj != null)
		{
			return obj.GetCommunity() == CurrentCommunity;
		}
		return false;
	}

	public static void BuildSortedListOfCommunityMembers(Community community)
	{
		using (new UnityProfileMarker(BuildListStr))
		{
			Temp.Clear();
			foreach (Character member in community.Members)
			{
				if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
				{
					Temp.Add(new CharacterScore(member));
				}
			}
			if (Session.Instance.CharactersSortBy != SortCharactersBy.TimeJoined)
			{
				Temp.Sort(CharacterSorterByScore);
			}
			if (Session.Instance.CharactersSortOrder == SortOrder.Descending)
			{
				Temp.Reverse();
			}
		}
	}

	private void UpdateTimeDateText()
	{
		Session instance = Session.Instance;
		int day = instance.Day;
		LogEventBehaviour.CalcTimeOfDay(instance.PlayTime, out var hour, out var minute);
		if (CurrentDay != day || CurrentHour != hour || CurrentMinute != minute)
		{
			CurrentDay = day;
			CurrentHour = hour;
			CurrentMinute = minute;
			UnityDayText.SetUnityText(hour + ":" + minute.ToString("00") + " - " + LoadingMenu.BuildSessionDescriptionString(day, (int)instance.DayOfYear));
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateTimeDateText();
		if (CharacterIconBehaviour.CurrentHovered != null)
		{
			LastSelectedMember = CharacterIconBehaviour.CurrentHovered;
		}
		if (EquipmentTotalBehaviour.CurrentHovered != null)
		{
			LastSelectedEquipmentTotal = EquipmentTotalBehaviour.CurrentHovered;
		}
		if (WantUpdateOnScreen > 0)
		{
			WantUpdateOnScreen--;
			if (WantUpdateOnScreen == 0)
			{
				UpdateMembersOnScreen();
				UpdateEquipmnentOnScreen();
			}
		}
	}

	private void UpdateMembersOnScreen()
	{
		using (new UnityProfileMarker(MembersStr))
		{
			RectTransform obj = (RectTransform)UnityMembersContents.transform;
			RectTransform rectTransform = (RectTransform)UnityMembersPanel.transform;
			float y = obj.localPosition.y;
			float num = y + rectTransform.rect.height;
			for (int i = 0; i < UnityMembers.Count; i++)
			{
				RectTransform rectTransform2 = (RectTransform)UnityMembers[i].transform;
				float num2 = 0f - rectTransform2.localPosition.y + rectTransform2.rect.height;
				float num3 = 0f - rectTransform2.localPosition.y;
				UnityMembers[i].SetOnScreen(num2 >= y && num3 <= num);
			}
		}
	}

	public void UpdateEquipmnentOnScreen()
	{
		using (new UnityProfileMarker(EquipmentStr))
		{
			RectTransform obj = (RectTransform)UnityEquipmentTotalsContents.transform;
			RectTransform rectTransform = (RectTransform)UnityEquipmentTotalsPanel.transform;
			float y = obj.localPosition.y;
			float num = y + rectTransform.rect.height;
			for (int i = 0; i < EquipmentTotalBehaviours.Count; i++)
			{
				RectTransform rectTransform2 = (RectTransform)EquipmentTotalBehaviours[i].transform;
				float num2 = 0f - rectTransform2.localPosition.y + rectTransform2.rect.height;
				float num3 = 0f - rectTransform2.localPosition.y;
				EquipmentTotalBehaviours[i].SetOnScreen(num2 >= y && num3 <= num);
			}
		}
	}

	public override void Populate()
	{
		using (new UnityProfileMarker(CommunityPagePopulateStr))
		{
			base.Populate();
			Temp.Clear();
			using (new UnityProfileMarker(MembersStr))
			{
				WasOnScreen.Clear();
				foreach (CharacterIconBehaviour unityMember in UnityMembers)
				{
					WasOnScreen.Add(unityMember.OnScreen);
				}
				BuildSortedListOfCommunityMembers(CurrentCommunity);
				List<CharacterIconBehaviour> list = new List<CharacterIconBehaviour>();
				list.Capacity = Temp.Count;
				int num = 0;
				for (int i = 0; i < Temp.Count; i++)
				{
					Character character = Temp[i].Character;
					CharacterIconBehaviour characterIconBehaviour = FindUnityMember(character);
					if (characterIconBehaviour == null)
					{
						characterIconBehaviour = Object.Instantiate(InfoScreen.CharacterIconWide.GetAsset(), UnityMembersContents.transform, worldPositionStays: false).GetComponent<CharacterIconBehaviour>();
						characterIconBehaviour.Initialize(character, checkOnScreen: true);
					}
					characterIconBehaviour.SortCharactersBy = Session.Instance.CharactersSortBy;
					characterIconBehaviour.gameObject.transform.SetSiblingIndex(num);
					characterIconBehaviour.Dirty = true;
					if (i < WasOnScreen.Count)
					{
						characterIconBehaviour.SetOnScreen(WasOnScreen[i]);
					}
					list.Add(characterIconBehaviour);
					num++;
				}
				for (int num2 = UnityMembersContents.transform.childCount - 1; num2 >= num; num2--)
				{
					GameObject gameObject = UnityMembersContents.transform.GetChild(num2).gameObject;
					if (LastSelectedMember != null && LastSelectedMember.gameObject == gameObject)
					{
						LastSelectedMember = null;
					}
					Object.Destroy(gameObject);
				}
				list.CopyToList(UnityMembers);
				WantUpdateOnScreen = 2;
			}
			UpdateTimeDateText();
			CommunityStatBehaviours.Clear();
			if (Session.Instance.DifficultySettings.GetTotalHunterDensity() > 0f)
			{
				AddStatDisplay(CommunityStatType.Aggro, CurrentCommunity);
			}
			AddStatDisplay(CommunityStatType.CropsPlanted, CurrentCommunity);
			AddStatDisplay(CommunityStatType.StocksNeededForWinter, CurrentCommunity);
			AddStatDisplay(CommunityStatType.Accomodation, CurrentCommunity);
			AddStatDisplay(CommunityStatType.Outhouses, CurrentCommunity);
			for (int j = CommunityStatBehaviours.Count; j < UnityStatsPanel.transform.childCount; j++)
			{
				Object.Destroy(UnityStatsPanel.transform.GetChild(j).gameObject);
			}
			using (new UnityProfileMarker(EquipmentStr))
			{
				WasOnScreen.Clear();
				foreach (EquipmentTotalBehaviour equipmentTotalBehaviour2 in EquipmentTotalBehaviours)
				{
					WasOnScreen.Add(equipmentTotalBehaviour2.OnScreen);
				}
				List<EquipmentTotalBehaviour> list2 = new List<EquipmentTotalBehaviour>();
				list2.Capacity = GameImpl.Instance.CurrentEquipmentPrototypesDeterministic.Count + GameImpl.Instance.CurrentLiquidPrototypesDeterministic.Count;
				using (new UnityProfileMarker(BuildListStr))
				{
					AddAllEquipmentTotals(CurrentCommunity);
					if (Session.Instance.InventorySortBy == SortBy.Type)
					{
						Temp2.Sort(EquipmentSorterByCategory);
					}
					else
					{
						Temp2.Sort(EquipmentSorterByScore);
					}
					if (Session.Instance.InventorySortOrder == SortOrder.Descending)
					{
						Temp2.Reverse();
					}
				}
				for (int k = 0; k < Temp2.Count; k++)
				{
					EquipmentPrototype proto = Temp2[k].Proto;
					LiquidPrototype liquid = Temp2[k].Liquid;
					EquipmentTotalBehaviour equipmentTotalBehaviour = FindEquipmentTotalBehaviour(proto, liquid);
					if (equipmentTotalBehaviour == null)
					{
						equipmentTotalBehaviour = Object.Instantiate(InfoScreen.EquipmentTotal.GetAsset(), UnityEquipmentTotalsContents.transform, worldPositionStays: false).GetComponent<EquipmentTotalBehaviour>();
						equipmentTotalBehaviour.Initialize(proto, liquid, checkOnScreen: true);
					}
					equipmentTotalBehaviour.gameObject.transform.SetSiblingIndex(list2.Count);
					equipmentTotalBehaviour.SetAmount(Temp2[k].Amount);
					if (k < WasOnScreen.Count)
					{
						equipmentTotalBehaviour.SetOnScreen(WasOnScreen[k]);
					}
					list2.Add(equipmentTotalBehaviour);
				}
				for (int num3 = UnityEquipmentTotalsContents.transform.childCount - 1; num3 >= list2.Count; num3--)
				{
					GameObject gameObject2 = UnityEquipmentTotalsContents.transform.GetChild(num3).gameObject;
					if (LastSelectedEquipmentTotal != null && LastSelectedEquipmentTotal.gameObject == gameObject2)
					{
						LastSelectedEquipmentTotal = null;
					}
					Object.Destroy(gameObject2);
				}
				list2.CopyToList(EquipmentTotalBehaviours);
				WantUpdateOnScreen = 2;
			}
			for (int l = 0; l < 12; l++)
			{
				UnityCharactersSortByButton[l].SetSelected(l == (int)Session.Instance.CharactersSortBy);
			}
			for (int m = 0; m < 2; m++)
			{
				UnityCharactersSortOrderButton[m].SetActive(m == (int)Session.Instance.CharactersSortOrder);
			}
			for (int n = 0; n < 6; n++)
			{
				UnityEquipmentSortByButton[n].SetSelected(n == (int)Session.Instance.InventorySortBy);
			}
			for (int num4 = 0; num4 < 2; num4++)
			{
				UnityEquipmentSortOrderButton[num4].SetActive(num4 == (int)Session.Instance.InventorySortOrder);
			}
			Temp.Clear();
			Temp2.Clear();
		}
	}

	public static void AddAllEquipmentTotals(Community community)
	{
		Temp2.Clear();
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			item.Value.Counter = 0;
		}
		foreach (KeyValuePair<string, LiquidPrototype> item2 in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			item2.Value.Counter = 0f;
		}
		foreach (Character member in community.Members)
		{
			if (member.AliveAndNotZombie)
			{
				member.Inventory.AddPrototypeCounters();
			}
		}
		foreach (Prop building in community.Buildings)
		{
			building.Inventory.AddPrototypeCounters();
		}
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId != community.Id || patch.CropsInPatch.Count <= 0)
			{
				continue;
			}
			EquipmentPrototype harvestPrototype = patch.CropsInPatch[0].GetHarvestPrototype();
			EquipmentPrototype harvestSeedsPrototype = patch.CropsInPatch[0].GetHarvestSeedsPrototype();
			if (harvestPrototype != null)
			{
				foreach (PlantableCrop item3 in patch.CropsInPatch)
				{
					if (item3.IsRipe())
					{
						harvestPrototype.Counter += item3.HarvestableAmount;
					}
				}
			}
			if (harvestSeedsPrototype == null)
			{
				continue;
			}
			foreach (PlantableCrop item4 in patch.CropsInPatch)
			{
				if (item4.IsRipe())
				{
					harvestSeedsPrototype.Counter += item4.HarvestableSeedsAmount;
				}
			}
		}
		foreach (KeyValuePair<string, EquipmentPrototype> item5 in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			EquipmentPrototype value = item5.Value;
			if (value.Discovered)
			{
				Temp2.Add(new EquipmentScore(value, null, value.Counter));
			}
		}
		foreach (KeyValuePair<string, LiquidPrototype> item6 in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			LiquidPrototype value2 = item6.Value;
			if (value2.Discovered)
			{
				Temp2.Add(new EquipmentScore(null, value2, value2.Counter));
			}
		}
	}

	private CommunityStatBehaviour GetOrCreateStatDisplay(CommunityStatType communityStatType)
	{
		CommunityStatBehaviour communityStatBehaviour = null;
		for (int i = 0; i < UnityStatsPanel.transform.childCount; i++)
		{
			CommunityStatBehaviour component = UnityStatsPanel.transform.GetChild(i).GetComponent<CommunityStatBehaviour>();
			if (component != null && component.CommunityStatType == communityStatType)
			{
				communityStatBehaviour = component;
			}
		}
		if (communityStatBehaviour == null)
		{
			communityStatBehaviour = Object.Instantiate(InfoScreen.CommunityStatDisplay.GetAsset(), UnityStatsPanel.transform).GetComponent<CommunityStatBehaviour>();
		}
		communityStatBehaviour.transform.SetSiblingIndex(CommunityStatBehaviours.Count);
		CommunityStatBehaviours.Add(communityStatBehaviour);
		return communityStatBehaviour;
	}

	private CommunityStatBehaviour AddStatDisplay(CommunityStatType communityStatType, Community community)
	{
		CommunityStatBehaviour orCreateStatDisplay = GetOrCreateStatDisplay(communityStatType);
		orCreateStatDisplay.Initialize(communityStatType, community);
		return orCreateStatDisplay;
	}

	private CharacterIconBehaviour FindUnityMember(Character character)
	{
		foreach (CharacterIconBehaviour unityMember in UnityMembers)
		{
			if (unityMember.Character == character)
			{
				return unityMember;
			}
		}
		return null;
	}

	private EquipmentTotalBehaviour FindEquipmentTotalBehaviour(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		foreach (EquipmentTotalBehaviour equipmentTotalBehaviour in EquipmentTotalBehaviours)
		{
			if (equipmentTotalBehaviour.Proto == proto && equipmentTotalBehaviour.Liquid == liquid)
			{
				return equipmentTotalBehaviour;
			}
		}
		return null;
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		if (!GameImpl.Instance.IsDialogOpen())
		{
			if (CharacterIconBehaviour.CurrentHovered != null)
			{
				if (instance2.IsJustPressed(InputFunction.InventorySortBy, capture: true, AvailableAction.Caption[(int)(9 + instance.CharactersSortBy)]))
				{
					OnSortByPressed();
				}
				if (instance2.IsJustPressed(InputFunction.InventorySortOrder, capture: true, AvailableAction.Caption[(int)(1 + instance.CharactersSortOrder)]))
				{
					OnSortOrderPressed();
				}
			}
			else if (EquipmentTotalBehaviour.CurrentHovered != null)
			{
				CursorAction cursorAction = (CursorAction)((instance.InventorySortBy == SortBy.Time) ? ((SortBy)10) : (3 + instance.InventorySortBy));
				if (instance2.IsJustPressed(InputFunction.InventorySortBy, capture: true, AvailableAction.Caption[(int)cursorAction]))
				{
					InventoryBehaviour.OnSortByPressedStatic();
				}
				if (instance2.IsJustPressed(InputFunction.InventorySortOrder, capture: true, AvailableAction.Caption[(int)(1 + instance.InventorySortOrder)]))
				{
					InventoryBehaviour.OnSortOrderPressedStatic();
				}
			}
		}
		base.HandleInput(inputFrame);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (UnityMembers.Count > 0 && EquipmentTotalBehaviours.Count > 0 && !GameImpl.Instance.IsDialogOpen())
		{
			float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MenuSwitchInventory);
			if (axis < 0f)
			{
				CharacterIconBehaviour characterIconBehaviour = ((LastSelectedMember != null) ? LastSelectedMember : UnityMembers[0]);
				UnityEventSystem.SetSelectedGameObject(characterIconBehaviour.gameObject);
				SelectableBehaviour.MoveCursorToButton(characterIconBehaviour);
			}
			else if (axis > 0f)
			{
				EquipmentTotalBehaviour equipmentTotalBehaviour = ((LastSelectedEquipmentTotal != null) ? LastSelectedEquipmentTotal : EquipmentTotalBehaviours[0]);
				UnityEventSystem.SetSelectedGameObject(equipmentTotalBehaviour.gameObject);
				SelectableBehaviour.MoveCursorToButton(equipmentTotalBehaviour);
			}
		}
	}

	public void OnSortByPressed()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.CharactersSortBy = (SortCharactersBy)((int)(Session.Instance.CharactersSortBy + 1 + 12) % 12);
		InfoScreen.Instance.WantRepopulate = true;
	}

	public void OnSetSortByPressed(int i)
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.CharactersSortBy = (SortCharactersBy)i;
		InfoScreen.Instance.WantRepopulate = true;
	}

	public void OnSortOrderPressed()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.CharactersSortOrder = (SortOrder)((int)(Session.Instance.CharactersSortOrder + 1 + 2) % 2);
		InfoScreen.Instance.WantRepopulate = true;
	}

	public void OnEquipmentSortByPressed()
	{
		InventoryBehaviour.OnSortByPressedStatic();
	}

	public void OnSetEquipmentSortByPressed(int i)
	{
		InventoryBehaviour.OnSetSortByPressedStatic(i);
	}

	public void OnEquipmentSortOrderPressed()
	{
		InventoryBehaviour.OnSortOrderPressedStatic();
	}

	public void OnMembersScrollRectChanged(Vector2 scrollPos)
	{
		UpdateMembersOnScreen();
	}

	public void OnEquipmentTotalsScrollRectChanged(Vector2 scrollPos)
	{
		UpdateEquipmnentOnScreen();
	}
}
