public class ThrowableFood : Throwable
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.ThrowableFood;
	}

	public override BaseObjectType GetProjectileType()
	{
		return BaseObjectType.FoodProjectile;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		bestInfectedWith = InfectionType.None;
		return 0f;
	}
}
