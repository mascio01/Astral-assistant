public struct Variable : IReflectable
{
	public string UniqueID;

	public int SubjectID;

	public int ObjectID;

	public float Value;

	public static Variable Create(string uniqueID, BaseObject sub, BaseObject ob, float value)
	{
		return new Variable
		{
			UniqueID = uniqueID,
			SubjectID = (sub?.Id ?? 0),
			ObjectID = (ob?.Id ?? 0),
			Value = value
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref UniqueID);
		reflector.AddAfter(ref SubjectID, 194);
		reflector.AddAfter(ref ObjectID, 194);
		reflector.Add(ref Value);
	}
}
