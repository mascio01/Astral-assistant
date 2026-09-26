using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectManager : IReflectable
{
	public static BaseObjectManager Instance;

	public static string[] BaseObjectNames = StringUtil.GetEnumNames<BaseObjectType>();

	public static Type[] BaseObjectTypes = StringUtil.GetEnumTypes<BaseObjectType>();

	private int NextFreeId = 1;

	public bool BaseObjectsMightNotBeSorted;

	public List<BaseObject> BaseObjects = new List<BaseObject>();

	private Dictionary<string, BaseObject> UniqueIDToObject = new Dictionary<string, BaseObject>();

	private static GameProfiler TerrainHeaderReflectTimer = new GameProfiler("TerrainHeaderReflect");

	private static GameProfiler CharacterHeadersReflectTimer = new GameProfiler("CharacterHeadersReflect");

	private static GameProfiler PropHeadersReflectTimer = new GameProfiler("PropHeadersReflect");

	private static GameProfiler CommunityHeadersReflectTimer = new GameProfiler("CommunityHeadersReflect");

	private static GameProfiler TerrainReflectTimer = new GameProfiler("TerrainReflect");

	private static GameProfiler CharactersReflectTimer = new GameProfiler("CharactersReflect");

	private static GameProfiler PropsReflectTimer = new GameProfiler("PropsReflect");

	private static GameProfiler CommunitiesReflectTimer = new GameProfiler("CommunitiesReflect");

	public static bool IsDeletingOrphans = false;

	public static BaseObject[] PrototypeGameObjects = new BaseObject[244];

	public int GetNextFreeId()
	{
		return NextFreeId;
	}

	public BaseObjectManager()
	{
		Instance = this;
	}

	public void Unload()
	{
		Instance = null;
	}

	public int Assign(BaseObject obj)
	{
		BaseObjects.Add(obj);
		return NextFreeId++;
	}

	public void Remove(BaseObject obj)
	{
		int num = FindBaseObjectIndexByID(obj.Id);
		if (num != -1)
		{
			BaseObjects.RemoveAt(num);
		}
	}

	private int FindBaseObjectIndexByID(int id)
	{
		int num = 0;
		int num2 = BaseObjects.Count - 1;
		while (num <= num2)
		{
			int num3 = (num + num2) / 2;
			int id2 = BaseObjects[num3].Id;
			if (id2 < id)
			{
				num = num3 + 1;
				continue;
			}
			if (id2 > id)
			{
				num2 = num3 - 1;
				continue;
			}
			return num3;
		}
		return -1;
	}

	public BaseObject FindBaseObjectByID(int id)
	{
		int num = 0;
		int num2 = BaseObjects.Count - 1;
		while (num <= num2)
		{
			int num3 = (num + num2) / 2;
			BaseObject baseObject = BaseObjects[num3];
			if (baseObject.Id < id)
			{
				num = num3 + 1;
				continue;
			}
			if (baseObject.Id > id)
			{
				num2 = num3 - 1;
				continue;
			}
			return baseObject;
		}
		return null;
	}

	public void RegisterUniqueID(string uniqueID, BaseObject obj)
	{
		UniqueIDToObject[uniqueID] = obj;
	}

	public void UnregisterUniqueID(string uniqueID, BaseObject obj)
	{
		UniqueIDToObject.Remove(uniqueID);
	}

	public BaseObject GetObjectByUniqueID(string uniqueID)
	{
		UniqueIDToObject.TryGetValue(uniqueID, out var value);
		return value;
	}

	public int CountObjectsOfType(BaseObjectType type)
	{
		int num = 0;
		foreach (BaseObject baseObject in BaseObjects)
		{
			if (baseObject.GetBaseObjectType() == type)
			{
				num++;
			}
		}
		return num;
	}

	public int CountObjectsOfType(Type type)
	{
		int num = 0;
		foreach (BaseObject baseObject in BaseObjects)
		{
			if (baseObject.GetType() == type || baseObject.GetType().IsSubclassOf(type))
			{
				num++;
			}
		}
		return num;
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.IsFastPath && reflector is CustomBinaryWriter customBinaryWriter)
		{
			List<Character> characters = Session.Instance.CharacterManager.Characters;
			List<Prop> allProps = Session.Instance.PropManager.AllProps;
			List<Community> communities = Session.Instance.CommunityManager.Communities;
			GameTerrain gameTerrain = BaseObjects[0] as GameTerrain;
			int value = 1 + characters.Count + allProps.Count + communities.Count;
			reflector.Add(ref value);
			using (new ProfileMarker(TerrainHeaderReflectTimer))
			{
				if (gameTerrain != null)
				{
					customBinaryWriter.Write((byte)gameTerrain.GetBaseObjectType());
					customBinaryWriter.Write(gameTerrain.Id);
					gameTerrain.ReflectEarly(customBinaryWriter);
				}
			}
			using (new ProfileMarker(CharacterHeadersReflectTimer))
			{
				foreach (Character item in characters)
				{
					customBinaryWriter.Write((byte)item.GetBaseObjectType());
					customBinaryWriter.Write(item.Id);
					item.ReflectEarly(customBinaryWriter);
				}
			}
			using (new ProfileMarker(PropHeadersReflectTimer))
			{
				foreach (Prop item2 in allProps)
				{
					customBinaryWriter.Write((byte)item2.GetBaseObjectType());
					customBinaryWriter.Write(item2.Id);
					item2.ReflectEarly(customBinaryWriter);
				}
			}
			using (new ProfileMarker(CommunityHeadersReflectTimer))
			{
				foreach (Community item3 in communities)
				{
					customBinaryWriter.Write((byte)item3.GetBaseObjectType());
					customBinaryWriter.Write(item3.Id);
					item3.ReflectEarly(customBinaryWriter);
				}
			}
			using (new ProfileMarker(TerrainReflectTimer))
			{
				gameTerrain?.Reflect(customBinaryWriter);
			}
			using (new ProfileMarker(CharactersReflectTimer))
			{
				foreach (Character item4 in characters)
				{
					item4.Reflect(customBinaryWriter);
				}
			}
			using (new ProfileMarker(PropsReflectTimer))
			{
				foreach (Prop item5 in allProps)
				{
					item5.Reflect(customBinaryWriter);
				}
			}
			using (new ProfileMarker(CommunitiesReflectTimer))
			{
				foreach (Community item6 in communities)
				{
					item6.Reflect(customBinaryWriter);
				}
				return;
			}
		}
		int value2 = BaseObjects.Count;
		reflector.Add(ref value2);
		if (reflector.IsDeserialising)
		{
			BaseObjects.Capacity = value2;
		}
		for (int i = 0; i < value2; i++)
		{
			BaseObject baseObject = null;
			BaseObjectType value3;
			int value4;
			if (reflector.IsDeserialising)
			{
				value3 = BaseObjectType.Invalid;
				value4 = 0;
			}
			else
			{
				baseObject = BaseObjects[i];
				value3 = baseObject.GetBaseObjectType();
				value4 = baseObject.Id;
			}
			reflector.Add(ref value3);
			reflector.Add(ref value4);
			if (reflector.IsDeserialising)
			{
				bool num = FindBaseObjectByID(value4) != null;
				NextFreeId = Math.Max(NextFreeId, value4 + 1);
				baseObject = Create(value3);
				baseObject.AssignId(value4);
				if (!num)
				{
					BaseObjects.Add(baseObject);
				}
			}
			baseObject.ReflectEarly(reflector);
		}
		List<BaseObject> list = BaseObjects;
		if (BaseObjectsMightNotBeSorted)
		{
			list = new List<BaseObject>();
			BaseObjects.CopyToList(list);
			BaseObjects.Sort(SortBaseObjectsByIdAscending.Instance);
		}
		if (BaseObjects.Count == 0 || !(BaseObjects[0] is GameTerrain))
		{
			throw new Exception("Expected terrain as first base object");
		}
		for (int j = 0; j < value2; j++)
		{
			BaseObject baseObject2 = list[j];
			reflector.Add(baseObject2);
			if (reflector.IsDeserialising)
			{
				baseObject2.Init();
			}
		}
		if (reflector.IsDeserialising)
		{
			DeleteOrphans();
		}
	}

	private void DeleteOrphans()
	{
		List<BaseObject> list = new List<BaseObject>();
		foreach (BaseObject baseObject in BaseObjects)
		{
			if (baseObject is Equipment equipment)
			{
				if (equipment.InventoryOwner == null)
				{
					list.Add(baseObject);
				}
				else if (equipment.InventoryOwner is Prop && equipment.InventoryOwner.GetMaxInventoryWeight() <= 0f)
				{
					equipment.InventoryOwner.GetInventory().RemoveEquipmentWithNoPrototype(equipment.InventoryOwner, equipment);
					list.Add(baseObject);
				}
				else if (equipment.GetPrototype() == null)
				{
					equipment.InventoryOwner.GetInventory().RemoveEquipmentWithNoPrototype(equipment.InventoryOwner, equipment);
					list.Add(baseObject);
				}
				continue;
			}
			if (baseObject is QuestInstance { Quest: null } questInstance)
			{
				Debug.LogWarning("Deleting orphaned quest instance " + ((questInstance.QuestGiver != null) ? (" giver: " + questInstance.QuestGiver.GetDisplayNameString()) : "") + ((questInstance.QuestSeeker != null) ? (" seeker: " + questInstance.QuestSeeker.GetDisplayNameString()) : "") + ((questInstance.QuestObject != null) ? (" object: " + questInstance.QuestObject.GetDisplayNameString()) : ""));
				list.Add(questInstance);
			}
			if (!(baseObject is TileObject tileObject))
			{
				continue;
			}
			UnderConstructionInfo underConstructionInfo = tileObject.GetUnderConstructionInfo();
			if (underConstructionInfo != null && underConstructionInfo.Recipe == null)
			{
				list.Add(baseObject);
				continue;
			}
			if (baseObject is PlantableCrop { Prototype: null })
			{
				list.Add(baseObject);
				continue;
			}
			if (baseObject is FoodProp foodProp && GameTerrain.Instance.IsTileOutsideBounds(foodProp.Tile.x, foodProp.Tile.y))
			{
				list.Add(baseObject);
				continue;
			}
			if (baseObject is PitTrap pitTrap && !list.Contains(pitTrap))
			{
				GameTerrain.Instance.FindDuplicatePitTraps(pitTrap, list);
			}
			if (!(baseObject is Building building))
			{
				continue;
			}
			if (building.Prototype == null)
			{
				list.Add(baseObject);
				continue;
			}
			for (int i = 0; i < building.Inhabitants.Length; i++)
			{
				if (building.Inhabitants[i] != null && building.Inhabitants[i].InsideBuilding != building)
				{
					building.Inhabitants[i] = null;
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		Debug.Log("Found orphaned " + list.Count + " objects");
		foreach (BaseObject item in list)
		{
			item.Delete();
		}
	}

	public static BaseObject Create(BaseObjectType baseObjectType)
	{
		Type type = BaseObjectTypes[(int)baseObjectType];
		if (!(type != null))
		{
			return null;
		}
		return (BaseObject)Activator.CreateInstance(type);
	}

	public static void SetupPrototypeGameObjects()
	{
		for (int i = 0; i < 244; i++)
		{
			if (i != 0 && i != 1 && !(BaseObjectTypes[i] == null))
			{
				PrototypeGameObjects[i] = Create((BaseObjectType)i);
			}
		}
		for (int j = 1; j < 210; j++)
		{
			Goal.Create((GoalType)j);
		}
	}
}
