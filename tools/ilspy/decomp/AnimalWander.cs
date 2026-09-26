using System;
using UnityEngine;

internal class AnimalWander : Wander
{
	public TimeSpan LastMovedWithinRangeOfLeaderTime;

	private static float Morsel = Sun.DayLengthSecs * 0.01f;

	private static float Droplet = Sun.DayLengthSecs * 0.05f;

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref LastMovedWithinRangeOfLeaderTime, 276);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalWander;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (character.CanEatGrass())
		{
			if (Active)
			{
				if (character.Hunger >= Character.HungerCriticalTime || character.Thirst >= Character.ThirstCriticalTime)
				{
					return GoalPriority.Animal_EatGrass_Critical;
				}
			}
			else if (character.Hunger >= Character.HungerExtraCriticalTime || character.Thirst >= Character.ThirstExtraCriticalTime)
			{
				return GoalPriority.Animal_EatGrass_Critical;
			}
		}
		return GoalPriority.Animal_Wander;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		(character as Animal).CurrentPose = AnimalPose.Normal;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Animal animal = character as Animal;
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (animal.InsideBuilding != null)
		{
			return new LeaveBuilding();
		}
		if (animal.IsSitting())
		{
			return new StopSittingGoal();
		}
		if (SubGoal is MoveWithinRangeOfTarget { Success: false })
		{
			LastMovedWithinRangeOfLeaderTime = currentTime;
		}
		if (animal.GetBaseObjectType() == BaseObjectType.Deer && animal.Community != null && animal.Community.SpawnPoint != null && parent is AnimalGoal && Session.Instance.PlayTime - ((AnimalGoal)parent).LastFleeTime >= TimeSpan.FromMinutes(5.0) && currentTime - LastMovedWithinRangeOfLeaderTime >= TimeSpan.FromSeconds(60.0) && (animal.PosXZ - animal.Community.SpawnPoint.PosXZ).sqrMagnitude >= MathUtil.Squared(64f))
		{
			animal.CurrentPose = AnimalPose.Normal;
			return new MoveWithinRange(MovementType.Walk, animal.Community.SpawnPoint.Tile, aiming: false, 0f, 32f, dontOpenOurGates: false, default(StayInRangeParams));
		}
		if (SubGoal is AnimationGoal && MathUtil.RandomChoice((float)(character.Id * 2343) + (float)currentTime.TotalSeconds, 0.5f))
		{
			return new MoveToRandomTile();
		}
		if (SubGoal is Wait)
		{
			if (animal.CanGraze() && animal.CurrentPose == AnimalPose.Grazing)
			{
				character.SetHunger(Math.Max(0f, character.GetHunger() - Morsel));
				character.SetThirst(Math.Max(0f, character.GetThirst() - Droplet));
			}
			return new MoveToRandomTile();
		}
		if (character is Chicken chicken && chicken.GetChickenModel() == ChickenModel.Rooster)
		{
			Session instance = Session.Instance;
			float hourOfDay = instance.HourOfDay;
			if (hourOfDay >= 5f + Mathf.Lerp(0f, 0.5f, MathUtil.RandomFloat((float)chicken.Id + (float)chicken.LastCrowTime.TotalSeconds + 42069f)) && hourOfDay <= 8f && instance.PlayTime - chicken.LastCrowTime >= TimeSpan.FromSeconds(Mathf.Lerp(30f, 60f, MathUtil.RandomFloat((float)chicken.Id + (float)chicken.LastCrowTime.TotalSeconds + 1337f))))
			{
				return new AnimationGoal(ActionAnim.RoosterCrow);
			}
		}
		if (WantToEatGrass(animal))
		{
			animal.CurrentPose = AnimalPose.Grazing;
			if (animal.CanGraze())
			{
				return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(2f, 8f, MathUtil.RandomFloat((float)(character.Id * 8934) + (float)currentTime.TotalSeconds + 8345f))));
			}
			return new AnimationGoal(ActionAnim.Eat);
		}
		animal.CurrentPose = AnimalPose.Normal;
		if (!animal.IsAnyVoiceSoundPlaying())
		{
			animal.PlayIdleSound();
		}
		return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(2f, 8f, MathUtil.RandomFloat((float)(character.Id * 8934) + (float)currentTime.TotalSeconds + 8345f))));
	}

	public bool WantToEatGrass(Animal animal)
	{
		if (!animal.CanEatGrass())
		{
			return false;
		}
		if (animal is Chicken chicken)
		{
			if (chicken.GetHunger() >= Character.HungerCriticalTime)
			{
				return true;
			}
			if (chicken.GetThirst() >= Character.ThirstCriticalTime)
			{
				return true;
			}
			if (Session.Instance.PlayTime - chicken.LastAteFromGround >= TimeSpan.FromSeconds(Mathf.Lerp(30f, 60f, Session.Instance.DeterministicRand.RandomFloat())))
			{
				return true;
			}
		}
		else
		{
			if (animal.CurrentPose == AnimalPose.Grazing && (animal.GetHunger() > Morsel || animal.GetThirst() > Droplet))
			{
				return true;
			}
			if (animal.GetHunger() >= Sun.DayLengthSecs * 0.25f)
			{
				return true;
			}
		}
		return false;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishEat)
		{
			if (character is Chicken chicken)
			{
				chicken.LastAteFromGround = Session.Instance.PlayTime;
				character.SetHunger(Math.Max(Math.Min(character.GetHunger(), Character.HungerCriticalTime), character.GetHunger() - Morsel));
				character.SetThirst(Math.Max(Math.Min(character.GetThirst(), Character.ThirstCriticalTime), character.GetThirst() - Droplet));
			}
			else
			{
				character.SetHunger(Math.Max(0f, character.GetHunger() - Morsel));
				character.SetThirst(Math.Max(0f, character.GetThirst() - Droplet));
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
