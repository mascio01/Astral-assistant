public class CharacterSurvivalFactorsEditor : DebugMenu
{
	private Character CharacterToEdit;

	public CharacterSurvivalFactorsEditor()
		: base(GameImpl.Translate("DEBUG_SurvivalFactorsEditor"))
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
		Items.Clear();
		CharacterToEdit = currentCharacter;
		if (CharacterToEdit == null)
		{
			return;
		}
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_BloodLoss"), 0f, 1f, CharacterToEdit.GetBloodLoss, CharacterToEdit.SetBloodLoss, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Fatigue"), 0f, 1f, CharacterToEdit.GetFatigue, CharacterToEdit.SetFatigue, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Hunger"), 0f, Character.HungerDieTime, CharacterToEdit.GetHunger, CharacterToEdit.SetHunger, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Thirst"), 0f, Character.ThirstDieTime, CharacterToEdit.GetThirst, CharacterToEdit.SetThirst, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Toilet"), 0f, Character.WetPantsTime, CharacterToEdit.GetToilet, CharacterToEdit.SetToilet, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_SleepDeprivation"), 0f, Character.SleepDeprivationDieTime, CharacterToEdit.GetSleepDeprivation, CharacterToEdit.SetSleepDeprivation, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_BodyTemperature"), Character.BodyTemperatureInCelsiusDeath, Character.BodyTemperatureInCelsiusHyperpyrexia, CharacterToEdit.GetBodyTemperatureInCelsius, CharacterToEdit.SetBodyTemperatureInCelsius, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_InfectionProgression"), 0f, 2f, CharacterToEdit.GetInfectionProgression, CharacterToEdit.SetInfectionProgression, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Excitement"), 0f, 1f, CharacterToEdit.GetExcitement, CharacterToEdit.SetExcitement, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_LimitBloodLoss"), 0f, 1f, () => CharacterToEdit.LimitBloodLoss, delegate(float v)
		{
			CharacterToEdit.LimitBloodLoss = v;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_BloodAlcoholConcentration"), 0f, 1f, () => CharacterToEdit.BloodAlcoholConcentration, delegate(float v)
		{
			CharacterToEdit.BloodAlcoholConcentration = v;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_SkinnedAmount"), 0f, 1f, () => CharacterToEdit.SkinnedAmount, delegate(float v)
		{
			CharacterToEdit.SkinnedAmount = v;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_NameKnown"), CharacterToEdit, "NameKnown", affectsGameState: true));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate(StringUtil.JenkinsHash("DEBUG_Investigated"), StringUtil.JenkinsHash("DEBUG_Investigated_Female"), CharacterToEdit.GetGender()), CharacterToEdit, "Investigated", affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_DontSimulateUntilDiscovered"), CharacterToEdit.GetDontSimulateSurvivalFactorsUntilDiscovered, CharacterToEdit.SetDontSimulateSurvivalFactorsUntilDiscovered, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_DontSimulateUntilJoin"), CharacterToEdit.GetDontSimulateSurvivalFactorsUntilJoinCommunity, CharacterToEdit.SetDontSimulateSurvivalFactorsUntilJoinCommunity, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_DontLeaveCommunity"), () => CharacterToEdit.DontLeaveCommunity, delegate(bool v)
		{
			CharacterToEdit.DontLeaveCommunity = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_DontAbandonPlayer"), () => CharacterToEdit.DontAbandonPlayer, delegate(bool v)
		{
			CharacterToEdit.DontAbandonPlayer = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ReservedGoldAmount"), 0, 100000, CharacterToEdit.GetReservedGoldAmount, CharacterToEdit.SetReservedGoldAmount, affectsGameState: true));
		Chicken chicken = CharacterToEdit as Chicken;
		if (chicken != null)
		{
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_EggProduction"), 0f, 1f, () => chicken.EggProduction, delegate(float v)
			{
				chicken.EggProduction = v;
				Session.Instance.AchievementsEnabled = false;
			}));
		}
	}
}
