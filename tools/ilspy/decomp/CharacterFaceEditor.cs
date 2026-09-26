public class CharacterFaceEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterFaceEditor()
		: base(GameImpl.Translate("DEBUG_FaceEditor"))
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
		for (int i = 0; i < HumanAppearance.FaceTypeNames.Length; i++)
		{
			if ((appearance.Gender != GenderType.Male || !HumanAppearance.FaceTypeNames[i].StartsWith("Female")) && (appearance.Gender != GenderType.Female || !HumanAppearance.FaceTypeNames[i].StartsWith("Male")))
			{
				FaceType faceType = (FaceType)i;
				Items.Add(new DebugMenuItemToggle(HumanAppearance.FaceTypeNames[i], () => appearance.FaceType == faceType, delegate
				{
					appearance.FaceType = faceType;
					character.UnityOnChangedAppearance();
				}));
			}
		}
	}
}
