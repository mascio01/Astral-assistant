using UnityEngine;

public struct MemoryParam : IReflectable
{
	private object Mem;

	public MemoryParam(Character owner, int index)
	{
		if (owner != null && index >= 0 && index < owner.Memories.Count)
		{
			Mem = owner.Memories[index];
		}
		else
		{
			Mem = null;
		}
	}

	public MemoryParam(Memory memory)
	{
		Mem = memory;
	}

	public MemoryParam(BaseObject obj)
	{
		Mem = obj;
	}

	public MemoryParam(int intParam)
	{
		Mem = intParam;
	}

	public MemoryParam(EquipmentPrototype proto)
	{
		Mem = proto;
	}

	public MemoryParam(LiquidPrototype liquid)
	{
		Mem = liquid;
	}

	public ParamType GetParamType()
	{
		if (Mem != null)
		{
			if (!(Mem is Memory))
			{
				if (!(Mem is int))
				{
					if (!(Mem is BaseObject))
					{
						if (!(Mem is EquipmentPrototype))
						{
							if (!(Mem is LiquidPrototype))
							{
								return ParamType.None;
							}
							return ParamType.LiquidPrototype;
						}
						return ParamType.EquipmentPrototype;
					}
					return ParamType.BaseObject;
				}
				return ParamType.Int;
			}
			return ParamType.Memory;
		}
		return ParamType.None;
	}

	public Memory GetMemory()
	{
		if (!(Mem is Memory))
		{
			return default(Memory);
		}
		return (Memory)Mem;
	}

	public BaseObject GetBaseObject()
	{
		return Mem as BaseObject;
	}

	public EquipmentPrototype GetEquipmentPrototype()
	{
		return Mem as EquipmentPrototype;
	}

	public LiquidPrototype GetLiquidPrototype()
	{
		return Mem as LiquidPrototype;
	}

	public int GetInt()
	{
		if (!(Mem is int))
		{
			return 0;
		}
		return (int)Mem;
	}

	public bool IsInt()
	{
		return Mem is int;
	}

	public void Reflect(Reflector reflector)
	{
		ParamType paramType = ParamType.None;
		if (reflector.Version < 246)
		{
			bool value = false;
			reflector.Add(ref value);
			paramType = (value ? ParamType.Memory : ParamType.None);
		}
		else
		{
			paramType = GetParamType();
			reflector.Add(ref paramType);
		}
		switch (paramType)
		{
		case ParamType.Memory:
			if (reflector.IsDeserialising)
			{
				Memory memory = default(Memory);
				memory.Reflect(reflector);
				Mem = memory;
			}
			else
			{
				((Memory)Mem).Reflect(reflector);
			}
			if (((Memory)Mem).Prototype == null)
			{
				Debug.Log("Null memory prototype");
			}
			break;
		case ParamType.BaseObject:
		{
			BaseObject value4 = Mem as BaseObject;
			reflector.Add(ref value4);
			if (reflector.IsDeserialising)
			{
				Mem = value4;
			}
			break;
		}
		case ParamType.Int:
		{
			int value5 = GetInt();
			reflector.Add(ref value5);
			if (reflector.IsDeserialising)
			{
				Mem = value5;
			}
			break;
		}
		case ParamType.EquipmentPrototype:
		{
			EquipmentPrototype value3 = Mem as EquipmentPrototype;
			reflector.Add(ref value3);
			if (reflector.IsDeserialising)
			{
				Mem = value3;
			}
			break;
		}
		case ParamType.LiquidPrototype:
		{
			LiquidPrototype value2 = Mem as LiquidPrototype;
			reflector.Add(ref value2);
			if (reflector.IsDeserialising)
			{
				Mem = value2;
			}
			break;
		}
		}
	}

	public bool Equals(MemoryParam other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is MemoryParam)
		{
			return this == (MemoryParam)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Mem == null)
		{
			return 0;
		}
		return Mem.GetHashCode();
	}

	public static bool operator ==(MemoryParam a, MemoryParam b)
	{
		if (a.Mem is Memory && b.Mem is Memory)
		{
			Memory obj = (Memory)a.Mem;
			Memory memory = (Memory)b.Mem;
			return obj == memory;
		}
		if (a.Mem is BaseObject && b.Mem is BaseObject)
		{
			return a.Mem == b.Mem;
		}
		if (a.Mem is int && b.Mem is int)
		{
			return (int)a.Mem == (int)b.Mem;
		}
		if (a.Mem is EquipmentPrototype && b.Mem is EquipmentPrototype)
		{
			return a.Mem == b.Mem;
		}
		if (a.Mem is LiquidPrototype && b.Mem is LiquidPrototype)
		{
			return a.Mem == b.Mem;
		}
		if (a.Mem == null && b.Mem == null)
		{
			return true;
		}
		return false;
	}

	public static bool operator !=(MemoryParam a, MemoryParam b)
	{
		return !(a == b);
	}
}
