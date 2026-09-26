using UnityEngine;

public class SunSettings : MonoBehaviour
{
	public GlobalLightingSettings DayLight;

	public GlobalLightingSettings DuskDawnLight;

	public GlobalLightingSettings SwitchOverLight;

	public GlobalLightingSettings MoonLight;

	public void SetToDefaults()
	{
		DayLight = new GlobalLightingSettings(MathUtil.Color255(64, 54, 74), MathUtil.Color255(210, 199, 157), MathUtil.Color255(70, 71, 89), MathUtil.Color255(128, 108, 148), MathUtil.Color255(192, 192, 192), MathUtil.Color255(111, 122, 133));
		DuskDawnLight = new GlobalLightingSettings(MathUtil.Color255(64, 54, 74), MathUtil.Color255(218, 137, 129), MathUtil.Color255(66, 56, 82), MathUtil.Color255(128, 108, 148), MathUtil.Color255(98, 79, 108), MathUtil.Color255(45, 39, 75));
		SwitchOverLight = new GlobalLightingSettings(MathUtil.Color255(90, 90, 116), MathUtil.Color255(122, 88, 126), MathUtil.Color255(51, 58, 96), MathUtil.Color255(55, 60, 77), MathUtil.Color255(61, 69, 90), MathUtil.Color255(24, 30, 46));
		MoonLight = new GlobalLightingSettings(MathUtil.Color255(30, 34, 40), MathUtil.Color255(70, 92, 134), MathUtil.Color255(74, 92, 96), MathUtil.Color255(61, 69, 90), MathUtil.Color255(22, 24, 35), MathUtil.Color255(8, 9, 16));
	}
}
