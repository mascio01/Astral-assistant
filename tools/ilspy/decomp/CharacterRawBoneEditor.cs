using System.Reflection;
using UMA;

public class CharacterRawBoneEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterRawBoneEditor()
		: base(GameImpl.Translate("DEBUG_RawBoneEditor"))
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
		UMADnaCustom dna = appearance.UmaDnaHumanoid;
		FieldInfo[] fields = typeof(UMADnaCustom).GetFields();
		foreach (FieldInfo field in fields)
		{
			if (field.FieldType == typeof(float))
			{
				Items.Add(new DebugMenuFloatAdjuster(field.Name, 0f, 1f, () => (float)field.GetValue(dna), delegate(float v)
				{
					field.SetValue(dna, v);
					character.UnityOnChangedBones();
				}));
			}
		}
	}
}
