using System;
using System.IO;
using System.Text;
using UnityEngine;

internal class TextDumper : Reflector, IDisposable
{
	public StreamWriter StreamWriter;

	private StringBuilder _sb = new StringBuilder(16777216);

	private string Tabs = "";

	private int TabsCount;

	public override bool IsDeserialising => false;

	public override bool IsTextDumping => true;

	public TextDumper(string path)
	{
		StreamWriter = new StreamWriter(path);
	}

	public void Dispose()
	{
		StreamWriter.Close();
	}

	public override void OnEnterObject()
	{
		TabsCount++;
		Tabs += "\t";
	}

	public override void OnExitObject()
	{
		TabsCount--;
		Tabs = Tabs.Substring(0, TabsCount);
	}

	public override void AddTextDumpLine(string text)
	{
		StreamWriter.WriteLine(Tabs + text);
	}

	public override void Add(ref bool value)
	{
		StreamWriter.WriteLine(Tabs + "bool: " + (value ? "true" : "false"));
	}

	public override void Add(ref byte value)
	{
		StreamWriter.WriteLine(Tabs + "byte: " + value);
	}

	public override void Add(ref sbyte value)
	{
		StreamWriter.WriteLine(Tabs + "sbyte: " + value);
	}

	public override void Add(ref char value)
	{
		StreamWriter.WriteLine(Tabs + "char: " + value);
	}

	public override void Add(ref short value)
	{
		StreamWriter.WriteLine(Tabs + "short: " + value);
	}

	public override void Add(ref int value)
	{
		StreamWriter.WriteLine(Tabs + "int: " + value);
	}

	public override void Add(ref uint value)
	{
		StreamWriter.WriteLine(Tabs + "uint: " + value);
	}

	public override void Add(ref long value)
	{
		StreamWriter.WriteLine(Tabs + "long: " + value);
	}

	public override void Add(ref ulong value)
	{
		StreamWriter.WriteLine(Tabs + "ulong: " + value);
	}

	public override void Add(ref float value)
	{
		StreamWriter.WriteLine(Tabs + "float: " + value.ToString("G9"));
	}

	public override void Add(ref double value)
	{
		StreamWriter.WriteLine(Tabs + "double: " + value);
	}

	public override void Add(ref TimeSpan value)
	{
		StreamWriter.WriteLine(Tabs + "TimeSpan: " + value);
	}

	public override void Add(ref TerrainCoord value)
	{
		StreamWriter.WriteLine(Tabs + "TerrainCoord: " + value.ToString());
	}

	public override void Add(ref Vector2 value)
	{
		StreamWriter.WriteLine(Tabs + "Vector2: " + value.ToString("G9"));
	}

	public override void Add(ref Vector3 value)
	{
		StreamWriter.WriteLine(Tabs + "Vector3: " + value.ToString("G9"));
	}

	public override void Add(ref Vector4 value)
	{
		StreamWriter.WriteLine(Tabs + "Vector4: " + value.ToString("G9"));
	}

	public override void Add(ref Half3 value)
	{
		StreamWriter.WriteLine(Tabs + "Vector3 (low precision): " + value.ToVector3().ToString("G9"));
	}

	public override void Add(ref Quaternion value)
	{
		StreamWriter.WriteLine(Tabs + "Quaternion: " + value.ToString("G9"));
	}

	public override void Add(ref Matrix4x4 value)
	{
		StreamWriter.WriteLine(Tabs + "Matrix4x4: " + value.ToString("G9"));
	}

	public override void Add(ref Color value)
	{
		StreamWriter.WriteLine(Tabs + "Color: " + value.ToString("G9"));
	}

	public override void Add(ref Color32 value)
	{
		StreamWriter.WriteLine(Tabs + "Color32: " + value.ToString());
	}

	public override void Add(ref string value)
	{
		StreamWriter.WriteLine(Tabs + "string: " + value);
	}

	public override void Add(ref BaseObject value)
	{
		StreamWriter.WriteLine(Tabs + "BaseObject: " + ((value != null) ? (value.Id + " (" + value.GetDisplayNameString() + ")") : "null"));
	}

	public override void Add(ref Speech value)
	{
		StreamWriter.WriteLine(Tabs + "Speech: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref Quest value)
	{
		StreamWriter.WriteLine(Tabs + "Quest: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref Trigger value)
	{
		StreamWriter.WriteLine(Tabs + "Trigger: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref Invader value)
	{
		StreamWriter.WriteLine(Tabs + "Invader: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref Recipe value)
	{
		StreamWriter.WriteLine(Tabs + "Recipe: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref MemoryPrototype value)
	{
		StreamWriter.WriteLine(Tabs + "MemoryPrototype: " + ((value != null) ? value.UniqueID : "null"));
	}

	public override void Add(ref EquipmentPrototype value)
	{
		StreamWriter.WriteLine(Tabs + "EquipmentPrototype: " + ((value != null) ? value.Name : "null"));
	}

	public override void Add(ref LiquidPrototype value)
	{
		StreamWriter.WriteLine(Tabs + "LiquidPrototype: " + ((value != null) ? value.Name : "null"));
	}

	public override void Add(ref PropPrototype value)
	{
		StreamWriter.WriteLine(Tabs + "PropPrototype: " + ((value != null) ? value.Name : "null"));
	}

	public override void Add(ref PlayerID value)
	{
		StreamWriter.WriteLine(Tabs + "PlayerID: " + value.SteamID.m_SteamID + " (" + value.GetPlayerName() + ")");
	}

	public override void Add(Array src, int size, int dimensions)
	{
		StreamWriter.Write(Tabs + "Array: " + src.ToString());
		for (int i = 0; i < src.GetLength(0); i++)
		{
			switch (dimensions)
			{
			case 1:
				if (i > 0)
				{
					StreamWriter.Write(',');
				}
				StreamWriter.Write(src.GetValue(i));
				break;
			case 2:
			{
				StreamWriter.WriteLine();
				StreamWriter.Write(Tabs + "Row: " + i + ": ");
				for (int j = 0; j < src.GetLength(1); j++)
				{
					if (j > 0)
					{
						StreamWriter.Write(',');
					}
					StreamWriter.Write(src.GetValue(j, i));
				}
				break;
			}
			}
		}
		StreamWriter.WriteLine();
	}

	public override void Add(ref byte[] bytes, ref int size)
	{
	}

	public override void Add(ref BaseObjectType value)
	{
		StreamWriter.WriteLine(Tabs + "BaseObjectType: " + value);
	}

	public override void Add(ref GoalType value)
	{
		StreamWriter.WriteLine(Tabs + "GoalType: " + value);
	}

	public override void Add(ref StoryEventType value)
	{
		StreamWriter.WriteLine(Tabs + "StoryEventType: " + value);
	}

	public override void Add(ref CommunityType value)
	{
		StreamWriter.WriteLine(Tabs + "CommunityType: " + value);
	}

	public override void Add(ref CommunityRelationshipType value)
	{
		StreamWriter.WriteLine(Tabs + "CommunityRelationshipType: " + value);
	}

	public override void Add(ref SquadAction value)
	{
		StreamWriter.WriteLine(Tabs + "SquadAction: " + value);
	}

	public override void Add(ref SquadBehaviour value)
	{
		StreamWriter.WriteLine(Tabs + "SquadBehaviour: " + value);
	}

	public override void Add(ref InjuryLocation value)
	{
		StreamWriter.WriteLine(Tabs + "InjuryLocation: " + value);
	}

	public override void Add(ref InjuryType value)
	{
		StreamWriter.WriteLine(Tabs + "InjuryType: " + value);
	}

	public override void Add(ref FenceModelType value)
	{
		StreamWriter.WriteLine(Tabs + "FenceModelType: " + value);
	}

	public override void Add(ref InfectionType value)
	{
		StreamWriter.WriteLine(Tabs + "InfectionType: " + value);
	}

	public override void Add(ref DeletionState value)
	{
		StreamWriter.WriteLine(Tabs + "DeletionState: " + value);
	}

	public override void Add(ref Prop.OrientationType value)
	{
		StreamWriter.WriteLine(Tabs + "Prop.OrientationType: " + value);
	}

	public override void Add(ref StructureDamageType value)
	{
		StreamWriter.WriteLine(Tabs + "StructureDamageType: " + value);
	}

	public override void Add(ref GateState value)
	{
		StreamWriter.WriteLine(Tabs + "GateState: " + value);
	}

	public override void Add(ref Bone value)
	{
		StreamWriter.WriteLine(Tabs + "Bone: " + value);
	}

	public override void Add(ref GenderType value)
	{
		StreamWriter.WriteLine(Tabs + "GenderType: " + value);
	}

	public override void Add(ref FacialHairType value)
	{
		StreamWriter.WriteLine(Tabs + "FacialHairType: " + value);
	}

	public override void Add(ref SkillType value)
	{
		StreamWriter.WriteLine(Tabs + "SkillType: " + value);
	}

	public override void Add(ref Role value)
	{
		StreamWriter.WriteLine(Tabs + "Role: " + value);
	}

	public override void Add(ref Rank value)
	{
		StreamWriter.WriteLine(Tabs + "Rank: " + value);
	}

	public override void Add(ref RecentActivityType value)
	{
		StreamWriter.WriteLine(Tabs + "RecentActivityType: " + value);
	}

	public override void Add(ref MovementType value)
	{
		StreamWriter.WriteLine(Tabs + "MovementType: " + value);
	}

	public override void Add(ref ThinkPriority value)
	{
		StreamWriter.WriteLine(Tabs + "ThinkPriority: " + value);
	}

	public override void Add(ref InteractionType value)
	{
		StreamWriter.WriteLine(Tabs + "InteractionType: " + value);
	}

	public override void Add(ref AStarPriority value)
	{
		StreamWriter.WriteLine(Tabs + "AStarPriority: " + value);
	}

	public override void Add(ref TerrainTex value)
	{
		StreamWriter.WriteLine(Tabs + "TerrainTex: " + value);
	}

	public override void Add(ref SortBy value)
	{
		StreamWriter.WriteLine(Tabs + "SortBy: " + value);
	}

	public override void Add(ref SortOrder value)
	{
		StreamWriter.WriteLine(Tabs + "SortOrder: " + value);
	}

	public override void Add(ref P2PMsgType value)
	{
		StreamWriter.WriteLine(Tabs + "P2PMsgType: " + value);
	}

	public override void Add(ref InputActionType value)
	{
		StreamWriter.WriteLine(Tabs + "InputActionType: " + value);
	}

	public override void Add(ref PlayerMode value)
	{
		StreamWriter.WriteLine(Tabs + "PlayerMode: " + value);
	}

	public override void Add(ref PlaySpeed value)
	{
		StreamWriter.WriteLine(Tabs + "PlaySpeed: " + value);
	}

	public override void Add(ref TargettableBodyLocation value)
	{
		StreamWriter.WriteLine(Tabs + "TargettableBodyLocation: " + value);
	}

	public override void Add(ref MovementMode value)
	{
		StreamWriter.WriteLine(Tabs + "MovementMode: " + value);
	}

	public override void Add(ref BodyType value)
	{
		StreamWriter.WriteLine(Tabs + "BodyType: " + value);
	}

	public override void Add(ref FaceType value)
	{
		StreamWriter.WriteLine(Tabs + "FaceType: " + value);
	}

	public override void Add(ref HairType value)
	{
		StreamWriter.WriteLine(Tabs + "HairType: " + value);
	}

	public override void Add(ref GatePolicy value)
	{
		StreamWriter.WriteLine(Tabs + "GatePolicy: " + value);
	}

	public override void Add(ref FollowCommand value)
	{
		StreamWriter.WriteLine(Tabs + "FollowCommand: " + value);
	}

	public override void Add(ref GoalPriority value)
	{
		StreamWriter.WriteLine(Tabs + "GoalPriority: " + value);
	}

	public override void Add(ref ActionAnim value)
	{
		StreamWriter.WriteLine(Tabs + "ActionAnim: " + value);
	}

	public override void Add(ref AnimState value)
	{
		StreamWriter.WriteLine(Tabs + "AnimState: " + value);
	}

	public override void Add(ref AISoundType value)
	{
		StreamWriter.WriteLine(Tabs + "AISoundType: " + value);
	}

	public override void Add(ref TreeType value)
	{
		StreamWriter.WriteLine(Tabs + "TreeType: " + value);
	}

	public override void Add(ref GrassType value)
	{
		StreamWriter.WriteLine(Tabs + "GrassType: " + value);
	}

	public override void Add(ref SortCharactersBy value)
	{
		StreamWriter.WriteLine(Tabs + "SortCharactersBy: " + value);
	}

	public override void Add(ref WeatherType value)
	{
		StreamWriter.WriteLine(Tabs + "WeatherType: " + value);
	}

	public override void Add(ref AttackType value)
	{
		StreamWriter.WriteLine(Tabs + "AttackType: " + value);
	}

	public override void Add(ref Specifier value)
	{
		StreamWriter.WriteLine(Tabs + "Specifier: " + value);
	}

	public override void Add(ref LogEventType value)
	{
		StreamWriter.WriteLine(Tabs + "LogEventType: " + value);
	}

	public override void Add(ref QuestInstance.EState value)
	{
		StreamWriter.WriteLine(Tabs + "QuestInstance.EState: " + value);
	}

	public override void Add(ref Importance value)
	{
		StreamWriter.WriteLine(Tabs + "Importance: " + value);
	}

	public override void Add(ref FacialExpression value)
	{
		StreamWriter.WriteLine(Tabs + "FacialExpression: " + value);
	}

	public override void Add(ref CanAttackState value)
	{
		StreamWriter.WriteLine(Tabs + "CanAttackState: " + value);
	}

	public override void Add(ref TerrainModificationType value)
	{
		StreamWriter.WriteLine(Tabs + "TerrainModificationType: " + value);
	}

	public override void Add(ref SpeechAnimState value)
	{
		StreamWriter.WriteLine(Tabs + "SpeechAnimState: " + value);
	}

	public override void Add(ref CampfireState value)
	{
		StreamWriter.WriteLine(Tabs + "CampfireState: " + value);
	}

	public override void Add(ref GangNameType value)
	{
		StreamWriter.WriteLine(Tabs + "GangNameType: " + value);
	}

	public override void Add(ref FindType value)
	{
		StreamWriter.WriteLine(Tabs + "FindType: " + value);
	}

	public override void Add(ref SpeechParamModifier value)
	{
		StreamWriter.WriteLine(Tabs + "SpeechParamModifier: " + value);
	}

	public override void Add(ref Consciousness value)
	{
		StreamWriter.WriteLine(Tabs + "Consciousness: " + value);
	}

	public override void Add(ref RelationshipType value)
	{
		StreamWriter.WriteLine(Tabs + "RelationshipType: " + value);
	}

	public override void Add(ref SecrecyMode value)
	{
		StreamWriter.WriteLine(Tabs + "SecrecyMode: " + value);
	}

	public override void Add(ref GraveState value)
	{
		StreamWriter.WriteLine(Tabs + "GraveState: " + value);
	}

	public override void Add(ref FindResult value)
	{
		StreamWriter.WriteLine(Tabs + "FindResult: " + value);
	}

	public override void Add(ref ObeyLeaderGoal.SourceType value)
	{
		StreamWriter.WriteLine(Tabs + "ObeyLeaderGoal.SourceType: " + value);
	}

	public override void Add(ref SparringType value)
	{
		StreamWriter.WriteLine(Tabs + "SparringType: " + value);
	}

	public override void Add(ref TownNameType value)
	{
		StreamWriter.WriteLine(Tabs + "TownNameType: " + value);
	}
}
