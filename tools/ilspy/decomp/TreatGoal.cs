using System;

public class TreatGoal : StateMachineGoal
{
	public Character Recipient;

	private CraftingProp CraftingProp;

	private Recipe CraftingRecipe;

	private EquipmentPrototype TreatProto;

	private LiquidPrototype TreatLiquid;

	private Speech SpeechOnFinish;

	private float TastinessForRecipient;

	public TreatGoal()
	{
	}

	public TreatGoal(Character recipient, Speech speechOnFinish)
	{
		Recipient = recipient;
		SpeechOnFinish = speechOnFinish;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TreatGoal;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Recipient);
		reflector.AddAfter(ref CraftingRecipe, 540);
		reflector.AddAfter(ref CraftingProp, 539);
		reflector.Add(ref TreatProto);
		reflector.Add(ref TreatLiquid);
		reflector.Add(ref SpeechOnFinish);
		reflector.Add(ref TastinessForRecipient);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Recipient != null && Recipient.ConsciousAndNotZombie && Recipient.SquadLeader == character && CraftingProp != null && !CraftingProp.IsDestroyed() && !CraftingProp.Deleted)
		{
			return CraftingRecipe != null;
		}
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		CraftingProp = PickCraftingPropAndRecipe(character, GetTargetCharacter(), out CraftingRecipe);
		if (CraftingProp != null && CraftingRecipe != null)
		{
			TreatProto = CraftingRecipe.ProductPrototype;
			TreatLiquid = CraftingRecipe.ProductLiquidPrototype;
			CraftGoal craftGoal = new CraftGoal();
			craftGoal.StartRecipe(character, CraftingRecipe, null, 1, CraftingProp.GetCentreTile(), Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: false, urgent: false);
			SetSubGoal(character, parent, craftGoal);
			Recipient.Follow(character, canSetHangOutLocation: false);
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (Recipient != null && Recipient.SquadLeader == character)
		{
			Recipient.Follow(null, canSetHangOutLocation: false);
		}
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is CraftGoal craftGoal)
		{
			Equipment food = CraftingProp.Inventory.GetFood(CraftingProp, character, includeGifts: false, ignoreIfUsingForCrafting: false, forSharing: false);
			if (food != null)
			{
				TreatProto = food.GetPrototype();
				if (food.GetLiquidContentsType() != null)
				{
					TreatLiquid = food.GetLiquidContentsType();
				}
				TastinessForRecipient = Recipient.CalcTastiness(food);
				if (craftGoal.UsedHumanIngredients)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(Recipient, character, food, SpeechSituation.FedHumanMeat, new MemoryParam((int)TastinessForRecipient));
					if (speechForSituation != null)
					{
						Recipient.Speak(speechForSituation, character, food, new MemoryParam((int)TastinessForRecipient));
						Recipient.SetRecentActivity(RecentActivityType.Conversation, character);
						character.SetRecentActivity(RecentActivityType.Conversation, Recipient);
						return null;
					}
				}
				if (food.GetLiquidContentsType() != null)
				{
					float num = food.GetLiquidContentsAmount() * 0.5f;
					character.IssueCommandToFollowers(FollowCommand.EatAroundFire, CraftingProp.Tile, TreatProto, TreatLiquid, num);
					if (character.Inventory.GetBestLiquidContainerToFill(TreatLiquid) != null)
					{
						return new MoveToAndInteractGoal(character, CraftingProp, InteractionType.FillFromPot, food, num, MovementType.Walk);
					}
					return new MoveToAndInteractGoal(character, CraftingProp, InteractionType.EatFromPot, food, num, MovementType.Walk);
				}
				character.IssueCommandToFollowers(FollowCommand.EatAroundFire, CraftingProp.Tile, TreatProto, TreatLiquid, 1f);
				if (food.GetAmount() > 1)
				{
					return new MoveToAndTake(character, CraftingProp, food, 1, ignoreWeight: true, MovementType.Walk);
				}
				return new Wait(TimeSpan.FromSeconds(2.0));
			}
		}
		if (SubGoal is MoveToAndInteractGoal)
		{
			return new SitAroundFireGoal(character, CraftingProp, MovementType.Walk);
		}
		if (SubGoal is MoveToAndTake)
		{
			return new SitAroundFireGoal(character, CraftingProp, MovementType.Walk);
		}
		if (SubGoal is SitAroundFireGoal sitAroundFireGoal && sitAroundFireGoal.GetTargetProp() != null)
		{
			Equipment equipment = null;
			if (character.GetHunger() > 0f)
			{
				if (TreatLiquid != null)
				{
					equipment = character.Inventory.FindBestItemWithLiquid(TreatLiquid);
				}
				else if (TreatProto != null)
				{
					equipment = character.Inventory.FindItemOfType(TreatProto);
				}
			}
			if (equipment != null)
			{
				return new EatGoal(equipment);
			}
			return new Wait(TimeSpan.FromSeconds(2.0));
		}
		if (SubGoal is EatGoal)
		{
			return new Wait(TimeSpan.FromSeconds(2.0));
		}
		if (SubGoal is Wait)
		{
			if (Recipient.HasFollowCommandActive())
			{
				return new Wait(TimeSpan.FromSeconds(2.0));
			}
			if (SpeechOnFinish != null)
			{
				return new Conversation(character, Recipient, null, new MemoryParam((int)TastinessForRecipient), SpeechOnFinish, controlledByPlayer: false, null, null, default(MemoryParam));
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public static CraftingProp PickCraftingPropAndRecipe(Character character, Character targetCharacter, out Recipe recipe)
	{
		recipe = null;
		if (character.Community == null)
		{
			return null;
		}
		float num = MathUtil.Squared(64f);
		if (character.Community.GetNearestBuildingOfType(character.Tile, typeof(Campfire), num, character, targetCharacter) is Campfire craftingProp)
		{
			recipe = CraftGoal.PickCookRecipe(character, craftingProp, checkIfAllowedToEatIt: true, prioritiseSkinning: false, null);
			if (recipe != null && ((recipe.ProductPrototype != null && recipe.ProductPrototype.Tastiness > 0f) || (recipe.ProductLiquidPrototype != null && recipe.ProductLiquidPrototype.Tastiness > 0f)))
			{
				return character.Community.GetNearestCraftingPropForRecipe(recipe, character, num, character, targetCharacter) as CraftingProp;
			}
		}
		return null;
	}
}
