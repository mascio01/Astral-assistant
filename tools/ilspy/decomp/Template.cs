using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public class Template : BaseScriptObject
{
	[AlwaysVisible]
	public TemplateType Type;

	[DefaultValue(TemplateSpawnLocation.InheritFromParent)]
	public TemplateSpawnLocation SpawnLocation;

	[DefaultValue("")]
	[OnlyVisibleForSpawnLocation(TemplateSpawnLocation.PreferMarker)]
	public string SpawnMarkerName = "";

	[DefaultValue(CommunityType.Temporary)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public CommunityType CommunityType = CommunityType.Temporary;

	[DefaultValue("")]
	[XmlAttribute]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public string NativeCommunityName = "";

	[TranslatedTextField]
	[DefaultValue("")]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public string TranslatedCommunityName = "";

	[XmlIgnore]
	public int CommunityNameHash;

	[DefaultValue("")]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public string LootLocation = "";

	[DefaultValue("")]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public string PersonalityFaction = "";

	[DefaultValue("")]
	[OnlyVisibleForTemplateType(TemplateType.Equipment, TemplateType.Invalid)]
	public string EquipmentPrototypeName = "";

	[DefaultValue("")]
	[OnlyVisibleForTemplateType(TemplateType.Prop, TemplateType.Invalid)]
	public string PropPrototypeName = "";

	[DefaultValue(Role.None)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public Role Role;

	[DefaultValue(0)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	[OnlyVisibleForRole(Role.Trader)]
	public int TraderLevel;

	[DefaultValue(50f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float GenderRatio = 50f;

	[DefaultValue(TemplateSpawnState.Alive)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public TemplateSpawnState SpawnState;

	[DefaultValue(RelationshipType.None)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public RelationshipType RelationshipWithActor;

	[DefaultValue(RelationshipType.None)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public RelationshipType RelationshipWithTarget;

	[DefaultValue(InfectionType.None)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Equipment)]
	public InfectionType Infection;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	[OnlyVisibleForZombie]
	public bool NamedZombie;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float BodyArmorProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float LegArmorProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float HelmetProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float MeleeWeaponProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float AmmoWeaponProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float MolotovProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float PipeBombProbability;

	[DefaultValue(InfectionType.None)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public InfectionType InfectedWeaponsMaxStrain;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float InfectedWeaponsProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public float WaterBottleProbability;

	[DefaultValue(TemplateHasInvisibleStrain.Default)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	[NotVisibleForZombie]
	public TemplateHasInvisibleStrain HasInvisibleStrain;

	[DefaultValue(TemplateCrippled.No)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	[OnlyVisibleForZombie]
	public TemplateCrippled ZombieCrippled;

	[DefaultValue(true)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public bool SeasonallyAppropriateClothing = true;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public bool DontSimulateSurvivalFactorsUntilDiscovered;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public bool DontSimulateSurvivalFactorsUntilJoinCommunity;

	[DefaultValue(0)]
	[OnlyVisibleForTemplateType(TemplateType.Character, TemplateType.Invalid)]
	public int GeneratedLootCount;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public float GeneratedRelationshipsProbability;

	[DefaultValue(0f)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public float PlayerTrackingProbability;

	[DefaultValue(-1f)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public float StayOnMapProbability = -1f;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	[OnlyVisibleForCommunityType(CommunityType.RovingRefugee, CommunityType.HunterLooter)]
	public bool CanOccupyEmptyBases;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public bool IsFEMA;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Community, TemplateType.Invalid)]
	public bool PlayerSurrenderDisabled;

	[DefaultValue(1)]
	[OnlyVisibleForTemplateType(TemplateType.Equipment, TemplateType.Invalid)]
	public int Amount = 1;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Equipment, TemplateType.Invalid)]
	public bool AllowExceedWeightLimit;

	[DefaultValue(false)]
	[OnlyVisibleForTemplateType(TemplateType.Equipment, TemplateType.Invalid)]
	public bool Concealed;

	[DefaultValue(false)]
	[OnlyVisibleForClothing]
	public bool WearMe;

	[DefaultValue(-1)]
	[OnlyVisibleForEquipmentWithVariation(OnlyVisibleForEquipmentWithVariation.VariationType.Color1)]
	public int ColorVariation = -1;

	[DefaultValue(-1)]
	[OnlyVisibleForEquipmentWithVariation(OnlyVisibleForEquipmentWithVariation.VariationType.Color2)]
	public int ColorVariation2 = -1;

	[DefaultValue(-1)]
	[OnlyVisibleForEquipmentWithVariation(OnlyVisibleForEquipmentWithVariation.VariationType.Color3)]
	public int ColorVariation3 = -1;

	[DefaultValue(-1)]
	[OnlyVisibleForEquipmentWithVariation(OnlyVisibleForEquipmentWithVariation.VariationType.Material)]
	public int MaterialVariation = -1;

	public List<TemplateChild> Children;

	public List<Condition> Conditions;

	public List<StoryEvent> Events;

	private static List<Community> TempDeadCommunitiesToOccupy = new List<Community>();

	private static string Countryside = "Countryside";

	public string GetCommunityNameKey()
	{
		return "COMMUNITYNAME_" + UniqueID;
	}

	public override void OnUniqueIDChanged()
	{
		CommunityNameHash = StringUtil.JenkinsHash(GetCommunityNameKey());
	}

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		OnUniqueIDChanged();
		story.EnglishTranslation.Keys[CommunityNameHash] = NativeCommunityName;
		if (Children != null)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				Children[i] = Children[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Conditions != null)
		{
			for (int j = 0; j < Conditions.Count; j++)
			{
				Conditions[j] = Conditions[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Events != null)
		{
			for (int k = 0; k < Events.Count; k++)
			{
				Events[k] = Events[k].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Children != null && Children.Count == 0)
		{
			Children = null;
		}
		if (Conditions != null && Conditions.Count == 0)
		{
			Conditions = null;
		}
		if (Events != null && Events.Count == 0)
		{
			Events = null;
		}
	}

	public void SaveTranslatedText(Story story)
	{
		story.ForeignTranslation.Keys[CommunityNameHash] = TranslatedCommunityName;
		TranslatedCommunityName = "";
	}

	public void LoadTranslatedText(Story story)
	{
		TranslatedCommunityName = ((story.ForeignTranslation != null) ? story.ForeignTranslation.Translate(CommunityNameHash) : "");
	}

	public void GetInfectionTypes(List<InfectionType> infectionTypes)
	{
		if (Type == TemplateType.Character && !infectionTypes.Contains(Infection))
		{
			infectionTypes.Add(Infection);
		}
		if (Children != null)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				GameImpl.Instance.FindTemplateByUniqueID(Children[i].UniqueID)?.GetInfectionTypes(infectionTypes);
			}
		}
	}

	private bool WantNotEnclosed()
	{
		TemplateSpawnLocation spawnLocation = SpawnLocation;
		if ((uint)(spawnLocation - 2) <= 8u || spawnLocation == TemplateSpawnLocation.PreferMarker)
		{
			return true;
		}
		return false;
	}

	private bool WantNotOwned()
	{
		return WantNotEnclosed();
	}

	private bool IsHunterCommunity()
	{
		if (Type == TemplateType.Community)
		{
			CommunityType communityType = CommunityType;
			if ((uint)(communityType - 7) <= 3u || communityType == CommunityType.HunterMercenary)
			{
				return true;
			}
		}
		return false;
	}

	private bool WantOutOfSightOfPlayer()
	{
		return true;
	}

	private bool IsValidLocation(TerrainCoord tile, TileObject spawnedObj, ref List<CommunityManager.TileAndRadius> cachedPlayerCommunityTiles)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		GameTerrain instance = GameTerrain.Instance;
		if (instance.IsTileOutsideBounds(tile.x, tile.y))
		{
			return false;
		}
		if (instance.IsImpassable(tile.x, tile.y, 2051, null, spawnedObj))
		{
			return false;
		}
		if (WantOutOfSightOfPlayer())
		{
			if (cachedPlayerCommunityTiles == null)
			{
				cachedPlayerCommunityTiles = new List<CommunityManager.TileAndRadius>();
				communityManager.CachePlayerCommunityTiles(cachedPlayerCommunityTiles);
			}
			bool inSightRange = false;
			communityManager.GetMinDistSqrCachedPlayerCommunityTiles(cachedPlayerCommunityTiles, tile, 10f, ref inSightRange);
			if (inSightRange)
			{
				return false;
			}
		}
		if (WantNotEnclosed())
		{
			TerrainCoord terrainCoord = ((spawnedObj is Prop) ? ((Prop)spawnedObj).GetNearestPassableTileTo(tile, 0, null, null) : tile);
			if (instance.IsTileEnclosed(terrainCoord.x, terrainCoord.y) || !instance.IsTileSpawnable(terrainCoord.x, terrainCoord.y))
			{
				return false;
			}
		}
		if (WantNotOwned() && instance.GetOwnerCommunityIdForTile(tile.x, tile.y) != 0)
		{
			return false;
		}
		if (IsHunterCommunity() && communityManager.IsWithinRangeOfHunters(tile, 64f))
		{
			return false;
		}
		return true;
	}

	public BaseObject SpawnFromTemplate(TerrainCoord tile, bool canBeEnclosed, Building spawnInBuilding, Community spawnInCommunity, TileObject spawnedObj, Character actor, Character target, BaseObject obj, MemoryParam param, int numToSpawn, InvaderInstance invaderInstance, ref List<CommunityManager.TileAndRadius> cachedPlayerCommunityTiles)
	{
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		GameTerrain instance3 = GameTerrain.Instance;
		CustomRandom deterministicRand = instance2.DeterministicRand;
		bool isInvader = invaderInstance != null;
		if (!StoryManager.AreAllConditionsSatisfied(Conditions, actor, target, obj, param))
		{
			return null;
		}
		TemplateSpawnLocation templateSpawnLocation = SpawnLocation;
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			switch (templateSpawnLocation)
			{
			case TemplateSpawnLocation.Anywhere:
				tile = deterministicRand.RandomTile(new TerrainCoord(32, 32), new TerrainCoord(instance3.Size - 32, instance3.Size - 32));
				canBeEnclosed = true;
				flag = true;
				break;
			case TemplateSpawnLocation.InheritFromParent:
				if (tile == TerrainCoord.Invalid)
				{
					templateSpawnLocation = TemplateSpawnLocation.Anywhere;
				}
				else
				{
					flag = true;
				}
				break;
			case TemplateSpawnLocation.PreferOpenCountryside:
			{
				for (int i = 0; i < 100; i++)
				{
					tile = deterministicRand.RandomTile(new TerrainCoord(32, 32), new TerrainCoord(instance3.Size - 32, instance3.Size - 32));
					if (instance2.CommunityManager.GetTownForTile(tile, 0f) == null && !instance3.GetNearestPointOnPath(instance3.GetTileCentreXZ(tile), river: false, 8f, out var _, out var _, out var _).HasValue)
					{
						break;
					}
				}
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferRoadEntrance:
			{
				ControlPoint? controlPoint = CommunityManager.PickRandomMapEntrance(deterministicRand);
				if (!controlPoint.HasValue)
				{
					templateSpawnLocation = TemplateSpawnLocation.Anywhere;
					break;
				}
				tile = GameTerrain.Instance.GetTileCoordForPosXZ(controlPoint.Value.Pos) + deterministicRand.RandomTile(new TerrainCoord(-32, -32), new TerrainCoord(32, 32));
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferTownBuildings:
			{
				List<Town> list6 = new List<Town>();
				foreach (Town town5 in instance2.CommunityManager.Towns)
				{
					if (!town5.IgnoreForQuests)
					{
						list6.Add(town5);
					}
				}
				if (list6.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferCountrysideProps;
					break;
				}
				Town town = list6[deterministicRand.Next(list6.Count)];
				List<Building> list7 = new List<Building>();
				foreach (Prop building6 in town.Buildings)
				{
					if (building6 is Building building3 && !(instance2.CommunityManager.PlayerCommunity.GetDistSqToNearestBuilding(building3.Tile) <= MathUtil.Squared(48f)) && !building3.IgnoreForQuests)
					{
						list7.Add(building3);
					}
				}
				if (list7.Count == 0)
				{
					foreach (Prop building7 in town.Buildings)
					{
						if (building7 is Building building4 && (building4.Community == null || building4.Community.CommunityType != CommunityType.Player) && !building4.IgnoreForQuests)
						{
							list7.Add(building4);
						}
					}
				}
				if (list7.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferVehiclesOnRoads;
					break;
				}
				spawnedObj = (spawnInBuilding = list7[deterministicRand.Next(list7.Count)]);
				tile = spawnInBuilding.Tile;
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferTowns:
			{
				List<Town> list12 = new List<Town>();
				foreach (Town town6 in instance2.CommunityManager.Towns)
				{
					if (!town6.IgnoreForQuests)
					{
						list12.Add(town6);
					}
				}
				if (list12.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferRoadEntrance;
					break;
				}
				Town town4 = list12[deterministicRand.Next(list12.Count)];
				tile = town4.Tile + deterministicRand.RandomTile(new TerrainCoord(-Mathf.CeilToInt(town4.Radius), -Mathf.CeilToInt(town4.Radius)), new TerrainCoord(Mathf.CeilToInt(town4.Radius), Mathf.CeilToInt(town4.Radius)));
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferVisitedTowns:
			{
				List<Town> list9 = new List<Town>();
				foreach (Town town7 in instance2.CommunityManager.Towns)
				{
					if (town7.Visited)
					{
						list9.Add(town7);
					}
				}
				if (list9.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferTowns;
					break;
				}
				Town town2 = list9[deterministicRand.Next(list9.Count)];
				tile = town2.Tile + deterministicRand.RandomTile(new TerrainCoord(-Mathf.CeilToInt(town2.Radius), -Mathf.CeilToInt(town2.Radius)), new TerrainCoord(Mathf.CeilToInt(town2.Radius), Mathf.CeilToInt(town2.Radius)));
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferSourceTown:
			{
				Town town3 = obj as Town;
				if (town3 == null && obj != null)
				{
					town3 = instance2.CommunityManager.GetClosestTown(obj.GetTile());
				}
				if (town3 != null)
				{
					tile = town3.Tile + deterministicRand.RandomTile(new TerrainCoord(-Mathf.CeilToInt(town3.Radius), -Mathf.CeilToInt(town3.Radius)), new TerrainCoord(Mathf.CeilToInt(town3.Radius), Mathf.CeilToInt(town3.Radius)));
				}
				else
				{
					if (obj == null)
					{
						templateSpawnLocation = TemplateSpawnLocation.PreferVisitedTowns;
						break;
					}
					tile = obj.GetTile() + deterministicRand.RandomTile(new TerrainCoord(-32, -32), new TerrainCoord(32, 32));
				}
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferMarker:
			{
				BaseObject baseObject = BaseObjectManager.Instance.GetObjectByUniqueID(SpawnMarkerName);
				if (baseObject == null)
				{
					baseObject = obj;
				}
				if (baseObject == null)
				{
					templateSpawnLocation = TemplateSpawnLocation.Anywhere;
					break;
				}
				int num2 = num;
				tile = baseObject.GetTile() + deterministicRand.RandomTile(new TerrainCoord(-num2, -num2), new TerrainCoord(num2, num2));
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferCountrysideProps:
			{
				List<Building> list2 = new List<Building>();
				foreach (Prop allProp in instance2.PropManager.AllProps)
				{
					if (allProp.Town == null && allProp.Community == null && allProp is Building building && (building.Community == null || building.Community.CommunityType != CommunityType.Player) && !building.IgnoreForQuests && building.Category.StartsWith(Countryside))
					{
						list2.Add(building);
					}
				}
				if (list2.Count == 0)
				{
					foreach (Prop allProp2 in instance2.PropManager.AllProps)
					{
						if (allProp2.Town == null && allProp2.Community == null && allProp2 is Building building2 && (building2.Community == null || building2.Community.CommunityType != CommunityType.Player) && (!(building2 is EnterableVehicle) || !instance3.GetNearestPointOnPath(allProp2.PosXZ, river: false, 8f, out var _, out var _, out var _).HasValue))
						{
							list2.Add(building2);
						}
					}
				}
				List<Building> list3 = new List<Building>();
				foreach (Building item2 in list2)
				{
					if (!(instance2.CommunityManager.PlayerCommunity.GetDistSqToNearestBuilding(item2.Tile) <= MathUtil.Squared(48f)))
					{
						list3.Add(item2);
					}
				}
				if (list3.Count > 0)
				{
					list2 = list3;
				}
				if (list2.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferVehiclesOnRoads;
					break;
				}
				spawnedObj = (spawnInBuilding = list2[deterministicRand.Next(list2.Count)]);
				tile = spawnInBuilding.Tile;
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferVehiclesOnRoads:
			{
				List<EnterableVehicle> list4 = new List<EnterableVehicle>();
				foreach (Prop allProp3 in instance2.PropManager.AllProps)
				{
					if (allProp3.Town == null && allProp3 is EnterableVehicle enterableVehicle && (enterableVehicle.Community == null || enterableVehicle.Community.CommunityType != CommunityType.Player) && !enterableVehicle.IgnoreForQuests && instance3.GetNearestPointOnPath(allProp3.PosXZ, river: false, 8f, out var _, out var _, out var _).HasValue)
					{
						list4.Add(enterableVehicle);
					}
				}
				List<EnterableVehicle> list5 = new List<EnterableVehicle>();
				foreach (EnterableVehicle item3 in list4)
				{
					if (!(instance2.CommunityManager.PlayerCommunity.GetDistSqToNearestBuilding(item3.Tile) <= MathUtil.Squared(48f)))
					{
						list5.Add(item3);
					}
				}
				if (list5.Count > 0)
				{
					list4 = list5;
				}
				if (list4.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.Anywhere;
					break;
				}
				spawnedObj = (spawnInBuilding = list4[deterministicRand.Next(list4.Count)]);
				tile = spawnInBuilding.Tile;
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferUnownedVehicles:
			{
				List<EnterableVehicle> list10 = new List<EnterableVehicle>();
				foreach (Prop allProp4 in instance2.PropManager.AllProps)
				{
					if (allProp4.Community == null && allProp4.GetBaseObjectType() == BaseObjectType.EnterableVehicle)
					{
						EnterableVehicle enterableVehicle2 = (EnterableVehicle)allProp4;
						if (!enterableVehicle2.IgnoreForQuests)
						{
							list10.Add(enterableVehicle2);
						}
					}
				}
				List<EnterableVehicle> list11 = new List<EnterableVehicle>();
				foreach (EnterableVehicle item4 in list10)
				{
					if (!(instance2.CommunityManager.PlayerCommunity.GetDistSqToNearestBuilding(item4.Tile) <= MathUtil.Squared(128f)))
					{
						list11.Add(item4);
					}
				}
				if (list11.Count > 0)
				{
					list10 = list11;
				}
				if (list10.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.Anywhere;
					break;
				}
				spawnedObj = (spawnInBuilding = list10[deterministicRand.Next(list10.Count)]);
				tile = spawnInBuilding.Tile;
				canBeEnclosed = false;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.PreferSettlements:
			case TemplateSpawnLocation.PreferLooterSettlements:
			case TemplateSpawnLocation.PreferNonLooterSettlements:
			{
				List<Building> list8 = new List<Building>();
				foreach (Community community2 in instance2.CommunityManager.Communities)
				{
					if ((community2.CommunityType != CommunityType.Normal && community2.CommunityType != CommunityType.Looter) || (templateSpawnLocation == TemplateSpawnLocation.PreferLooterSettlements && community2.CommunityType != CommunityType.Looter) || (templateSpawnLocation == TemplateSpawnLocation.PreferNonLooterSettlements && community2.CommunityType != CommunityType.Normal))
					{
						continue;
					}
					foreach (Prop building8 in community2.Buildings)
					{
						if (building8 is Building { IgnoreForQuests: false } building5)
						{
							list8.Add(building5);
						}
					}
				}
				if (list8.Count == 0)
				{
					templateSpawnLocation = TemplateSpawnLocation.PreferCountrysideProps;
					break;
				}
				spawnedObj = (spawnInBuilding = list8[deterministicRand.Next(list8.Count)]);
				spawnInCommunity = spawnedObj.GetCommunity();
				tile = spawnInBuilding.Tile;
				canBeEnclosed = true;
				flag = true;
				break;
			}
			case TemplateSpawnLocation.AnySettler:
			case TemplateSpawnLocation.AnySettlerOrTrader:
			case TemplateSpawnLocation.PreferLooterSettlers:
			case TemplateSpawnLocation.PreferNonLooterSettlers:
			case TemplateSpawnLocation.PreferTraders:
			{
				List<Character> list = new List<Character>();
				foreach (Character character7 in instance2.CharacterManager.Characters)
				{
					if (character7.Community == null || !character7.AliveAndNotZombie || character7.GetBaseObjectType() != BaseObjectType.Human || character7.Disappeared)
					{
						continue;
					}
					switch (templateSpawnLocation)
					{
					case TemplateSpawnLocation.AnySettler:
						if (!character7.Community.IsAISettlement())
						{
							continue;
						}
						break;
					case TemplateSpawnLocation.AnySettlerOrTrader:
						if (!character7.Community.IsAISettlement() && character7.Community.CommunityType != CommunityType.RovingTrader)
						{
							continue;
						}
						break;
					case TemplateSpawnLocation.PreferLooterSettlers:
						if (character7.Community.CommunityType != CommunityType.Looter)
						{
							continue;
						}
						break;
					case TemplateSpawnLocation.PreferNonLooterSettlers:
						if (character7.Community.CommunityType != CommunityType.Normal)
						{
							continue;
						}
						break;
					case TemplateSpawnLocation.PreferTraders:
						if (character7.Community.CommunityType != CommunityType.RovingTrader)
						{
							continue;
						}
						break;
					}
					list.Add(character7);
				}
				if (list.Count == 0)
				{
					foreach (Character character8 in instance2.CharacterManager.Characters)
					{
						if (character8.Community == null || character8.Disappeared)
						{
							continue;
						}
						switch (templateSpawnLocation)
						{
						case TemplateSpawnLocation.AnySettler:
							if (!character8.Community.IsAISettlement())
							{
								continue;
							}
							break;
						case TemplateSpawnLocation.AnySettlerOrTrader:
							if (!character8.Community.IsAISettlement() && character8.Community.CommunityType != CommunityType.RovingTrader)
							{
								continue;
							}
							break;
						case TemplateSpawnLocation.PreferLooterSettlers:
							if (character8.Community.CommunityType != CommunityType.Looter)
							{
								continue;
							}
							break;
						case TemplateSpawnLocation.PreferNonLooterSettlers:
							if (character8.Community.CommunityType != CommunityType.Normal)
							{
								continue;
							}
							break;
						case TemplateSpawnLocation.PreferTraders:
							if (character8.Community.CommunityType != CommunityType.RovingTrader)
							{
								continue;
							}
							break;
						}
						list.Add(character8);
					}
				}
				if (list.Count == 0)
				{
					templateSpawnLocation = templateSpawnLocation switch
					{
						TemplateSpawnLocation.AnySettler => TemplateSpawnLocation.PreferSettlements, 
						TemplateSpawnLocation.AnySettlerOrTrader => TemplateSpawnLocation.PreferSettlements, 
						TemplateSpawnLocation.PreferLooterSettlers => TemplateSpawnLocation.PreferLooterSettlements, 
						TemplateSpawnLocation.PreferNonLooterSettlers => TemplateSpawnLocation.PreferNonLooterSettlements, 
						TemplateSpawnLocation.PreferTraders => TemplateSpawnLocation.PreferTownBuildings, 
						_ => TemplateSpawnLocation.PreferTownBuildings, 
					};
				}
				else
				{
					spawnedObj = list[deterministicRand.Next(list.Count)];
					spawnInCommunity = spawnedObj.GetCommunity();
					tile = spawnedObj.GetCentreTile();
					canBeEnclosed = true;
					flag = true;
				}
				break;
			}
			}
			if (flag && num < 100 && templateSpawnLocation != TemplateSpawnLocation.InheritFromParent && !IsValidLocation(tile, spawnedObj, ref cachedPlayerCommunityTiles))
			{
				templateSpawnLocation = SpawnLocation;
				flag = false;
				num++;
			}
		}
		BaseObject baseObject2 = null;
		switch (Type)
		{
		case TemplateType.Community:
			baseObject2 = (spawnInCommunity = Community.Spawn(CommunityType));
			spawnInCommunity.PlayerSurrenderDisabled = PlayerSurrenderDisabled;
			spawnInCommunity.IsFEMA = IsFEMA;
			if (!string.IsNullOrEmpty(NativeCommunityName))
			{
				spawnInCommunity.CommunityName.SetTranslatedString(GetCommunityNameKey());
			}
			else if (!spawnInCommunity.IsAmbientCommunity())
			{
				spawnInCommunity.CommunityName.Randomise(deterministicRand, unique: true, spawnInCommunity);
			}
			break;
		case TemplateType.Character:
		{
			bool flag2 = spawnInBuilding != null && SpawnState == TemplateSpawnState.Alive && Infection == InfectionType.None;
			TerrainCoord tile2 = tile;
			if (!flag2)
			{
				int num3 = 0;
				int num4 = 0;
				while (instance3.IsImpassable(tile2.x, tile2.y, 1, null, null) || (!canBeEnclosed && (instance3.IsTileEnclosed(tile2.x, tile2.y) || !instance3.IsTileSpawnable(tile2.x, tile2.y))))
				{
					tile2 += instance2.DeterministicRand.RandomTile(new TerrainCoord(-1, -1), new TerrainCoord(1, 1));
					tile2 = instance3.ClampTileWithinBounds(tile2);
					num3++;
					if (num3 >= 100)
					{
						num4++;
						if (num4 >= 10)
						{
							break;
						}
						tile2 = tile;
						num3 = 0;
					}
				}
			}
			GenderType genderType = ((!deterministicRand.RandomChoice(Mathf.Clamp01(GenderRatio / 100f))) ? GenderType.Female : GenderType.Male);
			if (GenderRatio != 0f && GenderRatio != 100f)
			{
				if (Relationship.IsRomanticRelationshipType(RelationshipWithActor) && actor != null && !actor.IsAttractedTo(genderType))
				{
					genderType = (GenderType)((int)(genderType + 1) % 2);
				}
				if (Relationship.IsRomanticRelationshipType(RelationshipWithTarget) && target != null && !target.IsAttractedTo(genderType))
				{
					genderType = (GenderType)((int)(genderType + 1) % 2);
				}
			}
			HumanAppearance humanAppearance = new HumanAppearance(genderType, HumanAppearance.PickRandomAge(deterministicRand));
			humanAppearance.Randomize(InfectionType.None, deterministicRand);
			Human human = Human.Spawn(tile2, instance2.DeterministicRand.RandomFloat() * (MathF.PI * 2f), humanAppearance, Infection);
			human.LastUpdateTime = (human.LastThinkTime = Session.Instance.PlayTime);
			human.LootLocation = LootLocation;
			if (Role != Role.None)
			{
				human.AddRole(new RoleInfo(Role));
			}
			human.TraderLevel = TraderLevel;
			if (spawnInCommunity == null)
			{
				spawnInCommunity = Community.Spawn((Infection != InfectionType.None) ? CommunityType.TemporaryZombie : CommunityType.Temporary);
				spawnInCommunity.CommunityName.Randomise(Session.Instance.DeterministicRand, unique: true, spawnInCommunity);
			}
			if (Infection == InfectionType.None || NamedZombie)
			{
				human.RandomizeName(deterministicRand);
			}
			spawnInCommunity.AddMember(human);
			human.InitialCommunity = spawnInCommunity;
			if (RelationshipWithActor != RelationshipType.None && actor != null)
			{
				Relationship.SetRelationship(human, RelationshipWithActor, actor, allowChange: false, generating: true, silent: true);
			}
			if (RelationshipWithTarget != RelationshipType.None && target != null)
			{
				Relationship.SetRelationship(human, RelationshipWithTarget, target, allowChange: false, generating: true, silent: true);
			}
			if (human.Infection != InfectionType.None)
			{
				human.Rotten = true;
				bool flag3 = false;
				switch (ZombieCrippled)
				{
				case TemplateCrippled.Yes:
					flag3 = true;
					break;
				case TemplateCrippled.UseDifficultySetting:
					flag3 = deterministicRand.RandomChoice(Session.Instance.DifficultySettings.ZombieCrippledPercentage / 100f);
					break;
				}
				if (flag3)
				{
					bool flag4 = deterministicRand.RandomChoice(0.5f);
					human.AddInjury(new Injury(human, InjuryType.SharpObject, absorbedByVest: false, flag4 ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg, InfectionType.None, flag4 ? Bone.RightLeg : Bone.LeftLeg, Vector3.zero, human.GetMaxLimbDamage(), TimeSpan.Zero, null, assailantUnknown: false, intentional: true));
				}
			}
			else
			{
				human.RandomizeInvisibleStrain(HasInvisibleStrain, deterministicRand);
			}
			if (!spawnInCommunity.IsAmbientCommunity())
			{
				human.SetHangoutLocation(tile);
			}
			if (spawnInCommunity.Members.Count == 1)
			{
				human.SetRank(Rank.Leader);
				if (spawnInCommunity.CommunityType == CommunityType.RovingTrader)
				{
					human.AddRole(new RoleInfo(Role.Trader));
				}
			}
			human.Skillset.Randomize(human, deterministicRand);
			bool flag5 = human.Inventory.GetWeight(human) >= human.GetMaxInventoryWeight();
			for (int j = 0; j < human.Roles.Count; j++)
			{
				switch (human.Roles[j].Role)
				{
				case Role.Farmer:
				case Role.Gatherer:
				case Role.Builder:
				case Role.Lumberjack:
				case Role.Cook:
				case Role.Trader:
				case Role.Trapper:
					flag5 = (byte)((flag5 ? 1u : 0u) | 1u) != 0;
					break;
				}
			}
			bool mustHaveBodyArmor = deterministicRand.RandomChoice(Mathf.Clamp01(BodyArmorProbability / 100f));
			bool mustHaveLegArmor = deterministicRand.RandomChoice(Mathf.Clamp01(LegArmorProbability / 100f));
			bool mustHaveHelmet = deterministicRand.RandomChoice(Mathf.Clamp01(HelmetProbability / 100f));
			human.RandomizeClothing(deterministicRand, SeasonallyAppropriateClothing, isPortrait: false, flag5, mustHaveBodyArmor, mustHaveHelmet, mustHaveLegArmor);
			human.RandomizePersonality(deterministicRand, PersonalityFaction);
			human.SetSleepDeprivation(deterministicRand.RandomFloat() * Character.SleepyTime);
			human.SetHunger(deterministicRand.RandomFloat() * Character.HungryTime);
			human.SetThirst(deterministicRand.RandomFloat() * Character.ThirstyTime);
			human.DontSimulateSurvivalFactorsUntilDiscovered = DontSimulateSurvivalFactorsUntilDiscovered;
			human.DontSimulateSurvivalFactorsUntilJoinCommunity = DontSimulateSurvivalFactorsUntilJoinCommunity;
			if (deterministicRand.RandomChoice(Mathf.Clamp01(MeleeWeaponProbability / 100f)))
			{
				EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomItemOfClass(human.GetLootLocation(), typeof(MeleeWeapon), deterministicRand, mustPickSomething: true);
				if (equipmentPrototype != null)
				{
					Equipment equipment = Equipment.Spawn(equipmentPrototype);
					if (equipmentPrototype.InjuryType == InjuryType.SharpObject && deterministicRand.RandomChoice(Mathf.Clamp01(InfectedWeaponsProbability / 100f)))
					{
						equipment.Infect((InfectionType)deterministicRand.Next(1, (int)(InfectedWeaponsMaxStrain + 1)));
					}
					human.Inventory.Add(human, equipment);
				}
			}
			if (deterministicRand.RandomChoice(Mathf.Clamp01(AmmoWeaponProbability / 100f)))
			{
				EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.PickRandomItemOfClass(human.GetLootLocation(), typeof(AmmoWeapon), deterministicRand, mustPickSomething: true);
				if (equipmentPrototype2 != null)
				{
					Equipment equipment2 = Equipment.Spawn(equipmentPrototype2);
					if (equipment2 is Bow && deterministicRand.RandomChoice(Mathf.Clamp01(InfectedWeaponsProbability / 100f)))
					{
						equipment2.Infect((InfectionType)deterministicRand.Next(1, (int)(InfectedWeaponsMaxStrain + 1)));
					}
					human.Inventory.Add(human, equipment2);
					if (equipmentPrototype2.AmmoPrototypes != null && equipmentPrototype2.AmmoPrototypes.Count > 0)
					{
						Equipment equipment3 = Equipment.Spawn(equipmentPrototype2.AmmoPrototypes[0], Math.Max(equipmentPrototype2.MaxAmmo, 1));
						equipment3.Infect(equipment2.InfectedWith);
						human.Inventory.Add(human, equipment3);
					}
				}
			}
			if (deterministicRand.RandomChoice(Mathf.Clamp01(MolotovProbability / 100f)))
			{
				EquipmentPrototype equipmentPrototype3 = GameImpl.Instance.PickRandomItemOfClass(human.GetLootLocation(), typeof(MolotovCocktail), deterministicRand, mustPickSomething: true);
				if (equipmentPrototype3 != null)
				{
					human.Inventory.Add(human, Equipment.Spawn(equipmentPrototype3, 1));
				}
			}
			if (deterministicRand.RandomChoice(Mathf.Clamp01(PipeBombProbability / 100f)))
			{
				EquipmentPrototype equipmentPrototype4 = GameImpl.Instance.PickRandomItemOfClass(human.GetLootLocation(), typeof(PipeBomb), deterministicRand, mustPickSomething: true);
				if (equipmentPrototype4 != null)
				{
					Equipment equipment4 = Equipment.Spawn(equipmentPrototype4, 1);
					if (deterministicRand.RandomChoice(Mathf.Clamp01(InfectedWeaponsProbability / 100f)))
					{
						equipment4.Infect((InfectionType)deterministicRand.Next(1, (int)(InfectedWeaponsMaxStrain + 1)));
					}
					human.Inventory.Add(human, equipment4);
				}
			}
			if (deterministicRand.RandomChoice(Mathf.Clamp01(WaterBottleProbability / 100f)) && EquipmentPrototype.PlasticBottle != null)
			{
				Equipment equipment5 = Equipment.Spawn(EquipmentPrototype.PlasticBottle, 1);
				if (equipment5 != null)
				{
					equipment5.FillLiquid(LiquidPrototype.Water, equipment5.GetLiquidCapacity(), InfectionType.None);
					human.Inventory.Add(human, equipment5);
				}
			}
			GameTerrain.GenerateLoot(human, Mathf.RoundToInt((float)GeneratedLootCount * (Session.Instance.DifficultySettings.LootDensity / 100f)), deterministicRand, includeClothing: false, fillLiquidContainers: true);
			if (flag2)
			{
				spawnInBuilding.OnCharacterEnter(human, wasOrderedInsideBuilding: false);
			}
			if (SpawnState == TemplateSpawnState.Alive)
			{
				if (Infection != InfectionType.None)
				{
					human.SetGoal(new ZombieGoal());
				}
				else
				{
					human.SetGoal(new SurvivorGoal());
				}
			}
			else
			{
				human.OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
				if (SpawnState == TemplateSpawnState.Buried && PropPrototype.Grave != null)
				{
					spawnedObj = Recipe.PlaceProp(PropPrototype.Grave, null, spawnInCommunity, tile, instance2.DeterministicRand.RandomOrientationType(), null);
					if (spawnedObj is Grave grave)
					{
						grave.Bury(human);
					}
				}
			}
			baseObject2 = (spawnedObj = human);
			break;
		}
		case TemplateType.Equipment:
		{
			if (spawnedObj == null)
			{
				break;
			}
			EquipmentPrototype equipmentPrototype5 = instance.FindEquipmentPrototypeByName(EquipmentPrototypeName);
			if (equipmentPrototype5 == null)
			{
				break;
			}
			numToSpawn *= Amount;
			Equipment item = null;
			if (AllowExceedWeightLimit)
			{
				if (equipmentPrototype5.CanBeCombined)
				{
					item = Equipment.Spawn(equipmentPrototype5, numToSpawn);
					baseObject2 = spawnedObj.GetInventory().Add(spawnedObj, item);
				}
				else
				{
					for (int k = 0; k < numToSpawn; k++)
					{
						item = Equipment.Spawn(equipmentPrototype5);
						baseObject2 = spawnedObj.GetInventory().Add(spawnedObj, item);
					}
				}
			}
			else if (spawnedObj.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype5, numToSpawn, fillLiquidContainers: true, out item) > 0)
			{
				baseObject2 = item;
			}
			if (item == null)
			{
				break;
			}
			if (item.GetLiquidCapacity() > 0f && equipmentPrototype5.DefaultLiquidPrototype != null)
			{
				item.FillLiquid(equipmentPrototype5.DefaultLiquidPrototype, item.GetLiquidCapacity(), InfectionType.None);
			}
			if (Infection != InfectionType.None)
			{
				item.Infect(Infection);
			}
			item.Concealed = Concealed;
			item.ColorVariation = ((ColorVariation == -1) ? deterministicRand.Next(item.GetPrototype().GetNumColorVariations()) : ColorVariation);
			item.ColorVariation2 = ((ColorVariation2 == -1) ? deterministicRand.Next(item.GetPrototype().GetNumColorVariations2()) : ColorVariation2);
			item.ColorVariation3 = ((ColorVariation3 == -1) ? deterministicRand.Next(item.GetPrototype().GetNumColorVariations3()) : ColorVariation3);
			item.MaterialVariation = ((MaterialVariation == -1) ? deterministicRand.Next(item.GetPrototype().GetNumMaterialVariations()) : MaterialVariation);
			Human human2 = spawnedObj as Human;
			if (WearMe && human2 != null && item.GetClothingType() != ClothingType.Invalid && !human2.IsWearing(item))
			{
				Equipment equipment6 = human2.Clothes[(int)item.GetClothingType()];
				if (equipment6 != null)
				{
					human2.Inventory.Remove(human2, equipment6);
					equipment6.Delete();
				}
				item.Wear(human2);
			}
			break;
		}
		case TemplateType.Prop:
		{
			PropPrototype propPrototype = instance.FindPropPrototypeByName(PropPrototypeName);
			if (propPrototype != null)
			{
				baseObject2 = (spawnedObj = Recipe.PlaceProp(propPrototype, null, spawnInCommunity, tile, instance2.DeterministicRand.RandomOrientationType(), null));
			}
			else
			{
				Debug.LogWarning("Prop prototype not found: " + PropPrototypeName);
			}
			break;
		}
		}
		if (Children != null)
		{
			for (int l = 0; l < Children.Count; l++)
			{
				Template template = instance.FindTemplateByUniqueID(Children[l].UniqueID);
				if (template == null)
				{
					continue;
				}
				float num5 = Children[l].Min;
				float num6 = Children[l].Max;
				if (Children[l].MinFormula.GetConditionBlock() != null)
				{
					num5 += Children[l].MinFormula.GetConditionBlock().Evaluate(actor, target, obj, param);
				}
				if (Children[l].MaxFormula.GetConditionBlock() != null)
				{
					num6 += Children[l].MaxFormula.GetConditionBlock().Evaluate(actor, target, obj, param);
				}
				int num7 = Mathf.RoundToInt(Mathf.Lerp(num5 - 0.4999f, num6 + 0.4999f, instance2.DeterministicRand.RandomFloat()));
				if (template.Type == TemplateType.Equipment)
				{
					template.SpawnFromTemplate(tile, canBeEnclosed, spawnInBuilding, spawnInCommunity, spawnedObj, actor, target, baseObject2, param, num7, invaderInstance, ref cachedPlayerCommunityTiles);
					continue;
				}
				for (int m = 0; m < num7; m++)
				{
					int num8 = Mathf.CeilToInt((float)Math.Sqrt(m)) * 4;
					template.SpawnFromTemplate(tile + instance2.DeterministicRand.RandomTile(new TerrainCoord(-num8, -num8), new TerrainCoord(num8, num8)), canBeEnclosed, spawnInBuilding, spawnInCommunity, spawnedObj, actor, target, baseObject2, param, 1, invaderInstance, ref cachedPlayerCommunityTiles);
				}
			}
		}
		switch (Type)
		{
		case TemplateType.Community:
		{
			if (spawnInCommunity.Members.Count <= 0)
			{
				break;
			}
			spawnInCommunity.InitialMemberCount = spawnInCommunity.Members.Count;
			spawnInCommunity.InitialChickenCount = spawnInCommunity.GetLivingNonZombieMemberCountBySpecies(BaseObjectType.Chicken);
			if (GeneratedRelationshipsProbability > 0f)
			{
				int desiredRelationships = Mathf.RoundToInt(Mathf.Lerp(0.5f, 1f, deterministicRand.RandomFloat()) * GeneratedRelationshipsProbability * (float)spawnInCommunity.Members.Count);
				spawnInCommunity.RandomizeRelationships(deterministicRand, desiredRelationships);
			}
			if (!spawnInCommunity.IsHunterCommunity())
			{
				break;
			}
			Community community = null;
			if ((spawnInCommunity.CommunityType == CommunityType.HunterLooter || spawnInCommunity.CommunityType == CommunityType.RovingRefugee) && CanOccupyEmptyBases)
			{
				TempDeadCommunitiesToOccupy.Clear();
				foreach (Community community3 in instance2.CommunityManager.Communities)
				{
					if (community3.CanBeOccupiedBy(spawnInCommunity))
					{
						List<TileObject> playerObjectsInBaseRect = community3.GetPlayerObjectsInBaseRect();
						if (playerObjectsInBaseRect.Count > 0)
						{
							playerObjectsInBaseRect.Clear();
						}
						else
						{
							TempDeadCommunitiesToOccupy.Add(community3);
						}
					}
				}
				if (TempDeadCommunitiesToOccupy.Count > 0 && deterministicRand.RandomChoice(0.5f))
				{
					community = TempDeadCommunitiesToOccupy[deterministicRand.Next(TempDeadCommunitiesToOccupy.Count)];
				}
			}
			bool flag6 = false;
			if (community == null && (spawnInCommunity.CommunityType == CommunityType.HunterLooter || spawnInCommunity.CommunityType == CommunityType.HunterMercenary || spawnInCommunity.CommunityType == CommunityType.HunterZombie))
			{
				flag6 |= deterministicRand.RandomChoice(Mathf.Clamp01(PlayerTrackingProbability / 100f));
			}
			if (community != null)
			{
				Squad squad = spawnInCommunity.AddSquad(SquadBehaviour.Occupy, community.Id);
				foreach (Character member in spawnInCommunity.Members)
				{
					spawnInCommunity.AddToSquad(member, squad);
				}
				if (spawnInCommunity.CountInventoryItemsOfClass(typeof(MolotovCocktail), includeBuildings: true) == 0)
				{
					Character character2 = spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)];
					EquipmentPrototype equipmentPrototype6 = GameImpl.Instance.PickRandomItemOfClass(LootLocationDef.Survivor, typeof(MolotovCocktail), instance2.DeterministicRand, mustPickSomething: true);
					if (equipmentPrototype6 != null)
					{
						character2.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype6, 1, fillLiquidContainers: false);
					}
				}
				if (spawnInCommunity.CountInventoryItemsOfClass(typeof(PipeBomb), includeBuildings: true) == 0)
				{
					Character character3 = spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)];
					EquipmentPrototype equipmentPrototype7 = GameImpl.Instance.PickRandomItemOfClass(LootLocationDef.Survivor, typeof(PipeBomb), instance2.DeterministicRand, mustPickSomething: true);
					if (equipmentPrototype7 != null)
					{
						character3.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype7, 1, fillLiquidContainers: false);
					}
				}
				if (spawnInCommunity.CountInventoryItemsOfClass(typeof(Axe), includeBuildings: true) == 0 && community.CountInventoryItemsOfClass(typeof(Axe), includeBuildings: true) == 0)
				{
					Character character4 = spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)];
					EquipmentPrototype equipmentPrototype8 = GameImpl.Instance.PickRandomItemOfClass(LootLocationDef.Survivor, typeof(Axe), instance2.DeterministicRand, mustPickSomething: true);
					if (equipmentPrototype8 != null)
					{
						character4.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype8, 1, fillLiquidContainers: false);
					}
				}
				if (spawnInCommunity.CountInventoryItemsOfClass(typeof(Toolbox), includeBuildings: true) == 0 && community.CountInventoryItemsOfClass(typeof(Toolbox), includeBuildings: true) == 0)
				{
					Character character5 = spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)];
					EquipmentPrototype equipmentPrototype9 = GameImpl.Instance.PickRandomItemOfClass(LootLocationDef.Survivor, typeof(Toolbox), instance2.DeterministicRand, mustPickSomething: true);
					if (equipmentPrototype9 != null)
					{
						character5.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype9, 1, fillLiquidContainers: false);
					}
				}
				for (int n = spawnInCommunity.CountInventoryItemsOfClothingType(ClothingType.Backpack) + community.CountInventoryItemsOfClothingType(ClothingType.Backpack); n < 2; n++)
				{
					Character character6 = spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)];
					EquipmentPrototype equipmentPrototype10 = GameImpl.Instance.PickRandomClothing(LootLocationDef.Survivor, ClothingType.Backpack, mustPickSomething: true, instance2.DeterministicRand);
					if (equipmentPrototype10 != null)
					{
						character6.SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype10, 1, fillLiquidContainers: false);
					}
				}
				if (spawnInCommunity.CountInventoryItemsOfType(EquipmentPrototype.Pot) == 0 && community.CountInventoryItemsOfType(EquipmentPrototype.Pot) == 0)
				{
					spawnInCommunity.Members[deterministicRand.Next(spawnInCommunity.Members.Count)].SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.Pot, 1, fillLiquidContainers: false);
				}
				squad.GoalTile = community.PickRandomTileInsidePerimeter(deterministicRand);
				spawnInCommunity.SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null, isInvader);
				break;
			}
			if (flag6)
			{
				Squad squad2 = spawnInCommunity.AddSquad(SquadBehaviour.Hunt, Session.Instance.CommunityManager.PlayerCommunity.Id);
				foreach (Character member2 in spawnInCommunity.Members)
				{
					spawnInCommunity.AddToSquad(member2, squad2);
				}
				spawnInCommunity.StartPillaging(squad2, canAttackOutsideBase: true, isInvader);
				break;
			}
			bool flag7 = ((StayOnMapProbability == -1f) ? (spawnInCommunity.CommunityType == CommunityType.RovingTrader) : deterministicRand.RandomChoice(Mathf.Clamp01(StayOnMapProbability / 100f)));
			Squad squad3 = spawnInCommunity.AddSquad(flag7 ? SquadBehaviour.Trade : SquadBehaviour.Travel, 0);
			foreach (Character member3 in spawnInCommunity.Members)
			{
				spawnInCommunity.AddToSquad(member3, squad3);
			}
			if (squad3.Behaviour == SquadBehaviour.Trade)
			{
				spawnInCommunity.GoToNextTradeDestination(squad3, deterministicRand, mustMove: false, canOccupyBases: false, isInvader);
				break;
			}
			TerrainCoord terrainCoord = TerrainCoord.Invalid;
			for (int num9 = 0; num9 < 100; num9++)
			{
				ControlPoint? controlPoint2 = CommunityManager.PickRandomMapEntrance(deterministicRand);
				if (!controlPoint2.HasValue)
				{
					break;
				}
				terrainCoord = instance3.GetTileCoordForPosXZ(controlPoint2.Value.Pos);
				terrainCoord = instance3.ClampTileWithinBounds(terrainCoord);
				if (terrainCoord.GetDist(tile) > 64f)
				{
					break;
				}
			}
			if (terrainCoord == TerrainCoord.Invalid)
			{
				for (int num10 = 0; num10 < 100; num10++)
				{
					terrainCoord = deterministicRand.RandomTile(new TerrainCoord(32, 32), new TerrainCoord(instance3.Size - 32, instance3.Size - 32));
					if (!instance3.IsImpassable(terrainCoord.x, terrainCoord.y, 0, null, null))
					{
						break;
					}
				}
			}
			squad3.GoalTile = terrainCoord;
			spawnInCommunity.SetSquadAction(squad3, SquadAction.GoTo, 0, squad3.GoalTile, null, isInvader);
			break;
		}
		case TemplateType.Character:
			if (spawnedObj is Character character)
			{
				character.InitialBandageCount = character.Inventory.CountItemsOfType(EquipmentPrototype.Bandage);
			}
			break;
		}
		StoryManager.QueueEvents(Events, actor, target, baseObject2, param);
		return baseObject2;
	}
}
