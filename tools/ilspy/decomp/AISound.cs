using UnityEngine;

public struct AISound : IReflectable
{
	public AISoundType Type;

	public Vector3 Pos;

	public float SoundRadius;

	public float SightRadius;

	public TileObject Speaker;

	public Character Source;

	public TileObject Target;

	public TileObject IntendedTarget;

	public EquipmentPrototype EquipmentProto;

	public bool WasFightToTheDeath;

	public AISound(AISoundType type, Vector3 pos, float soundRadius, float sightRadius, TileObject speaker, Character source, TileObject target, TileObject intendedTarget)
	{
		Type = type;
		Pos = pos;
		SoundRadius = soundRadius;
		SightRadius = sightRadius;
		Speaker = speaker;
		Source = source;
		Target = target;
		IntendedTarget = intendedTarget;
		EquipmentProto = null;
		WasFightToTheDeath = false;
	}

	public AISound(AISoundType type, Vector3 pos, float soundRadius, float sightRadius, TileObject speaker, Character source, TileObject target, TileObject intendedTarget, EquipmentPrototype proto)
	{
		Type = type;
		Pos = pos;
		SoundRadius = soundRadius;
		SightRadius = sightRadius;
		Speaker = speaker;
		Source = source;
		Target = target;
		IntendedTarget = intendedTarget;
		EquipmentProto = proto;
		WasFightToTheDeath = false;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref Pos);
		reflector.Add(ref SoundRadius);
		reflector.Add(ref SightRadius);
		reflector.AddAfter(ref Speaker, 231);
		reflector.Add(ref Source);
		reflector.Add(ref Target);
		reflector.AddAfter(ref IntendedTarget, 104);
		reflector.AddAfter(ref EquipmentProto, 593);
		reflector.AddAfter(ref WasFightToTheDeath, 615);
	}

	public static bool IsAttackSound(Character source, Character listener, TileObject intendedTarget, AISoundType type, EquipmentPrototype proto)
	{
		switch (type)
		{
		case AISoundType.Attack:
		case AISoundType.Hit:
		case AISoundType.Pain:
		case AISoundType.Death:
		case AISoundType.Choke:
		case AISoundType.WokeUpAngry:
		case AISoundType.Explode:
			return true;
		case AISoundType.StealthAttack:
			if (listener.GetBaseObjectType() != BaseObjectType.Human && proto != null && listener.LikesFood(proto))
			{
				return false;
			}
			return true;
		case AISoundType.Radio:
			return listener.Zombie;
		case AISoundType.Suspicious:
			return listener == intendedTarget;
		case AISoundType.Interesting:
			if (listener == intendedTarget)
			{
				return listener.IsEnemy(source);
			}
			return false;
		default:
			return false;
		}
	}

	public static bool SoundComesFromSource(AISoundType type)
	{
		switch (type)
		{
		case AISoundType.Attack:
		case AISoundType.StealthAttack:
		case AISoundType.Choke:
			return true;
		case AISoundType.Hit:
		case AISoundType.Pain:
		case AISoundType.Death:
		case AISoundType.Warning:
		case AISoundType.Suspicious:
		case AISoundType.Radio:
		case AISoundType.WokeUpAngry:
		case AISoundType.FoundBody:
		case AISoundType.Explode:
		case AISoundType.Interesting:
			return false;
		default:
			return true;
		}
	}
}
