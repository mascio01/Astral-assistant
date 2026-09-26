using UnityEngine;

public class DifficultySettings : IReflectable
{
	public string DifficultyName = "";

	public string NativeName = "";

	public string NativeDescription = "";

	public bool SaveTokensRequired;

	public bool TradersHaveSaveTokens = true;

	public float FriendlyFireSplashDamage;

	public float ZombieCrippledPercentage;

	public float ZombieRespawnDays = 1f;

	public float GreenStrainDensity = 25f;

	public float BlueStrainDensity = 25f;

	public float RedStrainDensity = 25f;

	public float WhiteStrainDensity = 25f;

	public float InvisibleStrainPercentage = 5f;

	public float InvisibleStrainIncrementPerYear;

	public float SurvivorCampDensity = 100f;

	public float SurvivorCampLooterPercentage = 50f;

	public float SurvivorRepopulationDays = 1f;

	public float HordeDensity = 25f;

	public float RaiderDensity = 25f;

	public float RefugeeDensity = 25f;

	public float TraderDensity = 25f;

	public float RoadDensity = 100f;

	public float TownDensity = 100f;

	public float VehicleDensity = 100f;

	public float CountrysidePropDensity = 100f;

	public float LootDensity = 100f;

	public float RiverDensity = 100f;

	public float RabbitDensity = 100f;

	public float DeerDensity = 100f;

	public float FlintDensity = 100f;

	public float LeadOreDensity = 100f;

	public float IronOreDensity = 100f;

	public string GetNameKey()
	{
		return "DIFFICULTY_NAME_" + DifficultyName;
	}

	public string GetDescriptionKey()
	{
		return "DIFFICULTY_DESC_" + DifficultyName;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref DifficultyName);
		reflector.Add(ref SaveTokensRequired);
		reflector.AddAfter(ref TradersHaveSaveTokens, 385);
		reflector.AddAfter(ref FriendlyFireSplashDamage, 301);
		reflector.Add(ref ZombieCrippledPercentage);
		reflector.Add(ref ZombieRespawnDays);
		reflector.Add(ref GreenStrainDensity);
		reflector.Add(ref BlueStrainDensity);
		reflector.Add(ref RedStrainDensity);
		reflector.Add(ref WhiteStrainDensity);
		reflector.Add(ref InvisibleStrainPercentage);
		reflector.AddAfter(ref InvisibleStrainIncrementPerYear, 551);
		reflector.Add(ref SurvivorCampDensity);
		reflector.Add(ref SurvivorCampLooterPercentage);
		reflector.AddAfter(ref SurvivorRepopulationDays, 573);
		reflector.Add(ref HordeDensity);
		reflector.Add(ref RaiderDensity);
		reflector.Add(ref RefugeeDensity);
		reflector.Add(ref TraderDensity);
		if (reflector.Version < 253)
		{
			float value = 1f;
			reflector.Add(ref value);
		}
		reflector.Add(ref RoadDensity);
		reflector.Add(ref TownDensity);
		reflector.Add(ref VehicleDensity);
		reflector.Add(ref CountrysidePropDensity);
		reflector.Add(ref LootDensity);
		reflector.Add(ref RiverDensity);
		reflector.Add(ref RabbitDensity);
		reflector.AddAfter(ref DeerDensity, 288);
		reflector.AddAfter(ref FlintDensity, 270);
		reflector.AddAfter(ref LeadOreDensity, 270);
		reflector.AddAfter(ref IronOreDensity, 270);
	}

	public bool Matches(DifficultySettings difficultySettings)
	{
		if (SaveTokensRequired == difficultySettings.SaveTokensRequired && TradersHaveSaveTokens == difficultySettings.TradersHaveSaveTokens && FriendlyFireSplashDamage == difficultySettings.FriendlyFireSplashDamage && ZombieCrippledPercentage == difficultySettings.ZombieCrippledPercentage && ZombieRespawnDays == difficultySettings.ZombieRespawnDays && GreenStrainDensity == difficultySettings.GreenStrainDensity && BlueStrainDensity == difficultySettings.BlueStrainDensity && RedStrainDensity == difficultySettings.RedStrainDensity && WhiteStrainDensity == difficultySettings.WhiteStrainDensity && InvisibleStrainPercentage == difficultySettings.InvisibleStrainPercentage && InvisibleStrainIncrementPerYear == difficultySettings.InvisibleStrainIncrementPerYear && SurvivorCampDensity == difficultySettings.SurvivorCampDensity && SurvivorCampLooterPercentage == difficultySettings.SurvivorCampLooterPercentage && SurvivorRepopulationDays == difficultySettings.SurvivorRepopulationDays && HordeDensity == difficultySettings.HordeDensity && RaiderDensity == difficultySettings.RaiderDensity && RefugeeDensity == difficultySettings.RefugeeDensity && TraderDensity == difficultySettings.TraderDensity && RoadDensity == difficultySettings.RoadDensity && TownDensity == difficultySettings.TownDensity && VehicleDensity == difficultySettings.VehicleDensity && CountrysidePropDensity == difficultySettings.CountrysidePropDensity && LootDensity == difficultySettings.LootDensity && RiverDensity == difficultySettings.RiverDensity && RabbitDensity == difficultySettings.RabbitDensity && DeerDensity == difficultySettings.DeerDensity && FlintDensity == difficultySettings.FlintDensity && LeadOreDensity == difficultySettings.LeadOreDensity)
		{
			return IronOreDensity == difficultySettings.IronOreDensity;
		}
		return false;
	}

	public DifficultySettings MakeCopy()
	{
		return new DifficultySettings
		{
			DifficultyName = DifficultyName,
			SaveTokensRequired = SaveTokensRequired,
			TradersHaveSaveTokens = TradersHaveSaveTokens,
			FriendlyFireSplashDamage = FriendlyFireSplashDamage,
			ZombieCrippledPercentage = ZombieCrippledPercentage,
			ZombieRespawnDays = ZombieRespawnDays,
			GreenStrainDensity = GreenStrainDensity,
			BlueStrainDensity = BlueStrainDensity,
			RedStrainDensity = RedStrainDensity,
			WhiteStrainDensity = WhiteStrainDensity,
			InvisibleStrainPercentage = InvisibleStrainPercentage,
			InvisibleStrainIncrementPerYear = InvisibleStrainIncrementPerYear,
			SurvivorCampDensity = SurvivorCampDensity,
			SurvivorCampLooterPercentage = SurvivorCampLooterPercentage,
			SurvivorRepopulationDays = SurvivorRepopulationDays,
			HordeDensity = HordeDensity,
			RaiderDensity = RaiderDensity,
			RefugeeDensity = RefugeeDensity,
			TraderDensity = TraderDensity,
			RoadDensity = RoadDensity,
			TownDensity = TownDensity,
			VehicleDensity = VehicleDensity,
			CountrysidePropDensity = CountrysidePropDensity,
			LootDensity = LootDensity,
			RiverDensity = RiverDensity,
			RabbitDensity = RabbitDensity,
			DeerDensity = DeerDensity,
			FlintDensity = FlintDensity,
			LeadOreDensity = LeadOreDensity,
			IronOreDensity = IronOreDensity
		};
	}

	public float GetTotalZombieDensity()
	{
		return GreenStrainDensity + BlueStrainDensity + RedStrainDensity + WhiteStrainDensity;
	}

	public InfectionType PickStrain(float t)
	{
		t = Mathf.Clamp01(t);
		float[] array = new float[4] { GreenStrainDensity, BlueStrainDensity, RedStrainDensity, WhiteStrainDensity };
		float totalZombieDensity = GetTotalZombieDensity();
		if (totalZombieDensity <= 0f)
		{
			return InfectionType.None;
		}
		InfectionType result = InfectionType.None;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] /= totalZombieDensity;
			if (array[i] > 0f)
			{
				result = (InfectionType)i;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			t -= array[j];
			if (t < 0f)
			{
				return (InfectionType)(1 + j);
			}
		}
		return result;
	}

	public float GetTotalHunterDensity()
	{
		return HordeDensity + RaiderDensity + RefugeeDensity + TraderDensity;
	}

	public float GetDifficultySetting(DifficultySetting difficultySetting)
	{
		return difficultySetting switch
		{
			DifficultySetting.None => 1f, 
			DifficultySetting.HordeDensity => HordeDensity / 25f, 
			DifficultySetting.RaiderDensity => RaiderDensity / 25f, 
			DifficultySetting.RefugeeDensity => RefugeeDensity / 25f, 
			DifficultySetting.TraderDensity => TraderDensity / 25f, 
			_ => 1f, 
		};
	}

	public float GetMineralDensity(MineralType mineralType)
	{
		return mineralType switch
		{
			MineralType.Flint => FlintDensity / 100f, 
			MineralType.Lead => LeadOreDensity / 100f, 
			MineralType.Iron => IronOreDensity / 100f, 
			_ => 1f, 
		};
	}
}
