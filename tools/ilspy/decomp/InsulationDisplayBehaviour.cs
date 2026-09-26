using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsulationDisplayBehaviour : MonoBehaviour
{
	private List<RawImage> UnityIcons = new List<RawImage>();

	private RawImage UnityDesiredInsulation;

	private TMP_Text UnityDampText;

	private int CurInsulation;

	private int MaxInsulation;

	private int DesiredInsulation;

	private GenderType ClothingGender;

	private int ClothingCount;

	public static int HUD_Damp = StringUtil.JenkinsHash("HUD_Damp");

	public static int HUD_Damp_Female = StringUtil.JenkinsHash("HUD_Damp_Female");

	public static int HUD_Damp_Plural = StringUtil.JenkinsHash("HUD_Damp_Plural");

	public static int HUD_Damp_Female_Plural = StringUtil.JenkinsHash("HUD_Damp_Female_Plural");

	private void Awake()
	{
		InitIfNeeded();
	}

	private void InitIfNeeded()
	{
		if (UnityIcons.Count == 0)
		{
			for (int i = 1; i <= 15; i++)
			{
				UnityIcons.Add(base.gameObject.FindChild("Star" + i).GetComponent<RawImage>());
			}
			UnityDesiredInsulation = base.gameObject.FindChild("DesiredInsulation").GetComponent<RawImage>();
			UnityDampText = base.gameObject.FindChild("Damp").GetComponent<TMP_Text>();
		}
	}

	public void SetInsulation(int curInsulation, int maxInsulation, int desiredInsulation, GenderType gender, int count)
	{
		bool num = curInsulation != CurInsulation || maxInsulation != MaxInsulation || desiredInsulation != DesiredInsulation || gender != ClothingGender || count != ClothingCount;
		CurInsulation = curInsulation;
		MaxInsulation = maxInsulation;
		DesiredInsulation = desiredInsulation;
		ClothingCount = count;
		ClothingGender = GenderType.Female;
		if (num)
		{
			Populate();
		}
	}

	private void Populate()
	{
		InitIfNeeded();
		for (int i = 0; i < UnityIcons.Count; i++)
		{
			UnityIcons[i].gameObject.SetActive(i < MaxInsulation || i < DesiredInsulation);
			UnityIcons[i].color = ((i < CurInsulation) ? HudBehaviour.InsulationJacketCol : ((i < MaxInsulation) ? HudBehaviour.InsulationJacketDampCol : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 32)));
		}
		UnityDesiredInsulation.transform.SetSiblingIndex(DesiredInsulation + 2);
		UnityDampText.gameObject.SetActive(CurInsulation < MaxInsulation);
		UnityDampText.SetUnityTextIfDifferent(GameImpl.Translate(HUD_Damp, HUD_Damp_Female, HUD_Damp_Plural, HUD_Damp_Female_Plural, ClothingGender, ClothingCount > 1));
	}
}
