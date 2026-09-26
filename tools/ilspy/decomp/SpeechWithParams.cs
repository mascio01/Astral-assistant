public struct SpeechWithParams : IReflectable
{
	public Speech Speech;

	public BaseObject Obj;

	public MemoryParam Params;

	public static SpeechWithParams Create(Speech speech, BaseObject obj, MemoryParam p)
	{
		return new SpeechWithParams
		{
			Speech = speech,
			Obj = obj,
			Params = p
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Speech);
		reflector.AddAfter(ref Obj, 5);
		if (reflector.Version < 144)
		{
			Character obj = null;
			int value = 0;
			reflector.AddAfter(ref obj, 143);
			reflector.Add(ref value);
			Params = new MemoryParam(obj, value);
		}
		else
		{
			Params.Reflect(reflector);
		}
	}

	public bool IsValid()
	{
		return Speech != null;
	}
}
