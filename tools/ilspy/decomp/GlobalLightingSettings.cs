using System;
using UnityEngine;

[Serializable]
public class GlobalLightingSettings
{
	public Color Ambient;

	public Color Diffuse;

	public Color Fill;

	public Color Back;

	public Color FogTop;

	public Color FogBottom;

	public GlobalLightingSettings(Color ambient, Color diffuse, Color fill, Color back, Color fogTop, Color fogBottom)
	{
		Ambient = ambient;
		Diffuse = diffuse;
		Fill = fill;
		Back = back;
		FogTop = fogTop;
		FogBottom = fogBottom;
	}

	public GlobalLightingSettings(GlobalLightingSettings other)
	{
		Ambient = other.Ambient;
		Diffuse = other.Diffuse;
		Fill = other.Fill;
		Back = other.Back;
		FogTop = other.FogTop;
		FogBottom = other.FogBottom;
	}
}
