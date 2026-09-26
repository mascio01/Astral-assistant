public class CharacterRelationshipEditor : DebugMenu
{
	public static bool WantRefresh;

	private Character CharacterToEdit;

	public CharacterRelationshipEditor()
		: base(GameImpl.Translate("DEBUG_RelationshipEditor"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Character character = CharacterEditor.GetCurrentCharacter();
		if (CharacterToEdit == character && !WantRefresh)
		{
			return;
		}
		Items.Clear();
		CharacterToEdit = character;
		WantRefresh = false;
		if (CharacterToEdit != null)
		{
			for (int i = 0; i < character.Relationships.Count; i++)
			{
				_ = character.Relationships[i];
				Items.Add(new DebugMenuItemRelationship(character, i));
			}
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Add"), delegate
			{
				character.Relationships.Add(default(Relationship));
				WantRefresh = true;
				Session.Instance.AchievementsEnabled = false;
			}));
		}
	}
}
