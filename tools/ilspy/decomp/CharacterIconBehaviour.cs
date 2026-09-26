using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterIconBehaviour : Selectable
{
	private struct EquipmentScore
	{
		public Equipment Item;

		public float Score;
	}

	public class SortEquipmentByScoreDescending : IComparer<EquipmentScore>
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
			if (a.Item.Id < b.Item.Id)
			{
				return 1;
			}
			if (a.Item.Id > b.Item.Id)
			{
				return -1;
			}
			return 0;
		}
	}

	private struct StatusIcon : IComparable<StatusIcon>
	{
		public Texture2D Icon;

		public Color Col;

		public float Priority;

		public StatusIcon(Texture2D icon, Color col, float priority)
		{
			Icon = icon;
			Col = col;
			Priority = priority;
		}

		public int CompareTo(StatusIcon other)
		{
			if (Priority < other.Priority)
			{
				return -1;
			}
			if (Priority > other.Priority)
			{
				return 1;
			}
			return 0;
		}
	}

	public static CharacterIconBehaviour CurrentHovered;

	private RawImage UnityIcon;

	private RawImage UnityBackground;

	private RawImage UnityBackground2;

	private RawImage UnityBackground3;

	private RawImage UnityRoleIcon;

	private RawImage UnityStatusIcon;

	private RawImage UnityFocusedArrow;

	private RawImage UnityFocusedArrowLarge;

	private RawImage[] UnityStarIcons;

	private RawImage[] UnityRoleIcons;

	private TextMeshProUGUI UnityShortcuts;

	private TextMeshProUGUI UnityName;

	private TextMeshProUGUI UnityRoles;

	private GameObject UnitySkillPanel;

	private GameObject UnityRolePanel;

	private GameObject UnityEquipmentPanel;

	private SnoreBehaviour UnitySnoreBehaviour;

	private Material UnityIconMat;

	public float HoveredTransition;

	public Character Character;

	public bool IsHudIcon;

	public bool IsWide;

	public bool IsSkillsChangeIcon;

	public bool ForceSelected;

	public SortCharactersBy SortCharactersBy;

	public bool Dirty;

	private static StringBuilder sb = new StringBuilder(100);

	private static List<SkillType> BestSkills = new List<SkillType>();

	private static List<EquipmentScore> BestEquipment = new List<EquipmentScore>();

	private static SortEquipmentByScoreDescending EquipmentSorter = new SortEquipmentByScoreDescending();

	private static bool[] DisplayedRoles = new bool[18];

	private static string StatusIconStr = "StatusIcon";

	private static string RoleIconStr = "RoleIcon";

	private static string RoleIconsStr = "RoleIcons";

	private static string IsWideStr = "IsWide";

	private static string ShortcutsStr = "Shortcuts";

	private static string StarsStr = "Stars";

	private static string NameStr = "Name";

	private static string EquipmentStr = "Equipment";

	public bool OnScreen;

	private static List<StatusIcon> StatusIcons = new List<StatusIcon>();

	protected override void Awake()
	{
		base.Awake();
		UnityIcon = base.transform.Find("Background/Icon").gameObject.GetComponent<RawImage>();
		if (Application.isPlaying)
		{
			UnityIcon.material = new Material(UnityIcon.material);
		}
		UnityBackground = base.transform.Find("Background").gameObject.GetComponent<RawImage>();
		UnityStatusIcon = base.transform.Find("Background/Icon/StatusIcon").gameObject.GetComponent<RawImage>();
		UnityFocusedArrow = base.transform.Find("Background/Icon/FocusedArrow").gameObject.GetComponent<RawImage>();
		UnityFocusedArrow.gameObject.SetActive(value: false);
		UnityFocusedArrowLarge = base.transform.Find("Background/Icon/FocusedArrowLarge").gameObject.GetComponent<RawImage>();
		UnityFocusedArrowLarge.gameObject.SetActive(value: false);
		if (IsWide)
		{
			UnityStarIcons = new RawImage[5];
			for (int i = 1; i <= 5; i++)
			{
				UnityStarIcons[i - 1] = base.transform.Find("Background/SkillPanel/Star" + i).gameObject.GetComponent<RawImage>();
			}
			UnityRoleIcons = new RawImage[10];
			for (int j = 1; j <= 10; j++)
			{
				UnityRoleIcons[j - 1] = base.transform.Find("Background/RolePanel/Role" + j).gameObject.GetComponent<RawImage>();
			}
			UnityName = base.transform.Find("Background/Name").gameObject.GetComponent<TextMeshProUGUI>();
			UnityRoles = base.transform.Find("Background/RolePanel/Text").gameObject.GetComponent<TextMeshProUGUI>();
			UnitySkillPanel = base.transform.Find("Background/SkillPanel").gameObject;
			UnityRolePanel = base.transform.Find("Background/RolePanel").gameObject;
			UnityEquipmentPanel = base.transform.Find("Background/EquipmentPanel").gameObject;
			UnityBackground2 = base.transform.Find("Background/Background2").gameObject.GetComponent<RawImage>();
			UnityBackground3 = base.transform.Find("Background/Background3").gameObject.GetComponent<RawImage>();
		}
		else
		{
			UnityRoleIcon = base.transform.Find("Background/Icon/RoleIcon").gameObject.GetComponent<RawImage>();
			UnityShortcuts = base.transform.Find("Background/Icon/Shortcuts").gameObject.GetComponent<TextMeshProUGUI>();
		}
		UnitySnoreBehaviour = GetComponent<SnoreBehaviour>();
		if (OnScreen)
		{
			UnityBackground.gameObject.SetActive(value: true);
			UnitySnoreBehaviour.enabled = true;
		}
	}

	protected override void OnDestroy()
	{
		if (UnityIcon != null && Application.isPlaying)
		{
			UnityEngine.Object.Destroy(UnityIcon.material);
		}
		base.OnDestroy();
	}

	public void Initialize(Character character, bool checkOnScreen = false)
	{
		Character = character;
		Dirty = true;
		if (checkOnScreen)
		{
			base.enabled = false;
			return;
		}
		if (UnityBackground == null)
		{
			UnityBackground = base.transform.Find("Background").gameObject.GetComponent<RawImage>();
		}
		UnityBackground.gameObject.SetActive(value: true);
		OnScreen = true;
	}

	public void SetCharacter(Character character)
	{
		Character = character;
		Dirty = true;
	}

	private void Populate()
	{
		if (Character == null)
		{
			return;
		}
		if (IsWide)
		{
			using (new UnityProfileMarker(IsWideStr))
			{
				bool flag = SortCharactersBy != SortCharactersBy.TimeJoined && SortCharactersBy != SortCharactersBy.Name;
				if (UnitySkillPanel != null && UnitySkillPanel.activeSelf != flag)
				{
					UnitySkillPanel.SetActive(flag);
				}
				if (UnityEquipmentPanel != null && UnityEquipmentPanel.activeSelf == flag)
				{
					UnityEquipmentPanel.SetActive(!flag);
				}
			}
		}
		if (UnityRoleIcons != null && UnityRolePanel.activeSelf)
		{
			using (new UnityProfileMarker(RoleIconsStr))
			{
				for (int i = 0; i < 10; i++)
				{
					if (i < Character.Roles.Count)
					{
						UnityRoleIcons[i].gameObject.SetActive(value: true);
						UnityRoleIcons[i].texture = RoleDisplayBehaviour.GetRoleDisplayIcon(Character, Character.Roles[i]);
						UnityRoleIcons[i].color = Character.Roles[i].GetRoleIconCol();
					}
					else
					{
						UnityRoleIcons[i].gameObject.SetActive(value: false);
					}
				}
				sb.Length = 0;
				for (int j = 0; j < Character.Roles.Count; j++)
				{
					if (!DisplayedRoles[(int)Character.Roles[j].Role])
					{
						if (j > 0)
						{
							sb.Append(',');
							sb.Append(' ');
						}
						RoleDisplayBehaviour.GetRoleDisplayText(Character, Character.Roles[j], RoleDisplayTextMode.Short, sb);
						DisplayedRoles[(int)Character.Roles[j].Role] = true;
					}
				}
				Array.Clear(DisplayedRoles, 0, DisplayedRoles.Length);
				UnityRoles.SetUnityTextIfDifferent(sb);
			}
		}
		if (UnityName != null)
		{
			using (new UnityProfileMarker(NameStr))
			{
				sb.Length = 0;
				PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
				if (localPlayerRecord != null)
				{
					foreach (ShortcutGroup shortcutGroup in localPlayerRecord.ShortcutGroups)
					{
						if (shortcutGroup.ControlledCharacter == Character || shortcutGroup.SelectedCharacters.Contains(Character))
						{
							sb.AppendButtonPromptString((InputFunction)(67 + shortcutGroup.Group));
							sb.Append(' ');
						}
					}
				}
				Character.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
				UnityName.SetUnityTextIfDifferent(sb);
			}
		}
		if (UnityShortcuts != null)
		{
			using (new UnityProfileMarker(ShortcutsStr))
			{
				sb.Length = 0;
				PlayerRecord localPlayerRecord2 = Session.Instance.GetLocalPlayerRecord();
				if (localPlayerRecord2 != null)
				{
					foreach (ShortcutGroup shortcutGroup2 in localPlayerRecord2.ShortcutGroups)
					{
						if (shortcutGroup2.ControlledCharacter == Character || shortcutGroup2.SelectedCharacters.Contains(Character))
						{
							sb.AppendButtonPromptString((InputFunction)(67 + shortcutGroup2.Group));
						}
					}
				}
				UnityShortcuts.SetUnityTextIfDifferent(sb);
			}
		}
		if (UnityStarIcons != null && UnitySkillPanel.activeSelf)
		{
			using (new UnityProfileMarker(StarsStr))
			{
				BestSkills.Clear();
				int num = 0;
				int num2 = 0;
				int negativeEffects = 0;
				int positiveEffects = 0;
				if (Session.Instance.CharactersSortBy == SortCharactersBy.TimeJoined || Session.Instance.CharactersSortBy == SortCharactersBy.Name)
				{
					for (int k = 0; k < 10; k++)
					{
						SkillType skillType = (SkillType)k;
						int level = Character.Skillset.GetLevel(skillType);
						int cap = Character.Skillset.GetCap(skillType);
						SkillDisplayBehaviour.CalcSkillEffects(Character, skillType, out var positiveEffects2, out var negativeEffects2);
						int num3 = level + positiveEffects2 + negativeEffects2;
						int num4 = num + positiveEffects2 + negativeEffects2;
						if (num3 > num4 || (num3 == num4 && cap > num2))
						{
							BestSkills.Clear();
							BestSkills.Add(skillType);
							num = level;
							num2 = cap;
							positiveEffects = positiveEffects2;
							negativeEffects = negativeEffects2;
						}
						else if (num3 == num4 && cap == num2 && positiveEffects2 == positiveEffects && negativeEffects2 == negativeEffects)
						{
							BestSkills.Add(skillType);
							num = level;
							num2 = cap;
							positiveEffects = positiveEffects2;
							negativeEffects = negativeEffects2;
						}
					}
				}
				else
				{
					SkillType skillType2 = (SkillType)(Session.Instance.CharactersSortBy - 2);
					BestSkills.Add(skillType2);
					num = Character.Skillset.GetLevel(skillType2);
					num2 = Character.Skillset.GetCap(skillType2);
					SkillDisplayBehaviour.CalcSkillEffects(Character, skillType2, out positiveEffects, out negativeEffects);
				}
				for (int l = 0; l < 5; l++)
				{
					UnityStarIcons[l].color = SkillDisplayBehaviour.GetSkillStarCol(l + 1, num, positiveEffects, negativeEffects);
					UnityStarIcons[l].gameObject.SetActive(l < num2);
				}
			}
		}
		if (!(UnityEquipmentPanel != null) || !UnityEquipmentPanel.activeSelf)
		{
			return;
		}
		using (new UnityProfileMarker(EquipmentStr))
		{
			BestEquipment.Clear();
			for (int m = 0; m < Character.Inventory.Count; m++)
			{
				Equipment item = Character.Inventory.GetItem(m);
				if (item.GetClothingType() == ClothingType.Invalid || item.GetClothingType() == ClothingType.Backpack)
				{
					float num5 = item.GetBasePrice() * (float)item.GetAmount();
					int num6 = FindMatchingEquipment(item);
					if (num6 != -1)
					{
						EquipmentScore value = BestEquipment[num6];
						value.Score += num5;
						BestEquipment[num6] = value;
					}
					else
					{
						EquipmentScore item2 = new EquipmentScore
						{
							Item = item,
							Score = num5
						};
						BestEquipment.Add(item2);
					}
				}
			}
			BestEquipment.Sort(EquipmentSorter);
			for (int n = 0; n < 6; n++)
			{
				RawImage component = UnityEquipmentPanel.transform.GetChild(n).GetComponent<RawImage>();
				component.gameObject.SetActive(n < BestEquipment.Count);
				if (n >= BestEquipment.Count)
				{
					continue;
				}
				LiquidPrototype liquidContentsType = BestEquipment[n].Item.GetLiquidContentsType();
				if (liquidContentsType != null && !(BestEquipment[n].Item is WateringCan))
				{
					if (liquidContentsType.Tex != null && liquidContentsType.Tex.GetAsset() != null)
					{
						component.texture = (Texture2D)liquidContentsType.Tex;
						component.material = null;
						component.color = liquidContentsType.Col;
					}
				}
				else
				{
					component.texture = BestEquipment[n].Item.GetIconWithoutGenerating(out var col);
					component.color = col;
				}
			}
		}
	}

	private static int FindMatchingEquipment(Equipment item)
	{
		if (item.CanBeCombined())
		{
			return -1;
		}
		for (int i = 0; i < BestEquipment.Count; i++)
		{
			if (item.GetLiquidContentsAmount() > 0f && !(item is WateringCan))
			{
				if (BestEquipment[i].Item.GetLiquidContentsType() == item.GetLiquidContentsType())
				{
					return i;
				}
			}
			else if (BestEquipment[i].Item.GetPrototype() == item.GetPrototype())
			{
				return i;
			}
		}
		return -1;
	}

	public void SetOnScreen(bool onScreen)
	{
		if (OnScreen != onScreen)
		{
			OnScreen = onScreen;
			base.enabled = onScreen;
			if (UnityBackground != null)
			{
				UnityBackground.gameObject.SetActive(OnScreen);
				UnitySnoreBehaviour.enabled = OnScreen;
			}
			if (OnScreen)
			{
				Update();
			}
		}
	}

	public void Update()
	{
		if (Dirty)
		{
			Populate();
			Dirty = false;
		}
		if (UnityFocusedArrow != null && UnityFocusedArrowLarge != null && Hud.Instance != null)
		{
			Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
			Color color = ((Character != null && Character.IsInPlayerCommunity()) ? GameTerrain.MinimapSettings.FriendCol : GameTerrain.MinimapSettings.AllyCol);
			UnityFocusedArrowLarge.gameObject.SetActive(Character != null && Character == localControlledCharacter);
			UnityFocusedArrowLarge.color = color;
			UnityFocusedArrow.gameObject.SetActive(Character != null && Character != localControlledCharacter && Character.IsInSameSquad(localControlledCharacter));
			UnityFocusedArrow.color = color;
		}
		HoveredTransition = Mathf.Clamp01(HoveredTransition + (((ForceSelected || base.currentSelectionState == SelectionState.Highlighted || base.currentSelectionState == SelectionState.Selected || base.currentSelectionState == SelectionState.Pressed) && !Session.Instance.IsDebugMenuOpen()) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f);
		float num = Mathf.SmoothStep(0f, 1f, HoveredTransition);
		UnityIcon.transform.localScale = Vector3.one * (1f + num * 0.1f);
		if (IsWide)
		{
			((RectTransform)UnityIcon.transform).anchoredPosition = new Vector2(54f, Mathf.Lerp(-54f, -50f, num));
		}
		else
		{
			((RectTransform)UnityIcon.transform).anchoredPosition = new Vector2(0f, 0f);
		}
		if (UnityIconMat == null)
		{
			UnityIconMat = UnityIcon.materialForRendering;
		}
		UnityIconMat.SetColor(ShaderHash._OutlineColor, Color.Lerp(Color.black, Color.red, HoveredTransition));
		UnityIconMat.color = ((base.interactable || IsHudIcon) ? Color.white : Color.gray);
		if (Character == null)
		{
			return;
		}
		Material mat;
		Color col;
		Texture2D icon = Character.GetIcon(out mat, out col, highlighted: false);
		UnityIcon.enabled = icon != null;
		if (UnityIcon.texture != icon)
		{
			UnityIcon.color = col;
			UnityIcon.texture = icon;
			if (UnityIcon.texture != null)
			{
				UnityIcon.SetMaterialDirty();
				UnityIconMat = UnityIcon.materialForRendering;
			}
		}
		if (IsSkillsChangeIcon)
		{
			UnityBackground.material = null;
			if (UnityBackground2 != null)
			{
				UnityBackground2.color = new Color32(byte.MaxValue, 197, 131, byte.MaxValue);
			}
			if (UnityBackground3 != null)
			{
				UnityBackground3.color = new Color32(byte.MaxValue, 197, 131, byte.MaxValue);
			}
			if (UnityStatusIcon != null)
			{
				UnityStatusIcon.gameObject.SetActive(value: false);
			}
			if (UnityRoleIcon != null)
			{
				UnityRoleIcon.gameObject.SetActive(value: false);
			}
			return;
		}
		if (Hud.Instance != null && Character == (IsWide ? Hud.Instance.LocalControlledCharacter : Hud.Instance.BuildingSelectedInhabitant))
		{
			UnityBackground.material = InfoScreen.HalftoneEquipped;
			if (UnityBackground2 != null)
			{
				UnityBackground2.color = new Color32(135, 184, 198, byte.MaxValue);
			}
			if (UnityBackground3 != null)
			{
				UnityBackground3.color = new Color32(135, 184, 198, byte.MaxValue);
			}
		}
		else if (Character.NonDeterministicSelected)
		{
			UnityBackground.material = InfoScreen.HalftoneSelected;
			if (UnityBackground2 != null)
			{
				UnityBackground2.color = new Color32(138, 181, 148, byte.MaxValue);
			}
			if (UnityBackground3 != null)
			{
				UnityBackground3.color = new Color32(138, 181, 148, byte.MaxValue);
			}
		}
		else
		{
			UnityBackground.material = null;
			if (UnityBackground2 != null)
			{
				UnityBackground2.color = new Color32(byte.MaxValue, 197, 131, byte.MaxValue);
			}
			if (UnityBackground3 != null)
			{
				UnityBackground3.color = new Color32(byte.MaxValue, 197, 131, byte.MaxValue);
			}
		}
		if (UnitySnoreBehaviour != null)
		{
			UnitySnoreBehaviour.Snoring = Character.IsSleepingOrUnconscious();
		}
		if (UnityStatusIcon != null)
		{
			using (new UnityProfileMarker(StatusIconStr))
			{
				UnityStatusIcon.texture = GetCharacterStatusIcon(Character, out var col2);
				UnityStatusIcon.color = col2;
				UnityStatusIcon.gameObject.SetActive(UnityStatusIcon.texture != null && base.interactable);
			}
		}
		if (!(UnityRoleIcon != null))
		{
			return;
		}
		using (new UnityProfileMarker(RoleIconStr))
		{
			RoleInfo topRunningRoleInfo = Character.GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: true);
			UnityRoleIcon.texture = RoleDisplayBehaviour.GetRoleDisplayIcon(Character, topRunningRoleInfo);
			UnityRoleIcon.color = topRunningRoleInfo.GetRoleIconCol();
			UnityRoleIcon.gameObject.SetActive(UnityRoleIcon.texture != null && base.interactable);
		}
	}

	private static Texture2D GetCharacterStatusIcon(Character character, out Color col)
	{
		StatusIcons.Clear();
		if (character.HasUnbandagedInjury(0))
		{
			StatusIcons.Add(new StatusIcon(GameCursor.BloodLossIcon, Color.red, float.MaxValue));
		}
		if (character.GetInfectionProgression() > 0f)
		{
			col = GameTerrain.MinimapSettings.GetInfectionCol(character.GetWorstInfectionTypeInProgression());
			StatusIcons.Add(new StatusIcon(GameCursor.BiohazardIcon, col, character.GetInfectionProgression()));
		}
		if (character.GetThirst() >= Character.ThirstCriticalTime)
		{
			float priority = (character.GetThirst() - Character.ThirstCriticalTime) / (Character.ThirstDieTime - Character.ThirstCriticalTime);
			StatusIcons.Add(new StatusIcon(GameCursor.CursorWaterBottle, Color.white, priority));
		}
		if (character.GetHunger() >= Character.HungerCriticalTime)
		{
			float priority2 = (character.GetHunger() - Character.HungerCriticalTime) / (Character.HungerDieTime - Character.HungerCriticalTime);
			StatusIcons.Add(new StatusIcon(GameCursor.CursorEat, Color.white, priority2));
		}
		if (character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime)
		{
			float priority3 = (character.GetSleepDeprivation() - Character.SleepDeprivationCriticalTime) / (Character.SleepDeprivationDieTime - Character.SleepDeprivationCriticalTime);
			StatusIcons.Add(new StatusIcon(GameCursor.CursorSleep, Color.white, priority3));
		}
		if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			float priority4 = (character.GetBodyTemperatureInCelsius() - Character.BodyTemperatureInCelsiusModerateHypothermia) / (Character.BodyTemperatureInCelsiusUnconscious - Character.BodyTemperatureInCelsiusModerateHypothermia);
			StatusIcons.Add(new StatusIcon(GameCursor.FrostIcon, Color.white, priority4));
		}
		if (character.IsTooDepressedToFollowOrders())
		{
			StatusIcons.Add(new StatusIcon(GameCursor.DepressedIcon, Color.white, 0.5f));
		}
		if (character.DownTime > 0f)
		{
			StatusIcons.Add(new StatusIcon(GameCursor.DownTimeIcon, Color.white, 0f));
		}
		StatusIcons.Sort();
		col = Color.white;
		if (StatusIcons.Count > 0)
		{
			col = StatusIcons[StatusIcons.Count - 1].Col;
			return StatusIcons[StatusIcons.Count - 1].Icon;
		}
		return null;
	}
}
