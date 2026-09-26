public class Outhouse : Building
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Buildings/Outhouse"),
		new PrefabResource("Prefabs/Buildings/PortableToilet")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Outhouse;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 1;
	}

	public override LiquidPrototype GetLiquidType()
	{
		if (Prototype == null || Prototype.LiquidPrototype == null)
		{
			return LiquidPrototype.Urine;
		}
		return Prototype.LiquidPrototype;
	}

	public override float GetLiquidCapacity()
	{
		if (Prototype == null || !(Prototype.LiquidCapacity > 0f))
		{
			return 500f;
		}
		return Prototype.LiquidCapacity;
	}

	public override bool CanBeEmptied()
	{
		return true;
	}

	public override bool CanSetStoragePolicy()
	{
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version >= 75 && reflector.Version < 468)
		{
			float value = 0f;
			reflector.Add(ref value);
			LiquidAmount = value;
		}
	}
}
