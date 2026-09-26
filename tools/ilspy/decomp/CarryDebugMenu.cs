using System;

public class CarryDebugMenu : DebugMenu
{
	private Character CharacterToEdit;

	public CarryDebugMenu()
		: base(GameImpl.Translate("DEBUG_CarryingDebug"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Character currentCharacter = CharacterEditor.GetCurrentCharacter();
		if (CharacterToEdit == currentCharacter)
		{
			return;
		}
		CharacterToEdit = currentCharacter;
		Items.Clear();
		if (currentCharacter == null)
		{
			return;
		}
		string text = "";
		Type type = typeof(Character);
		if (currentCharacter.CarryingObject != null)
		{
			type = currentCharacter.CarryingObject.GetType();
			text = ((type == typeof(Human)) ? string.Empty : type.ToString());
			if (currentCharacter.CarryingObject is Character character && !character.IsRagdoll())
			{
				text += "Awake";
			}
		}
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Rot X", -180f, 180f, type, text + "CarryRotX"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Rot Y", -180f, 180f, type, text + "CarryRotY"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Rot Z", -180f, 180f, type, text + "CarryRotZ"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Pos X", -1f, 1f, type, text + "CarryOffsetX"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Pos Y", -1f, 1f, type, text + "CarryOffsetY"));
		Items.Add(new DebugMenuFloatFieldAdjuster("Carry Pos Z", -1f, 1f, type, text + "CarryOffsetZ"));
	}
}
