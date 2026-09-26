using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PersonalityPage : BaseCharacterCreationPage
{
	public static PersonalityPage Instance;

	private GameObject UnityContents;

	private static int Name = StringUtil.JenkinsHash("MENU_PersonalityPage");

	public override void OnAwake()
	{
		base.OnAwake();
		UnityContents = base.gameObject.FindChild("Scroll View/Viewport/Content");
	}

	public PersonalityPage Initialize()
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
		GameImpl instance = GameImpl.Instance;
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		List<PersonalityGroup> allPersonalityGroups = instance.GetAllPersonalityGroups(PersonalityGroup.NormalFaction);
		int num = 0;
		foreach (PersonalityGroup item in allPersonalityGroups)
		{
			GameObject gameObject = null;
			if (num < UnityContents.transform.childCount)
			{
				gameObject = UnityContents.transform.GetChild(num).gameObject;
			}
			else
			{
				gameObject = Object.Instantiate(BaseMenu.OptionsRow.GetAsset(), UnityContents.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(num);
			}
			num++;
			for (int i = 0; i < item.Personalities.Count; i++)
			{
				string personality = item.Personalities[i].Personality;
				GameObject gameObject2 = gameObject.FindChild(personality);
				if (gameObject2 == null)
				{
					gameObject2 = Object.Instantiate(BaseMenu.OptionButton.GetAsset(), gameObject.transform, worldPositionStays: false);
					gameObject2.name = personality;
					gameObject2.GetComponent<Button>().onClick.AddListener(delegate
					{
						OnClickedPersonality(personality);
					});
				}
				OptionButtonBehaviour component = gameObject2.GetComponent<OptionButtonBehaviour>();
				string text = "PERSONALITY_" + personality.Replace(" ", "");
				if (characterCreationSettings.Appearance.Gender == GenderType.Female && GameImpl.Instance.TryTranslate(StringUtil.JenkinsHash(text + "_Female"), out var result, englishOnly: false))
				{
					component.SetCaption(result);
				}
				else
				{
					result = GameImpl.Translate(text);
					result = StringUtil.ApplyFormulae(result, GetCharacterCreationMenu().PreviewGimp);
					component.SetCaption(result);
				}
				component.SetTicked(characterCreationSettings.Personality.Contains(personality));
			}
		}
	}

	public void OnClickedPersonality(string personality)
	{
		GameImpl instance = GameImpl.Instance;
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		PersonalityGroup personalityGroup = instance.FindPersonalityGroup(personality, PersonalityGroup.NormalFaction);
		if (characterCreationSettings.Personality.Contains(personality))
		{
			if (personalityGroup.GetProbabilitySum() < 100f)
			{
				characterCreationSettings.Personality.Remove(personality);
			}
		}
		else
		{
			for (int i = 0; i < personalityGroup.Personalities.Count; i++)
			{
				characterCreationSettings.Personality.Remove(personalityGroup.Personalities[i].Personality);
			}
			characterCreationSettings.Personality.Add(personality);
		}
		characterCreationSettings.Personality.CopyToList(characterCreationMenu.PreviewGimp.Personality);
		Owner.WantRepopulate = true;
	}

	public void OnRandomise()
	{
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		characterCreationMenu.PreviewGimp.RandomizePersonality(MathUtil.NonDeterministicRand, PersonalityGroup.NormalFaction);
		characterCreationMenu.PreviewGimp.Personality.CopyToList(characterCreationSettings.Personality);
		Owner.WantRepopulate = true;
	}
}
