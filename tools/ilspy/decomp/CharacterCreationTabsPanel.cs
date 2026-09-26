public class CharacterCreationTabsPanel : BaseTabbableMenu
{
	public CharacterCreationMenu Owner;

	public void Activate()
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		ClearTabs();
		AddTab(VitalStatsPage.Instance.Initialize());
		AddTab(FacePage.Instance.Initialize());
		AddTab(ClothesPage.Instance.Initialize());
		EquipmentPage equipmentPage = EquipmentPage.Instance.Initialize();
		if (settings.EquipmentPoints > 0)
		{
			AddTab(equipmentPage);
		}
		else
		{
			equipmentPage.Owner = this;
		}
		SkillsPage skillsPage = SkillsPage.Instance.Initialize();
		if (settings.SkillPoints > 0)
		{
			AddTab(skillsPage);
		}
		else
		{
			skillsPage.Owner = this;
		}
		AddTab(PersonalityPage.Instance.Initialize());
		WorldPage worldPage = WorldPage.Instance.Initialize();
		if (settings.ProcedurallyGenerated && !OnlineParty.Instance.IsInMultiplayerGameAsFollower())
		{
			AddTab(worldPage);
		}
		else
		{
			worldPage.Owner = this;
		}
		OnActivate(0);
	}
}
