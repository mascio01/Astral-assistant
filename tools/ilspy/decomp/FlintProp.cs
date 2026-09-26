public class FlintProp : SingleTileProp
{
	private static PrefabResource[] Models = new PrefabResource[5]
	{
		new PrefabResource("Prefabs/Dynamic Nature/Rocks and Stones/prefab_rock_small_01_Snow_flint"),
		new PrefabResource("Prefabs/Dynamic Nature/Rocks and Stones/prefab_rock_small_02_Snow_flint"),
		new PrefabResource("Prefabs/Dynamic Nature/Rocks and Stones/prefab_rock_small_03_Snow_flint"),
		new PrefabResource("Prefabs/Dynamic Nature/Rocks and Stones/prefab_rock_small_04_Snow_flint"),
		new PrefabResource("Prefabs/Dynamic Nature/Rocks and Stones/prefab_rock_small_05_Snow_flint")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FlintProp;
	}
}
