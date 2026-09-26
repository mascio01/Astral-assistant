public class GlassBottle : Throwable
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GlassBottle;
	}

	public override BaseObjectType GetProjectileType()
	{
		return BaseObjectType.BottleProjectile;
	}

	public override SkillType GetRangeSkillType()
	{
		return SkillType.Strength;
	}

	public override SkillType GetDamageSkillType()
	{
		return SkillType.Strength;
	}

	public override bool IsBottle()
	{
		return true;
	}
}
