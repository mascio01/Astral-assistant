using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Reflector
{
	public const int SerialiseVersion = 629;

	public int Version = 629;

	public bool IsDoingSavedCharacter;

	public bool IsDoingNetworkChecksum;

	public bool IsDoingPrediction;

	public bool IsFastPath;

	public string AltTerrainFolder;

	public abstract bool IsDeserialising { get; }

	public bool IsSerialising => !IsDeserialising;

	public virtual bool IsTextDumping => false;

	public virtual void AddTextDumpLine(string text)
	{
	}

	public void AddAfter(ref bool value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref byte value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref short value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref float value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref int value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref uint value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref long value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref ulong value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref string value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref TimeSpan value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref TerrainCoord value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Vector3 value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Quaternion value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Color32 value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref BaseObjectType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref BaseObject value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Speech value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref PlayerID value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref EquipmentPrototype value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref LiquidPrototype value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Recipe value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref PropPrototype value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref TreeType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref FindResult value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref Specifier value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref SparringType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref GatePolicy value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref GameFinishedState value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref InfectionType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref InvisibleStrainType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref DeletionState value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref StayInRangeOf value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref MineralType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref MovementType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref CauseOfDeath value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref DifficultyMode value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref EquipmentPolicyType_DEPRECATED value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref AnimalPose value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref CanOpenGates value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref ChickenColor value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref SecrecyMode value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref SnowmanState value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref HelicopterAnimState value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref GearState value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref SurrenderMode value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref TargettableBodyLocation value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref SpecifierModifier value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref StringStatus value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter(ref RoadDestinationType value, int version)
	{
		if (Version >= version)
		{
			Add(ref value);
		}
	}

	public void AddAfter<T>(ref T obj, int version) where T : BaseObject
	{
		if (Version >= version)
		{
			Add(ref obj);
		}
	}

	public void AddAfter<T>(ref List<T> list, int version) where T : IReflectable, new()
	{
		if (Version >= version)
		{
			Add(ref list);
		}
	}

	public abstract void Add(ref bool value);

	public abstract void Add(ref byte value);

	public abstract void Add(ref sbyte value);

	public abstract void Add(ref char value);

	public abstract void Add(ref short value);

	public abstract void Add(ref int value);

	public abstract void Add(ref uint value);

	public abstract void Add(ref long value);

	public abstract void Add(ref ulong value);

	public abstract void Add(ref float value);

	public abstract void Add(ref double value);

	public abstract void Add(ref Color32 value);

	public abstract void Add(ref TimeSpan value);

	public abstract void Add(ref TerrainCoord value);

	public abstract void Add(ref Vector2 value);

	public abstract void Add(ref Vector3 value);

	public abstract void Add(ref Vector4 value);

	public abstract void Add(ref Half3 value);

	public abstract void Add(ref Quaternion value);

	public abstract void Add(ref Color value);

	public abstract void Add(ref Matrix4x4 value);

	public abstract void Add(ref string value);

	public abstract void Add(ref BaseObject value);

	public abstract void Add(ref PlayerID value);

	public abstract void Add(ref EquipmentPrototype value);

	public abstract void Add(ref LiquidPrototype value);

	public abstract void Add(ref MemoryPrototype value);

	public abstract void Add(ref PropPrototype value);

	public abstract void Add(ref Recipe value);

	public abstract void Add(ref Trigger value);

	public abstract void Add(ref Speech value);

	public abstract void Add(ref Quest value);

	public abstract void Add(ref Invader value);

	public abstract void Add(Array src, int size, int dimensions);

	public abstract void Add(ref byte[] bytes, ref int size);

	public virtual void Add(ref BaseObjectType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (BaseObjectType)value2;
	}

	public virtual void Add(ref GoalType value)
	{
		short value2 = (short)value;
		Add(ref value2);
		value = (GoalType)value2;
	}

	public virtual void Add(ref TerrainTex value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (TerrainTex)value2;
	}

	public virtual void Add(ref InputActionType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (InputActionType)value2;
	}

	public virtual void Add(ref CommunityType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CommunityType)value2;
	}

	public virtual void Add(ref CommunityRelationshipType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CommunityRelationshipType)value2;
	}

	public virtual void Add(ref PlayerMode value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (PlayerMode)value2;
	}

	public virtual void Add(ref PlaySpeed value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (PlaySpeed)value2;
	}

	public virtual void Add(ref TargettableBodyLocation value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (TargettableBodyLocation)value2;
	}

	public virtual void Add(ref MovementType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (MovementType)value2;
	}

	public virtual void Add(ref MovementMode value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (MovementMode)value2;
	}

	public virtual void Add(ref Bone value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (Bone)value2;
	}

	public virtual void Add(ref InjuryType value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (InjuryType)value2;
	}

	public virtual void Add(ref InjuryLocation value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (InjuryLocation)value2;
	}

	public virtual void Add(ref InfectionType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (InfectionType)value2;
	}

	public virtual void Add(ref GenderType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GenderType)value2;
	}

	public virtual void Add(ref BodyType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (BodyType)value2;
	}

	public virtual void Add(ref FaceType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FaceType)value2;
	}

	public virtual void Add(ref HairType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (HairType)value2;
	}

	public virtual void Add(ref FacialHairType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FacialHairType)value2;
	}

	public virtual void Add(ref Role value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Role)value2;
	}

	public virtual void Add(ref Rank value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Rank)value2;
	}

	public virtual void Add(ref Prop.OrientationType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Prop.OrientationType)value2;
	}

	public virtual void Add(ref GateState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GateState)value2;
	}

	public virtual void Add(ref GatePolicy value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GatePolicy)value2;
	}

	public virtual void Add(ref SquadAction value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SquadAction)value2;
	}

	public virtual void Add(ref SquadBehaviour value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SquadBehaviour)value2;
	}

	public virtual void Add(ref P2PMsgType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (P2PMsgType)value2;
	}

	public virtual void Add(ref FollowCommand value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FollowCommand)value2;
	}

	public virtual void Add(ref AStarPriority value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (AStarPriority)value2;
	}

	public virtual void Add(ref ThinkPriority value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (ThinkPriority)value2;
	}

	public virtual void Add(ref ActionAnim value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (ActionAnim)value2;
	}

	public virtual void Add(ref AnimState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (AnimState)value2;
	}

	public virtual void Add(ref AISoundType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (AISoundType)value2;
	}

	public virtual void Add(ref StructureDamageType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (StructureDamageType)value2;
	}

	public virtual void Add(ref TreeType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (TreeType)value2;
	}

	public virtual void Add(ref GrassType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GrassType)value2;
	}

	public virtual void Add(ref SortBy value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SortBy)value2;
	}

	public virtual void Add(ref SortCharactersBy value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SortCharactersBy)value2;
	}

	public virtual void Add(ref SortOrder value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SortOrder)value2;
	}

	public virtual void Add(ref WeatherType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (WeatherType)value2;
	}

	public virtual void Add(ref AttackType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (AttackType)value2;
	}

	public virtual void Add(ref FenceModelType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FenceModelType)value2;
	}

	public virtual void Add(ref RecentActivityType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (RecentActivityType)value2;
	}

	public virtual void Add(ref StoryEventType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (StoryEventType)value2;
	}

	public virtual void Add(ref Specifier value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Specifier)value2;
	}

	public virtual void Add(ref LogEventType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (LogEventType)value2;
	}

	public virtual void Add(ref SkillType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SkillType)value2;
	}

	public virtual void Add(ref QuestInstance.EState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (QuestInstance.EState)value2;
	}

	public virtual void Add(ref Importance value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Importance)value2;
	}

	public virtual void Add(ref FacialExpression value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FacialExpression)value2;
	}

	public virtual void Add(ref InteractionType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (InteractionType)value2;
	}

	public virtual void Add(ref CanAttackState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CanAttackState)value2;
	}

	public virtual void Add(ref TerrainModificationType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (TerrainModificationType)value2;
	}

	public virtual void Add(ref DeletionState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (DeletionState)value2;
	}

	public virtual void Add(ref SpeechAnimState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SpeechAnimState)value2;
	}

	public virtual void Add(ref CampfireState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CampfireState)value2;
	}

	public virtual void Add(ref GangNameType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GangNameType)value2;
	}

	public virtual void Add(ref FindType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FindType)value2;
	}

	public virtual void Add(ref SpeechParamModifier value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SpeechParamModifier)value2;
	}

	public virtual void Add(ref Consciousness value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (Consciousness)value2;
	}

	public virtual void Add(ref RelationshipType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (RelationshipType)value2;
	}

	public virtual void Add(ref SecrecyMode value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SecrecyMode)value2;
	}

	public virtual void Add(ref GraveState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GraveState)value2;
	}

	public virtual void Add(ref FindResult value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (FindResult)value2;
	}

	public virtual void Add(ref ObeyLeaderGoal.SourceType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (ObeyLeaderGoal.SourceType)value2;
	}

	public virtual void Add(ref SparringType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SparringType)value2;
	}

	public virtual void Add(ref TownNameType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (TownNameType)value2;
	}

	public virtual void Add(ref GameFinishedState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GameFinishedState)value2;
	}

	public virtual void Add(ref InvisibleStrainType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (InvisibleStrainType)value2;
	}

	public virtual void Add(ref StayInRangeOf value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (StayInRangeOf)value2;
	}

	public virtual void Add(ref MineralType value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (MineralType)value2;
	}

	public virtual void Add(ref CauseOfDeath value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (CauseOfDeath)value2;
	}

	public virtual void Add(ref HintType value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (HintType)value2;
	}

	public virtual void Add(ref DifficultyMode value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (DifficultyMode)value2;
	}

	public virtual void Add(ref EquipmentPolicyType_DEPRECATED value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (EquipmentPolicyType_DEPRECATED)value2;
	}

	public virtual void Add(ref MapSize value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (MapSize)value2;
	}

	public virtual void Add(ref ParamType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (ParamType)value2;
	}

	public virtual void Add(ref AnimalPose value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (AnimalPose)value2;
	}

	public virtual void Add(ref CanOpenGates value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CanOpenGates)value2;
	}

	public virtual void Add(ref ChickenColor value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (ChickenColor)value2;
	}

	public virtual void Add(ref HoldType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (HoldType)value2;
	}

	public virtual void Add(ref SnowmanState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SnowmanState)value2;
	}

	public virtual void Add(ref HelicopterAnimState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (HelicopterAnimState)value2;
	}

	public virtual void Add(ref GearState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (GearState)value2;
	}

	public virtual void Add(ref ScriptedMoveImportance value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (ScriptedMoveImportance)value2;
	}

	public virtual void Add(ref SurrenderMode value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SurrenderMode)value2;
	}

	public virtual void Add(ref CinematicState value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (CinematicState)value2;
	}

	public virtual void Add(ref SpecifierModifier value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (SpecifierModifier)value2;
	}

	public virtual void Add(ref MapMarkerType value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (MapMarkerType)value2;
	}

	public virtual void Add(ref StringStatus value)
	{
		sbyte value2 = (sbyte)value;
		Add(ref value2);
		value = (StringStatus)value2;
	}

	public virtual void Add(ref RoadDestinationType value)
	{
		byte value2 = (byte)value;
		Add(ref value2);
		value = (RoadDestinationType)value2;
	}

	public virtual void Add(ref GoalPriority value)
	{
		if (Version < 137)
		{
			sbyte value2 = (sbyte)value;
			Add(ref value2);
			value = (GoalPriority)value2;
		}
		else
		{
			int value3 = (int)value;
			Add(ref value3);
			value = (GoalPriority)value3;
		}
	}

	public virtual void OnEnterObject()
	{
	}

	public virtual void OnExitObject()
	{
	}

	public void Add<T>(ref T obj) where T : BaseObject
	{
		BaseObject value = obj;
		Add(ref value);
		obj = value as T;
	}

	public void Add<T>(T obj) where T : class, IReflectable
	{
		OnEnterObject();
		obj.Reflect(this);
		OnExitObject();
	}

	public void Add(ref Goal goal, Character character)
	{
		GoalType value = ((goal != null && !Session.Instance.Editor) ? goal.GetGoalType() : GoalType.Invalid);
		if (IsDoingPrediction && !Goal.WantPrediction(value))
		{
			value = GoalType.Invalid;
		}
		Add(ref value);
		if (IsDeserialising && (goal == null || goal.GetGoalType() != value))
		{
			goal = ((value != GoalType.Invalid) ? Goal.Create(value) : null);
		}
		if (value != GoalType.Invalid)
		{
			goal.Reflect(this, character);
		}
	}

	public void Add(ref Target target, Character character)
	{
		TileObject obj = ((target != null) ? target.Object : null);
		Add(ref obj);
		if (IsDeserialising)
		{
			if (obj != null)
			{
				target = character.GetTarget(obj);
			}
			else
			{
				target = null;
			}
		}
	}

	public void Add<T>(ref List<T> list) where T : IReflectable, new()
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null)
			{
				list = new List<T>();
				list.Capacity = value;
			}
			else
			{
				list.Clear();
				if (list.Capacity < value)
				{
					list.Capacity = value;
				}
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			T item = (IsDeserialising ? new T() : list[i]);
			item.Reflect(this);
			if (IsDeserialising)
			{
				list.Add(item);
			}
		}
		OnExitObject();
	}

	public void AddListThatCanBeNull<T>(ref List<T> list) where T : IReflectable, new()
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<T>();
			}
			if (list != null)
			{
				list.Clear();
				if (list.Capacity < value)
				{
					list.Capacity = value;
				}
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			T item = (IsDeserialising ? new T() : list[i]);
			item.Reflect(this);
			if (IsDeserialising)
			{
				list.Add(item);
			}
		}
		OnExitObject();
	}

	public void AddSmallList<T>(ref List<T> list) where T : IReflectable, new()
	{
		int num = ((list != null) ? list.Count : 0);
		byte value = (byte)num;
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && num > 0)
			{
				list = new List<T>();
			}
			num = value;
			if (list != null)
			{
				list.Clear();
				list.Capacity = num;
			}
		}
		OnEnterObject();
		for (int i = 0; i < num; i++)
		{
			T item = (IsDeserialising ? new T() : list[i]);
			item.Reflect(this);
			if (IsDeserialising)
			{
				list.Add(item);
			}
		}
		OnExitObject();
	}

	public void AddGameObjectRefList<T>(ref List<T> list) where T : BaseObject
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<T>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			T obj = (IsDeserialising ? null : list[i]);
			Add(ref obj);
			if (IsDeserialising && obj != null)
			{
				list.Add(obj);
			}
		}
		OnExitObject();
	}

	public void AddTriggerList(ref List<Trigger> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<Trigger>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Trigger value2 = (IsDeserialising ? null : list[i]);
			Add(ref value2);
			if (IsDeserialising && value2 != null)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddPlayerIDList(ref List<PlayerID> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<PlayerID>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			PlayerID value2 = (IsDeserialising ? default(PlayerID) : list[i]);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddEquipmentPrototypeList(ref List<EquipmentPrototype> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<EquipmentPrototype>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			EquipmentPrototype value2 = (IsDeserialising ? null : list[i]);
			Add(ref value2);
			if (IsDeserialising && value2 != null)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddPropPrototypeList(ref List<PropPrototype> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<PropPrototype>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			PropPrototype value2 = (IsDeserialising ? null : list[i]);
			Add(ref value2);
			if (IsDeserialising && value2 != null)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddStringList(ref List<string> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<string>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			string value2 = (IsDeserialising ? null : list[i]);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddInfectionTypeList(ref List<InfectionType> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<InfectionType>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			InfectionType value2 = ((!IsDeserialising) ? list[i] : InfectionType.None);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddIntList(ref List<int> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<int>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			int value2 = ((!IsDeserialising) ? list[i] : 0);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddVector3List(ref List<Vector3> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<Vector3>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Vector3 value2 = (IsDeserialising ? Vector3.zero : list[i]);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddBaseObjectTypeList(ref List<BaseObjectType> list)
	{
		int value = ((list != null) ? list.Count : 0);
		Add(ref value);
		if (IsDeserialising)
		{
			if (list == null && value > 0)
			{
				list = new List<BaseObjectType>();
			}
			if (list != null)
			{
				list.Clear();
				list.Capacity = value;
			}
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			BaseObjectType value2 = ((!IsDeserialising) ? list[i] : BaseObjectType.Invalid);
			Add(ref value2);
			if (IsDeserialising)
			{
				list.Add(value2);
			}
		}
		OnExitObject();
	}

	public void AddBoolArray(ref bool[] array)
	{
		int value;
		int num = (value = ((array != null) ? array.Length : 0));
		Add(ref value);
		if (num != value)
		{
			array = new bool[value];
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Add(ref array[i]);
		}
		OnExitObject();
	}

	public void AddByteArray(ref byte[] array)
	{
		int value;
		int num = (value = ((array != null) ? array.Length : 0));
		Add(ref value);
		if (num != value)
		{
			array = new byte[value];
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Add(ref array[i]);
		}
		OnExitObject();
	}

	public void AddByteArray(ref byte[] array, ref int count)
	{
		int num = ((array != null) ? array.Length : 0);
		Add(ref count);
		if (IsDeserialising && count > num)
		{
			array = new byte[count];
		}
		OnEnterObject();
		for (int i = 0; i < count; i++)
		{
			Add(ref array[i]);
		}
		OnExitObject();
	}

	public void AddIntArray(ref int[] array)
	{
		int value;
		int num = (value = ((array != null) ? array.Length : 0));
		Add(ref value);
		if (num != value)
		{
			array = new int[value];
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Add(ref array[i]);
		}
		OnExitObject();
	}

	public void AddFloatArray(ref float[] array)
	{
		int value;
		int num = (value = ((array != null) ? array.Length : 0));
		Add(ref value);
		if (num != value)
		{
			array = new float[value];
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			Add(ref array[i]);
		}
		OnExitObject();
	}

	public void AddStructArray<T>(ref T[] array) where T : IReflectable
	{
		int num = ((array != null) ? array.Length : 0);
		int value = num;
		Add(ref value);
		if (value > num)
		{
			array = new T[value];
		}
		OnEnterObject();
		for (int i = 0; i < value; i++)
		{
			array[i].Reflect(this);
		}
		OnExitObject();
	}
}
