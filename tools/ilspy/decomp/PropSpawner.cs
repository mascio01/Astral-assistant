using System;
using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : DebugMenu
{
	public class Category
	{
		public string Name;

		public List<Category> SubCategories = new List<Category>();

		public PropPrototype Proto;

		public BaseObjectType ObjectType;

		public Category(string name, PropPrototype proto, BaseObjectType objectType)
		{
			Name = name;
			Proto = proto;
			ObjectType = objectType;
		}

		public Category FindOrCreateSubCategory(string name, PropPrototype proto, BaseObjectType objectType)
		{
			foreach (Category subCategory in SubCategories)
			{
				if (subCategory.Name == name)
				{
					return subCategory;
				}
			}
			Category category = new Category(name, proto, objectType);
			SubCategories.Add(category);
			return category;
		}
	}

	private static Category RootCategory;

	private Category Cat;

	private PropPrototype CurrentSpawnProto;

	private BaseObjectType CurrentSpawnType;

	private TreeType CurrentTreeType;

	private EquipmentPrototype CurrentMeatType;

	private DebugMenuCommunity CommunitySetter;

	public static void SetupPropCategories()
	{
		RootCategory = new Category("", null, BaseObjectType.Invalid);
		foreach (KeyValuePair<int, PropPrototype> item3 in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			PropPrototype value = item3.Value;
			if (!string.IsNullOrEmpty(value.Category))
			{
				Category category = RootCategory;
				string[] array = value.Category.Split('/');
				for (int i = 0; i < array.Length; i++)
				{
					category = category.FindOrCreateSubCategory(array[i], null, BaseObjectType.Invalid);
				}
				Category item = new Category(value.Name, value, BaseObjectType.Invalid);
				category.SubCategories.Add(item);
			}
		}
		BaseObject[] prototypeGameObjects = BaseObjectManager.PrototypeGameObjects;
		foreach (BaseObject baseObject in prototypeGameObjects)
		{
			if (baseObject != null && !string.IsNullOrEmpty(baseObject.Category))
			{
				Category category2 = RootCategory;
				string[] array2 = baseObject.Category.Split('/');
				for (int k = 0; k < array2.Length; k++)
				{
					category2 = category2.FindOrCreateSubCategory(array2[k], null, BaseObjectType.Invalid);
				}
				Category item2 = new Category(baseObject.GetDisplayNameString(), null, baseObject.GetBaseObjectType());
				category2.SubCategories.Add(item2);
				if (baseObject is Prop prop && prop.GetFlammability() != Flammability.Invulnerable && prop.GetMaxDamage() == float.MaxValue && !(prop is Campfire))
				{
					Debug.LogWarning("GetMaxDamage not set for " + prop.GetDisplayNameString());
				}
			}
		}
	}

	public PropSpawner()
		: base(GameImpl.Translate("DEBUG_PropSpawner"))
	{
		Cat = RootCategory;
		BuildMenu();
	}

	public PropSpawner(Category cat)
		: base(cat.Name)
	{
		Cat = cat;
		BuildMenu();
	}

	private void BuildMenu()
	{
		CommunitySetter = new DebugMenuCommunity(GameImpl.Translate("DEBUG_Community"));
		Items.Add(CommunitySetter);
		foreach (Category subCategory in Cat.SubCategories)
		{
			if (subCategory.Proto == null && subCategory.ObjectType == BaseObjectType.Invalid)
			{
				Items.Add(new DebugMenuItemOpenPropPage(subCategory));
				continue;
			}
			Category localCat = subCategory;
			if (localCat.ObjectType == BaseObjectType.FallenTreeProp)
			{
				for (int i = 0; i < 18; i++)
				{
					TreeType treeType = (TreeType)i;
					Items.Add(new DebugMenuItemToggle(treeType.ToString(), () => CurrentSpawnType == localCat.ObjectType && CurrentTreeType == treeType, delegate
					{
						CurrentSpawnType = localCat.ObjectType;
						CurrentTreeType = treeType;
					}));
				}
			}
			else if (localCat.ObjectType == BaseObjectType.FoodProp)
			{
				for (int num = 0; num < EquipmentPrototype.Meat.Length; num++)
				{
					EquipmentPrototype meatType = EquipmentPrototype.Meat[num];
					Items.Add(new DebugMenuItemToggle(meatType.ToString(), () => CurrentSpawnType == localCat.ObjectType && CurrentMeatType == meatType, delegate
					{
						CurrentSpawnType = localCat.ObjectType;
						CurrentMeatType = meatType;
					}));
				}
			}
			else if (localCat.ObjectType != BaseObjectType.Invalid)
			{
				Items.Add(new DebugMenuItemToggle(localCat.Name, () => CurrentSpawnType == localCat.ObjectType, delegate
				{
					CurrentSpawnType = localCat.ObjectType;
				}));
			}
			else
			{
				Items.Add(new DebugMenuItemToggle(localCat.Name, () => CurrentSpawnProto == localCat.Proto, delegate
				{
					CurrentSpawnProto = localCat.Proto;
				}));
			}
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (!instance3.IsJustPressed(InputFunction.MainAction))
		{
			return;
		}
		RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
		if (raycastResult.HitObject != instance2)
		{
			return;
		}
		TileObject fixedObjectOnTile = instance2.GetFixedObjectOnTile(raycastResult.Tile.x, raycastResult.Tile.y);
		if ((fixedObjectOnTile == null || fixedObjectOnTile is Zone) && (CurrentSpawnProto != null || CurrentSpawnType != BaseObjectType.Invalid))
		{
			Prop.OrientationType orientation = Prop.OrientationType.Deg0;
			TileObject tileObject = ((CurrentSpawnType == BaseObjectType.Invalid) ? CurrentSpawnProto.ProtoInstance : (BaseObjectManager.PrototypeGameObjects[(int)CurrentSpawnType] as TileObject));
			if (tileObject is Prop prop)
			{
				prop.CalcMinMaxTile(raycastResult.Tile, orientation, out var minTile, out var maxTile);
				if (!instance2.IsTileRectWithinBounds(minTile - new TerrainCoord(1, 1), maxTile + new TerrainCoord(1, 1)))
				{
					return;
				}
			}
			TileObject tileObject2 = null;
			if (tileObject is Zone)
			{
				tileObject2 = Zone.Spawn(raycastResult.Tile);
			}
			else if (tileObject is FallenTreeProp)
			{
				tileObject2 = FallenTreeProp.Spawn(raycastResult.Tile, CurrentTreeType, MathUtil.NonDeterministicRand);
			}
			else if (tileObject is FoodProp)
			{
				CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
				Vector3 tileCentrePos = GameTerrain.Instance.GetTileCentrePos(raycastResult.Tile);
				tileObject2 = FoodProp.Spawn(CurrentMeatType, tileCentrePos + new Vector3(0f, 0.05f, 0f), Quaternion.Euler(nonDeterministicRand.RandomFloat() * (MathF.PI * 2f), nonDeterministicRand.RandomFloat() * (MathF.PI * 2f), nonDeterministicRand.RandomFloat() * (MathF.PI * 2f)), InfectionType.None, null);
			}
			else if (CurrentSpawnType != BaseObjectType.Invalid)
			{
				if (tileObject is SingleTileProp)
				{
					tileObject2 = SingleTileProp.Spawn(CurrentSpawnType, raycastResult.Tile);
					tileObject2.SetCommunity(DebugMenuCommunity.Community);
				}
				else
				{
					tileObject2 = Prop.Spawn(CurrentSpawnType, raycastResult.Tile, orientation);
					tileObject2.SetCommunity(DebugMenuCommunity.Community);
					if (DebugMenuCommunity.Community != null && DebugMenuCommunity.Community.CommunityType == CommunityType.Player)
					{
						tileObject2.MarkInvestigated(null);
					}
				}
			}
			else if (tileObject is PlantableCrop)
			{
				tileObject2 = PlantableCrop.Spawn(CurrentSpawnProto, raycastResult.Tile, DebugMenuCommunity.Community, (byte)(MathUtil.NonDeterministicRand.Next(5) + 1), MathUtil.NonDeterministicRand);
			}
			else if (tileObject is SingleTileProp)
			{
				tileObject2 = SingleTileProp.Spawn(CurrentSpawnProto, raycastResult.Tile);
				tileObject2.SetCommunity(DebugMenuCommunity.Community);
			}
			else
			{
				tileObject2 = Prop.Spawn(CurrentSpawnProto, raycastResult.Tile, orientation);
				tileObject2.SetCommunity(DebugMenuCommunity.Community);
				if (DebugMenuCommunity.Community != null && DebugMenuCommunity.Community.CommunityType == CommunityType.Player)
				{
					tileObject2.MarkInvestigated(null);
				}
			}
		}
		Session.Instance.AchievementsEnabled = false;
	}
}
