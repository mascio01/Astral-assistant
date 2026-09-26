using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class EditRecipeMenu : BaseMenu
{
	public class SortIngredients : IComparer<Ingredient>
	{
		int IComparer<Ingredient>.Compare(Ingredient a, Ingredient b)
		{
			float averagePrice = a.GetAveragePrice();
			float averagePrice2 = b.GetAveragePrice();
			if (averagePrice < averagePrice2)
			{
				return 1;
			}
			if (averagePrice > averagePrice2)
			{
				return -1;
			}
			return 0;
		}
	}

	public class SortEquipmentPrototypesByPriceAscending : IComparer<EquipmentPrototype>
	{
		int IComparer<EquipmentPrototype>.Compare(EquipmentPrototype a, EquipmentPrototype b)
		{
			if (a.BasePrice < b.BasePrice)
			{
				return -1;
			}
			if (a.BasePrice > b.BasePrice)
			{
				return 1;
			}
			return 0;
		}
	}

	public class SortLiquidPrototypesByPriceAscending : IComparer<LiquidPrototype>
	{
		int IComparer<LiquidPrototype>.Compare(LiquidPrototype a, LiquidPrototype b)
		{
			if (a.BasePricePerFlOz < b.BasePricePerFlOz)
			{
				return -1;
			}
			if (a.BasePricePerFlOz > b.BasePricePerFlOz)
			{
				return 1;
			}
			return 0;
		}
	}

	private Recipe Selected;

	private string SelectedUniqueID;

	private bool HasChanged;

	private bool WantRepopulate;

	private static SortIngredients IngredientsSorter = new SortIngredients();

	private static SortEquipmentPrototypesByPriceAscending EquipmentSorter = new SortEquipmentPrototypesByPriceAscending();

	private static SortLiquidPrototypesByPriceAscending LiquidSorter = new SortLiquidPrototypesByPriceAscending();

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0.25f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		WantRepopulate = true;
		Selected = null;
		if (GameImpl.Instance.GetCurrentlyEditingStory().Recipes.Count > 0)
		{
			OnSelectRecipe(GameImpl.Instance.GetCurrentlyEditingStory().Recipes.Values[0]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			SaveRecipe();
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			PopulateRecipeList();
			PopulateRecipeInfo();
			WantRepopulate = false;
		}
	}

	public void OnCreateNew()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterRecipeUniqueID"), "", multiline: false, readOnly: false);
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().Recipes.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string uniqueID)
	{
		if (!string.IsNullOrEmpty(uniqueID))
		{
			CheckUniqueID(ref uniqueID);
			Recipe recipe = new Recipe();
			recipe.UniqueID = uniqueID;
			GameImpl.Instance.GetCurrentlyEditingStory().Recipes[uniqueID] = recipe;
			OnSelectRecipe(recipe);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveRecipe();
		WantPop = true;
	}

	private void OnSelectRecipe(Recipe recipe)
	{
		SaveRecipe();
		Selected = recipe;
		SelectedUniqueID = recipe?.UniqueID;
		PopulateRecipeList();
		PopulateRecipeInfo();
	}

	private void SaveRecipe()
	{
		if (Selected == null || !HasChanged)
		{
			return;
		}
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		foreach (KeyValuePair<string, Recipe> recipe in currentlyEditingStory.Recipes)
		{
			foreach (Ingredient ingredient in recipe.Value.Ingredients)
			{
				if (ingredient.Prototypes != null)
				{
					ingredient.Prototypes.Sort(EquipmentSorter);
				}
				if (ingredient.LiquidTypes != null)
				{
					ingredient.LiquidTypes.Sort(LiquidSorter);
				}
			}
			recipe.Value.Ingredients.Sort(IngredientsSorter);
		}
		if (!RecipeList.SaveToFile(currentlyEditingStory.Path + "/Recipes.xml", currentlyEditingStory.Recipes))
		{
			Selected.UniqueID = SelectedUniqueID;
		}
		if (Selected.UniqueID != SelectedUniqueID)
		{
			GameImpl.Instance.GetCurrentlyEditingStory().Recipes.Remove(SelectedUniqueID);
			GameImpl.Instance.GetCurrentlyEditingStory().Recipes[Selected.UniqueID] = Selected;
			SelectedUniqueID = Selected.UniqueID;
		}
		else
		{
			GameImpl.Instance.GetCurrentlyEditingStory().Recipes[Selected.UniqueID] = Selected;
		}
		HasChanged = false;
		GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
		GameImpl.Instance.BuildPrototypeLookupLists();
	}

	private void PopulateRecipeList()
	{
		GameObject gameObject = base.gameObject.transform.Find("RecipeList/Viewport/Content").gameObject;
		int num = 0;
		foreach (KeyValuePair<string, Recipe> recipe2 in GameImpl.Instance.GetCurrentlyEditingStory().Recipes)
		{
			Transform transform = gameObject.transform.Find(recipe2.Key);
			GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
			if (gameObject2 == null)
			{
				Recipe recipe = recipe2.Value;
				gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/ListButton"));
				gameObject2.name = recipe2.Key;
				gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					OnSelectRecipe(recipe);
				});
			}
			gameObject2.transform.Find("Text").gameObject.GetComponent<Text>().SetUnityText(recipe2.Value.UniqueID);
			gameObject2.transform.SetSiblingIndex(num);
			num++;
		}
		for (; num < gameObject.transform.childCount; num++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulateRecipeInfo()
	{
		GameObject gameObject = base.gameObject.FindChild("RecipeInfo/Viewport/Content");
		int index = 0;
		if (Selected != null)
		{
			PopulateFields(gameObject, ref index, Selected, -1);
		}
		for (int num = gameObject.transform.childCount - 1; num >= index; num--)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulateFields(GameObject panel, ref int index, object obj, int arrayIndex)
	{
		FieldInfo[] fields = obj.GetType().GetFields();
		foreach (FieldInfo field in fields)
		{
			FieldInfo localField = field;
			if (field.IsStatic || field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0)
			{
				continue;
			}
			if (obj is Recipe recipe)
			{
				OnlyVisibleForRecipeType customAttribute = field.GetCustomAttribute<OnlyVisibleForRecipeType>();
				if ((customAttribute != null && !customAttribute.Matches(recipe)) || (field.Name == "ProductAmount" && string.IsNullOrEmpty(recipe.ProductPrototypeName)) || (field.Name == "ProductLiquidAmount" && string.IsNullOrEmpty(recipe.ProductLiquidPrototypeName)) || (field.Name == "ProductLiquidNutritionMultiplier" && string.IsNullOrEmpty(recipe.ProductLiquidPrototypeName)))
				{
					continue;
				}
			}
			if (obj is Ingredient ingredient && ((ingredient.PrototypeNames != null && ingredient.PrototypeNames.Count > 0 && (field.Name == "LiquidTypeNames" || field.Name == "LiquidAmount")) || (ingredient.LiquidTypeNames != null && ingredient.LiquidTypeNames.Count > 0 && (field.Name == "PrototypeNames" || field.Name == "Amount")) || ((Selected == null || string.IsNullOrEmpty(Selected.ProductType)) && field.Name == "ReturnType")))
			{
				continue;
			}
			if (field.FieldType == typeof(bool))
			{
				BaseMenu.AddCheckbox(panel, ref index, "EDITOR_" + field.Name, arrayIndex, showArrayIndex: false, (bool)field.GetValue(obj), delegate(bool flag)
				{
					localField.SetValue(obj, flag);
					HasChanged = true;
				});
			}
			else if (field.FieldType == typeof(int))
			{
				BaseMenu.AddInputField(panel, ref index, "EDITOR_" + field.Name, arrayIndex, showArrayIndex: false, ((int)field.GetValue(obj)).ToString(), delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						localField.SetValue(obj, result);
						HasChanged = true;
					}
				});
			}
			else if (field.FieldType == typeof(float))
			{
				BaseMenu.AddInputField(panel, ref index, "EDITOR_" + field.Name, arrayIndex, showArrayIndex: false, ((float)field.GetValue(obj)).ToString(), delegate(string s)
				{
					if (float.TryParse(s, out var result))
					{
						localField.SetValue(obj, result);
						HasChanged = true;
					}
				});
			}
			else if (field.FieldType == typeof(string))
			{
				if (field.Name == "ProductPrototypeName")
				{
					List<string> equipmentOptions = BaseMenu.GetEquipmentOptions();
					AddStringDropDown(panel, ref index, obj, field, arrayIndex, equipmentOptions);
					continue;
				}
				if (field.Name == "ProductLiquidPrototypeName")
				{
					List<string> liquidOptions = BaseMenu.GetLiquidOptions();
					AddStringDropDown(panel, ref index, obj, field, arrayIndex, liquidOptions);
					continue;
				}
				if (field.Name == "ProductType")
				{
					List<string> propOptions = BaseMenu.GetPropOptions();
					AddStringDropDown(panel, ref index, obj, field, arrayIndex, propOptions);
					continue;
				}
				if (field.Name == "CraftingStationType")
				{
					List<string> propOptions2 = BaseMenu.GetPropOptions(Selected.RequiredPropToWorkOn());
					AddStringDropDown(panel, ref index, obj, field, arrayIndex, propOptions2);
					continue;
				}
				BaseMenu.AddInputField(panel, ref index, "EDITOR_" + field.Name, arrayIndex, showArrayIndex: false, (string)field.GetValue(obj), delegate(string v3)
				{
					if (field.Name == "UniqueID" && v3 != (string)field.GetValue(obj))
					{
						if (string.IsNullOrEmpty(v3))
						{
							return;
						}
						CheckUniqueID(ref v3);
					}
					localField.SetValue(obj, v3);
					HasChanged = true;
				});
			}
			else if (field.FieldType == typeof(List<string>))
			{
				List<string> options = null;
				if (field.Name == "PrototypeNames")
				{
					options = BaseMenu.GetEquipmentOptions();
				}
				else if (field.Name == "LiquidTypeNames")
				{
					options = BaseMenu.GetLiquidOptions();
				}
				List<string> list = (List<string>)field.GetValue(obj);
				for (int num = 0; num <= (list?.Count ?? 0); num++)
				{
					int localIndex = num;
					int v = ((list != null && num < list.Count) ? options.IndexOf(list[num]) : 0);
					BaseMenu.AddDropDown(panel, ref index, "EDITOR_" + field.Name, arrayIndex * 1000 + num, showArrayIndex: false, (num > 0) ? "EDITOR_OR" : null, v, options, delegate(int index2)
					{
						if (list == null)
						{
							list = new List<string>();
							field.SetValue(obj, list);
						}
						if (localIndex >= list.Count)
						{
							list.Add(options[index2]);
						}
						else
						{
							list[localIndex] = options[index2];
						}
						if (options[index2] == "" && localIndex >= 0 && localIndex < list.Count)
						{
							list.RemoveAt(localIndex);
						}
						HasChanged = true;
						WantRepopulate = true;
					});
				}
			}
			else if (field.FieldType.IsEnum)
			{
				Type enumType = field.FieldType;
				FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
				List<string> options2 = new List<string>();
				FieldInfo[] array = fields2;
				foreach (FieldInfo fieldInfo in array)
				{
					options2.Add(fieldInfo.GetValue(null).ToString());
				}
				int v2 = options2.IndexOf(field.GetValue(obj).ToString());
				BaseMenu.AddDropDown(panel, ref index, "EDITOR_" + field.Name, arrayIndex, showArrayIndex: false, null, v2, options2, delegate(int index2)
				{
					int num5 = (int)Enum.Parse(enumType, options2[index2]);
					localField.SetValue(obj, num5);
					HasChanged = true;
					WantRepopulate = true;
				});
			}
			else if (field.FieldType == typeof(List<Ingredient>))
			{
				List<Ingredient> ingredients = (List<Ingredient>)field.GetValue(obj);
				for (int num3 = 0; num3 < ingredients.Count; num3++)
				{
					Ingredient obj2 = ingredients[num3];
					int localIndex2 = num3;
					BaseMenu.AddTitleField(panel, ref index, "Ingredient " + num3, -1, delegate
					{
						ingredients.RemoveAt(localIndex2);
						HasChanged = true;
						WantRepopulate = true;
					});
					PopulateFields(panel, ref index, obj2, num3);
				}
				BaseMenu.AddButtonField(panel, ref index, "Add Ingredient", arrayIndex, delegate
				{
					ingredients.Add(new Ingredient());
					HasChanged = true;
					WantRepopulate = true;
				});
			}
			else
			{
				if (!(field.FieldType == typeof(List<RecipeExtraOutput>)))
				{
					continue;
				}
				List<RecipeExtraOutput> extraOutputs = (List<RecipeExtraOutput>)field.GetValue(obj);
				if (extraOutputs != null)
				{
					for (int num4 = 0; num4 < extraOutputs.Count; num4++)
					{
						RecipeExtraOutput obj3 = extraOutputs[num4];
						int localIndex3 = num4;
						BaseMenu.AddTitleField(panel, ref index, "ExtraOutput " + num4, -1, delegate
						{
							extraOutputs.RemoveAt(localIndex3);
							HasChanged = true;
							WantRepopulate = true;
						});
						PopulateFields(panel, ref index, obj3, num4);
					}
				}
				BaseMenu.AddButtonField(panel, ref index, "Add Extra Output", arrayIndex, delegate
				{
					if (extraOutputs == null)
					{
						extraOutputs = new List<RecipeExtraOutput>();
						field.SetValue(obj, extraOutputs);
					}
					extraOutputs.Add(new RecipeExtraOutput());
					HasChanged = true;
					WantRepopulate = true;
				});
			}
		}
	}

	private void AddStringDropDown(GameObject panel, ref int index, object obj, FieldInfo field, int arrayIndex, List<string> options)
	{
		int v = options.IndexOf((string)field.GetValue(obj));
		BaseMenu.AddDropDown(panel, ref index, "EDITOR_" + field.Name, arrayIndex, null, v, options, delegate(int index2)
		{
			field.SetValue(obj, options[index2]);
			HasChanged = true;
			WantRepopulate = true;
		});
	}
}
