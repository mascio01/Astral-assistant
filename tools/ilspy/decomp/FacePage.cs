using System.Text;
using UnityEngine.UI;

public class FacePage : BaseCharacterCreationPage
{
	public static FacePage Instance;

	private static int Name = StringUtil.JenkinsHash("MENU_FacePage");

	public FacePage Initialize()
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
		HumanAppearance humanAppearance = GetCharacterCreationSettings().GetHumanAppearance();
		SetSliderValue("MenuLayout1/FaceTypeField/Slider", (float)humanAppearance.FaceType, (humanAppearance.Gender != GenderType.Male) ? 12 : 0, (humanAppearance.Gender == GenderType.Male) ? 9 : 20);
		SetSliderValue("MenuLayout1/EarsSizeField/Slider", humanAppearance.Bones.EarsSize, 0f, 1f);
		SetSliderValue("MenuLayout1/EarsRotationField/Slider", humanAppearance.Bones.EarsRotation, 0f, 1f);
		SetSliderValue("MenuLayout1/NoseSizeField/Slider", humanAppearance.Bones.NoseSize, 0f, 1f);
		SetSliderValue("MenuLayout1/NoseCurveField/Slider", humanAppearance.Bones.NoseCurve, 0f, 1f);
		SetSliderValue("MenuLayout1/NoseWidthField/Slider", humanAppearance.Bones.NoseWidth, 0f, 1f);
		SetSliderValue("MenuLayout1/NoseInclinationField/Slider", humanAppearance.Bones.NoseInclination, 0f, 1f);
		SetSliderValue("MenuLayout1/NosePositionField/Slider", humanAppearance.Bones.NosePosition, 0f, 1f);
		SetSliderValue("MenuLayout1/NosePronouncedField/Slider", humanAppearance.Bones.NosePronounced, 0f, 1f);
		SetSliderValue("MenuLayout1/NoseFlattenField/Slider", humanAppearance.Bones.NoseFlatten, 0f, 1f);
		SetSliderValue("MenuLayout1/ChinSizeField/Slider", humanAppearance.Bones.ChinSize, 0f, 1f);
		SetSliderValue("MenuLayout1/ChinPronouncedField/Slider", humanAppearance.Bones.ChinPronounced, 0f, 1f);
		SetSliderValue("MenuLayout1/ChinPositionField/Slider", humanAppearance.Bones.ChinPosition, 0f, 1f);
		SetSliderValue("MenuLayout2/JawSizeField/Slider", humanAppearance.Bones.JawsSize, 0f, 1f);
		SetSliderValue("MenuLayout2/JawPositionField/Slider", humanAppearance.Bones.JawsPosition, 0f, 1f);
		SetSliderValue("MenuLayout2/CheekSizeField/Slider", humanAppearance.Bones.CheekSize, 0f, 1f);
		SetSliderValue("MenuLayout2/CheekPositionField/Slider", humanAppearance.Bones.CheekPosition, 0f, 1f);
		SetSliderValue("MenuLayout2/LowCheekPronounceField/Slider", humanAppearance.Bones.LowCheekPronounced, 0f, 1f);
		SetSliderValue("MenuLayout2/LowCheekPositionField/Slider", humanAppearance.Bones.LowCheekPosition, 0f, 1f);
		SetSliderValue("MenuLayout2/ForeheadSizeField/Slider", humanAppearance.Bones.ForeheadSize, 0f, 1f);
		SetSliderValue("MenuLayout2/ForeheadPositionField/Slider", humanAppearance.Bones.ForeheadPosition, 0f, 1f);
		SetSliderValue("MenuLayout2/LipsSizeField/Slider", humanAppearance.Bones.LipsSize, 0f, 1f);
		SetSliderValue("MenuLayout2/MouthSizeField/Slider", humanAppearance.Bones.MouthSize, 0f, 1f);
		SetSliderValue("MenuLayout2/EyeSizeField/Slider", humanAppearance.Bones.EyeSize, 0f, 1f);
		SetSliderValue("MenuLayout2/EyeRotationField/Slider", humanAppearance.Bones.EyeRotation, 0f, 1f);
		SetSliderValue("MenuLayout2/EyeSpacingField/Slider", humanAppearance.Bones.EyeSpacing, 0f, 1f);
	}

	public void OnSetFaceTypeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().FaceType = (FaceType)v.value;
		GetCharacterCreationMenu().SetChangedAppearance();
	}

	public void OnSetEarsSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.EarsSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetEarsRotationSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.EarsRotation = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNoseSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NoseSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNoseCurveSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NoseCurve = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNoseWidthSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NoseWidth = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNoseInclinationSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NoseInclination = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNosePositionSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NosePosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNosePronouncedSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NosePronounced = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetNoseFlattenSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.NoseFlatten = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetChinSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.ChinSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetChinPronouncedSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.ChinPronounced = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetChinPositionSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.ChinPosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetJawSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.JawsSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetJawPositionSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.JawsPosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetCheekSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.CheekSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetCheekPositionSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.CheekPosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetLowCheekPronouncedSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.LowCheekPronounced = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetLowCheekPositionSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.LowCheekPosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetForeheadSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.ForeheadSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetForeheadPositionSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.ForeheadPosition = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetLipsSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.LipsSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetMouthSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.MouthSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetEyeSizeSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.EyeSize = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetEyeRotationSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.EyeRotation = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnSetEyeSpacingSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().Bones.EyeSpacing = v.value;
		GetCharacterCreationMenu().SetChangedBones();
	}

	public void OnRandomise()
	{
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		GetCharacterCreationSettings().GetHumanAppearance().Randomize(InfectionType.None, nonDeterministicRand, faceOnly: true);
		GetCharacterCreationMenu().SetChangedBones();
		Owner.WantRepopulate = true;
	}
}
