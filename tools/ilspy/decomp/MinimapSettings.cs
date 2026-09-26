using UnityEngine;

public class MinimapSettings : MonoBehaviour
{
	public Color32 GrassCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 ForestCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 RockCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 RoadCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 RiverCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 PropCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 TreeCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 BushCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 BoulderCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 LeadOreCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 IronOreCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 CropsCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 ArrowCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 FlintCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 RadioCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 MeatCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 TrapCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 FriendCol = new Color32(byte.MaxValue, 165, 0, byte.MaxValue);

	public Color32 NeutralCol = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color32 EnemyCol = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color32 AllyCol = new Color32(0, 0, byte.MaxValue, byte.MaxValue);

	public Color32 DeadCol = new Color32(128, 128, 128, byte.MaxValue);

	public Color32 FriendCol2 = new Color32(byte.MaxValue, 165, 0, byte.MaxValue);

	public Color32 NeutralCol2 = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color32 EnemyCol2 = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color32 AllyCol2 = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	public Color32 DeadCol2 = new Color32(128, 128, 128, byte.MaxValue);

	public Color32 GreenStrainCol = new Color32(184, 230, 26, byte.MaxValue);

	public Color32 BlueStrainCol = new Color32(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 RedStrainCol = new Color32(byte.MaxValue, 0, 128, byte.MaxValue);

	public Color32 WhiteStrainCol = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 InvisibleStrainCol = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 GreenStrainCol2 = new Color32(184, 230, 26, byte.MaxValue);

	public Color32 BlueStrainCol2 = new Color32(0, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 RedStrainCol2 = new Color32(byte.MaxValue, 0, 128, byte.MaxValue);

	public Color32 WhiteStrainCol2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 InvisibleStrainCol2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public Color32 MaizeCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 CarrotCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 PumpkinCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 SunflowerCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 CabbageCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 BeetCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 PepperCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 CucumberCol = new Color32(0, 0, 0, byte.MaxValue);

	public Color32 UrgentCol = new Color32(byte.MaxValue, 128, 192, byte.MaxValue);

	public Color32 AmountCol = new Color32(byte.MaxValue, 164, 0, byte.MaxValue);

	public Color32 IncrementCol = new Color32(0, 128, 0, byte.MaxValue);

	public Color32 DecrementCol = new Color32(128, 0, 0, byte.MaxValue);

	public Color32 GetInfectionCol(InfectionType infectionType)
	{
		return infectionType switch
		{
			InfectionType.Green => GreenStrainCol, 
			InfectionType.Blue => BlueStrainCol, 
			InfectionType.Red => RedStrainCol, 
			InfectionType.White => WhiteStrainCol, 
			InfectionType.Invisible => InvisibleStrainCol, 
			_ => MathUtil.White, 
		};
	}

	public Color32 GetInfectionCol2(InfectionType infectionType)
	{
		return infectionType switch
		{
			InfectionType.Green => GreenStrainCol2, 
			InfectionType.Blue => BlueStrainCol2, 
			InfectionType.Red => RedStrainCol2, 
			InfectionType.White => WhiteStrainCol2, 
			InfectionType.Invisible => InvisibleStrainCol2, 
			_ => MathUtil.White, 
		};
	}

	public Color GetMineralCol(MineralType mineralType)
	{
		return mineralType switch
		{
			MineralType.Stone => BoulderCol, 
			MineralType.Flint => FlintCol, 
			MineralType.Lead => LeadOreCol, 
			MineralType.Iron => IronOreCol, 
			_ => MathUtil.White, 
		};
	}
}
