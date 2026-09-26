using System;
using UnityEngine;

public struct RoadDestinationTraits : IReflectable
{
	public float Urbanized;

	public float Populated;

	public float Infected;

	public float InvisibleStrain;

	public RoadDestinationType Type;

	public int SkillCapBonus;

	public static RoadDestinationTraits Create()
	{
		return new RoadDestinationTraits
		{
			Urbanized = 0.5f,
			Infected = 0.5f,
			Populated = 0.5f,
			InvisibleStrain = 0.5f
		};
	}

	public static RoadDestinationTraits CreateRandomized(CustomRandom rand)
	{
		RoadDestinationTraits result = new RoadDestinationTraits
		{
			Urbanized = rand.RandomFloat(),
			Infected = rand.RandomFloat(),
			Populated = rand.RandomFloat(),
			InvisibleStrain = rand.RandomFloat()
		};
		while (result.Infected + result.InvisibleStrain < 0.5f)
		{
			result.Infected += rand.RandomFloat() * 0.25f;
			result.InvisibleStrain += rand.RandomFloat() * 0.25f;
		}
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Urbanized);
		reflector.Add(ref Populated);
		reflector.Add(ref Infected);
		reflector.Add(ref InvisibleStrain);
		reflector.AddAfter(ref Type, 602);
		reflector.AddAfter(ref SkillCapBonus, 602);
	}

	public DifficultySettings GenerateDifficultySettings(DifficultySettings oldSettings)
	{
		CustomRandom customRandom = new CustomRandom(Session.Instance.GameUniqueId + Session.Instance.HitTheRoadCount * 7351);
		DifficultySettings difficultySettings = new DifficultySettings();
		difficultySettings.SaveTokensRequired = oldSettings.SaveTokensRequired;
		difficultySettings.TradersHaveSaveTokens = oldSettings.TradersHaveSaveTokens;
		difficultySettings.FriendlyFireSplashDamage = oldSettings.FriendlyFireSplashDamage;
		difficultySettings.ZombieRespawnDays = oldSettings.ZombieRespawnDays;
		difficultySettings.ZombieCrippledPercentage = oldSettings.ZombieCrippledPercentage;
		difficultySettings.InvisibleStrainIncrementPerYear = oldSettings.InvisibleStrainIncrementPerYear;
		difficultySettings.SurvivorRepopulationDays = oldSettings.SurvivorRepopulationDays;
		switch (Type)
		{
		case RoadDestinationType.Random:
			difficultySettings.HordeDensity = Infected * customRandom.RandomFloat() * 100f;
			difficultySettings.RaiderDensity = Populated * customRandom.RandomFloat() * 100f;
			difficultySettings.TraderDensity = Populated * customRandom.RandomFloat() * 100f;
			difficultySettings.RefugeeDensity = Populated * customRandom.RandomFloat() * 100f;
			difficultySettings.GreenStrainDensity = Infected * customRandom.RandomFloat() * 100f;
			difficultySettings.BlueStrainDensity = Infected * customRandom.RandomFloat() * 100f;
			difficultySettings.RedStrainDensity = Infected * customRandom.RandomFloat() * 100f;
			difficultySettings.WhiteStrainDensity = Infected * customRandom.RandomFloat() * 100f;
			difficultySettings.InvisibleStrainPercentage = InvisibleStrain * 10f;
			difficultySettings.SurvivorCampDensity = Populated * 200f;
			difficultySettings.SurvivorCampLooterPercentage = Mathf.Lerp(25f, 75f, customRandom.RandomFloat());
			difficultySettings.LootDensity = (1f - Populated) * Infected * 200f;
			difficultySettings.RoadDensity = Mathf.Max(25f, Urbanized * 200f);
			difficultySettings.VehicleDensity = Mathf.Max(25f, Urbanized * 200f);
			difficultySettings.TownDensity = Urbanized * 200f;
			difficultySettings.CountrysidePropDensity = (1f - Urbanized) * 200f;
			difficultySettings.RiverDensity = customRandom.RandomFloat() * 200f;
			difficultySettings.RabbitDensity = (1f - Urbanized) * (1f - Populated) * 200f;
			difficultySettings.DeerDensity = (1f - Urbanized) * (1f - Populated) * 200f;
			difficultySettings.FlintDensity = customRandom.RandomFloat() * 200f;
			difficultySettings.IronOreDensity = customRandom.RandomFloat() * 200f;
			difficultySettings.LeadOreDensity = customRandom.RandomFloat() * 200f;
			break;
		case RoadDestinationType.KeepCurrentSettings:
			difficultySettings.HordeDensity = oldSettings.HordeDensity;
			difficultySettings.RaiderDensity = oldSettings.RaiderDensity;
			difficultySettings.TraderDensity = oldSettings.TraderDensity;
			difficultySettings.RefugeeDensity = oldSettings.RefugeeDensity;
			difficultySettings.GreenStrainDensity = oldSettings.GreenStrainDensity;
			difficultySettings.BlueStrainDensity = oldSettings.BlueStrainDensity;
			difficultySettings.RedStrainDensity = oldSettings.RedStrainDensity;
			difficultySettings.WhiteStrainDensity = oldSettings.WhiteStrainDensity;
			difficultySettings.InvisibleStrainPercentage = oldSettings.InvisibleStrainPercentage;
			difficultySettings.SurvivorCampDensity = oldSettings.SurvivorCampDensity;
			difficultySettings.SurvivorCampLooterPercentage = oldSettings.SurvivorCampLooterPercentage;
			difficultySettings.LootDensity = oldSettings.LootDensity;
			difficultySettings.RoadDensity = oldSettings.RoadDensity;
			difficultySettings.VehicleDensity = oldSettings.VehicleDensity;
			difficultySettings.TownDensity = oldSettings.TownDensity;
			difficultySettings.CountrysidePropDensity = oldSettings.CountrysidePropDensity;
			difficultySettings.RiverDensity = oldSettings.RiverDensity;
			difficultySettings.RabbitDensity = oldSettings.RabbitDensity;
			difficultySettings.DeerDensity = oldSettings.DeerDensity;
			difficultySettings.FlintDensity = oldSettings.FlintDensity;
			difficultySettings.IronOreDensity = oldSettings.IronOreDensity;
			difficultySettings.LeadOreDensity = oldSettings.LeadOreDensity;
			break;
		case RoadDestinationType.HarderSettings:
			difficultySettings.HordeDensity = Math.Min(100f, oldSettings.HordeDensity + (oldSettings.TraderDensity + oldSettings.RefugeeDensity) * 0.25f);
			difficultySettings.RaiderDensity = Math.Min(100f, oldSettings.RaiderDensity + (oldSettings.TraderDensity + oldSettings.RefugeeDensity) * 0.25f);
			difficultySettings.TraderDensity = oldSettings.TraderDensity * 0.5f;
			difficultySettings.RefugeeDensity = oldSettings.RefugeeDensity * 0.5f;
			difficultySettings.GreenStrainDensity = oldSettings.GreenStrainDensity * 0.5f;
			difficultySettings.BlueStrainDensity = oldSettings.BlueStrainDensity * 0.5f;
			difficultySettings.RedStrainDensity = Math.Min(100f, oldSettings.RedStrainDensity + (oldSettings.GreenStrainDensity + oldSettings.BlueStrainDensity) * 0.25f);
			difficultySettings.WhiteStrainDensity = Math.Min(100f, oldSettings.WhiteStrainDensity + (oldSettings.GreenStrainDensity + oldSettings.BlueStrainDensity) * 0.25f);
			difficultySettings.InvisibleStrainPercentage = Math.Min(100f, oldSettings.InvisibleStrainPercentage + 10f);
			difficultySettings.SurvivorCampLooterPercentage = Mathf.Lerp(oldSettings.SurvivorCampLooterPercentage, 100f, 0.25f);
			difficultySettings.LootDensity = Mathf.Lerp(oldSettings.LootDensity, 0f, 0.5f);
			difficultySettings.InvisibleStrainIncrementPerYear = Math.Min(oldSettings.InvisibleStrainIncrementPerYear, 5f);
			difficultySettings.ZombieCrippledPercentage = Mathf.Lerp(oldSettings.ZombieCrippledPercentage, 0f, 0.5f);
			difficultySettings.FriendlyFireSplashDamage = 100f;
			difficultySettings.SurvivorCampDensity = oldSettings.SurvivorCampDensity;
			difficultySettings.RoadDensity = oldSettings.RoadDensity;
			difficultySettings.VehicleDensity = oldSettings.VehicleDensity;
			difficultySettings.TownDensity = oldSettings.TownDensity;
			difficultySettings.CountrysidePropDensity = oldSettings.CountrysidePropDensity;
			difficultySettings.RiverDensity = Math.Max(Math.Min(50f, oldSettings.RiverDensity), oldSettings.RiverDensity * 0.75f);
			difficultySettings.RabbitDensity = Math.Max(Math.Min(50f, oldSettings.RabbitDensity), oldSettings.RabbitDensity * 0.75f);
			difficultySettings.DeerDensity = Math.Max(Math.Min(50f, oldSettings.DeerDensity), oldSettings.DeerDensity * 0.75f);
			difficultySettings.FlintDensity = Math.Max(Math.Min(50f, oldSettings.FlintDensity), oldSettings.FlintDensity * 0.75f);
			difficultySettings.IronOreDensity = Math.Max(Math.Min(50f, oldSettings.IronOreDensity), oldSettings.IronOreDensity * 0.75f);
			difficultySettings.LeadOreDensity = Math.Max(Math.Min(50f, oldSettings.LeadOreDensity), oldSettings.LeadOreDensity * 0.75f);
			break;
		}
		return difficultySettings;
	}

	public void CalcSkillCapBonus(DifficultySettings oldSettings)
	{
		DifficultySettings difficultySettings = GenerateDifficultySettings(oldSettings);
		float num = (0f + difficultySettings.GreenStrainDensity + difficultySettings.BlueStrainDensity * 2f + difficultySettings.RedStrainDensity * 3f + difficultySettings.WhiteStrainDensity * 4f) * (1f - difficultySettings.ZombieCrippledPercentage / 100f);
		float num2 = 0f;
		num2 += difficultySettings.InvisibleStrainPercentage * 10f;
		num2 += difficultySettings.InvisibleStrainIncrementPerYear * 10f;
		num2 = Math.Min(num2, 1000f);
		float num3 = difficultySettings.WhiteStrainDensity + difficultySettings.RedStrainDensity + difficultySettings.BlueStrainDensity + difficultySettings.GreenStrainDensity;
		float num4 = difficultySettings.RedStrainDensity + difficultySettings.BlueStrainDensity + difficultySettings.GreenStrainDensity;
		float num5 = difficultySettings.BlueStrainDensity + difficultySettings.GreenStrainDensity;
		float num6 = ((num3 == 0f) ? 0f : (difficultySettings.WhiteStrainDensity / num3));
		num6 += ((num4 == 0f) ? 0f : (difficultySettings.RedStrainDensity / num4 * (1f - num6) * 0.6f));
		num6 += ((num5 == 0f) ? 0f : (difficultySettings.BlueStrainDensity / num5 * (1f - num6) * 0.33333f));
		num6 *= 1f - difficultySettings.ZombieCrippledPercentage / 100f;
		float num7 = num + num2;
		num7 += difficultySettings.RaiderDensity * 4f;
		num7 += difficultySettings.HordeDensity * 8f * num6;
		num7 -= difficultySettings.TraderDensity * 2f;
		num7 -= difficultySettings.RefugeeDensity * 2f;
		num7 += difficultySettings.SurvivorCampLooterPercentage * 4f;
		num7 -= difficultySettings.LootDensity * 2f;
		if (difficultySettings.SaveTokensRequired && !difficultySettings.TradersHaveSaveTokens)
		{
			num7 += 200f;
		}
		num7 += difficultySettings.FriendlyFireSplashDamage * 2f;
		num7 -= difficultySettings.RabbitDensity * 0.5f;
		num7 -= difficultySettings.DeerDensity * 0.5f;
		num7 -= difficultySettings.FlintDensity * 0.5f;
		num7 -= difficultySettings.IronOreDensity * 0.5f;
		num7 -= difficultySettings.LeadOreDensity * 0.5f;
		if (num7 >= 2000f)
		{
			SkillCapBonus = 5;
		}
		else if (num7 >= 1000f)
		{
			SkillCapBonus = 4;
		}
		else if (num7 >= 500f)
		{
			SkillCapBonus = 3;
		}
		else if (num7 >= 100f)
		{
			SkillCapBonus = 2;
		}
		else
		{
			SkillCapBonus = 1;
		}
	}
}
