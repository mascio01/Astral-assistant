using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDisplayBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private Character Character;

	public SkillType SkillType;

	private TextMeshProUGUI UnitySkillText;

	private RawImage[] UnityStar = new RawImage[5];

	private ProgressBarBehaviour UnityProgressBar;

	public static List<SkillEffect> _skillEffects = new List<SkillEffect>();

	public static Color StarColActive = new Color32(111, 147, byte.MaxValue, byte.MaxValue);

	public static Color StarColInactive = Color.gray;

	public static Color StarColInjured = new Color(0.5f, 0f, 0f);

	public static Color StarColBonus = new Color(0f, 0.5f, 0f);

	public static SkillDisplayBehaviour Hovered;

	public static GameObject HoveredGameObject;

	public void Awake()
	{
		UnitySkillText = base.transform.Find("Text").GetComponent<TextMeshProUGUI>();
		UnitySkillText.SetUnityText(GameImpl.Translate(Skillset.GetSkillNameHash(SkillType)));
		UnityProgressBar = base.transform.Find("ProgressBar").GetComponent<ProgressBarBehaviour>();
		for (int i = 0; i < UnityStar.Length; i++)
		{
			UnityStar[i] = base.transform.Find("StarsPanel/Star" + (i + 1)).GetComponent<RawImage>();
		}
		Update();
	}

	public void Initialize(Character character, SkillType skillType)
	{
		Character = character;
		SkillType = skillType;
		Update();
	}

	public static Color GetSkillStarCol(int level, int levelWithoutEffects, int positiveEffects, int negativeEffects)
	{
		Color result = StarColActive;
		if (level > levelWithoutEffects)
		{
			result = StarColBonus;
		}
		if (level > levelWithoutEffects + positiveEffects)
		{
			result = StarColInactive;
		}
		else if (level > levelWithoutEffects + positiveEffects + negativeEffects)
		{
			result = StarColInjured;
		}
		return result;
	}

	public static void CalcSkillEffects(Character character, SkillType skillType, out int positiveEffects, out int negativeEffects)
	{
		positiveEffects = 0;
		negativeEffects = 0;
		character.GetSkillEffects(_skillEffects);
		for (int i = 0; i < _skillEffects.Count; i++)
		{
			if (_skillEffects[i].SkillType == skillType)
			{
				if (_skillEffects[i].Effect > 0)
				{
					positiveEffects += _skillEffects[i].Effect;
				}
				else if (_skillEffects[i].Effect < 0)
				{
					negativeEffects += _skillEffects[i].Effect;
				}
			}
		}
	}

	public void Update()
	{
		if (Character == null || UnityProgressBar == null)
		{
			return;
		}
		if (Character.Skillset.IsSkillKnown(SkillType) || InfoScreen.AllowViewInfoOnAnyone || Session.Instance.Editor)
		{
			int cap = Character.Skillset.GetCap(SkillType);
			int level = Character.Skillset.GetLevel(SkillType);
			CalcSkillEffects(Character, SkillType, out var positiveEffects, out var negativeEffects);
			for (int i = 0; i < 5; i++)
			{
				Color skillStarCol = GetSkillStarCol(i + 1, level, positiveEffects, negativeEffects);
				UnityStar[i].gameObject.SetActive(i < cap);
				UnityStar[i].color = skillStarCol;
				UnityStar[i].texture = (Texture2D)GameCursor.SkillIcon;
			}
			float progression = Character.Skillset.GetProgression(SkillType);
			float value = ((level < cap) ? ((progression - Skillset.ProgressionToLevel[level]) / (Skillset.ProgressionToLevel[level + 1] - Skillset.ProgressionToLevel[level])) : 1f);
			bool flag = Character.IsControllableByPlayer();
			flag &= level < cap || SkillType == SkillType.Constitution || SkillType == SkillType.Strength;
			UnityProgressBar.gameObject.SetActive(flag);
			UnityProgressBar.SetValue(value);
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				UnityStar[j].gameObject.SetActive(value: true);
				UnityStar[j].texture = (Texture2D)GameCursor.SkillUnknownIcon;
				UnityStar[j].color = Color.white;
			}
			UnityProgressBar.gameObject.SetActive(value: false);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Hovered = this;
		HoveredGameObject = base.gameObject;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HoveredGameObject = null;
		Hovered = null;
	}
}
