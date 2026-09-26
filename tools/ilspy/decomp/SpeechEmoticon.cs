public struct SpeechEmoticon : IReflectable
{
	public static string[] Emoticons = new string[23]
	{
		":-|", ":-)", ":-(", ":-[", "}:-|", "}:-)", "}:-(", "}:-[", "{:-|", "{:-)",
		"{:-(", "{:-[", "8-o", "8-|", "8-)", "8-(", "8-[", ":-p", "}8-0", "/:-|",
		"/:-)", "/:-(", "/:-["
	};

	public FacialExpression FacialExpression;

	public int Pos;

	public static SpeechEmoticon Create(FacialExpression facialExpression, int pos)
	{
		return new SpeechEmoticon
		{
			FacialExpression = facialExpression,
			Pos = pos
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref FacialExpression);
		reflector.Add(ref Pos);
	}
}
