using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForRecipeType : Attribute
{
	public List<RecipeType> RecipeTypes = new List<RecipeType>();

	public OnlyVisibleForRecipeType(RecipeType recipeType1)
	{
		RecipeTypes.Add(recipeType1);
	}

	public OnlyVisibleForRecipeType(RecipeType recipeType1, RecipeType recipeType2)
	{
		RecipeTypes.Add(recipeType1);
		RecipeTypes.Add(recipeType2);
	}

	public OnlyVisibleForRecipeType(RecipeType recipeType1, RecipeType recipeType2, RecipeType recipeType3)
	{
		RecipeTypes.Add(recipeType1);
		RecipeTypes.Add(recipeType2);
		RecipeTypes.Add(recipeType3);
	}

	public bool Matches(Recipe recipe)
	{
		return RecipeTypes.Contains(recipe.RecipeType);
	}
}
