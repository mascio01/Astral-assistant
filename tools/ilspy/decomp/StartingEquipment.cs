using System;

public class StartingEquipment
{
	public string Name;

	public int Amount = 1;

	public int Points = 1;

	public int GetAmount()
	{
		return Math.Max(1, Amount);
	}
}
