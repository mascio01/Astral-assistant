public class CharacterSkillEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterSkillEditor()
		: base(GameImpl.Translate("DEBUG_SkillEditor"))
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
			character.Skillset.Randomize(character, MathUtil.NonDeterministicRand);
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Reset"), delegate
		{
			character.Skillset.Reset(character);
			Session.Instance.AchievementsEnabled = false;
		}));
		for (int num = 0; num < 10; num++)
		{
			SkillType skillType = (SkillType)num;
			Items.Add(new DebugMenuIntAdjuster(skillType.ToString(), 0, () => CharacterToEdit.Skillset.GetCap(skillType), () => CharacterToEdit.Skillset.GetLevel(skillType), delegate(int level)
			{
				CharacterToEdit.Skillset.SetLevel(CharacterToEdit, skillType, level);
				Session.Instance.AchievementsEnabled = false;
			}));
		}
		for (int num2 = 0; num2 < 10; num2++)
		{
			SkillType skillType2 = (SkillType)num2;
			Items.Add(new DebugMenuIntAdjuster(skillType2.ToString() + " Cap", 0, 5, () => CharacterToEdit.Skillset.GetCap(skillType2), delegate(int level)
			{
				CharacterToEdit.Skillset.SetCap(CharacterToEdit, skillType2, level);
				Session.Instance.AchievementsEnabled = false;
			}));
		}
		Items.Add(new DebugMenuIntAdjuster(StringUtil.ApplyFormulae(GameImpl.Translate("HUD_Trader"), character), 0, Character.ProgressionToTraderLevel.Length, () => CharacterToEdit.TraderLevel, delegate(int level)
		{
			CharacterToEdit.TraderLevel = level;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_TraderProgression"), character.ValueOfGoodsTradedWithPlayerAndSoldOn.ToString()));
		for (int num3 = 0; num3 < 10; num3++)
		{
			SkillType skillType3 = (SkillType)num3;
			Items.Add(new DebugMenuItemToggle(skillType3.ToString() + " Known", () => CharacterToEdit.Skillset.IsSkillKnown(skillType3), delegate(bool v)
			{
				if (v)
				{
					CharacterToEdit.Skillset.SetSkillKnown(skillType3);
				}
				else
				{
					CharacterToEdit.Skillset.ClearSkillKnown(skillType3);
				}
				Session.Instance.AchievementsEnabled = false;
			}));
		}
	}
}
