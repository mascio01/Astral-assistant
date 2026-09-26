using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsChangeBehaviour : MonoBehaviour
{
	public static float StarTransitionTime = 0.5f;

	public static float StarTime = 1f;

	public void Initialize()
	{
		base.gameObject.DeleteAllChildrenImmediately();
		foreach (SkillsChangePopup skillsChangePopup in Session.Instance.SkillsChangePopups)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.SkillsChangePopupPrefab, base.transform);
			gameObject.FindChild("CharacterName").GetComponent<TextMeshProUGUI>().SetUnityText(skillsChangePopup.Character.GetDisplayNameString());
			CharacterIconBehaviour component = gameObject.FindChild("CharacterIcon").GetComponent<CharacterIconBehaviour>();
			component.interactable = false;
			component.IsHudIcon = true;
			component.IsSkillsChangeIcon = true;
			component.Initialize(skillsChangePopup.Character);
			for (int i = 0; i < skillsChangePopup.SkillChanges.Count; i++)
			{
				GameObject gameObject2 = ((i != 0) ? UnityEngine.Object.Instantiate((GameObject)Hud.StarsPanelPrefab, gameObject.transform) : gameObject.FindChild("StarsPanel"));
				gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate(Skillset.GetSkillNameHash(skillsChangePopup.SkillChanges[i].Skill)));
				for (int j = 1; j <= 5; j++)
				{
					RawImage component2 = gameObject2.transform.GetChild(j).GetComponent<RawImage>();
					component2.gameObject.SetActive(j <= skillsChangePopup.SkillChanges[i].OldCap);
					component2.color = ((j <= skillsChangePopup.SkillChanges[i].OldLevel) ? SkillDisplayBehaviour.StarColActive : ((j <= skillsChangePopup.SkillChanges[i].OldCap) ? SkillDisplayBehaviour.StarColInactive : MathUtil.TransparentBlackCol));
				}
			}
		}
	}

	private void Update()
	{
		Session instance = Session.Instance;
		for (int i = 0; i < base.transform.childCount && i < instance.SkillsChangePopups.Count; i++)
		{
			SkillsChangePopup skillsChangePopup = instance.SkillsChangePopups[i];
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			for (int j = 0; j < skillsChangePopup.SkillChanges.Count; j++)
			{
				GameObject gameObject2 = gameObject.transform.GetChild(j + 2).gameObject;
				float num = Mathf.Clamp01((skillsChangePopup.Displayed - (float)(j + 1)) * StarTime / StarTransitionTime);
				for (int k = 1; k <= 5; k++)
				{
					RawImage component = gameObject2.transform.GetChild(k).GetComponent<RawImage>();
					component.gameObject.SetActive(value: true);
					SkillChange skillChange = skillsChangePopup.SkillChanges[j];
					Color a = ((k <= skillChange.OldLevel) ? SkillDisplayBehaviour.StarColActive : ((k <= skillChange.OldCap) ? Color.white : Color.black));
					Color b = ((k <= skillChange.NewLevel) ? SkillDisplayBehaviour.StarColActive : ((k <= skillChange.NewCap) ? Color.white : Color.black));
					float num2 = 1f;
					if ((k > skillChange.OldLevel && k <= skillChange.NewLevel) || (k <= skillChange.OldLevel && k > skillChange.NewLevel) || (k > skillChange.OldCap && k <= skillChange.NewCap) || (k <= skillChange.OldCap && k > skillChange.NewCap))
					{
						num2 += Mathf.Sin(num * MathF.PI);
					}
					component.color = Color.Lerp(a, b, num);
					component.transform.localScale = new Vector3(num2, num2, 1f);
				}
			}
		}
	}
}
