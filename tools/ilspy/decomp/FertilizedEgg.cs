using System;

public class FertilizedEgg : Equipment
{
	public float EmbryoGrowth;

	public float EmbryoDeath;

	public Chicken Mother;

	public static float DaysToHatch = 4f;

	public static float DaysToDie = 1f;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FertilizedEgg;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 436)
		{
			TimeSpan value = TimeSpan.Zero;
			reflector.Add(ref value);
			LastRareUpdateTime = value;
		}
		reflector.Add(ref EmbryoGrowth);
		reflector.Add(ref EmbryoDeath);
		reflector.Add(ref Mother);
	}

	public override bool WantRareUpdate()
	{
		return true;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		Session instance = Session.Instance;
		TimeSpan playTime = instance.PlayTime;
		Chicken chicken = null;
		Building building = InventoryOwner as Building;
		if (building != null)
		{
			Character[] inhabitants = building.Inhabitants;
			for (int i = 0; i < inhabitants.Length; i++)
			{
				if (inhabitants[i] is Chicken chicken2 && chicken2.IsBroody())
				{
					chicken = chicken2;
					if (chicken == Mother)
					{
						break;
					}
				}
			}
		}
		if (chicken != null)
		{
			EmbryoGrowth = Math.Min(1f, EmbryoGrowth + (float)(playTime - LastRareUpdateTime).TotalSeconds / (DaysToHatch * Sun.DayLengthSecs));
			if (EmbryoGrowth >= 1f)
			{
				chicken.BroodyStartTime = Target.Never;
				CustomRandom deterministicRand = instance.DeterministicRand;
				ChickenAppearance chickenAppearance = new ChickenAppearance(deterministicRand.RandomChoice(0.5f) ? GenderType.Female : GenderType.Male, 0f);
				chickenAppearance.ChickenColor = ChickenAppearance.PickRandomChickenColor(deterministicRand);
				Chicken chicken3 = Chicken.Spawn(GameTerrain.Instance.GetTileCoordForPos(building.GetEntrancePos(0)), deterministicRand.RandomFloat() * (MathF.PI * 2f), chickenAppearance);
				Community community = building.Community;
				if (community == null && Mother != null)
				{
					community = Mother.Community;
				}
				if (community == null)
				{
					community = Community.Spawn(CommunityType.Temporary);
					community.CommunityName.Randomise(Session.Instance.DeterministicRand, unique: true, community);
				}
				chicken3.SetCommunity(community);
				if (community.CommunityType != CommunityType.Player)
				{
					chicken3.RandomizeName(deterministicRand);
				}
				if (building.GetInhabitantCount() < building.GetInhabitantSlotDefs().Length)
				{
					building.OnCharacterEnter(chicken3, wasOrderedInsideBuilding: false);
				}
				chicken3.SetGoal(new AnimalGoal());
			}
		}
		else
		{
			EmbryoDeath = Math.Min(1f, EmbryoDeath + (float)(playTime - LastRareUpdateTime).TotalSeconds / (DaysToDie * Sun.DayLengthSecs));
		}
		LastRareUpdateTime = playTime;
		stillNeedUpdating = true;
	}

	public override bool PropWantDelete()
	{
		if (!(EmbryoGrowth >= 1f))
		{
			return EmbryoDeath >= 1f;
		}
		return true;
	}
}
