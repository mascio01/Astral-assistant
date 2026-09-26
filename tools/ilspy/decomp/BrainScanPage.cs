using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrainScanPage : InfoPage
{
	public static BrainScanPage Instance;

	private Character CurrentCharacter;

	private bool FullBrainScan;

	public TileObject FromMapPageObj;

	private GameObject[] UnitySkillsPanel = new GameObject[2];

	private SkillDisplayBehaviour[] SkillDisplays = new SkillDisplayBehaviour[10];

	private GameObject UnityPersonalityPanelContents;

	private GameObject UnityMainPanelContents;

	private List<Character> CharactersOfInterest = new List<Character>();

	private List<GameObject> UnityOpinions = new List<GameObject>();

	private TextMeshProUGUI UnityDiscoverMore;

	private TextMeshProUGUI UnityMoraleTitle;

	public static int INFOPAGE_BrainScan = StringUtil.JenkinsHash("INFOPAGE_BrainScan");

	public static int INFOPAGE_CharacterNotes = StringUtil.JenkinsHash("INFOPAGE_CharacterNotes");

	private float PersonalityMaxWidth;

	private static string UnityIconName = "MemoryPanel/RelationshipDisplay/Image";

	private static string UnityRelationshipTextName = "MemoryPanel/RelationshipDisplay/Text";

	private static string UnityRelationshipApprovalTextName = "MemoryPanel/RelationshipDisplay/Approval";

	private static string UnityRelationshipRespectTextName = "MemoryPanel/RelationshipDisplay/Respect";

	private static string UnityOpinionGraphName = "OpinionGraph";

	private static string UnityMemoryPanelName = "MemoryPanel";

	public static List<SpeechParamResult> FakeParamResults = new List<SpeechParamResult>();

	private static StringBuilder sb = new StringBuilder(100);

	private static int HUD_Approval = StringUtil.JenkinsHash("HUD_Approval");

	private static int HUD_Respect = StringUtil.JenkinsHash("HUD_Respect");

	private static int HUD_Morale = StringUtil.JenkinsHash("HUD_Morale");

	private static int HUD_Close = StringUtil.JenkinsHash("HUD_Close");

	private static int HUD_Today = StringUtil.JenkinsHash("HUD_Today");

	private static int HUD_Yesterday = StringUtil.JenkinsHash("HUD_Yesterday");

	private static int HUD_DaysAgo = StringUtil.JenkinsHash("HUD_DaysAgo");

	public override void OnAwake()
	{
		Instance = this;
		UnityMoraleTitle = base.gameObject.FindChild("MoraleTitle").GetComponent<TextMeshProUGUI>();
		UnityPersonalityPanelContents = base.gameObject.FindChild("PersonalityPanel/Viewport/Content");
		UnityMainPanelContents = base.gameObject.FindChild("ContentsPanel/Viewport/Content");
		for (int i = 0; i < UnitySkillsPanel.Length; i++)
		{
			UnitySkillsPanel[i] = base.gameObject.transform.Find("SkillsPanel/SkillsPanel" + (i + 1)).gameObject;
		}
	}

	public BrainScanPage Initialize(Character character, bool fullBrainScan, TileObject fromMapPageObj)
	{
		CurrentCharacter = character;
		FullBrainScan = fullBrainScan;
		FromMapPageObj = fromMapPageObj;
		UnityPersonalityPanelContents.DeleteAllChildrenImmediately();
		UnityMainPanelContents.DeleteAllChildrenImmediately();
		CharactersOfInterest.Clear();
		UnityOpinions.Clear();
		UnityDiscoverMore = null;
		for (int i = 0; i < UnitySkillsPanel.Length; i++)
		{
			UnitySkillsPanel[i].DeleteAllChildren();
		}
		for (int j = 0; j < 10; j++)
		{
			GameObject gameObject = Object.Instantiate(InfoScreen.SkillDisplay.GetAsset(), UnitySkillsPanel[j / 5].transform);
			SkillDisplays[j] = gameObject.GetComponent<SkillDisplayBehaviour>();
			SkillDisplays[j].Initialize(character, (SkillType)j);
		}
		return this;
	}

	public override void OnDeactivate()
	{
		FromMapPageObj = null;
		base.OnDeactivate();
	}

	public override bool IsShowingBrainScanFor(Character character)
	{
		return CurrentCharacter == character;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate((CurrentCharacter != null && CurrentCharacter.BrainScanned) ? INFOPAGE_BrainScan : INFOPAGE_CharacterNotes));
	}

	public override void Update()
	{
		base.Update();
		float width = ((RectTransform)UnityPersonalityPanelContents.transform).rect.width;
		if (PersonalityMaxWidth != width)
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		for (int i = 0; i < UnityOpinions.Count; i++)
		{
			if (CharactersOfInterest[i] != null)
			{
				RawImage component = UnityOpinions[i].FindChild(UnityIconName).GetComponent<RawImage>();
				if (component.texture != (Texture2D)GameCursor.AlertIcon)
				{
					component.texture = CharactersOfInterest[i].GetIcon(out var mat, out var col, highlighted: false);
					component.material = mat;
					component.color = col;
				}
			}
		}
	}

	public override void Populate()
	{
		bool noStrangers = InfoScreen.AllowViewInfoOnAnyone || Session.Instance.Editor;
		float width = ((RectTransform)UnityPersonalityPanelContents.transform).rect.width;
		if (width == 0f)
		{
			InfoScreen.Instance.WantRepopulate = true;
			return;
		}
		PersonalityMaxWidth = width;
		UnityMoraleTitle.gameObject.SetActive(FullBrainScan);
		if (FullBrainScan)
		{
			sb.Length = 0;
			sb.Append(GameImpl.Translate(HUD_Morale));
			sb.Append(':');
			sb.Append(' ');
			sb.AppendWithoutGarbage(Mathf.RoundToInt(CurrentCharacter.CalcMorale()));
			UnityMoraleTitle.SetUnityTextIfDifferent(sb);
		}
		float num = 0f;
		int num2 = 0;
		int i = 0;
		GameObject gameObject = null;
		foreach (string item in CurrentCharacter.Personality)
		{
			float num3 = Mathf.Lerp(80f, 160f, MathUtil.RandomFloat((float)CurrentCharacter.Id * 100f + (float)num2 * 10f + (float)i));
			bool flag = FullBrainScan || CurrentCharacter.IsPersonalityKnown(item);
			string text = "PERSONALITY_" + item.Replace(" ", "");
			if (CurrentCharacter.Appearance.Gender != GenderType.Female || !GameImpl.Instance.TryTranslate(StringUtil.JenkinsHash(text + "_Female"), out var result, englishOnly: false))
			{
				result = GameImpl.Translate(text);
				result = StringUtil.ApplyFormulae(result, CurrentCharacter);
			}
			Vector2 preferredValues = BaseMenu.PersonalityTag.GetAsset().transform.GetChild(0).GetComponent<TextMeshProUGUI>().GetPreferredValues(result);
			float minHeight = preferredValues.y + 16f;
			float num4;
			if (flag)
			{
				num4 = preferredValues.x + 16f + 16f;
			}
			else
			{
				result = " ";
				num4 = num3 + 16f;
			}
			if (i > 0 && num + num4 >= width)
			{
				for (; i < gameObject.transform.childCount; i++)
				{
					Object.Destroy(gameObject.transform.GetChild(i).gameObject);
				}
				num2++;
				i = 0;
				num = 0f;
			}
			num += num4;
			if (num2 < UnityPersonalityPanelContents.transform.childCount)
			{
				gameObject = UnityPersonalityPanelContents.transform.GetChild(num2).gameObject;
			}
			else
			{
				gameObject = Object.Instantiate(BaseMenu.OptionsRow.GetAsset(), UnityPersonalityPanelContents.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(num2);
			}
			GameObject gameObject2;
			if (i < gameObject.transform.childCount)
			{
				gameObject2 = gameObject.transform.GetChild(i).gameObject;
			}
			else
			{
				gameObject2 = Object.Instantiate(BaseMenu.PersonalityTag.GetAsset(), gameObject.transform, worldPositionStays: false);
				gameObject2.name = item;
			}
			gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetUnityText(result);
			LayoutElement component = gameObject2.GetComponent<LayoutElement>();
			component.minWidth = (flag ? 0f : num3);
			component.minHeight = minHeight;
			i++;
		}
		for (num2++; num2 < UnityPersonalityPanelContents.transform.childCount; num2++)
		{
			Object.Destroy(UnityPersonalityPanelContents.transform.GetChild(num2).gameObject);
		}
		CharactersOfInterest.Clear();
		for (int j = 0; j < CurrentCharacter.Relationships.Count; j++)
		{
			Character relationshipTarget = CurrentCharacter.Relationships[j].RelationshipTarget;
			if (relationshipTarget != null && !relationshipTarget.Deleted)
			{
				CharactersOfInterest.Add(relationshipTarget);
			}
		}
		if (FullBrainScan)
		{
			bool flag2 = false;
			for (int k = 0; k < CurrentCharacter.Memories.Count; k++)
			{
				if (CurrentCharacter.Memories[k].IsTrivial())
				{
					continue;
				}
				Character actor = CurrentCharacter.Memories[k].Actor;
				if (actor != CurrentCharacter && actor != null && !actor.Deleted)
				{
					if (!CharactersOfInterest.Contains(actor))
					{
						CharactersOfInterest.Add(actor);
					}
				}
				else if (CurrentCharacter.Memories[k].MoraleContribution != 0f)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				CharactersOfInterest.Add(null);
			}
		}
		int num5 = 0;
		for (int l = 0; l < CharactersOfInterest.Count; l++)
		{
			Character character = CharactersOfInterest[l];
			GameObject gameObject3;
			if (l < UnityOpinions.Count)
			{
				gameObject3 = UnityOpinions[l];
			}
			else
			{
				gameObject3 = Object.Instantiate(InfoScreen.OpinionDisplay.GetAsset(), UnityMainPanelContents.transform);
				UnityOpinions.Add(gameObject3);
			}
			gameObject3.transform.SetSiblingIndex(num5);
			num5++;
			OpinionGraphBehaviour component2 = gameObject3.FindChild(UnityOpinionGraphName).GetComponent<OpinionGraphBehaviour>();
			RawImage component3 = gameObject3.FindChild(UnityIconName).GetComponent<RawImage>();
			TextMeshProUGUI component4 = gameObject3.FindChild(UnityRelationshipTextName).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI component5 = gameObject3.FindChild(UnityRelationshipApprovalTextName).GetComponent<TextMeshProUGUI>();
			TextMeshProUGUI component6 = gameObject3.FindChild(UnityRelationshipRespectTextName).GetComponent<TextMeshProUGUI>();
			GameObject gameObject4 = gameObject3.FindChild(UnityMemoryPanelName);
			component3.gameObject.SetActive(character != null);
			component4.gameObject.SetActive(character != null);
			component5.gameObject.SetActive(character != null);
			component6.gameObject.SetActive(character != null);
			if (character != null)
			{
				int num6 = Relationship.FindRelationshipIndex(CurrentCharacter, character);
				if (FullBrainScan || (num6 != -1 && CurrentCharacter.Relationships[num6].KnownToPlayer))
				{
					component3.texture = character.GetIcon(out var mat, out var col, highlighted: false);
					component3.color = col;
					component3.material = mat;
					sb.Length = 0;
					if (num6 != -1)
					{
						int hash = 0;
						switch (CurrentCharacter.Relationships[num6].RelationshipType)
						{
						case RelationshipType.ParentOf:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Son : Speech.SPEECH_Daughter);
							break;
						case RelationshipType.ChildOf:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Dad : Speech.SPEECH_Mom);
							break;
						case RelationshipType.SiblingOf:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Brother : Speech.SPEECH_Sister);
							break;
						case RelationshipType.MarriedTo:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Husband : Speech.SPEECH_Wife);
							break;
						case RelationshipType.SleepingWith:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Boyfriend : Speech.SPEECH_Girlfriend);
							break;
						case RelationshipType.FriendsWith:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Friend_Male : Speech.SPEECH_Friend_Female);
							break;
						case RelationshipType.Ex:
							hash = ((character.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Ex_Male : Speech.SPEECH_Ex_Female);
							break;
						case RelationshipType.InLoveWith:
							hash = ((CurrentCharacter.Appearance.Gender == GenderType.Female && GameImpl.HasTranslationForHash(Speech.SPEECH_InLoveWith_Female, englishOnly: false)) ? Speech.SPEECH_InLoveWith_Female : Speech.SPEECH_InLoveWith);
							break;
						case RelationshipType.NotInLoveWith:
							hash = ((CurrentCharacter.Appearance.Gender == GenderType.Female && GameImpl.HasTranslationForHash(Speech.SPEECH_NotInLoveWith_Female, englishOnly: false)) ? Speech.SPEECH_NotInLoveWith_Female : Speech.SPEECH_NotInLoveWith);
							break;
						}
						sb.Append(GameImpl.Translate(hash));
						sb.Append(':');
						sb.Append(' ');
					}
					character.BuildDisplayName(sb, noStrangers, englishOnly: false);
					StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(sb, 0, sb.Length, isProperNoun: false, GameImpl.Instance.Settings.Language);
					component4.SetUnityText(sb);
					bool flag3 = FullBrainScan && num6 != -1 && CurrentCharacter.Relationships[num6].ApprovalContribution != 0f;
					component5.gameObject.SetActive(flag3);
					if (flag3)
					{
						BuildApprovalRespectString(sb, HUD_Approval, CurrentCharacter.Relationships[num6].ApprovalContribution);
						component5.SetUnityText(sb);
					}
					bool flag4 = FullBrainScan && num6 != -1 && CurrentCharacter.Relationships[num6].RespectContribution != 0f;
					component6.gameObject.SetActive(flag4);
					if (flag4)
					{
						BuildApprovalRespectString(sb, HUD_Respect, CurrentCharacter.Relationships[num6].RespectContribution);
						component6.SetUnityText(sb);
					}
				}
				else
				{
					component3.texture = (Texture2D)GameCursor.AlertIcon;
					component3.material = null;
					component4.SetUnityText(string.Empty);
					component5.gameObject.SetActive(value: false);
					component6.gameObject.SetActive(value: false);
				}
			}
			component2.gameObject.SetActive(FullBrainScan && character != null);
			int m = 1;
			if (FullBrainScan)
			{
				if (character != null)
				{
					component2.Init(CurrentCharacter, character, null, null);
				}
				for (int n = 0; n < CurrentCharacter.Memories.Count; n++)
				{
					if (CurrentCharacter.Memories[n].IsTrivial())
					{
						continue;
					}
					Memory memory = CurrentCharacter.Memories[n];
					bool flag5 = memory.Actor == character;
					if (character == null)
					{
						if (memory.MoraleContribution == 0f)
						{
							continue;
						}
						if (memory.Actor != null)
						{
							flag5 |= memory.Actor == CurrentCharacter;
							flag5 |= memory.Actor.Deleted;
						}
					}
					else
					{
						flag5 |= memory.Prototype.RuleSet == MemoryRuleSet.OpinionOfGroup && memory.Object == character.GetCommunity();
					}
					if (flag5)
					{
						GameObject gameObject5 = ((m >= gameObject4.transform.childCount) ? Object.Instantiate(InfoScreen.MemoryDisplay.GetAsset(), gameObject4.transform) : gameObject4.transform.GetChild(m).gameObject);
						gameObject5.transform.SetSiblingIndex(m);
						m++;
						FakeParamResults.Clear();
						AddStringParameter(1, memory.Actor, noStrangers);
						AddStringParameter(2, memory.Object, noStrangers);
						AddStringParameter(3, memory.ThirdParty, noStrangers);
						sb.Length = 0;
						sb.Append(GameImpl.Translate(memory.Prototype.DescriptionHash));
						StringUtil.ApplyFormulae(sb, CurrentCharacter, null, null, FakeParamResults, englishOnly: false);
						for (int num7 = 0; num7 < FakeParamResults.Count; num7++)
						{
							ApplyStringParameter(sb, num7 + 1, FakeParamResults[num7].Str1);
						}
						int num8 = Mathf.FloorToInt((float)(Session.Instance.PlayTime - memory.Time).TotalSeconds / Sun.DayLengthSecs);
						sb.Append(" (" + num8 switch
						{
							1 => GameImpl.Translate(HUD_Yesterday), 
							0 => GameImpl.Translate(HUD_Today), 
							_ => GameImpl.Translate(HUD_DaysAgo).Replace("%1", num8.ToString()), 
						} + ")");
						gameObject5.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetUnityText(sb.ToString());
						gameObject5.transform.GetChild(1).gameObject.SetActive(memory.ApprovalContribution != 0f && character != null);
						if (memory.ApprovalContribution != 0f)
						{
							BuildApprovalRespectString(sb, HUD_Approval, memory.ApprovalContribution);
							gameObject5.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetUnityText(sb);
						}
						gameObject5.transform.GetChild(2).gameObject.SetActive(memory.RespectContribution != 0f && character != null);
						if (memory.RespectContribution != 0f)
						{
							BuildApprovalRespectString(sb, HUD_Respect, memory.RespectContribution);
							gameObject5.transform.GetChild(2).GetComponent<TextMeshProUGUI>().SetUnityText(sb);
						}
						gameObject5.transform.GetChild(3).gameObject.SetActive(memory.MoraleContribution != 0f);
						if (memory.MoraleContribution != 0f)
						{
							BuildApprovalRespectString(sb, HUD_Morale, memory.MoraleContribution);
							gameObject5.transform.GetChild(3).GetComponent<TextMeshProUGUI>().SetUnityText(sb);
						}
					}
				}
			}
			for (; m < gameObject4.transform.childCount; m++)
			{
				Object.Destroy(gameObject4.transform.GetChild(m).gameObject);
			}
		}
		while (UnityOpinions.Count > CharactersOfInterest.Count)
		{
			Object.Destroy(UnityOpinions[UnityOpinions.Count - 1]);
			UnityOpinions.RemoveAt(UnityOpinions.Count - 1);
		}
		if (!FullBrainScan && (CurrentCharacter.Skillset.HasAnyUnknownSkills() || CurrentCharacter.HasAnyUnknownPersonality() || Relationship.HasAnyUnknownRelationships(CurrentCharacter)))
		{
			string text2 = GameImpl.Translate("HUD_DiscoverMore");
			text2 = text2.Replace("%1", CurrentCharacter.GetDisplayNameString());
			text2 = text2.Replace("%2", GameImpl.Translate((CurrentCharacter.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Him : Speech.SPEECH_Her));
			text2 = text2.Replace("%3", GameImpl.Translate((CurrentCharacter.Appearance.Gender == GenderType.Male) ? Speech.SPEECH_Him : Speech.SPEECH_Her));
			text2 = StringUtil.ApplyFormulae(text2, null, CurrentCharacter, CurrentCharacter, CurrentCharacter);
			if (UnityDiscoverMore == null)
			{
				UnityDiscoverMore = Object.Instantiate(InfoScreen.DiscoverMore.GetAsset(), UnityMainPanelContents.transform).GetComponent<TextMeshProUGUI>();
			}
			UnityDiscoverMore.transform.SetSiblingIndex(num5);
			num5++;
			UnityDiscoverMore.SetUnityText(text2);
		}
		else if (UnityDiscoverMore != null)
		{
			Object.Destroy(UnityDiscoverMore);
		}
	}

	private void AddStringParameter(int param, BaseObject obj, bool noStrangers)
	{
		AddStringParameter(CurrentCharacter, param, obj, noStrangers);
	}

	public static void AddStringParameter(Character brainScannedCharacter, int param, BaseObject obj, bool noStrangers)
	{
		int num = 0;
		string empty = string.Empty;
		if (obj != null)
		{
			if (obj == brainScannedCharacter)
			{
				num = ((param == 1) ? Speech.SPEECH_I : Speech.SPEECH_Me);
				empty = GameImpl.Translate(num);
			}
			else
			{
				empty = obj.GetDisplayNameString(noStrangers, englishOnly: false);
				num = obj.GetDisplayNameHashIfGeneric(noStrangers);
			}
		}
		else
		{
			num = Speech.SPEECH_Someone;
			empty = GameImpl.Translate(num);
		}
		SpeechParamResult item = SpeechParamResult.Create(obj, num);
		item.Str1 = empty;
		FakeParamResults.Add(item);
	}

	public static void ApplyStringParameter(StringBuilder sb, int param, string str)
	{
		char c = (char)(48 + param);
		for (int i = 0; i < sb.Length - 1; i++)
		{
			if (sb[i] == '%' && sb[i + 1] == c)
			{
				sb.Remove(i, 2);
				sb.Insert(i, str);
				StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(sb, i, sb.Length, isProperNoun: true, GameImpl.Instance.Settings.Language);
				break;
			}
		}
	}

	private void BuildApprovalRespectString(StringBuilder sb, int titleHash, float approval)
	{
		sb.Length = 0;
		sb.Append(GameImpl.Translate(titleHash));
		sb.Append(':');
		sb.Append(' ');
		if (MathUtil.IsNaNorInfinity(approval))
		{
			sb.Append(approval.ToString());
			return;
		}
		if (approval > 0f)
		{
			sb.Append('+');
		}
		sb.AppendWithoutGarbage(approval, 1);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (FromMapPageObj != null)
		{
			if (instance.IsJustPressed(InputFunction.MapBrainScan, capture: true, HUD_Close))
			{
				TileObject fromMapPageObj = FromMapPageObj;
				InfoScreen.Instance.OnDeactivate();
				InfoScreen.Instance.Activate(fromMapPageObj, typeof(MapPage));
			}
		}
		else if (instance.IsJustPressed(InputFunction.BrainScan, capture: true, HUD_Close))
		{
			InfoScreen.Instance.CloseInfoScreen();
		}
		base.PreHandleInput(inputFrame);
	}
}
