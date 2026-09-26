using System.Reflection;

public class CharacterBoneEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterBoneEditor()
		: base(GameImpl.Translate("DEBUG_BoneEditor"))
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
		HumanAppearance.BoneSettings boneSettings = appearance.Bones;
		FieldInfo[] fields = typeof(HumanAppearance.BoneSettings).GetFields();
		foreach (FieldInfo field in fields)
		{
			if (field.FieldType == typeof(float) && (!(field.Name == "BreastSize") || appearance.Gender != GenderType.Male))
			{
				Items.Add(new DebugMenuFloatAdjuster(field.Name, 0f, 1f, () => (float)field.GetValue(boneSettings), delegate(float v)
				{
					field.SetValue(boneSettings, v);
					appearance.SetupDNA();
					character.UnityOnChangedBones();
				}));
			}
		}
	}
}
