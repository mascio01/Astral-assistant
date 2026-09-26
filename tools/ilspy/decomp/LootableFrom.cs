using System.ComponentModel;

public class LootableFrom
{
	public string Name;

	[DefaultValue(LootScarcity.None)]
	public LootScarcity OverrideScarcity;
}
