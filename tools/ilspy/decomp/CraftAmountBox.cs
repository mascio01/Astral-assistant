using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftAmountBox : BaseDialog
{
	public static CraftAmountBox Instance;

	private TabBehaviour UnityOneOffTab;

	private TabBehaviour UnityRecurringTab;

	private TextMeshProUGUI UnityTabLeftPrompt;

	private TextMeshProUGUI UnityTabRightPrompt;

	private TextMeshProUGUI UnityMessage;

	private TextMeshProUGUI UnityFlOz;

	private TextMeshProUGUI UnityLiters;

	private TextMeshProUGUI UnityCommunityLimit;

	private TMP_InputField UnityInputField;

	private Slider UnitySlider;

	private Toggle UnityInfiniteToggle;

	private GameObject UnityIngredients;

	private List<IngredientBehaviour> UnityIngredientIcons = new List<IngredientBehaviour>();

	public ActionMenu ActionMenu = new ActionMenu();

	public Recipe Recipe;

	public Character Crafter;

	public Equipment UsingItem;

	public bool Recurring;

	public int RecurringLimit = 1;

	public float RecurringLiquidLimit;

	public int OneOffAmount = 1;

	public int OneOffMaxCraftable = 1;

	public TerrainCoord DestTile;

	private float PressedTime;

	public override ActionMenu GetActionMenu()
	{
		return ActionMenu;
	}

	public override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	private void AddIngredient(Equipment item, bool enabled)
	{
		IngredientBehaviour component = UnityEngine.Object.Instantiate(Hud.IngredientIcon.GetAsset(), UnityIngredients.transform).GetComponent<IngredientBehaviour>();
		component.Initialize(Crafter, item, enabled);
		UnityIngredientIcons.Add(component);
	}

	private void AddIngredient(EquipmentPrototype proto, bool enabled, InfectionType infectedWith, bool isProduct)
	{
		IngredientBehaviour component = UnityEngine.Object.Instantiate(Hud.IngredientIcon.GetAsset(), UnityIngredients.transform).GetComponent<IngredientBehaviour>();
		component.Initialize(Crafter, proto, null, enabled, infectedWith, isProduct);
		UnityIngredientIcons.Add(component);
	}

	private void AddLiquidIngredient(LiquidPrototype liquid, bool enabled, InfectionType infectedWith, bool isProduct)
	{
		IngredientBehaviour component = UnityEngine.Object.Instantiate(Hud.IngredientIcon.GetAsset(), UnityIngredients.transform).GetComponent<IngredientBehaviour>();
		component.Initialize(Crafter, null, liquid, enabled, infectedWith, isProduct);
		UnityIngredientIcons.Add(component);
	}

	private void AddBaseObject(PropPrototype proto, bool enabled, bool isProduct)
	{
		IngredientBehaviour component = UnityEngine.Object.Instantiate(Hud.IngredientIcon.GetAsset(), UnityIngredients.transform).GetComponent<IngredientBehaviour>();
		component.Initialize(Crafter, proto, enabled, isProduct);
		UnityIngredientIcons.Add(component);
	}

	private void AddText(string str, bool enabled)
	{
		TextMeshProUGUI component = UnityEngine.Object.Instantiate(Hud.IngredientText.GetAsset(), UnityIngredients.transform).GetComponent<TextMeshProUGUI>();
		component.SetUnityText(str);
		component.color = Color.black * (enabled ? 1f : 0.5f);
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnityOneOffTab = base.gameObject.FindChild("TabOneOff").GetComponent<TabBehaviour>();
		UnityRecurringTab = base.gameObject.FindChild("TabRecurring").GetComponent<TabBehaviour>();
		UnityTabLeftPrompt = base.transform.Find("TabLeftPrompt").GetComponent<TextMeshProUGUI>();
		UnityTabRightPrompt = base.transform.Find("TabRightPrompt").GetComponent<TextMeshProUGUI>();
		UnityMessage = base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>();
		UnityFlOz = base.gameObject.FindChild("FlOz").GetComponent<TextMeshProUGUI>();
		UnityLiters = base.gameObject.FindChild("Liters").GetComponent<TextMeshProUGUI>();
		UnityCommunityLimit = base.gameObject.FindChild("CommunityLimit").GetComponent<TextMeshProUGUI>();
		UnityInputField = base.gameObject.FindChild("InputField").GetComponent<TMP_InputField>();
		UnitySlider = base.gameObject.FindChild("Slider").GetComponent<Slider>();
		UnityInfiniteToggle = base.gameObject.FindChild("InfiniteToggle").GetComponent<Toggle>();
		OneOffMaxCraftable = Recipe.GetMaxCraftableUsingItemsInInventory(Crafter, UsingItem);
		CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
		if (craftingPropOnTile != null && craftingPropOnTile.IsCrafting())
		{
			OneOffMaxCraftable = 0;
		}
		OneOffAmount = 1;
		RecurringLimit = 1;
		UnityInfiniteToggle.isOn = false;
		UnityMessage.SetUnityText(GameImpl.Translate(Recipe.NameHash));
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		if (Recipe.ProductPrototype != null && Crafter.Community != null)
		{
			int craftingLimit = Crafter.Community.GetCraftingLimit(Recipe.ProductPrototype);
			if (craftingLimit == int.MaxValue)
			{
				UnityInfiniteToggle.isOn = true;
				RecurringLimit = Crafter.Community.CountInventoryItemsOfType(Recipe.ProductPrototype);
			}
			else
			{
				RecurringLimit = craftingLimit;
			}
		}
		if (Recipe.ProductLiquidPrototype != null && Crafter.Community != null)
		{
			int craftingLimit2 = Crafter.Community.GetCraftingLimit(Recipe.ProductLiquidPrototype);
			if (craftingLimit2 == int.MaxValue)
			{
				UnityInfiniteToggle.isOn = true;
				RecurringLiquidLimit = Crafter.Community.GetTotalLiquid(Recipe.ProductLiquidPrototype) * (useMetricWeights ? 0.0295735f : 1f);
			}
			else
			{
				RecurringLiquidLimit = (float)craftingLimit2 * (useMetricWeights ? 0.0295735f : 1f);
			}
		}
		CraftGoal craftGoal = Crafter.GetCraftGoal();
		if (craftGoal != null && craftGoal.FollowingRecipe == Recipe && craftGoal.DesiredAmount != int.MaxValue)
		{
			OneOffAmount = craftGoal.DesiredAmount;
		}
		UnitySlider.minValue = 1f;
		UnitySlider.maxValue = OneOffMaxCraftable;
		UnitySlider.onValueChanged.RemoveAllListeners();
		UnitySlider.value = OneOffAmount;
		UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
		UnityInputField.onEndEdit.AddListener(OnInputValueSubmitted);
		SetRecurring(OneOffMaxCraftable == 0);
	}

	private void PopulateIngredients()
	{
		UnityIngredients = base.gameObject.FindChild("Ingredients");
		UnityIngredients.DeleteAllChildren();
		UnityIngredientIcons.Clear();
		bool flag = true;
		InfectionType infectionType = InfectionType.None;
		foreach (Ingredient ingredient in Recipe.Ingredients)
		{
			bool flag2 = ingredient.HasEnoughOfIngredient(Crafter, Crafter, Recipe, UsingItem, null);
			if (!flag)
			{
				AddText("+", OneOffMaxCraftable > 0);
			}
			flag = false;
			if (ingredient.Prototypes != null)
			{
				Equipment equipment = null;
				EquipmentPrototype equipmentPrototype = null;
				float num = float.MinValue;
				InfectionType infectionType2 = InfectionType.None;
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					if (prototype == null || prototype.Tex == null)
					{
						continue;
					}
					if (!Recurring && UsingItem != null && UsingItem.GetPrototype() == prototype)
					{
						infectionType2 = UsingItem.InfectedWith;
						equipment = UsingItem;
						equipmentPrototype = UsingItem.GetPrototype();
						break;
					}
					Equipment equipment2 = Crafter.Inventory.FindItemOfTypePreferringUnused(Crafter, Crafter, prototype, Recipe, checkCraftingPolicy: false, ingredient.IngredientInfectionState);
					float num2 = equipment2?.GetAmount() ?? 0;
					if (Recurring)
					{
						if (!Crafter.IsActionAllowedForItem(prototype, null, equipment2?.InfectedWith ?? InfectionType.None, EquipmentPolicyAction.CanCraftWith))
						{
							num2 -= 100000f;
						}
						else if (equipment2 != null && (Recipe.IsRecipeTool(Crafter, equipment2) || !Crafter.CanUseItemForCrafting(Crafter, equipment2, UsingItem, Recipe)))
						{
							num2 -= 100000f;
						}
					}
					if (num2 > num)
					{
						infectionType2 = equipment2?.InfectedWith ?? InfectionType.None;
						equipment = equipment2;
						equipmentPrototype = prototype;
						num = num2;
					}
				}
				infectionType = (InfectionType)Math.Max((int)infectionType, (int)infectionType2);
				if (equipment != null)
				{
					AddIngredient(equipment, flag2);
					if (ingredient.Amount > 1)
					{
						AddText("x" + ingredient.Amount, flag2);
					}
				}
				else if (equipmentPrototype != null)
				{
					AddIngredient(equipmentPrototype, flag2, infectionType2, isProduct: false);
					if (ingredient.Amount > 1)
					{
						AddText("x" + ingredient.Amount, flag2);
					}
				}
			}
			else
			{
				if (ingredient.LiquidTypes == null)
				{
					continue;
				}
				LiquidPrototype liquidPrototype = null;
				float num3 = float.MinValue;
				InfectionType infectionType3 = InfectionType.None;
				foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
				{
					if (liquidType != null)
					{
						InfectionType ingredientsInfectedWith;
						float num4 = Crafter.Inventory.GetAmountOfLiquidType(Crafter, liquidType, out ingredientsInfectedWith, ingredient.IngredientInfectionState);
						if (Recurring && !Crafter.IsActionAllowedForItem(null, liquidType, ingredientsInfectedWith, EquipmentPolicyAction.CanCraftWith))
						{
							num4 -= 100000f;
						}
						if (num4 > num3)
						{
							liquidPrototype = liquidType;
							num3 = num4;
							infectionType3 = ingredientsInfectedWith;
						}
						break;
					}
				}
				if (liquidPrototype != null)
				{
					infectionType = (InfectionType)Math.Max((int)infectionType, (int)infectionType3);
					AddLiquidIngredient(liquidPrototype, flag2, infectionType3, isProduct: false);
					bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
					AddText((ingredient.LiquidAmount * (useMetricWeights ? 0.0295735f : 1f)).ToString(AvailableAction.SensibleFloatFormat) + GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz), flag2);
				}
			}
		}
		AddText("=", OneOffMaxCraftable > 0);
		InfectionType infectedWith = (Recipe.CanPassInfectionToProduct ? infectionType : InfectionType.None);
		flag = true;
		if (Recipe.ProductLiquidPrototype != null)
		{
			AddLiquidIngredient(Recipe.ProductLiquidPrototype, OneOffMaxCraftable > 0, infectedWith, isProduct: true);
			flag = false;
		}
		if (Recipe.ProductPrototype != null)
		{
			if (!flag)
			{
				AddText("+", OneOffMaxCraftable > 0);
			}
			AddIngredient(Recipe.ProductPrototype, OneOffMaxCraftable > 0, infectedWith, isProduct: true);
			flag = false;
			if (Recipe.ProductAmount > 1)
			{
				AddText("x" + Recipe.ProductAmount, OneOffMaxCraftable > 0);
			}
		}
		if (Recipe.ExtraOutputs != null)
		{
			foreach (RecipeExtraOutput extraOutput in Recipe.ExtraOutputs)
			{
				AddIngredient(extraOutput.ProductPrototype, OneOffMaxCraftable > 0, infectedWith, isProduct: true);
				flag = false;
				if (extraOutput.MinAmount > 1)
				{
					AddText("x" + extraOutput.MinAmount + ((extraOutput.MaxAmount > extraOutput.MinAmount) ? ("-" + extraOutput.MaxAmount) : ""), OneOffMaxCraftable > 0);
				}
				if (extraOutput.Probability != 100)
				{
					AddText("(" + extraOutput.Probability + "%)", OneOffMaxCraftable > 0);
				}
			}
		}
		if (Recipe.ProductPropPrototype != null)
		{
			if (!flag)
			{
				AddText("+", OneOffMaxCraftable > 0);
			}
			AddBaseObject(Recipe.ProductPropPrototype, OneOffMaxCraftable > 0, isProduct: true);
			flag = false;
		}
	}

	private void SetRecurring(bool recurring)
	{
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		Recurring = recurring;
		UnityOneOffTab.Selected = !Recurring;
		UnityRecurringTab.Selected = Recurring;
		UnityInfiniteToggle.gameObject.SetActive(Recurring);
		UnitySlider.gameObject.SetActive(!Recurring && OneOffMaxCraftable > 1);
		UnityCommunityLimit.gameObject.SetActive(Recurring);
		UnityFlOz.gameObject.SetActive(Recurring && Recipe.ProductLiquidPrototype != null && !useMetricWeights);
		UnityLiters.gameObject.SetActive(Recurring && Recipe.ProductLiquidPrototype != null && useMetricWeights);
		UnityInputField.onValueChanged.RemoveAllListeners();
		UnityInputField.SetUnityText((!Recurring) ? OneOffAmount.ToString() : ((Recipe.ProductLiquidPrototype != null) ? RecurringLiquidLimit.ToString(AvailableAction.SensibleFloatFormat) : RecurringLimit.ToString()));
		UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
		PopulateIngredients();
	}

	public override void DialogUpdate()
	{
		base.DialogUpdate();
		bool flag = Crafter.Community.CanUseRoleCommands();
		bool flag2 = flag && OneOffMaxCraftable > 0;
		UnityOneOffTab.gameObject.SetActive(flag);
		UnityRecurringTab.gameObject.SetActive(flag);
		UnityTabLeftPrompt.gameObject.SetActive(flag2);
		UnityTabRightPrompt.gameObject.SetActive(flag2);
		UnityOneOffTab.Interactable = flag2 || !Recurring;
		UnityRecurringTab.Interactable = flag2 || Recurring;
		bool flag3 = Recurring && UnityInfiniteToggle.isOn;
		UnityInputField.interactable = SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor && !flag3;
		UnityInputField.GetComponent<RawImage>().color = (flag3 ? Color.gray : Color.white);
		StringUtil.SetUnityTextButtonPrompt(UnityTabLeftPrompt, InputFunction.TabLeft);
		StringUtil.SetUnityTextButtonPrompt(UnityTabRightPrompt, InputFunction.TabRight);
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (Recurring && current != null && current.currentSelectedGameObject != null && (SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor || SelectableBehaviour.CurrentCursorHovered != null))
		{
			foreach (IngredientBehaviour unityIngredientIcon in UnityIngredientIcons)
			{
				if (unityIngredientIcon.gameObject == current.currentSelectedGameObject)
				{
					ActionMenu.FocusUnityObj = current.currentSelectedGameObject;
					ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.EquipmentTotalName, unityIngredientIcon.Proto, unityIngredientIcon.Liquid, 0, CursorActionDisabledReason.Enabled));
					GameCursor.AddEquipmentPolicyAction(unityIngredientIcon.Proto, unityIngredientIcon.Liquid, unityIngredientIcon.InfectedWith, Crafter, ActionMenu.AvailableActions);
					if (ActionMenu.AvailableActions.Count > 0)
					{
						ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Separator, null));
					}
				}
			}
		}
		ActionMenu.OnFinishAddingActions();
	}

	public void OnSliderValueChanged(float v)
	{
		OneOffAmount = (int)v;
		UnityInputField.onValueChanged.RemoveAllListeners();
		UnityInputField.SetUnityText(OneOffAmount.ToString());
		UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
	}

	public void OnInputValueSubmitted(string v)
	{
		bool flag = true;
		try
		{
			if (Recurring)
			{
				if (Recipe.ProductLiquidPrototype != null)
				{
					RecurringLiquidLimit = StringUtil.ParseFloat(v);
				}
				else
				{
					RecurringLimit = StringUtil.ParseInt(v);
				}
			}
			else
			{
				OneOffAmount = StringUtil.ParseInt(v);
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!Recurring)
		{
			if (OneOffAmount < 1 || OneOffAmount > OneOffMaxCraftable)
			{
				OneOffAmount = MathUtil.Clamp(OneOffAmount, 1, OneOffMaxCraftable);
				flag = false;
			}
			if (!flag)
			{
				UnityInputField.onValueChanged.RemoveAllListeners();
				UnityInputField.SetUnityText(OneOffAmount.ToString());
				UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
			}
			UnitySlider.onValueChanged.RemoveAllListeners();
			UnitySlider.value = OneOffAmount;
			UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
		}
	}

	public void OnInputValueChanged(string v)
	{
		bool flag = true;
		try
		{
			if (Recurring)
			{
				if (Recipe.ProductLiquidPrototype != null)
				{
					RecurringLiquidLimit = StringUtil.ParseFloat(v);
				}
				else
				{
					RecurringLimit = StringUtil.ParseInt(v);
				}
			}
			else
			{
				OneOffAmount = StringUtil.ParseInt(v);
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		if (!Recurring)
		{
			if (OneOffAmount < 1 || OneOffAmount > OneOffMaxCraftable)
			{
				OneOffAmount = MathUtil.Clamp(OneOffAmount, 1, OneOffMaxCraftable);
				flag = true;
			}
			if (flag)
			{
				UnitySlider.onValueChanged.RemoveAllListeners();
				UnitySlider.value = OneOffAmount;
				UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
			}
		}
	}

	public void OnInfiniteToggled()
	{
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (Recurring && UnityOneOffTab.Selected)
		{
			SetRecurring(recurring: false);
		}
		if (!Recurring && UnityRecurringTab.Selected)
		{
			SetRecurring(recurring: true);
		}
		if (instance.IsJustPressed(InputFunction.TabLeft))
		{
			if (Recurring && Crafter.Community.CanUseRoleCommands())
			{
				SoundManager.PlayMenuSound(SoundManager.TabSound);
				SetRecurring(recurring: false);
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
		}
		else if (instance.IsJustPressed(InputFunction.TabRight))
		{
			if (!Recurring && Crafter.Community.CanUseRoleCommands())
			{
				SoundManager.PlayMenuSound(SoundManager.TabSound);
				SetRecurring(recurring: true);
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
		}
		if (Recurring)
		{
			if (!UnityInfiniteToggle.isOn)
			{
				int num = 0;
				num += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
				num += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
				if (num != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
				{
					PressedTime = Time.realtimeSinceStartup;
					if (Recipe.ProductLiquidPrototype != null)
					{
						RecurringLiquidLimit = Math.Max(RecurringLiquidLimit + (float)num, 0f);
					}
					else
					{
						RecurringLimit = Math.Max(RecurringLimit + num, 1);
					}
					UnityInputField.onValueChanged.RemoveAllListeners();
					UnityInputField.SetUnityText((Recipe.ProductLiquidPrototype != null) ? RecurringLiquidLimit.ToString(AvailableAction.SensibleFloatFormat) : RecurringLimit.ToString());
					UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
				}
			}
		}
		else
		{
			int num2 = 0;
			num2 += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
			num2 += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
			if (num2 != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
			{
				PressedTime = Time.realtimeSinceStartup;
				OneOffAmount = MathUtil.Clamp(OneOffAmount + num2, 1, OneOffMaxCraftable);
				UnityInputField.onValueChanged.RemoveAllListeners();
				UnityInputField.SetUnityText(OneOffAmount.ToString());
				UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
				UnitySlider.onValueChanged.RemoveAllListeners();
				UnitySlider.value = OneOffAmount;
				UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
			}
		}
		if (ActionMenu.GetCursorAction() != CursorAction.None && instance.IsJustPressed(InputFunction.MainAction))
		{
			CursorAction cursorAction = ActionMenu.GetCursorAction();
			if ((uint)(cursorAction - 235) <= 4u)
			{
				GameImpl.Instance.ShowEquipmentPolicyDialog(Crafter, ActionMenu.GetAvailableAction().Proto, ActionMenu.GetAvailableAction().Liquid, ActionMenu.GetAvailableAction().InfectedWith);
			}
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		_ = InputFunctionManager.Instance;
		if (!OKSelected)
		{
			return;
		}
		if (inputFrame != null)
		{
			int amount;
			if (Recurring)
			{
				if (Crafter.Roles.Count >= 10)
				{
					GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HINT_TooManyRoles").Replace("%1", 10.ToString()));
					OKSelected = false;
					return;
				}
				if (UnityInfiniteToggle.isOn)
				{
					amount = int.MaxValue;
				}
				else if (Recipe.ProductLiquidPrototype != null)
				{
					bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
					amount = Mathf.RoundToInt(RecurringLiquidLimit / (useMetricWeights ? 0.0295735f : 1f));
				}
				else
				{
					amount = RecurringLimit;
				}
			}
			else
			{
				amount = OneOffAmount;
			}
			ShowRestrictedItemsMessageIfNeeded(Recipe, Crafter, Recurring);
			inputFrame.AddAction(InputAction.Craft(Recipe, Crafter, Recurring ? null : UsingItem, Recurring, amount, DestTile, Prop.OrientationType.Deg0, isDoubleClick: false));
			InfoScreen.Instance.CloseInfoScreen();
		}
		OKSelected = false;
		Finished = true;
	}

	public override void OnShown()
	{
		base.OnShown();
		PopulateIngredients();
	}

	public static void ShowRestrictedItemsMessageIfNeeded(Recipe recipe, Character crafter, bool recurring)
	{
		string text = string.Empty;
		bool flag = false;
		if (recurring)
		{
			foreach (Ingredient ingredient in recipe.Ingredients)
			{
				bool flag2 = false;
				if (ingredient.Prototypes != null)
				{
					foreach (EquipmentPrototype prototype in ingredient.Prototypes)
					{
						bool flag3 = false;
						bool flag4 = false;
						for (InfectionType infectionType = InfectionType.None; infectionType < InfectionType.Count; infectionType++)
						{
							if (crafter.IsActionAllowedForItem(prototype, null, infectionType, EquipmentPolicyAction.CanCraftWith))
							{
								if (crafter.Community == null || crafter.Community.CountInventoryItemsOfType(prototype, infectionType) > 0)
								{
									flag4 = true;
								}
							}
							else
							{
								flag3 = true;
							}
						}
						if (flag3 && !flag4)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += GameImpl.Translate(prototype.GetNameKey());
						}
						else
						{
							flag2 = true;
						}
					}
				}
				if (ingredient.LiquidTypes != null)
				{
					foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
					{
						bool flag5 = false;
						bool flag6 = false;
						for (InfectionType infectionType2 = InfectionType.None; infectionType2 < InfectionType.Count; infectionType2++)
						{
							if (crafter.IsActionAllowedForItem(null, liquidType, infectionType2, EquipmentPolicyAction.CanCraftWith))
							{
								if (crafter.Community == null || crafter.Community.GetTotalLiquid(liquidType, infectionType2) > 0f)
								{
									flag6 = true;
								}
							}
							else
							{
								flag5 = true;
							}
						}
						if (flag5 && !flag6)
						{
							if (text.Length > 0)
							{
								text += ", ";
							}
							text += GameImpl.Translate(liquidType.GetNameKey());
						}
						else
						{
							flag2 = true;
						}
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if (text.Length > 0)
		{
			string text2 = GameImpl.Translate(flag ? "HUD_CantCraftRestrictedItems" : "HUD_CanCraftButSomeRestrictedItems");
			text2 = text2.Replace("%1", crafter.GetDisplayNameString());
			text2 = text2.Replace("%2", GameImpl.Translate(recipe.GetNameKey()).ToLower());
			text2 = text2.Replace("%3", text);
			text2 = StringUtil.ApplyFormulae(text2, null, crafter, null, null);
			HudBehaviour.Instance.SetStatusBarMsg(text2);
		}
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
