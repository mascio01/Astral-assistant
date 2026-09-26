public class CharacterSpawner : DebugMenu
{
	public enum SpawnType
	{
		Male,
		Female,
		GreenStrainZombie,
		BlueStrainZombie,
		RedStrainZombie,
		WhiteStrainZombie,
		PsychoUnarmed,
		PsychoMelee,
		PsychoBow,
		PsychoGun,
		NeutralUnarmed,
		NeutralMelee,
		NeutralBow,
		NeutralGun,
		NeutralMilitary,
		Rabbit,
		Deer,
		Chicken,
		Count
	}

	private static string[] SpawnTypeNames = StringUtil.GetEnumNames<SpawnType>();

	public SpawnType CurrentSpawnType;

	public bool HasAI = true;

	private DebugMenuCommunity CommunitySetter;

	public CharacterSpawner()
		: base(GameImpl.Translate("DEBUG_CharacterSpawner"))
	{
		if (Session.Instance.Editor)
		{
			CommunitySetter = new DebugMenuCommunity(GameImpl.Translate("DEBUG_Community"));
			Items.Add(CommunitySetter);
		}
		else
		{
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_HasAI"), this, "HasAI"));
		}
		for (int i = 0; i < 18; i++)
		{
			SpawnType localSpawnType = (SpawnType)i;
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_" + SpawnTypeNames[i]), () => CurrentSpawnType == localSpawnType, delegate
			{
				CurrentSpawnType = localSpawnType;
			}));
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		GameTerrain instance3 = GameTerrain.Instance;
		InputFunctionManager instance4 = InputFunctionManager.Instance;
		if (!instance4.IsJustPressed(InputFunction.MainAction))
		{
			return;
		}
		instance2.AchievementsEnabled = false;
		RaycastResult raycastResult = instance2.GameCamera.RayCastFromPointOnScreen(instance4.GetCursorPos(), 0);
		if (raycastResult.HitObject != instance3 || instance3.IsImpassable(raycastResult.Tile.x, raycastResult.Tile.y, 199, null, null))
		{
			return;
		}
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		switch (CurrentSpawnType)
		{
		case SpawnType.Male:
		case SpawnType.Female:
		{
			HumanAppearance humanAppearance4 = new HumanAppearance((CurrentSpawnType == SpawnType.Female) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(nonDeterministicRand));
			humanAppearance4.Randomize(InfectionType.None, nonDeterministicRand);
			Character character4 = Human.Spawn(raycastResult.Tile, 0f, humanAppearance4, InfectionType.None);
			character4.RandomizeClothing(nonDeterministicRand, seasonallyAppropriate: true);
			character4.RandomizeName(nonDeterministicRand);
			character4.Skillset.Randomize(character4, nonDeterministicRand);
			character4.RandomizePersonality(nonDeterministicRand, PersonalityGroup.NormalFaction);
			if (instance2.Editor)
			{
				character4.SetCommunity(DebugMenuCommunity.Community);
			}
			else
			{
				character4.NameKnown = true;
				character4.Investigated = true;
				character4.SetAllSkillsKnown();
				character4.SetCommunity(instance2.CommunityManager.PlayerCommunity);
			}
			if (HasAI && !instance2.Editor)
			{
				character4.SetGoal(new SurvivorGoal());
			}
			break;
		}
		case SpawnType.GreenStrainZombie:
		case SpawnType.BlueStrainZombie:
		case SpawnType.RedStrainZombie:
		case SpawnType.WhiteStrainZombie:
		{
			InfectionType infectionType = InfectionType.Green;
			switch (CurrentSpawnType)
			{
			case SpawnType.GreenStrainZombie:
				infectionType = InfectionType.Green;
				break;
			case SpawnType.BlueStrainZombie:
				infectionType = InfectionType.Blue;
				break;
			case SpawnType.RedStrainZombie:
				infectionType = InfectionType.Red;
				break;
			case SpawnType.WhiteStrainZombie:
				infectionType = InfectionType.White;
				break;
			}
			HumanAppearance humanAppearance2 = new HumanAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(nonDeterministicRand));
			humanAppearance2.Randomize(infectionType, nonDeterministicRand);
			Character character2 = Human.Spawn(raycastResult.Tile, 0f, humanAppearance2, infectionType);
			character2.Rotten = true;
			character2.RandomizeClothing(nonDeterministicRand, seasonallyAppropriate: false);
			if (instance2.Editor && DebugMenuCommunity.Community != null)
			{
				character2.SetCommunity(DebugMenuCommunity.Community);
			}
			if (HasAI && !instance2.Editor)
			{
				character2.SetGoal(new ZombieGoal());
			}
			break;
		}
		case SpawnType.PsychoUnarmed:
		case SpawnType.PsychoMelee:
		case SpawnType.PsychoBow:
		case SpawnType.PsychoGun:
		{
			HumanAppearance humanAppearance = new HumanAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(nonDeterministicRand));
			humanAppearance.Randomize(InfectionType.None, nonDeterministicRand);
			Community community = null;
			if (instance2.Editor)
			{
				community = DebugMenuCommunity.Community;
			}
			if (community == null)
			{
				foreach (Community community3 in instance2.CommunityManager.Communities)
				{
					if (community3.CommunityType == CommunityType.Psycho)
					{
						community = community3;
						break;
					}
				}
			}
			if (community == null)
			{
				community = Community.Spawn(CommunityType.Psycho);
				community.CommunityName.Randomise(MathUtil.NonDeterministicRand, unique: true, community);
			}
			Character character = Human.Spawn(raycastResult.Tile, 0f, humanAppearance, InfectionType.None);
			character.SetCommunity(community);
			if (!instance2.Editor)
			{
				character.InitialCommunity = community;
			}
			character.RandomizeClothing(nonDeterministicRand, seasonallyAppropriate: true, isPortrait: false, mustHaveBackpack: false, nonDeterministicRand.RandomChoice(0.5f), nonDeterministicRand.RandomChoice(0.5f), nonDeterministicRand.RandomChoice(0.5f));
			character.RandomizeName(nonDeterministicRand);
			character.Skillset.Randomize(character, nonDeterministicRand);
			character.RandomizePersonality(nonDeterministicRand, PersonalityGroup.LooterFaction);
			switch (CurrentSpawnType)
			{
			case SpawnType.PsychoMelee:
			{
				EquipmentPrototype equipmentPrototype3 = instance.PickRandomItemOfClass(typeof(MeleeWeapon), nonDeterministicRand);
				if (equipmentPrototype3 != null)
				{
					character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype3));
				}
				break;
			}
			case SpawnType.PsychoBow:
			{
				EquipmentPrototype equipmentPrototype2 = instance.PickRandomItemOfClass(typeof(Bow), nonDeterministicRand);
				if (equipmentPrototype2 != null)
				{
					character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype2));
				}
				break;
			}
			case SpawnType.PsychoGun:
			{
				EquipmentPrototype equipmentPrototype = instance.PickRandomItemOfClass(typeof(Gun), nonDeterministicRand);
				if (equipmentPrototype != null)
				{
					character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype));
					if (equipmentPrototype.GetDefaultAmmoPrototype() != null)
					{
						character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype.GetDefaultAmmoPrototype(), nonDeterministicRand.Next(10, 20)));
					}
				}
				break;
			}
			}
			if (nonDeterministicRand.RandomChoice(0.5f))
			{
				EquipmentPrototype equipmentPrototype4 = instance.PickRandomItemOfClass(nonDeterministicRand.RandomChoice(0.5f) ? typeof(PipeBomb) : typeof(MolotovCocktail), nonDeterministicRand);
				if (equipmentPrototype4 != null)
				{
					character.Inventory.Add(character, Equipment.Spawn(equipmentPrototype4));
				}
			}
			if (HasAI && !instance2.Editor)
			{
				character.SetGoal(new SurvivorGoal());
			}
			break;
		}
		case SpawnType.NeutralUnarmed:
		case SpawnType.NeutralMelee:
		case SpawnType.NeutralBow:
		case SpawnType.NeutralGun:
		case SpawnType.NeutralMilitary:
		{
			HumanAppearance humanAppearance3 = new HumanAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(nonDeterministicRand));
			humanAppearance3.Randomize(InfectionType.None, nonDeterministicRand);
			Community community2 = null;
			if (instance2.Editor)
			{
				community2 = DebugMenuCommunity.Community;
			}
			if (community2 == null)
			{
				foreach (Community community4 in instance2.CommunityManager.Communities)
				{
					if (instance2.Editor && (community4.CommunityType == CommunityType.Normal || community4.CommunityType == CommunityType.Looter))
					{
						community2 = community4;
						break;
					}
					if (community4.CommunityType == CommunityType.Temporary)
					{
						community2 = community4;
						break;
					}
				}
			}
			if (community2 == null)
			{
				community2 = Community.Spawn(CommunityType.Temporary);
				community2.CommunityName.Randomise(MathUtil.NonDeterministicRand, unique: true, community2);
			}
			Character character3 = Human.Spawn(raycastResult.Tile, 0f, humanAppearance3, InfectionType.None);
			character3.SetCommunity(community2);
			if (!instance2.Editor)
			{
				character3.InitialCommunity = community2;
			}
			if (CurrentSpawnType == SpawnType.NeutralMilitary)
			{
				character3.LootLocation = "Military";
			}
			else
			{
				character3.RandomizeClothing(nonDeterministicRand, seasonallyAppropriate: true);
				character3.RandomizeName(nonDeterministicRand);
				character3.RandomizePersonality(nonDeterministicRand, PersonalityGroup.NormalFaction);
			}
			switch (CurrentSpawnType)
			{
			case SpawnType.NeutralMelee:
			{
				EquipmentPrototype equipmentPrototype7 = instance.PickRandomItemOfClass(typeof(MeleeWeapon), nonDeterministicRand);
				if (equipmentPrototype7 != null)
				{
					character3.Inventory.Add(character3, Equipment.Spawn(equipmentPrototype7));
				}
				break;
			}
			case SpawnType.NeutralBow:
			{
				EquipmentPrototype equipmentPrototype6 = instance.PickRandomItemOfClass(typeof(Bow), nonDeterministicRand);
				if (equipmentPrototype6 != null)
				{
					character3.Inventory.Add(character3, Equipment.Spawn(equipmentPrototype6));
				}
				break;
			}
			case SpawnType.NeutralGun:
			{
				EquipmentPrototype equipmentPrototype5 = instance.PickRandomItemOfClass(typeof(Gun), nonDeterministicRand);
				if (equipmentPrototype5 != null)
				{
					character3.Inventory.Add(character3, Equipment.Spawn(equipmentPrototype5));
					if (equipmentPrototype5.GetDefaultAmmoPrototype() != null)
					{
						character3.Inventory.Add(character3, Equipment.Spawn(equipmentPrototype5.GetDefaultAmmoPrototype(), nonDeterministicRand.Next(10, 20)));
					}
				}
				break;
			}
			case SpawnType.NeutralMilitary:
				LooterSpawnPoint.SetupSurvivor(character3, "Normal", 1f, 1f, 0.25f, 0.5f, 0.5f, 0.5f, nonDeterministicRand);
				break;
			}
			if (HasAI && !instance2.Editor)
			{
				character3.SetGoal(new SurvivorGoal());
			}
			break;
		}
		case SpawnType.Rabbit:
		{
			RabbitAppearance appearance2 = new RabbitAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, RabbitAppearance.PickRandomAge(nonDeterministicRand));
			Rabbit rabbit = Rabbit.Spawn(raycastResult.Tile, 0f, appearance2);
			rabbit.RandomizeSurvivalFactors(nonDeterministicRand);
			if (instance2.Editor && DebugMenuCommunity.Community != null)
			{
				rabbit.SetCommunity(DebugMenuCommunity.Community);
			}
			if (HasAI && !instance2.Editor)
			{
				rabbit.SetGoal(new AnimalGoal());
			}
			break;
		}
		case SpawnType.Deer:
		{
			DeerAppearance appearance = new DeerAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, DeerAppearance.PickRandomAge(nonDeterministicRand));
			Deer deer = Deer.Spawn(raycastResult.Tile, 0f, appearance);
			deer.RandomizeSurvivalFactors(nonDeterministicRand);
			if (instance2.Editor && DebugMenuCommunity.Community != null)
			{
				deer.SetCommunity(DebugMenuCommunity.Community);
			}
			if (HasAI && !instance2.Editor)
			{
				deer.SetGoal(new AnimalGoal());
			}
			break;
		}
		case SpawnType.Chicken:
		{
			ChickenAppearance chickenAppearance = new ChickenAppearance(nonDeterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, ChickenAppearance.PickRandomAge(nonDeterministicRand));
			chickenAppearance.Randomize(nonDeterministicRand);
			Chicken chicken = Chicken.Spawn(raycastResult.Tile, 0f, chickenAppearance);
			chicken.RandomizeSurvivalFactors(nonDeterministicRand);
			if (instance2.Editor && DebugMenuCommunity.Community != null)
			{
				chicken.SetCommunity(DebugMenuCommunity.Community);
			}
			if (HasAI && !instance2.Editor)
			{
				chicken.SetGoal(new AnimalGoal());
			}
			break;
		}
		}
	}
}
