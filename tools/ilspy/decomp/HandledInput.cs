public struct HandledInput
{
	public InputFunction CapturedBy;

	public Captured Captured;

	public float Value;

	public float RawValue;

	public float LastValue;

	public bool Update(float value, float rawValue)
	{
		LastValue = Value;
		Value = value;
		RawValue = rawValue;
		switch (Captured)
		{
		case Captured.ThisFrame:
			Captured = Captured.None;
			CapturedBy = InputFunction.Invalid;
			break;
		case Captured.UntilReleased:
			if (Value == 0f)
			{
				Captured = Captured.None;
				CapturedBy = InputFunction.Invalid;
			}
			break;
		}
		return Value != 0f;
	}

	public void Capture(Captured captureType, InputFunction captureBy)
	{
		Captured = captureType;
		CapturedBy = captureBy;
	}

	public bool IsPressed(bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (Captured == Captured.None)
		{
			if (capture)
			{
				Captured = Captured.ThisFrame;
				CapturedBy = captureBy;
			}
			return Value != 0f;
		}
		return false;
	}

	public bool IsJustPressed(bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (Captured == Captured.None)
		{
			if (capture)
			{
				Captured = Captured.ThisFrame;
				CapturedBy = captureBy;
			}
			if (Value != 0f && LastValue == 0f)
			{
				if (capture)
				{
					Captured = Captured.UntilReleased;
					CapturedBy = captureBy;
				}
				return true;
			}
		}
		return false;
	}

	public bool IsJustReleased()
	{
		if (Captured == Captured.None && Value == 0f && LastValue != 0f)
		{
			return true;
		}
		return false;
	}

	public float GetAxis(bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (Captured == Captured.None)
		{
			if (capture)
			{
				Captured = Captured.ThisFrame;
				CapturedBy = captureBy;
			}
			return Value;
		}
		return 0f;
	}

	public float GetAxisRaw(bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (Captured == Captured.None)
		{
			if (capture)
			{
				Captured = Captured.ThisFrame;
				CapturedBy = captureBy;
			}
			return RawValue;
		}
		return 0f;
	}
}
