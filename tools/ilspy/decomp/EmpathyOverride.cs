public struct EmpathyOverride : IReflectable
{
	public BaseObject OverrideObject;

	public float OverrideValue;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref OverrideObject);
		reflector.Add(ref OverrideValue);
	}
}
