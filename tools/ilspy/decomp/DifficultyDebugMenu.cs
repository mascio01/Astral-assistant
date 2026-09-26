public class DifficultyDebugMenu : DebugMenu
{
	public bool WantPopulate;

	public DifficultyDebugMenu()
		: base(GameImpl.Translate("DEBUG_DifficultySettings"))
	{
		WantPopulate = true;
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		WantPopulate = true;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		if (WantPopulate)
		{
			Populate();
			WantPopulate = false;
		}
	}

	public void Populate()
	{
		Session instance = Session.Instance;
		DifficultySettings difficultySettings = instance.DifficultySettings;
		Items.Clear();
		Items.Add(new DebugMenuItemCustom("Save", OnSaveWorldGenSettings));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("EDITOR_SaveTokensRequired"), () => difficultySettings.SaveTokensRequired, delegate(bool v)
		{
			difficultySettings.SaveTokensRequired = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("EDITOR_TradersHaveSaveTokens"), () => difficultySettings.TradersHaveSaveTokens, delegate(bool v)
		{
			difficultySettings.TradersHaveSaveTokens = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_FriendlyFireSplashDamage").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.FriendlyFireSplashDamage, delegate(float v)
		{
			difficultySettings.FriendlyFireSplashDamage = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_GreenStrain").Replace("%1", difficultySettings.GreenStrainDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_BlueStrain").Replace("%1", difficultySettings.BlueStrainDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_RedStrain").Replace("%1", difficultySettings.RedStrainDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_WhiteStrain").Replace("%1", difficultySettings.WhiteStrainDensity.ToString())));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_InvisibleStrain").Replace("%1", difficultySettings.InvisibleStrainPercentage.ToString()), 0f, 100f, () => difficultySettings.InvisibleStrainPercentage, delegate(float v)
		{
			difficultySettings.InvisibleStrainPercentage = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_InvisibleStrainIncrement").Replace("%1", difficultySettings.InvisibleStrainIncrementPerYear.ToString()), 0f, 100f, () => difficultySettings.InvisibleStrainIncrementPerYear, delegate(float v)
		{
			difficultySettings.InvisibleStrainIncrementPerYear = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_ZombieCrippled").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.ZombieCrippledPercentage, delegate(float v)
		{
			difficultySettings.ZombieCrippledPercentage = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate((difficultySettings.ZombieRespawnDays == 1f) ? "MENU_ZombieRespawnDay" : "MENU_ZombieRespawnDays").Replace("%1", string.Empty), 0f, WorldPage.MaxZombieRespawnDays, () => difficultySettings.ZombieRespawnDays, delegate(float v)
		{
			difficultySettings.ZombieRespawnDays = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_SurvivorDensity").Replace("%1", difficultySettings.SurvivorCampDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_SurvivorType").Replace("%1", difficultySettings.SurvivorCampLooterPercentage.ToString())));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate((difficultySettings.SurvivorRepopulationDays == 1f) ? "MENU_SurvivorRepopulationDay" : "MENU_SurvivorRepopulationDays").Replace("%1", string.Empty), 0f, WorldPage.MaxSurvivorRepopulationDays, () => difficultySettings.SurvivorRepopulationDays, delegate(float v)
		{
			difficultySettings.SurvivorRepopulationDays = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_HordeDensity").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.HordeDensity, delegate(float v)
		{
			difficultySettings.HordeDensity = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_RaiderDensity").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.RaiderDensity, delegate(float v)
		{
			difficultySettings.RaiderDensity = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_RefugeeDensity").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.RefugeeDensity, delegate(float v)
		{
			difficultySettings.RefugeeDensity = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("MENU_TraderDensity").Replace("%1", string.Empty), 0f, 100f, () => difficultySettings.TraderDensity, delegate(float v)
		{
			difficultySettings.TraderDensity = v;
			WantPopulate = true;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_RoadDensity").Replace("%1", difficultySettings.RoadDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_TownDensity").Replace("%1", difficultySettings.TownDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_VehicleDensity").Replace("%1", difficultySettings.VehicleDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_CountrysidePropDensity").Replace("%1", difficultySettings.CountrysidePropDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_LootDensity").Replace("%1", difficultySettings.LootDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_RiverDensity").Replace("%1", difficultySettings.RiverDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_RabbitDensity").Replace("%1", difficultySettings.RabbitDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_DeerDensity").Replace("%1", difficultySettings.DeerDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_FlintDensity").Replace("%1", difficultySettings.FlintDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_LeadOreDensity").Replace("%1", difficultySettings.LeadOreDensity.ToString())));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("MENU_IronOreDensity").Replace("%1", difficultySettings.IronOreDensity.ToString())));
	}

	public void OnSaveWorldGenSettings()
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		DifficultySettings difficultySettings = instance.DifficultySettings;
		UtilsDebugMenu.SaveWorldGenSettings(new WorldGenSettings
		{
			RandomSeed = instance.RandomSeed,
			MapSize = GameTerrain.GetMapSizeFromSize(instance2.Size),
			StartDayOfYear = instance.Weather.StartDayOfYear,
			StartHourOfDay = instance.Weather.StartHourOfDay,
			DifficultySettings = difficultySettings.MakeCopy()
		});
	}
}
