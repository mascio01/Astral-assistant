public class CraftAnim : AnimationGoal
{
	public Recipe FollowingRecipe;

	public Equipment UsingItem;

	public float TimeTaken;

	public bool Urgent;

	public CraftAnim()
	{
	}

	public CraftAnim(ActionAnim anim, Recipe recipe, Equipment usingItem, bool urgent)
		: base(anim, urgent ? RoleGoal.UrgentWorkSpeed : 1f)
	{
		FollowingRecipe = recipe;
		UsingItem = usingItem;
		Urgent = urgent;
	}

	public CraftAnim(Character character, TileObject targetObj, ActionAnim anim, Recipe recipe, Equipment usingItem, bool urgent)
		: base(character, targetObj, anim, urgent ? RoleGoal.UrgentWorkSpeed : 1f)
	{
		FollowingRecipe = recipe;
		UsingItem = usingItem;
		Urgent = urgent;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.CraftAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FollowingRecipe);
		reflector.Add(ref UsingItem);
		reflector.Add(ref TimeTaken);
		reflector.AddAfter(ref Urgent, 407);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		float num = (float)(Session.Instance.PlayTime - character.LastThinkTime).TotalSeconds * (Urgent ? RoleGoal.UrgentWorkSpeed : 1f);
		TimeTaken += num;
		if (!(parent is CraftGoal craftGoal))
		{
			return;
		}
		if (craftGoal.FollowingRecipe != FollowingRecipe || FollowingRecipe == null)
		{
			Finished = true;
			return;
		}
		switch (FollowingRecipe.RecipeType)
		{
		case RecipeType.Campfire_Pot:
		case RecipeType.Campfire_FryingPan:
			if (GameTerrain.Instance.GetCraftingPropOnTile(craftGoal.DestTile.x, craftGoal.DestTile.y) is Campfire campfire)
			{
				if (!campfire.IsBurning())
				{
					Finished = true;
				}
				else
				{
					campfire.StirThePot(num);
				}
			}
			else
			{
				Finished = true;
			}
			return;
		case RecipeType.Campfire_SpitRoast_Rabbit:
		case RecipeType.Campfire_SpitRoast_Chicken:
		case RecipeType.Campfire_SpitRoast_Venison:
		case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
		case RecipeType.Still:
		case RecipeType.Kiln:
		case RecipeType.Nitrary:
			Finished = true;
			return;
		case RecipeType.Forge:
		case RecipeType.Workbench:
		{
			CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(craftGoal.DestTile.x, craftGoal.DestTile.y);
			if (craftingPropOnTile != null)
			{
				craftingPropOnTile.Craft(num);
			}
			else
			{
				Finished = true;
			}
			return;
		}
		}
		if (!(TimeTaken >= FollowingRecipe.CraftingTime))
		{
			return;
		}
		bool addToGatheredItems = craftGoal.DesiredAmount == int.MaxValue;
		if (FollowingRecipe.UseIngredientsAndCreateProduct(character, character.Community, UsingItem, craftGoal.DestTile, craftGoal.DestOrientationType, addToGatheredItems, craftGoal.WantCheckEquipmentPolicy(character)) > 0f)
		{
			if (UsingItem != null && !character.InventoryContains(UsingItem))
			{
				UsingItem = null;
			}
			TimeTaken -= FollowingRecipe.CraftingTime;
			craftGoal.SubtractDesiredAmount();
			RoleInfo roleInfoBeingPerformed = craftGoal.GetRoleInfoBeingPerformed(character);
			if (roleInfoBeingPerformed.Role != Role.None)
			{
				if (roleInfoBeingPerformed.Role == Role.Cook || FollowingRecipe == null || character.Community.HasReachedCraftingLimitForProduct(FollowingRecipe))
				{
					character.SetRoleInProgress(roleInfoBeingPerformed, inProgress: false);
				}
				character.OnRoleSucceeded();
			}
			if (craftGoal.DesiredAmount <= 0)
			{
				Finished = true;
			}
			else if (craftGoal.DesiredAmount == int.MaxValue && character.Community != null)
			{
				if (FollowingRecipe.ProductPrototype != null && character.Community.CountInventoryItemsOfType(FollowingRecipe.ProductPrototype) >= character.Community.GetCraftingLimit(FollowingRecipe.ProductPrototype))
				{
					Finished = true;
				}
				else if (FollowingRecipe.ProductLiquidPrototype != null && character.Community.GetTotalLiquid(FollowingRecipe.ProductLiquidPrototype) >= (float)character.Community.GetCraftingLimit(FollowingRecipe.ProductLiquidPrototype))
				{
					Finished = true;
				}
			}
			if (!Finished && (!FollowingRecipe.HasAllIngredients(character, character, UsingItem, craftGoal.WantCheckEquipmentPolicy(character) ? character : null) || !FollowingRecipe.HasSuitableContainer(character, UsingItem)))
			{
				Finished = true;
			}
		}
		else
		{
			Finished = true;
		}
		if (character.HasRole(Role.Cook) && craftGoal.DesiredAmount == int.MaxValue)
		{
			craftGoal.StopRecipe(character);
			Finished = true;
		}
	}
}
