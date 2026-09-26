using System.ComponentModel;
using System.Xml.Serialization;

public class RecipeExtraOutput
{
	[XmlIgnore]
	public EquipmentPrototype ProductPrototype;

	[DefaultValue("")]
	public string ProductPrototypeName = "";

	public int MinAmount = 1;

	public int MaxAmount = 1;

	public int Probability = 100;
}
