public class Gun : AmmoWeapon
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Gun;
	}

	public override SkillType GetRangeSkillType()
	{
		return SkillType.Firearms;
	}

	public override SkillType GetDamageSkillType()
	{
		return SkillType.Firearms;
	}

	public override SkillType GetReloadSpeedSkillType()
	{
		return SkillType.Firearms;
	}
}
