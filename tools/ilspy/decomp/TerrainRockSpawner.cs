using System.Collections.Generic;

public class TerrainRockSpawner : DebugMenu
{
	private MineralType CurrentMineralType;

	private static List<PropPrototype> TempPropPicks = new List<PropPrototype>();

	public TerrainRockSpawner()
		: base(GameImpl.Translate("DEBUG_RockSpawner"))
	{
		for (int i = -1; i < 4; i++)
		{
			MineralType mineralType = (MineralType)i;
			Items.Add(new DebugMenuItemToggle(mineralType.ToString(), () => CurrentMineralType == mineralType, delegate
			{
				CurrentMineralType = mineralType;
			}));
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (!instance3.IsPressed(InputFunction.MainAction))
		{
			return;
		}
		RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
		if (raycastResult.HitObject != instance2)
		{
			return;
		}
		TileObject fixedObjectOnTile = instance2.GetFixedObjectOnTile(raycastResult.Tile.x, raycastResult.Tile.y);
		if (fixedObjectOnTile == null && CurrentMineralType != MineralType.None)
		{
			PropPrototype propPrototype = PickRockPrototypeByMineralType(CurrentMineralType, MathUtil.NonDeterministicRand);
			if (propPrototype != null)
			{
				Prop.OrientationType orientation = MathUtil.NonDeterministicRand.RandomOrientationType();
				if (!instance2.HasObstructions(raycastResult.Tile, orientation, propPrototype, 0, canBeNextToFence: true, notOnRoads: false, notOnRivers: false, null))
				{
					TileObject.SpawnProp(propPrototype, raycastResult.Tile, orientation);
					Session.Instance.AchievementsEnabled = false;
				}
			}
		}
		else if (fixedObjectOnTile != null && CurrentMineralType == MineralType.None && fixedObjectOnTile.GetMineralType() != MineralType.None)
		{
			fixedObjectOnTile.Delete();
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public PropPrototype PickRockPrototypeByMineralType(MineralType mineralType, CustomRandom rand)
	{
		TempPropPicks.Clear();
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if ((item.Value.TypeName == BaseObjectType.Rock || item.Value.TypeName == BaseObjectType.Boulder) && item.Value.MineralType == mineralType)
			{
				TempPropPicks.Add(item.Value);
				if (item.Value.TypeName == BaseObjectType.Rock)
				{
					TempPropPicks.Add(item.Value);
					TempPropPicks.Add(item.Value);
				}
			}
		}
		PropPrototype result = null;
		if (TempPropPicks.Count > 0)
		{
			result = TempPropPicks[rand.Next(TempPropPicks.Count)];
		}
		TempPropPicks.Clear();
		return result;
	}
}
