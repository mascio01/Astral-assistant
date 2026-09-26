using System.Text;
using UnityEngine;

public class EquipmentNotification
{
	public enum Type
	{
		Item,
		Recipe
	}

	public Type NotificationType;

	public Recipe Recipe;

	public Equipment Item;

	public InfectionType InfectedWith;

	public int Amount;

	protected bool Finished;

	protected float DisplayedTime;

	protected float Transition;

	private static int HUD_NewRecipe = StringUtil.JenkinsHash("HUD_NewRecipe");

	private static int HUD_NewRecipeForToolbox = StringUtil.JenkinsHash("HUD_NewRecipeForToolbox");

	private static int HUD_NewRecipeForShovel = StringUtil.JenkinsHash("HUD_NewRecipeForShovel");

	private static int HUD_NewRecipeAtCraftingStation = StringUtil.JenkinsHash("HUD_NewRecipeAtCraftingStation");

	private static int HUD_NewRecipeAtCampfire = StringUtil.JenkinsHash("HUD_NewRecipeAtCampfire");

	private static StringBuilder sb = new StringBuilder(100);

	public EquipmentNotification(Equipment item, InfectionType infectedWith, int amount)
	{
		NotificationType = Type.Item;
		Item = item;
		InfectedWith = infectedWith;
		Amount = amount;
	}

	public EquipmentNotification(Recipe recipe)
	{
		NotificationType = Type.Recipe;
		Recipe = recipe;
	}

	public bool IsStarted()
	{
		return DisplayedTime > 0f;
	}

	public bool IsFinished()
	{
		if (Finished)
		{
			return Transition == 0f;
		}
		return false;
	}

	public static string GetNewRecipeTitle(Recipe recipe)
	{
		sb.Length = 0;
		if (recipe.CraftingStationPrototype != null)
		{
			sb.Append(GameImpl.Translate(recipe.IsProductDrinkableOrEdible() ? HUD_NewRecipeAtCampfire : HUD_NewRecipeAtCraftingStation));
			sb.Replace("%1", GameImpl.Translate(recipe.CraftingStationPrototype.NameHash));
			StringUtil.FakeParamResults.Clear();
			StringUtil.FakeParamResults.Add(SpeechParamResult.Create(recipe.CraftingStationPrototype));
			StringUtil.ApplyFormulae(sb, null, null, null, StringUtil.FakeParamResults, englishOnly: false);
		}
		else if (recipe.RecipeType == RecipeType.Toolbox)
		{
			sb.Append(GameImpl.Translate(HUD_NewRecipeForToolbox));
			StringUtil.ApplyFormulae(sb);
		}
		else if (recipe.RecipeType == RecipeType.Shovel)
		{
			sb.Append(GameImpl.Translate(HUD_NewRecipeForShovel));
			StringUtil.ApplyFormulae(sb);
		}
		else
		{
			sb.Append(GameImpl.Translate(HUD_NewRecipe));
			StringUtil.ApplyFormulae(sb);
		}
		return sb.ToString();
	}

	public static string GetNewRecipeText(Recipe recipe)
	{
		return GameImpl.Translate(recipe.NameHash);
	}

	public void UpdateEquipmentNotification()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		RectTransform rectTransform = (RectTransform)instance.UnityEquipmentNotification.transform;
		if (DisplayedTime == 0f)
		{
			SoundManager.PlayMenuSoundFromList(SoundManager.EquipmentNotificationSounds);
			switch (NotificationType)
			{
			case Type.Item:
				instance.UnityEquipmentNotificationName.SetUnityText(Item.GetDisplayNameString());
				instance.UnityEquipmentNotificationAmount.SetUnityText(((Amount >= 0) ? "+" : "-") + Mathf.Abs(Amount));
				instance.UnityEquipmentNotificationIcon.Initialize(Item, InfectedWith);
				instance.UnityEquipmentNotificationBG.material = InfoScreen.HalftoneDialog;
				break;
			case Type.Recipe:
				instance.UnityEquipmentNotificationName.SetUnityText(GetNewRecipeTitle(Recipe));
				instance.UnityEquipmentNotificationAmount.SetUnityText(GetNewRecipeText(Recipe));
				instance.UnityEquipmentNotificationIcon.Initialize(Recipe);
				instance.UnityEquipmentNotificationBG.material = InfoScreen.HalftoneNewRecipe;
				break;
			}
		}
		DisplayedTime += Time.unscaledDeltaTime;
		if (DisplayedTime >= ((NotificationType == Type.Recipe) ? 2f : 1f))
		{
			Finished = true;
		}
		Transition = Mathf.Clamp(Transition + Time.unscaledDeltaTime * (Finished ? (-1f) : 1f) * 4f, 0f, 1f);
		instance.UnityEquipmentNotification.gameObject.SetActive(Transition > 0f);
		if (GameImpl.Instance.Settings.SidebarLayoutEnabled)
		{
			rectTransform.anchoredPosition = new Vector2(32f + rectTransform.rect.width - instance.Size.x, Mathf.Lerp(0f - rectTransform.rect.height - 8f, 32f, Transition));
		}
		else
		{
			rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(rectTransform.rect.width + 8f, -8f, Transition), 256f);
		}
	}

	public void SetAmount(int amount)
	{
		Amount = amount;
		DisplayedTime = 0f;
		Finished = false;
	}
}
