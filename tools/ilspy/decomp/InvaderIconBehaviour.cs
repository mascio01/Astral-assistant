using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InvaderIconBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public RawImage UnityIcon;

	public RawImage UnityBackground;

	private Material UnityIconMat;

	private Material UnityBackgroundMat;

	public InvaderInstance InvaderInstance;

	public Squad Squad;

	public float HoveredTransition;

	public bool Dirty;

	private static List<InfectionType> TempInfectionTypes = new List<InfectionType>();

	public static InvaderIconBehaviour CurrentHovered = null;

	public static GameObject CurrentHoveredGameObject;

	private void Awake()
	{
		UnityIcon = base.transform.Find("Background/Icon").gameObject.GetComponent<RawImage>();
		UnityIcon.material = new Material(UnityIcon.material);
		UnityBackground = base.transform.Find("Background").gameObject.GetComponent<RawImage>();
		UnityBackground.material = new Material(UnityBackground.material);
		Dirty = true;
	}

	public void Initialize(InvaderInstance invader, Squad squad)
	{
		InvaderInstance = invader;
		Squad = squad;
	}

	public void Populate()
	{
		Resource<Texture2D> resource = null;
		Color color = Color.white;
		if (InvaderInstance != null && InvaderInstance.Invader != null && InvaderInstance.Invader.IconResource != null && InvaderInstance.Invader.IconResource.GetAsset() != null)
		{
			resource = InvaderInstance.Invader.IconResource;
			UnityIcon.texture = (Texture2D)InvaderInstance.Invader.IconResource;
			Template template = GameImpl.Instance.FindTemplateByUniqueID(InvaderInstance.Invader.TemplateID);
			if (template != null)
			{
				TempInfectionTypes.Clear();
				template.GetInfectionTypes(TempInfectionTypes);
				if (TempInfectionTypes.Count == 1)
				{
					color = GameTerrain.MinimapSettings.GetInfectionCol(TempInfectionTypes[0]);
				}
			}
		}
		else if (Squad != null)
		{
			resource = MinimapCameraBehaviour.GetIconForSquadBehaviour(Squad.Behaviour);
		}
		if (resource != null && resource.GetAsset() != null)
		{
			UnityIcon.texture = (Texture2D)resource;
			UnityIcon.color = color;
			UnityIcon.SetMaterialDirty();
			UnityIconMat = UnityIcon.materialForRendering;
			UnityBackgroundMat = UnityBackground.materialForRendering;
		}
	}

	public void Update()
	{
		if (Dirty)
		{
			Populate();
			Dirty = false;
		}
		bool flag = Session.Instance != null && Session.Instance.IsDebugMenuOpen();
		HoveredTransition = Mathf.Clamp01(HoveredTransition + ((CurrentHovered == this && !flag) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f);
		if (UnityIcon != null)
		{
			UnityIcon.transform.localScale = Vector3.one * (1f + Mathf.SmoothStep(0f, 1f, HoveredTransition) * 0.25f);
		}
		if (UnityIconMat == null && UnityIcon != null)
		{
			UnityIconMat = UnityIcon.materialForRendering;
		}
		if (UnityBackgroundMat == null && UnityBackground != null)
		{
			UnityBackgroundMat = UnityBackground.materialForRendering;
		}
		if (UnityIconMat != null)
		{
			UnityIconMat.SetColor(ShaderHash._OutlineColor, Color.Lerp(Color.black, Color.red, HoveredTransition));
		}
		if (UnityBackgroundMat != null)
		{
			float value = ((InvaderInstance != null && InvaderInstance.Invader != null && InvaderInstance.Invader.MaxTimeoutDays > 0f) ? ((float)((Session.Instance.PlayTime - InvaderInstance.TriggeredTime).TotalSeconds / InvaderInstance.CalcTimeout().TotalSeconds)) : 0f);
			UnityBackgroundMat.SetFloat(ShaderHash._FillAmount, Mathf.Clamp01(value));
		}
		if (!Input.mousePresent)
		{
			if (CurrentHovered != this && InputFunctionManager.Instance.IsMouseOverPanel((RectTransform)base.transform))
			{
				OnPointerEnter(null);
			}
			else if (CurrentHovered == this && !InputFunctionManager.Instance.IsMouseOverPanel((RectTransform)base.transform))
			{
				OnPointerExit(null);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SoundManager.PlayMenuSound(SoundManager.HoverSound);
		CurrentHovered = this;
		CurrentHoveredGameObject = base.gameObject;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (CurrentHovered == this)
		{
			CurrentHovered = null;
			CurrentHoveredGameObject = null;
		}
	}
}
