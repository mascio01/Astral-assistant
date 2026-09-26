using System.Text;

public class Mine : Building
{
	public int[] MiningProgress = new int[4];

	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/Mine");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Mine;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddIntArray(ref MiningProgress);
	}

	public bool HasRichDeposits(MineralType mineralType)
	{
		return HasRichDeposits(mineralType, Tile);
	}

	public static bool HasRichDeposits(MineralType mineralType, TerrainCoord tile)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (!instance.HasRichMineralDeposits(tile, mineralType) && !instance.HasRichMineralDeposits(tile + new TerrainCoord(4, 4), mineralType) && !instance.HasRichMineralDeposits(tile + new TerrainCoord(-4, 4), mineralType) && !instance.HasRichMineralDeposits(tile + new TerrainCoord(4, -4), mineralType))
		{
			return instance.HasRichMineralDeposits(tile + new TerrainCoord(-4, -4), mineralType);
		}
		return true;
	}

	public override int GetMiningProgressNeededToExtract(MineralType mineralType)
	{
		if (!HasRichDeposits(mineralType))
		{
			return MoveToAndMine.ProgressNeededToExtractPoorDeposits;
		}
		return MoveToAndMine.ProgressNeededToExtractRichDeposits;
	}

	public static void BuildMineralDepositsString(StringBuilder sb, TerrainCoord tile)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			MineralType mineralType = (MineralType)i;
			if (!HasRichDeposits(mineralType, tile))
			{
				continue;
			}
			EquipmentPrototype equipmentPrototype = EquipmentPrototype.MiningResources[(int)mineralType];
			if (equipmentPrototype != null)
			{
				if (num == 0)
				{
					sb.Append(' ');
					sb.Append('(');
				}
				else
				{
					sb.Append(',');
					sb.Append(' ');
				}
				sb.Append(GameImpl.Translate(equipmentPrototype.NameHash));
				num++;
			}
		}
		if (num > 0)
		{
			sb.Append(')');
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		base.BuildDisplayName(sb, noStrangers, englishOnly);
		if (InTerrain)
		{
			BuildMineralDepositsString(sb, Tile);
		}
	}

	public override int GetMiningProgress(MineralType mineralType)
	{
		if (mineralType <= MineralType.None)
		{
			return 0;
		}
		return MiningProgress[(int)mineralType];
	}

	public override int GetMiningResourceRemaining(MineralType mineralType)
	{
		return int.MaxValue;
	}

	public override void IncrementMiningProgress(MineralType mineralType, int v)
	{
		MiningProgress[(int)mineralType] += v;
	}
}
