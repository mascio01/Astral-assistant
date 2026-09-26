public class CharacterMemoryEditor : DebugMenu
{
	public static Character CharacterToEdit;

	public static bool Refresh;

	private string AddMemoryProtoName = "";

	private string AddMemoryActorID = "";

	private string AddMemoryObjectID = "";

	private string AddMemoryThirdPartyID = "";

	private float AddMemoryQuantity = 1f;

	public CharacterMemoryEditor()
		: base(GameImpl.Translate("DEBUG_MemoryEditor"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Character character = CharacterEditor.GetCurrentCharacter();
		if (CharacterToEdit == character && !Refresh)
		{
			return;
		}
		Refresh = false;
		Items.Clear();
		CharacterToEdit = character;
		if (CharacterToEdit == null)
		{
			return;
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Morale"), character.CalcMorale().ToString()));
		for (int i = 0; i < character.Memories.Count; i++)
		{
			Items.Add(new DebugMenuItemMemory(character, i));
		}
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Add"), delegate
		{
			MemoryPrototype memoryPrototype = GameImpl.Instance.FindMemoryPrototypeByUniqueID(AddMemoryProtoName);
			Character actor = DebugMenuItemMemory.FindBaseObjectFromString(AddMemoryActorID) as Character;
			BaseObject obj = DebugMenuItemMemory.FindBaseObjectFromString(AddMemoryObjectID);
			BaseObject thirdParty = DebugMenuItemMemory.FindBaseObjectFromString(AddMemoryThirdPartyID);
			if (memoryPrototype != null)
			{
				character.AddMemory(memoryPrototype, actor, obj, thirdParty, AddMemoryQuantity, fakeNews: false, null, forceAdd: true);
				Session.Instance.AchievementsEnabled = false;
				Refresh = true;
			}
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_NewMemoryType"), () => AddMemoryProtoName, delegate(string v)
		{
			AddMemoryProtoName = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_WithActorID"), () => AddMemoryActorID, delegate(string v)
		{
			AddMemoryActorID = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_AndObjectID"), () => AddMemoryObjectID, delegate(string v)
		{
			AddMemoryObjectID = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_AndThirdPartyID"), () => AddMemoryThirdPartyID, delegate(string v)
		{
			AddMemoryThirdPartyID = v;
		}));
	}
}
