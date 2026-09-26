using UnityEngine;

public class Tent : Building
{
	private static PrefabResource[] Models = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Buildings/Tent"),
		new PrefabResource("Prefabs/Buildings/Tent2"),
		new PrefabResource("Prefabs/Buildings/LargeTent")
	};

	public static Resource<Material>[] MiscMaterials = new Resource<Material>[3]
	{
		new Resource<Material>("Materials/Buildings/LargeTent/LargeTent", includeInList: true),
		new Resource<Material>("Materials/Buildings/LargeTent/MilitaryTent1", includeInList: true),
		new Resource<Material>("Materials/Buildings/LargeTent/MilitaryTent2", includeInList: true)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Tent;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 1;
	}
}
