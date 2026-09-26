using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThermometerBehaviour : MonoBehaviour
{
	private float Temperature = Character.BodyTemperatureInCelsiusNormal;

	private const float MaxTemperature = 40f;

	private const float MinTemperature = 20f;

	private const float MercuryTopPixel = 21.333f;

	private const float MercuryBottomPixel = 554.667f;

	private const float HeightInPixels = 632f;

	private RawImage UnityMercury;

	private TextMeshProUGUI UnityTitle;

	private List<GameObject> UnityMarkers = new List<GameObject>();

	private int MarkerIndex;

	private const string MercuryStr = "Mercury";

	private const string TitleStr = "Title";

	public static int HUD_BodyTemperature = StringUtil.JenkinsHash("HUD_BodyTemperature");

	public static int HUD_BodyTemperatureF = StringUtil.JenkinsHash("HUD_BodyTemperatureF");

	private void Awake()
	{
	}

	private void AddMarker(float temperatureInCelcius)
	{
		bool useCelsius = GameImpl.Instance.Settings.UseCelsius;
		RectTransform rectTransform = (RectTransform)base.transform;
		float t = (temperatureInCelcius - 20f) / 20f;
		float num = Mathf.Lerp(554.667f, 21.333f, t) / 632f;
		GameObject gameObject;
		if (MarkerIndex < UnityMarkers.Count)
		{
			gameObject = UnityMarkers[MarkerIndex];
		}
		else
		{
			gameObject = Object.Instantiate(InfoScreen.TemperatureMarker.GetAsset(), base.transform, worldPositionStays: false);
			UnityMarkers.Add(gameObject);
		}
		((RectTransform)gameObject.transform).anchoredPosition = new Vector2(0f, rectTransform.rect.height * (1f - num));
		Color color = Color.white;
		if (temperatureInCelcius > Character.BodyTemperatureInCelsiusNormal)
		{
			color = Color.Lerp(Color.white, Color.red, (temperatureInCelcius - Character.BodyTemperatureInCelsiusNormal) / (Character.BodyTemperatureInCelsiusHyperpyrexia - Character.BodyTemperatureInCelsiusNormal));
		}
		else if (temperatureInCelcius < Character.BodyTemperatureInCelsiusNormal)
		{
			color = Color.Lerp(Color.white, Color.blue, (temperatureInCelcius - Character.BodyTemperatureInCelsiusNormal) / (Character.BodyTemperatureInCelsiusDeath - Character.BodyTemperatureInCelsiusNormal));
		}
		float num2 = (useCelsius ? temperatureInCelcius : MathUtil.CelsiusToFahrenheit(temperatureInCelcius));
		gameObject.GetComponent<RawImage>().color = color;
		TextMeshProUGUI component = gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		component.color = color;
		component.SetUnityText(num2.ToString("F0") + "°");
		MarkerIndex++;
	}

	public void Populate()
	{
		UnityMercury = base.transform.Find("Mercury").gameObject.GetComponent<RawImage>();
		UnityTitle = base.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>();
		bool useCelsius = GameImpl.Instance.Settings.UseCelsius;
		UnityTitle.SetUnityText(GameImpl.Translate(useCelsius ? HUD_BodyTemperature : HUD_BodyTemperatureF));
		MarkerIndex = 0;
		AddMarker(Character.BodyTemperatureInCelsiusHyperpyrexia);
		AddMarker(Character.BodyTemperatureInCelsiusFever);
		AddMarker(Character.BodyTemperatureInCelsiusNormal);
		AddMarker(Character.BodyTemperatureInCelsiusMildHypothermia);
		AddMarker(Character.BodyTemperatureInCelsiusModerateHypothermia);
		AddMarker(Character.BodyTemperatureInCelsiusUnconscious);
		AddMarker(Character.BodyTemperatureInCelsiusDeath);
	}

	private void Update()
	{
		float t = (Temperature - 20f) / 20f;
		float num = Mathf.Lerp(554.667f, 21.333f, t) / 632f;
		RectTransform rectTransform = (RectTransform)base.transform;
		UnityMercury.rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height * (1f - num));
		UnityMercury.uvRect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f - num));
	}

	public void SetTemperature(float temp)
	{
		Temperature = temp;
	}
}
