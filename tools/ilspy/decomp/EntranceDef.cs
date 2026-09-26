using System.Xml.Serialization;

public struct EntranceDef
{
	public TerrainCoord EntranceOffset;

	public float EntranceAngle;

	[XmlIgnore]
	public int NameHash;

	public string NativeName;

	public EntranceDef(TerrainCoord entranceOffset, float entranceAngle, string name)
	{
		EntranceOffset = entranceOffset;
		EntranceAngle = entranceAngle;
		NameHash = StringUtil.JenkinsHash(name);
		NativeName = null;
	}
}
