public struct SpeechWithCachedPriority
{
	public Speech Speech;

	public float CachedPriority;

	public int BaseOrder;

	public Character CachedSpeaker;

	public BaseObject CachedObject;

	public MemoryParam CachedMemoryParam;

	public bool VisibleButDisabled;

	public bool Enabled
	{
		get
		{
			if (CachedPriority >= 0f)
			{
				return !VisibleButDisabled;
			}
			return false;
		}
	}

	public bool Visible
	{
		get
		{
			if (!(CachedPriority >= 0f))
			{
				return VisibleButDisabled;
			}
			return true;
		}
	}

	public SpeechWithCachedPriority(Speech speech, float priority, int baseOrder, Character speaker, BaseObject obj, MemoryParam param, bool visibleButDisabled)
	{
		Speech = speech;
		CachedPriority = priority;
		BaseOrder = baseOrder;
		CachedSpeaker = speaker;
		CachedObject = obj;
		CachedMemoryParam = param;
		VisibleButDisabled = visibleButDisabled;
	}
}
