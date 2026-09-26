public class CharacterHairEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterHairEditor()
		: base(GameImpl.Translate("DEBUG_HairEditor"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Character character = CharacterEditor.GetCurrentCharacter();
		if (CharacterToEdit == character)
		{
			return;
		}
		Items.Clear();
		CharacterToEdit = character;
		if (!(CharacterToEdit is Human human))
		{
			return;
		}
		HumanAppearance appearance = human.GetAppearance();
		Items.Add(new DebugMenuColorSpectrumAdjuster(GameImpl.Translate("DEBUG_HairColor"), HumanAppearance.HairColorSpectrum, () => appearance.HairColorIndex, delegate(float v)
		{
			appearance.HairColorIndex = v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairRed"), 0, 255, () => HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].r, delegate(int v)
		{
			HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].r = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairGreen"), 0, 255, () => HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].g, delegate(int v)
		{
			HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].g = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairBlue"), 0, 255, () => HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].b, delegate(int v)
		{
			HumanAppearance.HairColorSpectrum[(int)(appearance.HairColorIndex + 0.5f)].b = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairBandRed"), 0, 255, () => appearance.HairBandColor.r, delegate(int v)
		{
			appearance.HairBandColor.r = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairBandGreen"), 0, 255, () => appearance.HairBandColor.g, delegate(int v)
		{
			appearance.HairBandColor.g = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_HairBandBlue"), 0, 255, () => appearance.HairBandColor.b, delegate(int v)
		{
			appearance.HairBandColor.b = (byte)v;
			character.UnityOnChangedAppearance();
		}));
		for (int num = 0; num < HumanAppearance.HairTypeNames.Length; num++)
		{
			if ((appearance.Gender != GenderType.Male || !HumanAppearance.HairTypeNames[num].StartsWith("Female")) && (appearance.Gender != GenderType.Female || !HumanAppearance.HairTypeNames[num].StartsWith("Male")))
			{
				HairType hairType = (HairType)num;
				Items.Add(new DebugMenuItemToggle(HumanAppearance.HairTypeNames[num], () => appearance.HairType == hairType, delegate
				{
					appearance.HairType = hairType;
					character.UnityOnChangedAppearance();
				}));
			}
		}
		if (appearance.Gender != GenderType.Male)
		{
			return;
		}
		for (int num2 = 0; num2 < HumanAppearance.FacialHairTypeNames.Length; num2++)
		{
			FacialHairType facialHairType = (FacialHairType)num2;
			Items.Add(new DebugMenuItemToggle(HumanAppearance.FacialHairTypeNames[num2], () => appearance.FacialHairType == facialHairType, delegate
			{
				appearance.FacialHairType = facialHairType;
				character.UnityOnChangedAppearance();
			}));
		}
	}
}
