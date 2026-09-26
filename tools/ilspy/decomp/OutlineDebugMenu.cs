public class OutlineDebugMenu : DebugMenu
{
	private AntiAliasing GetAntiAliasingFromInt(int n)
	{
		return n switch
		{
			2 => AntiAliasing.x2, 
			4 => AntiAliasing.x4, 
			8 => AntiAliasing.x8, 
			_ => AntiAliasing.None, 
		};
	}

	private int GetIntFromAntiAliasing(AntiAliasing antiAliasing)
	{
		return antiAliasing switch
		{
			AntiAliasing.x2 => 2, 
			AntiAliasing.x4 => 4, 
			AntiAliasing.x8 => 8, 
			_ => 0, 
		};
	}

	public OutlineDebugMenu()
		: base(GameImpl.Translate("DEBUG_Outline"))
	{
		FogOfWarBehaviour fogOfWarBehaviour = HudBehaviour.Instance.UnityGameCameraBehaviour.FogOfWarBehaviour;
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_EdgeWidth"), 0f, 2f, () => fogOfWarBehaviour.EdgeWidth, delegate(float v)
		{
			fogOfWarBehaviour.EdgeWidth = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_EdgeIntensity"), 0f, 2f, () => fogOfWarBehaviour.EdgeIntensity, delegate(float v)
		{
			fogOfWarBehaviour.EdgeIntensity = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_NormalThreshold"), 0f, 6f, () => fogOfWarBehaviour.NormalThreshold, delegate(float v)
		{
			fogOfWarBehaviour.NormalThreshold = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DepthThreshold"), 0f, 0.0005f, () => fogOfWarBehaviour.DepthThreshold, delegate(float v)
		{
			fogOfWarBehaviour.DepthThreshold = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_NormalSensitivity"), 0f, 2f, () => fogOfWarBehaviour.NormalSensitivity, delegate(float v)
		{
			fogOfWarBehaviour.NormalSensitivity = v;
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_DepthSensitivity"), 0f, 200f, () => fogOfWarBehaviour.DepthSensitivity, delegate(float v)
		{
			fogOfWarBehaviour.DepthSensitivity = v;
		}));
	}
}
