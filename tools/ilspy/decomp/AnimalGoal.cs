using System;
using UnityEngine;

public class AnimalGoal : PrioritiserGoal
{
	public TimeSpan LastFleeTime = Target.Never;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref LastFleeTime, 286);
		if (reflector.IsDeserialising && character is Chicken)
		{
			if (reflector.Version < 308)
			{
				AddSubGoal(new AnimalKeepWarm());
			}
			if (reflector.Version < 309 && character.GetGender() == GenderType.Female)
			{
				AddSubGoal(new ChickenLayEggGoal());
			}
			if (reflector.Version < 313 && character.GetGender() == GenderType.Male)
			{
				AddSubGoal(new AnimalMating());
			}
			if (reflector.Version < 314)
			{
				AddSubGoal(new AnimalSleepGoal());
			}
			if (reflector.Version < 315)
			{
				AddSubGoal(new AftermathGoal());
			}
			if (reflector.Version < 316 && character.GetGender() == GenderType.Female)
			{
				AddSubGoal(new ChickenBroodGoal());
			}
			if (reflector.Version < 317)
			{
				AddSubGoal(new AnimalLookForFoodGoal());
			}
			if (reflector.Version < 318)
			{
				AddSubGoal(new AnimalDrinkGoal());
			}
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalGoal;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		AddSubGoal(new AnimalWander());
		if (character is Chicken)
		{
			AddSubGoal(new AnimalKeepWarm());
			AddSubGoal(new AnimalSleepGoal());
			AddSubGoal(new AnimalLookForFoodGoal());
			AddSubGoal(new AnimalDrinkGoal());
			AddSubGoal(new AftermathGoal());
			if (character.GetGender() == GenderType.Male)
			{
				AddSubGoal(new AnimalMating());
			}
			else
			{
				AddSubGoal(new ChickenLayEggGoal());
				AddSubGoal(new ChickenBroodGoal());
			}
		}
		AddSubGoal(new AnimalFeedOnNearbySnackGoal());
		AddSubGoal(new RabbitAlertGoal());
		AddSubGoal(new RabbitFleeGoal());
		AddSubGoal(new UnconsciousGoal());
		base.OnActivate(character, parent);
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal == null)
		{
			return null;
		}
		return SubGoal.GetOverheadActionIcon(character);
	}

	public void FeedFromHand(Character character, Goal parent, Character feeder)
	{
		if (SubGoal is AnimalFeedOnNearbySnackGoal animalFeedOnNearbySnackGoal)
		{
			animalFeedOnNearbySnackGoal.FeedFromHand(character, parent, feeder);
		}
	}
}
