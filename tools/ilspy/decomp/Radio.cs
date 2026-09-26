public class Radio : Throwable
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Radio;
	}

	public override BaseObjectType GetProjectileType()
	{
		return BaseObjectType.RadioProjectile;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		bestInfectedWith = InfectionType.None;
		return 0f;
	}
}
