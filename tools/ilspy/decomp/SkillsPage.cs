using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsPage : BaseCharacterCreationPage
{
	public static SkillsPage Instance;

	public static int[] PointsPerLevel = new int[5] { 1, 2, 3, 4, 5 };

	public int SkillPointsRemaining;

	public GameObject[,] UnitySkillButtons = new GameObject[10, 2];

	private static int Name = StringUtil.JenkinsHash("MENU_SkillsPage");

	public static int[] SkillTooltip = new int[10];

	public SkillsPage Initialize()
	{
		return this;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(Name));
	}

	public override void Populate()
	{
		base.Populate();
		PopulateLoadoutDropdown();
		base.gameObject.FindChild("MenuLayout/PointsRemaining").GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate("MENU_SkillPoints").Replace("%1", SkillPointsRemaining.ToString()));
		SetSkillStars("MenuLayout1/Strength", SkillType.Strength);
		SetSkillStars("MenuLayout1/HandToHand", SkillType.HandToHand);
		SetSkillStars("MenuLayout1/Archery", SkillType.Archery);
		SetSkillStars("MenuLayout1/Firearms", SkillType.Firearms);
		SetSkillStars("MenuLayout1/Stealth", SkillType.Stealth);
		SetSkillStars("MenuLayout2/Construction", SkillType.Construction);
		SetSkillStars("MenuLayout2/Farming", SkillType.Farming);
		SetSkillStars("MenuLayout2/Medicine", SkillType.Medicine);
		SetSkillStars("MenuLayout2/Cooking", SkillType.Cooking);
		SetSkillStars("MenuLayout2/Constitution", SkillType.Constitution);
	}

	public void SetSkillStars(string name, SkillType skillType)
	{
		Color color = new Color32(251, byte.MaxValue, 147, byte.MaxValue);
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		for (int i = 1; i <= 5; i++)
		{
			base.gameObject.FindChild(name + "/Star" + i).GetComponent<RawImage>().color = ((i <= characterCreationSettings.Skills[(int)skillType]) ? color : Color.gray);
		}
		UnitySkillButtons[(int)skillType, 0] = base.gameObject.FindChild(name + "/MinusButton");
		UnitySkillButtons[(int)skillType, 1] = base.gameObject.FindChild(name + "/PlusButton");
	}

	public SkillType GetSkillTypeFromUnityButton(GameObject button, out int buttonIdx)
	{
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (UnitySkillButtons[i, j] == button)
				{
					buttonIdx = j;
					return (SkillType)i;
				}
			}
		}
		buttonIdx = -1;
		return SkillType.Count;
	}

	public void IncrementSkill(int skillType)
	{
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		if (characterCreationSettings.Skills[skillType] < 5)
		{
			int num = PointsPerLevel[characterCreationSettings.Skills[skillType]];
			if (SkillPointsRemaining >= num)
			{
				characterCreationSettings.Skills[skillType]++;
				SkillPointsRemaining -= num;
				GetCharacterCreationMenu().UpdatePreviewGimpSkills();
			}
		}
	}

	public void DecrementSkill(int skillType)
	{
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		if (characterCreationSettings.Skills[skillType] > 0)
		{
			characterCreationSettings.Skills[skillType]--;
			SkillPointsRemaining += PointsPerLevel[characterCreationSettings.Skills[skillType]];
			GetCharacterCreationMenu().UpdatePreviewGimpSkills();
		}
	}

	public void OnRandomise()
	{
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		ResetSkills(characterCreationMenu);
		RandomiseSkillPoints(characterCreationMenu, MathUtil.NonDeterministicRand);
	}

	public void ResetSkills(CharacterCreationMenu menu)
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		SkillPointsRemaining = settings.SkillPoints;
		for (int i = 0; i < menu.Settings.Skills.Length; i++)
		{
			menu.Settings.Skills[i] = 0;
		}
	}

	public void RandomiseSkillPoints(CharacterCreationMenu menu, CustomRandom rand)
	{
		int num = 0;
		while (SkillPointsRemaining > 0 && num < 1000)
		{
			int num2 = rand.Next(menu.Settings.Skills.Length);
			if (menu.Settings.Skills[num2] < 5)
			{
				int num3 = PointsPerLevel[menu.Settings.Skills[num2]];
				if (SkillPointsRemaining >= num3)
				{
					menu.Settings.Skills[num2]++;
					SkillPointsRemaining -= num3;
				}
			}
			num++;
		}
		menu.UpdatePreviewGimpSkills();
	}
}
