public class CharacterBodyTypeEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterBodyTypeEditor()
		: base(GameImpl.Translate("DEBUG_BodyTypeEditor"))
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
		for (int i = 0; i < HumanAppearance.BodyTypeNames.Length; i++)
		{
			if ((appearance.Gender != GenderType.Male || !HumanAppearance.BodyTypeNames[i].StartsWith("Female")) && (appearance.Gender != GenderType.Female || !HumanAppearance.BodyTypeNames[i].StartsWith("Male")))
			{
				BodyType bodyType = (BodyType)i;
				Items.Add(new DebugMenuItemToggle(HumanAppearance.BodyTypeNames[i], () => appearance.BodyType == bodyType, delegate
				{
					appearance.BodyType = bodyType;
					character.UnityOnChangedAppearance();
				}));
			}
		}
	}
}
