using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ActionMenuBehaviour : MonoBehaviour
{
	private ScrollRect ScrollView;

	public RawImage Tail;

	public RectTransform HeaderContent;

	public RectTransform Content;

	private TextMeshProUGUI ButtonPrompt;

	public float MaxHeight = 200f;

	public float Border = 16f;

	public int SelectedAction;

	private StringBuilder sb = new StringBuilder();

	public GameObject GetHeaderContent()
	{
		return HeaderContent.gameObject;
	}

	public GameObject GetContent()
	{
		return Content.gameObject;
	}

	private void Awake()
	{
		ScrollView = base.transform.Find("Scroll View").GetComponent<ScrollRect>();
		HeaderContent = (RectTransform)base.transform.Find("Header/Viewport/Content");
		Content = (RectTransform)base.transform.Find("Scroll View/Viewport/Content");
		Tail = base.transform.Find("Tail").GetComponent<RawImage>();
		ButtonPrompt = base.transform.Find("ButtonPrompt").GetComponent<TextMeshProUGUI>();
	}

	public void Update()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (HeaderContent != null)
		{
			for (int i = 0; i < HeaderContent.transform.childCount; i++)
			{
				RectTransform rectTransform = (RectTransform)HeaderContent.transform.GetChild(i).gameObject.transform;
				num = Mathf.Max(num, rectTransform.rect.width);
				num2 += rectTransform.rect.height;
			}
		}
		for (int j = 0; j < Content.childCount; j++)
		{
			RectTransform rectTransform2 = (RectTransform)Content.GetChild(j).gameObject.transform;
			if (j == SelectedAction)
			{
				num4 = num3 + rectTransform2.rect.height * 0.5f;
			}
			num = Mathf.Max(num, rectTransform2.rect.width);
			num3 += rectTransform2.rect.height;
		}
		((RectTransform)base.transform).sizeDelta = new Vector2(num + Border * 2f, Mathf.Min(MaxHeight, num2 + num3 + Border * 2f));
		RectTransform obj = (RectTransform)ScrollView.transform;
		obj.offsetMax = new Vector2(obj.offsetMax.x, 0f - (Border + num2));
		Content.localPosition = new Vector3(0f, Mathf.Clamp(num4 - (MaxHeight - num2 - Border * 2f) * 0.5f, 0f, Mathf.Max(0f, num3 - (MaxHeight - num2 - Border * 2f))), 0f);
		if (InputFunctionManager.Instance != null)
		{
			sb.Length = 0;
			sb.AppendButtonPromptString(InputFunction.SelectAction);
			ButtonPrompt.gameObject.SetActive(Content.transform.childCount > 1);
			ButtonPrompt.SetUnityTextIfDifferent(sb);
		}
		else
		{
			ButtonPrompt.gameObject.SetActive(value: true);
		}
	}

	public void SetWidth(float w)
	{
		Content.sizeDelta = new Vector2(w, Content.sizeDelta.y);
		HeaderContent.sizeDelta = new Vector2(w, HeaderContent.sizeDelta.y);
	}
}
