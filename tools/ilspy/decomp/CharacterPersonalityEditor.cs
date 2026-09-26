public class CharacterPersonalityEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterPersonalityEditor()
		: base(GameImpl.Translate("DEBUG_PersonalityEditor"))
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
		if (CharacterToEdit == null)
		{
			return;
		}
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Randomize"), delegate
		{
			character.RandomizePersonality(MathUtil.NonDeterministicRand, PersonalityGroup.NormalFaction);
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Reset"), delegate
		{
			character.Personality.Clear();
			Session.Instance.AchievementsEnabled = false;
		}));
		foreach (string personality in GameImpl.Instance.GetAllPersonalities())
		{
			Items.Add(new DebugMenuItemToggle(personality, () => character.Personality.Contains(personality), delegate(bool v)
			{
				character.SetPersonality(personality, v);
				Session.Instance.AchievementsEnabled = false;
			}));
		}
	}
}
