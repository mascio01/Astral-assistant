public class WorkBench : CraftingProp
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/CoveredWorkbench"),
		new PrefabResource("Prefabs/Props/BrickCoveredWorkbench")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WorkBench;
	}

	public override bool CanRepairArmor()
	{
		if (Prototype != null)
		{
			return Prototype.CanRepairArmor;
		}
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 217)
		{
			reflector.AddAfter(ref Variation, 159);
		}
	}
}
