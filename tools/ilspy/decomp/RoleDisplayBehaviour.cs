using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleDisplayBehaviour : MonoBehaviour
{
	private Character CurrentCharacter;

	private int RoleIndex;

	private TextMeshProUGUI UnityText;

	private RawImage UnityIcon;

	private Button UnityPriorityUpButton;

	private Button UnityPriorityDownButton;

	public Button UnityCancelButton;

	private Button UnityPauseButton;

	private TextMeshProUGUI UnityPauseButtonText;

	private Toggle UnityUrgentToggle;

	public static int HUD_Guard = StringUtil.JenkinsHash("HUD_Guard");

	public static int HUD_Guard_Female = StringUtil.JenkinsHash("HUD_Guard_Female");

	public static int HUD_Gatherer = StringUtil.JenkinsHash("HUD_Gatherer");

	public static int HUD_Gatherer_Female = StringUtil.JenkinsHash("HUD_Gatherer_Female");

	public static int HUD_Farmer = StringUtil.JenkinsHash("HUD_Farmer");

	public static int HUD_Farmer_Female = StringUtil.JenkinsHash("HUD_Farmer_Female");

	public static int HUD_Builder = StringUtil.JenkinsHash("HUD_Builder");

	public static int HUD_Builder_Female = StringUtil.JenkinsHash("HUD_Builder_Female");

	public static int HUD_Remaining = StringUtil.JenkinsHash("HUD_Remaining");

	public static int HUD_Remaining_Singular = StringUtil.JenkinsHash("HUD_Remaining_Singular");

	public static int HUD_Repairing = StringUtil.JenkinsHash("HUD_Repairing");

	public static int HUD_Repairing_Female = StringUtil.JenkinsHash("HUD_Repairing_Female");

	public static int HUD_Capturing = StringUtil.JenkinsHash("HUD_Capturing");

	public static int HUD_Capturing_Female = StringUtil.JenkinsHash("HUD_Capturing_Female");

	public static int HUD_Crafter = StringUtil.JenkinsHash("HUD_Crafter");

	public static int HUD_Crafter_Female = StringUtil.JenkinsHash("HUD_Crafter_Female");

	public static int HUD_Lumberjack = StringUtil.JenkinsHash("HUD_Lumberjack");

	public static int HUD_Lumberjack_Female = StringUtil.JenkinsHash("HUD_Lumberjack_Female");

	public static int HUD_Cook = StringUtil.JenkinsHash("HUD_Cook");

	public static int HUD_Cook_Female = StringUtil.JenkinsHash("HUD_Cook_Female");

	public static int HUD_Miner = StringUtil.JenkinsHash("HUD_Miner");

	public static int HUD_Miner_Female = StringUtil.JenkinsHash("HUD_Miner_Female");

	public static int HUD_Trader = StringUtil.JenkinsHash("HUD_Trader");

	public static int HUD_Trader_Female = StringUtil.JenkinsHash("HUD_Trader_Female");

	public static int HUD_Trapper = StringUtil.JenkinsHash("HUD_Trapper");

	public static int HUD_Trapper_Female = StringUtil.JenkinsHash("HUD_Trapper_Female");

	public static int HUD_AnimalFeeder = StringUtil.JenkinsHash("HUD_AnimalFeeder");

	public static int HUD_AnimalFeeder_Female = StringUtil.JenkinsHash("HUD_AnimalFeeder_Female");

	public static int HUD_Organizer = StringUtil.JenkinsHash("HUD_Organizer");

	public static int HUD_Organizer_Female = StringUtil.JenkinsHash("HUD_Organizer_Female");

	public static int HUD_Medic = StringUtil.JenkinsHash("HUD_Medic");

	public static int HUD_Medic_Female = StringUtil.JenkinsHash("HUD_Medic_Female");

	public static int HUD_RolePaused = StringUtil.JenkinsHash("HUD_RolePaused");

	public static int HUD_Pause = StringUtil.JenkinsHash("HUD_Pause");

	public static int HUD_Resume = StringUtil.JenkinsHash("HUD_Resume");

	private static StringBuilder sb = new StringBuilder(100);

	private bool WantCancelRole;

	private bool WantPauseRole;

	private bool WantPriorityUp;

	private bool WantPriorityDown;

	private bool WantUrgentToggled;

	public void Initialize(int roleIndex, CharacterPage page)
	{
		RoleIndex = roleIndex;
		UnityText = base.transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>();
		UnityIcon = base.transform.Find("RoleIcon").GetComponent<RawImage>();
		UnityPriorityUpButton = base.transform.Find("ArrowsPanel/PriorityUpButton").GetComponent<Button>();
		UnityPriorityDownButton = base.transform.Find("ArrowsPanel/PriorityDownButton").GetComponent<Button>();
		UnityCancelButton = base.transform.Find("Panel/ButtonsPanel/CancelButton").GetComponent<Button>();
		UnityPauseButton = base.transform.Find("Panel/ButtonsPanel/PauseButton").GetComponent<Button>();
		UnityPauseButtonText = base.transform.Find("Panel/ButtonsPanel/PauseButton/Text").GetComponent<TextMeshProUGUI>();
		UnityUrgentToggle = base.transform.Find("Panel/ButtonsPanel/UrgentCheckbox/Toggle").GetComponent<Toggle>();
	}

	public static Texture2D GetRoleDisplayIcon(Character character, RoleInfo roleInfo)
	{
		switch (roleInfo.Role)
		{
		case Role.Farmer:
			return GameCursor.CursorUseWateringCan;
		case Role.Gatherer:
			return GameCursor.CursorGather;
		case Role.Guard:
			return GameCursor.CursorGuard;
		case Role.Builder:
			return GameCursor.CursorBuild;
		case Role.Repairing:
			return GameCursor.CursorRepair;
		case Role.Capturing:
			return GameCursor.CursorFlag;
		case Role.Crafter:
		{
			Recipe recipe = roleInfo.Recipe;
			return (recipe != null && recipe.IsProductDrinkableOrEdible()) ? GameCursor.CursorCook : GameCursor.CursorCraft;
		}
		case Role.Lumberjack:
			return GameCursor.CursorAxe;
		case Role.Cook:
			return GameCursor.CursorCook;
		case Role.Trader:
			return GameCursor.CursorGather;
		case Role.Miner:
			return GameCursor.CursorPickaxe;
		case Role.Trapper:
			return GameCursor.TrapperIcon;
		case Role.AnimalFeeder:
			return GameCursor.ChickenIcon;
		case Role.Organizer:
			return GameCursor.CursorOrganize;
		case Role.Medic:
			return GameCursor.CursorBandages;
		default:
			return null;
		}
	}

	public static void GetRoleDisplayText(Character character, RoleInfo roleInfo, RoleDisplayTextMode mode, StringBuilder sb)
	{
		Language language = GameImpl.Instance.Settings.Language;
		switch (roleInfo.Role)
		{
		case Role.Farmer:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Farmer, HUD_Farmer_Female, character.GetGender(language)), character, character));
			break;
		case Role.Gatherer:
		{
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Gatherer, HUD_Gatherer_Female, character.GetGender(language)), character, character));
			Community community3 = character.Community;
			if (community3 != null && roleInfo.ResourceType != null && mode != RoleDisplayTextMode.Short)
			{
				int craftingLimit4 = community3.GetCraftingLimit(roleInfo.ResourceType);
				int number3 = community3.CountInventoryItemsOfType(roleInfo.ResourceType);
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(roleInfo.ResourceType.NameHash));
				if (mode == RoleDisplayTextMode.Long)
				{
					sb.Append(':');
					sb.Append(' ');
					sb.AppendWithoutGarbage(number3);
					if (craftingLimit4 < int.MaxValue)
					{
						sb.Append('/');
						sb.AppendWithoutGarbage(craftingLimit4);
					}
				}
				sb.Append(')');
			}
			if (mode == RoleDisplayTextMode.Long && roleInfo.ResourceType != null)
			{
				Prop prop = GameTerrain.Instance.GetProp(roleInfo.TargetLocation.x, roleInfo.TargetLocation.y);
				if (prop != null)
				{
					Equipment equipment = prop.Inventory.FindItemOfType(roleInfo.ResourceType);
					if (equipment != null)
					{
						sb.Append('\n');
						sb.AppendWithoutGarbage(equipment.GetAmount());
						sb.Append(' ');
						sb.Append((equipment.GetAmount() == 1) ? GameImpl.Translate(HUD_Remaining_Singular) : GameImpl.Translate(HUD_Remaining));
					}
				}
			}
			if (roleInfo.ResourceType != null)
			{
				break;
			}
			Prop prop2 = GameTerrain.Instance.GetProp(roleInfo.TargetLocation.x, roleInfo.TargetLocation.y);
			if (prop2 != null)
			{
				sb.Append(' ');
				sb.Append('(');
				prop2.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
				sb.Append(')');
				if (mode == RoleDisplayTextMode.Long)
				{
					int num = prop2.Inventory.CountAllItems();
					sb.Append('\n');
					sb.AppendWithoutGarbage(num);
					sb.Append(' ');
					sb.Append((num == 1) ? GameImpl.Translate(HUD_Remaining_Singular) : GameImpl.Translate(HUD_Remaining));
				}
			}
			break;
		}
		case Role.Guard:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Guard, HUD_Guard_Female, character.GetGender(language)), character, character));
			break;
		case Role.Builder:
		{
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Builder, HUD_Builder_Female, character.GetGender(language)), character, character));
			BuildGoal buildGoal = character.GetBuildGoal();
			if (buildGoal != null && buildGoal.CurrentBuilding != null && mode != RoleDisplayTextMode.Short)
			{
				sb.Append(' ');
				sb.Append('(');
				buildGoal.CurrentBuilding.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
				sb.Append(')');
			}
			break;
		}
		case Role.Repairing:
		{
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Repairing, HUD_Repairing_Female, character.GetGender(language)), character, character));
			RepairGoal repairGoal = character.GetRepairGoal();
			if (repairGoal != null && repairGoal.CurrentBuildingToRepair != null && mode != RoleDisplayTextMode.Short)
			{
				sb.Append(' ');
				sb.Append('(');
				repairGoal.CurrentBuildingToRepair.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
				sb.Append(')');
			}
			break;
		}
		case Role.Capturing:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Capturing, HUD_Capturing_Female, character.GetGender(language)), character, character));
			if (mode != RoleDisplayTextMode.Short)
			{
				TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(roleInfo.TargetLocation.x, roleInfo.TargetLocation.y);
				if (fixedObjectOnTile != null)
				{
					sb.Append(' ');
					sb.Append('(');
					fixedObjectOnTile.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
					sb.Append(')');
				}
			}
			break;
		case Role.Crafter:
		{
			Community community2 = character.Community;
			Recipe recipe = roleInfo.Recipe;
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Crafter, HUD_Crafter_Female, character.GetGender(language)), character, character));
			if (community2 == null || recipe == null || mode == RoleDisplayTextMode.Short)
			{
				break;
			}
			if (recipe.IsDisassembly)
			{
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(recipe.NameHash));
				sb.Append(')');
				break;
			}
			EquipmentPrototype productPrototype = recipe.ProductPrototype;
			LiquidPrototype productLiquidPrototype = recipe.ProductLiquidPrototype;
			if (productLiquidPrototype != null)
			{
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(productLiquidPrototype.NameHash));
				if (mode == RoleDisplayTextMode.Long)
				{
					float totalLiquid = community2.GetTotalLiquid(productLiquidPrototype);
					int craftingLimit2 = community2.GetCraftingLimit(productLiquidPrototype);
					bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
					sb.Append(':');
					sb.Append(' ');
					sb.AppendWithoutGarbage(totalLiquid * (useMetricWeights ? 0.0295735f : 1f), 1);
					if (craftingLimit2 < int.MaxValue)
					{
						sb.Append('/');
						sb.AppendWithoutGarbage((float)craftingLimit2 * (useMetricWeights ? 0.0295735f : 1f), 1);
					}
					sb.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
				}
				sb.Append(')');
			}
			else
			{
				if (productPrototype == null)
				{
					break;
				}
				int number2 = community2.CountInventoryItemsOfType(productPrototype);
				int craftingLimit3 = community2.GetCraftingLimit(productPrototype);
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(productPrototype.NameHash));
				if (mode == RoleDisplayTextMode.Long)
				{
					sb.Append(':');
					sb.Append(' ');
					sb.AppendWithoutGarbage(number2);
					if (craftingLimit3 < int.MaxValue)
					{
						sb.Append('/');
						sb.Append(craftingLimit3);
					}
				}
				sb.Append(')');
			}
			break;
		}
		case Role.Lumberjack:
		{
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Lumberjack, HUD_Lumberjack_Female, character.GetGender(language)), character, character));
			Community community4 = character.Community;
			if (community4 != null && mode != RoleDisplayTextMode.Short && mode == RoleDisplayTextMode.Long)
			{
				int number4 = community4.CountInventoryItemsOfType(EquipmentPrototype.Wood);
				int craftingLimit5 = community4.GetCraftingLimit(EquipmentPrototype.Wood);
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(EquipmentPrototype.Wood.NameHash));
				sb.Append(':');
				sb.Append(' ');
				sb.AppendWithoutGarbage(number4);
				if (craftingLimit5 < int.MaxValue)
				{
					sb.Append('/');
					sb.AppendWithoutGarbage(craftingLimit5);
				}
				sb.Append(')');
			}
			break;
		}
		case Role.Cook:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Cook, HUD_Cook_Female, character.GetGender(language)), character, character));
			break;
		case Role.Trader:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Trader, HUD_Trader_Female, character.GetGender(language)), character, character));
			break;
		case Role.Miner:
		{
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Miner, HUD_Miner_Female, character.GetGender(language)), character, character));
			if (roleInfo.ResourceType == null || mode == RoleDisplayTextMode.Short)
			{
				break;
			}
			Community community = character.Community;
			if (community == null)
			{
				break;
			}
			int number = community.CountInventoryItemsOfType(roleInfo.ResourceType);
			int craftingLimit = community.GetCraftingLimit(roleInfo.ResourceType);
			sb.Append(' ');
			sb.Append('(');
			sb.Append(GameImpl.Translate(roleInfo.ResourceType.NameHash));
			if (mode == RoleDisplayTextMode.Long)
			{
				sb.Append(':');
				sb.Append(' ');
				sb.AppendWithoutGarbage(number);
				if (craftingLimit < int.MaxValue)
				{
					sb.Append('/');
					sb.AppendWithoutGarbage(craftingLimit);
				}
			}
			sb.Append(')');
			break;
		}
		case Role.Trapper:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Trapper, HUD_Trapper_Female, character.GetGender(language)), character, character));
			break;
		case Role.AnimalFeeder:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_AnimalFeeder, HUD_AnimalFeeder_Female, character.GetGender(language)), character, character));
			break;
		case Role.Organizer:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Organizer, HUD_Organizer_Female, character.GetGender(language)), character, character));
			break;
		case Role.Medic:
			sb.Append(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_Medic, HUD_Medic_Female, character.GetGender(language)), character, character));
			break;
		}
		if (roleInfo.Paused && mode == RoleDisplayTextMode.Long)
		{
			sb.Append(' ');
			sb.Append(GameImpl.Translate(HUD_RolePaused));
		}
	}

	public void Populate(Character character)
	{
		CurrentCharacter = character;
		RoleInfo roleInfo = ((RoleIndex < CurrentCharacter.Roles.Count) ? CurrentCharacter.Roles[RoleIndex] : default(RoleInfo));
		UnityPriorityUpButton.gameObject.SetActive(CurrentCharacter.Roles.Count > 1 && RoleIndex > 0);
		UnityPriorityDownButton.gameObject.SetActive(CurrentCharacter.Roles.Count > 1 && RoleIndex < CurrentCharacter.Roles.Count - 1);
		Texture2D roleDisplayIcon = GetRoleDisplayIcon(CurrentCharacter, roleInfo);
		sb.Length = 0;
		GetRoleDisplayText(CurrentCharacter, roleInfo, RoleDisplayTextMode.Long, sb);
		if (roleDisplayIcon != null && sb.Length > 0)
		{
			base.gameObject.SetActive(value: true);
			UnityText.SetUnityTextIfDifferent(sb);
			UnityIcon.texture = roleDisplayIcon;
			UnityIcon.color = roleInfo.GetRoleIconCol();
			UnityPauseButtonText.SetUnityText(GameImpl.Translate(roleInfo.Paused ? HUD_Resume : HUD_Pause));
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		UnityUrgentToggle.onValueChanged.RemoveAllListeners();
		UnityUrgentToggle.isOn = roleInfo.Urgent;
		UnityUrgentToggle.onValueChanged.AddListener(OnUrgentToggled);
		UnityPauseButton.interactable = CurrentCharacter.DownTime == 0f;
	}

	public void OnCancelRoleButtonClicked()
	{
		WantCancelRole = true;
	}

	public void OnPauseRoleButtonClicked()
	{
		WantPauseRole = true;
	}

	public void OnPriorityUpButtonClicked()
	{
		WantPriorityUp = true;
	}

	public void OnPriorityDownButtonClicked()
	{
		WantPriorityDown = true;
	}

	public void OnUrgentToggled(bool v)
	{
		WantUrgentToggled = true;
	}

	public void HandleInput(InputFrame inputFrame)
	{
		if (WantCancelRole && RoleIndex < CurrentCharacter.Roles.Count)
		{
			if (inputFrame != null)
			{
				RoleInfo roleInfo = CurrentCharacter.Roles[RoleIndex];
				inputFrame.AddAction(InputAction.CancelRole(CurrentCharacter, roleInfo.Role, roleInfo.ResourceType, roleInfo.Recipe, roleInfo.TargetLocation));
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
			WantCancelRole = false;
		}
		if (WantPauseRole && RoleIndex < CurrentCharacter.Roles.Count)
		{
			if (inputFrame != null)
			{
				RoleInfo roleInfo2 = CurrentCharacter.Roles[RoleIndex];
				if (roleInfo2.Paused)
				{
					if (CurrentCharacter.DownTime == 0f)
					{
						inputFrame.AddAction(InputAction.ResumeRole(CurrentCharacter, roleInfo2.Role, roleInfo2.ResourceType, roleInfo2.Recipe, roleInfo2.TargetLocation));
					}
				}
				else
				{
					inputFrame.AddAction(InputAction.PauseRole(CurrentCharacter, roleInfo2.Role, roleInfo2.ResourceType, roleInfo2.Recipe, roleInfo2.TargetLocation));
				}
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
			WantPauseRole = false;
		}
		if (WantPriorityUp && RoleIndex < CurrentCharacter.Roles.Count)
		{
			if (inputFrame != null && RoleIndex > 0)
			{
				RoleInfo roleInfo3 = CurrentCharacter.Roles[RoleIndex];
				inputFrame.AddAction(InputAction.ChangeRolePriority(CurrentCharacter, roleInfo3.Role, roleInfo3.ResourceType, roleInfo3.Recipe, roleInfo3.TargetLocation, -1));
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
			WantPriorityUp = false;
		}
		if (WantPriorityDown && RoleIndex < CurrentCharacter.Roles.Count)
		{
			if (inputFrame != null && RoleIndex < CurrentCharacter.Roles.Count - 1)
			{
				RoleInfo roleInfo4 = CurrentCharacter.Roles[RoleIndex];
				inputFrame.AddAction(InputAction.ChangeRolePriority(CurrentCharacter, roleInfo4.Role, roleInfo4.ResourceType, roleInfo4.Recipe, roleInfo4.TargetLocation, 1));
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
			WantPriorityDown = false;
		}
		if (WantUrgentToggled && RoleIndex < CurrentCharacter.Roles.Count)
		{
			if (inputFrame != null)
			{
				RoleInfo roleInfo5 = CurrentCharacter.Roles[RoleIndex];
				inputFrame.AddAction(InputAction.SetRoleUrgent(CurrentCharacter, roleInfo5.Role, roleInfo5.ResourceType, roleInfo5.Recipe, roleInfo5.TargetLocation, UnityUrgentToggle.isOn));
			}
			else
			{
				SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			}
			WantUrgentToggled = false;
		}
	}
}
