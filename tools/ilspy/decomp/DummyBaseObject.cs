using System.Text;

public class DummyBaseObject : BaseObject
{
	public string Name;

	public GenderType Gender;

	public bool Plural;

	public bool Zero;

	public bool Swap;

	public DummyBaseObject(GenderType gender, bool plural)
	{
		Gender = gender;
		Plural = plural;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Invalid;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		return Gender;
	}

	public override bool IsPlural()
	{
		return Plural;
	}

	public override bool IsZero()
	{
		return Zero;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		sb.Append(Name);
	}
}
