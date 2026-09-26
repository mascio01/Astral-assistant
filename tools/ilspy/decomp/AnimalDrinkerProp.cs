public class AnimalDrinkerProp : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Trough");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.AnimalDrinkerProp;
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override LiquidPrototype GetLiquidType()
	{
		if (Prototype == null || Prototype.LiquidPrototype == null)
		{
			return LiquidPrototype.Water;
		}
		return Prototype.LiquidPrototype;
	}

	public override CursorActionDisabledReason CanBeFilled()
	{
		return CursorActionDisabledReason.Enabled;
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
