using UnityEngine;

public struct AStarChange
{
	public enum Type
	{
		Request,
		AddedFixedContents,
		RemovedFixedContents,
		AddCharacter,
		RemoveCharacter,
		SetCommunityRelationship,
		SetCommunityAllDeadOrUnconscious,
		SetCommunitySurrendering,
		FlattenTerrain,
		AddDitch,
		WarnAboutTraps,
		SetLookupSquareOwnerCommunityId,
		TerrainHeightChange,
		WarnAboutTripwires
	}

	public Type ChangeType;

	public AStarRequester Requester;

	public TileObject Object;

	public int CommunityId;

	public int OccupiedByCommunityId;

	public int BuildingId;

	public TerrainCoord MinTile;

	public TerrainCoord MaxTile;

	public TerrainRect Bounds;

	public float[,] TerrrainPatch;

	public float FlattenHeight;

	public float FlattenBorder;

	public float DitchHeight;

	public TerrainModificationType TerrainModificationType;

	public Matrix4x4 PropWorldMatrix;

	public Bounds PropBoundingBox;

	public PrefabResource PropModel;

	public GateState GateState;

	public GatePolicy GatePolicy;

	public bool GateBlockAnimals;

	public bool CharacterIsStationary;

	public bool CharacterIsAwake;

	public OldCommunityRelationship Relationship;

	public bool Val;

	public byte ModelIndex;

	public byte FixedObstaclePropProtoIndex;

	public static AStarChange Request(AStarRequester requester)
	{
		return new AStarChange
		{
			ChangeType = Type.Request,
			Requester = requester
		};
	}

	public static AStarChange AddedFixedContents(TileObject obj)
	{
		AStarChange result = new AStarChange
		{
			ChangeType = Type.AddedFixedContents,
			Object = obj,
			MinTile = obj.GetMinTile(),
			MaxTile = obj.GetMaxTile(),
			FixedObstaclePropProtoIndex = 0
		};
		PropPrototype propPrototype = obj.GetPropPrototype();
		if (propPrototype != null)
		{
			int num = GameTerrain.Instance.AStar.FixedPropPrototypes.IndexOf(propPrototype);
			if (num == -1)
			{
				if (GameTerrain.Instance.AStar.FixedPropPrototypes.Count < 255)
				{
					num = (byte)GameTerrain.Instance.AStar.FixedPropPrototypes.Count;
					GameTerrain.Instance.AStar.FixedPropPrototypes.Add(propPrototype);
				}
				else
				{
					Debug.LogWarning("oh noes, too many prop prototypes");
				}
			}
			result.FixedObstaclePropProtoIndex = (byte)(num + 1);
		}
		Community community = obj.GetCommunity();
		if (community != null)
		{
			result.CommunityId = community.Id;
		}
		if (obj is Building { Inhabitants: not null } building)
		{
			for (int i = 0; i < building.Inhabitants.Length; i++)
			{
				if (building.Inhabitants[i] != null && building.GetInhabitantSlotDefs()[i].External)
				{
					result.OccupiedByCommunityId = building.Inhabitants[i].GetCommunityId();
					break;
				}
			}
		}
		if (obj is Prop prop)
		{
			result.PropWorldMatrix = prop.GetCustomModelTransform();
			result.PropBoundingBox = prop.GetBoundingBox();
			result.PropModel = prop.GetUnityModel();
			result.ModelIndex = (byte)prop.Variation;
		}
		if (obj is SingleTileProp singleTileProp)
		{
			result.PropWorldMatrix = singleTileProp.GetCustomModelTransform();
			result.PropBoundingBox = singleTileProp.GetBoundingBox();
			result.PropModel = singleTileProp.GetUnityModel();
		}
		if (obj is Gate gate)
		{
			result.GateState = gate.GateState;
			result.GatePolicy = gate.GatePolicy;
			result.GateBlockAnimals = gate.BlockAnimals;
		}
		if (obj is BaseFence baseFence)
		{
			result.ModelIndex = baseFence.ModelIndex;
		}
		if (obj is PitTrap pitTrap)
		{
			result.ModelIndex = (byte)(pitTrap.IsCovered ? 1u : 0u);
		}
		return result;
	}

	public static AStarChange RemovedFixedContents(TileObject obj)
	{
		return new AStarChange
		{
			ChangeType = Type.RemovedFixedContents,
			Object = obj,
			MinTile = obj.GetMinTile(),
			MaxTile = obj.GetMaxTile()
		};
	}

	public static AStarChange RemovedFixedContents(TileObject obj, TerrainRect rect)
	{
		return new AStarChange
		{
			ChangeType = Type.RemovedFixedContents,
			Object = obj,
			MinTile = rect.min,
			MaxTile = rect.max
		};
	}

	public static AStarChange AddCharacter(Character character, bool isAwake, bool isStationary)
	{
		return new AStarChange
		{
			ChangeType = Type.AddCharacter,
			Object = character,
			CharacterIsStationary = isStationary,
			CharacterIsAwake = isAwake,
			MinTile = character.Tile,
			MaxTile = character.Tile
		};
	}

	public static AStarChange RemoveCharacter(Character character)
	{
		return new AStarChange
		{
			ChangeType = Type.RemoveCharacter,
			Object = character
		};
	}

	public static AStarChange SetCommunityRelationship(OldCommunityRelationship relationship)
	{
		return new AStarChange
		{
			ChangeType = Type.SetCommunityRelationship,
			Relationship = relationship
		};
	}

	public static AStarChange SetCommunityAllDeadOrUnconscious(int communityId, bool val)
	{
		return new AStarChange
		{
			ChangeType = Type.SetCommunityAllDeadOrUnconscious,
			CommunityId = communityId,
			Val = val
		};
	}

	public static AStarChange SetCommunitySurrendering(int communityId, bool val)
	{
		return new AStarChange
		{
			ChangeType = Type.SetCommunitySurrendering,
			CommunityId = communityId,
			Val = val
		};
	}

	public static AStarChange FlattenTerrain(TerrainRect bounds, TerrainCoord minTile, TerrainCoord maxTile, float flattenHeight, float flattenBorder, int buildingId, TerrainModificationType terrainModificationType)
	{
		return new AStarChange
		{
			ChangeType = Type.FlattenTerrain,
			TerrainModificationType = terrainModificationType,
			Bounds = bounds,
			BuildingId = buildingId,
			MinTile = minTile,
			MaxTile = maxTile,
			FlattenHeight = flattenHeight,
			FlattenBorder = flattenBorder
		};
	}

	public static AStarChange AddDitch(TerrainRect bounds, TerrainCoord minTile, TerrainCoord maxTile, float ditchHeight, TerrainModificationType terrainModificationType)
	{
		return new AStarChange
		{
			ChangeType = Type.AddDitch,
			TerrainModificationType = terrainModificationType,
			Bounds = bounds,
			MinTile = minTile,
			MaxTile = maxTile,
			DitchHeight = ditchHeight
		};
	}

	public static AStarChange TerrainHeightChange(TerrainCoord minTile, TerrainCoord maxTile, float[,] patch)
	{
		return new AStarChange
		{
			ChangeType = Type.TerrainHeightChange,
			MinTile = minTile,
			MaxTile = maxTile,
			TerrrainPatch = patch
		};
	}

	public static AStarChange WarnAboutTraps(int communityId)
	{
		return new AStarChange
		{
			ChangeType = Type.WarnAboutTraps,
			CommunityId = communityId
		};
	}

	public static AStarChange WarnAboutTripwires(int communityId)
	{
		return new AStarChange
		{
			ChangeType = Type.WarnAboutTripwires,
			CommunityId = communityId
		};
	}

	public static AStarChange SetLookupSquareOwnerCommunityId(TerrainCoord lookupSquare, int communityId)
	{
		AStarChange result = default(AStarChange);
		result.ChangeType = Type.SetLookupSquareOwnerCommunityId;
		result.OccupiedByCommunityId = (result.CommunityId = communityId);
		result.MinTile = (result.MaxTile = lookupSquare);
		return result;
	}

	public void DebugPrint(int seq)
	{
		switch (ChangeType)
		{
		case Type.Request:
		{
			string text = "";
			foreach (EquipmentPrototype requesterKey in Requester.RequesterKeys)
			{
				text = text + requesterKey.Name + " ";
			}
			string[] array = new string[38];
			array[0] = "(";
			array[1] = seq.ToString();
			array[2] = ") Request: ";
			array[3] = Requester.ReachedGoalCondition.ToString();
			array[4] = " ";
			TerrainCoord minTile = Requester.Start;
			array[5] = minTile.ToString();
			array[6] = " ";
			minTile = Requester.Goal;
			array[7] = minTile.ToString();
			array[8] = " ";
			Vector3 goalPos = Requester.GoalPos;
			array[9] = goalPos.ToString();
			array[10] = " ";
			array[11] = Requester.MinRange.ToString();
			array[12] = " ";
			array[13] = Requester.MaxRange.ToString();
			array[14] = " ";
			minTile = Requester.BoundsMin;
			array[15] = minTile.ToString();
			array[16] = " ";
			minTile = Requester.BoundsMax;
			array[17] = minTile.ToString();
			array[18] = " ";
			array[19] = Requester.DontOpenOurGates.ToString();
			array[20] = " ";
			array[21] = Requester.IgnoreExplodableDefences.ToString();
			array[22] = " ";
			array[23] = Requester.IgnoreFlammableDefences.ToString();
			array[24] = " ";
			array[25] = Requester.Requester.Id.ToString();
			array[26] = " ";
			array[27] = Requester.Requester.GetDisplayNameString();
			array[28] = " ";
			array[29] = ((Requester.IgnoreObject != null) ? (Requester.IgnoreObject.Id + " " + Requester.IgnoreObject.GetDisplayNameString()) : null);
			array[30] = " ";
			array[31] = Requester.StartInputFrame.ToString();
			array[32] = " ";
			array[33] = Requester.RequesterCommunity?.ToString();
			array[34] = " ";
			array[35] = Requester.RequesterInfectionType.ToString();
			array[36] = " ";
			array[37] = text;
			Debug.Log(string.Concat(array));
			break;
		}
		case Type.AddedFixedContents:
		{
			string[] obj6 = new string[14]
			{
				"(",
				seq.ToString(),
				") AddedFixedContents: ",
				Object.Id.ToString(),
				" ",
				Object.GetDisplayNameString(),
				" (",
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			TerrainCoord minTile = MinTile;
			obj6[7] = minTile.ToString();
			obj6[8] = ") (";
			minTile = MaxTile;
			obj6[9] = minTile.ToString();
			obj6[10] = ")";
			obj6[11] = CommunityId.ToString();
			obj6[12] = " ";
			obj6[13] = GateState.ToString();
			Debug.Log(string.Concat(obj6));
			break;
		}
		case Type.RemovedFixedContents:
		{
			string[] obj5 = new string[11]
			{
				"(",
				seq.ToString(),
				") RemovedFixedContents: ",
				Object.Id.ToString(),
				" ",
				Object.GetDisplayNameString(),
				" (",
				null,
				null,
				null,
				null
			};
			TerrainCoord minTile = MinTile;
			obj5[7] = minTile.ToString();
			obj5[8] = ") (";
			minTile = MaxTile;
			obj5[9] = minTile.ToString();
			obj5[10] = ")";
			Debug.Log(string.Concat(obj5));
			break;
		}
		case Type.AddCharacter:
		{
			string[] obj4 = new string[13]
			{
				"(",
				seq.ToString(),
				") AddCharacter: ",
				Object.Id.ToString(),
				" ",
				Object.GetDisplayNameString(),
				CharacterIsStationary ? " Stationary" : "",
				CharacterIsAwake ? " Awake" : "",
				" (",
				null,
				null,
				null,
				null
			};
			TerrainCoord minTile = MinTile;
			obj4[9] = minTile.ToString();
			obj4[10] = ") (";
			minTile = MaxTile;
			obj4[11] = minTile.ToString();
			obj4[12] = ")";
			Debug.Log(string.Concat(obj4));
			break;
		}
		case Type.RemoveCharacter:
			Debug.Log("(" + seq + ") RemoveCharacter: " + Object.Id + " " + Object.GetDisplayNameString());
			break;
		case Type.SetCommunityRelationship:
			Debug.Log("(" + seq + ") SetCommunityRelationship: " + Relationship.Community1Id + " <-> " + Relationship.Community2Id + ": " + Relationship.RelationshipType);
			break;
		case Type.SetCommunityAllDeadOrUnconscious:
			Debug.Log("(" + seq + ") SetCommunityAllDeadOrUnconscious: " + CommunityId + " " + (Val ? "True" : "False"));
			break;
		case Type.SetCommunitySurrendering:
			Debug.Log("(" + seq + ") SetCommunitySurrendering: " + CommunityId + " " + (Val ? "True" : "False"));
			break;
		case Type.FlattenTerrain:
		{
			string[] obj3 = new string[14]
			{
				"(",
				seq.ToString(),
				") FlattenTerrain: ",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			TerrainRect bounds = Bounds;
			obj3[3] = bounds.ToString();
			obj3[4] = " (";
			TerrainCoord minTile = MinTile;
			obj3[5] = minTile.ToString();
			obj3[6] = ") (";
			minTile = MaxTile;
			obj3[7] = minTile.ToString();
			obj3[8] = "), ";
			obj3[9] = FlattenHeight.ToString();
			obj3[10] = ", ";
			obj3[11] = FlattenBorder.ToString();
			obj3[12] = ", ";
			obj3[13] = BuildingId.ToString();
			Debug.Log(string.Concat(obj3));
			break;
		}
		case Type.AddDitch:
		{
			string[] obj2 = new string[10]
			{
				"(",
				seq.ToString(),
				") AddDitch: ",
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			TerrainRect bounds = Bounds;
			obj2[3] = bounds.ToString();
			obj2[4] = " (";
			TerrainCoord minTile = MinTile;
			obj2[5] = minTile.ToString();
			obj2[6] = ") (";
			minTile = MaxTile;
			obj2[7] = minTile.ToString();
			obj2[8] = "), ";
			obj2[9] = DitchHeight.ToString();
			Debug.Log(string.Concat(obj2));
			break;
		}
		case Type.TerrainHeightChange:
		{
			string[] obj = new string[7]
			{
				"(",
				seq.ToString(),
				") TerrainHeightChange: (",
				null,
				null,
				null,
				null
			};
			TerrainCoord minTile = MinTile;
			obj[3] = minTile.ToString();
			obj[4] = ") (";
			minTile = MaxTile;
			obj[5] = minTile.ToString();
			obj[6] = ")";
			Debug.Log(string.Concat(obj));
			break;
		}
		case Type.WarnAboutTraps:
		case Type.SetLookupSquareOwnerCommunityId:
			break;
		}
	}
}
