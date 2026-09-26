using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapPage : InfoPage
{
	public struct InvaderIcon : IComparable<InvaderIcon>
	{
		public InvaderInstance InvaderInstance;

		public Squad Squad;

		public TimeSpan CreatedTime;

		public int Index;

		public int CompareTo(InvaderIcon other)
		{
			if (CreatedTime > other.CreatedTime)
			{
				return -1;
			}
			if (CreatedTime < other.CreatedTime)
			{
				return 1;
			}
			if (Index > other.Index)
			{
				return -1;
			}
			if (Index < other.Index)
			{
				return 1;
			}
			return 0;
		}
	}

	public static MapPage Instance;

	public float MapCamAspect;

	public float MaxHeight;

	public TileObject HoveredObj;

	private RenderTexture MapRenderTexture;

	private RawImage MapPanelImage;

	private MinimapCameraBehaviour MinimapCameraBehaviour;

	private GameObject UnityGeologicalMapButtonsPanel;

	private GameObject UnityGeologicalMapButtons;

	private TextMeshProUGUI UnityGeologicalMapText;

	private List<CloseIconBehaviour> UnityMineralButton = new List<CloseIconBehaviour>();

	private GameObject UnityMarkerButtonsPanel;

	private GameObject UnityMarkerButtons;

	private TextMeshProUGUI UnityMarkerButtonsText;

	private List<CloseIconBehaviour> UnityMarkerButton = new List<CloseIconBehaviour>();

	private ScrollRect UnityInvadersPanel;

	public GameObject UnityInvadersObj;

	private bool HasGeologicalMap;

	private bool MapMarkersEnabled;

	private static float ScrollSpeed = 2f;

	private static float SidebarScrollSpeed = 400f;

	private static float ZoomSpeed = 4f;

	private static float HoverRangeFactor = 0.2f;

	public static float MinHeight = 16f;

	private static float SidebarWidthInWorld = 0.175f;

	public Vector2 MoveXZ;

	public float Zoom;

	public Vector2 LastMousePos;

	public Vector2 CursorPosWorldXZ;

	public Vector2 LastCursorPosWorldXZ;

	public bool WasUsingButtons;

	public bool Clicking;

	public bool Dragging;

	public float ClickTime;

	public bool AltClicking;

	public bool AltDragging;

	public float AltClickTime;

	public Vector2 AltDragStartPosWorldXZ;

	public bool IsMouseOverMap;

	public bool AltMenuOpened;

	public Character HoveredCharacter;

	public static float QuickClickTime = 0.5f;

	public static int INPUT_AddToSelection = StringUtil.JenkinsHash("INPUT_AddToSelection");

	private int ShowSelectMineral;

	private int ShowSelectMarkerType;

	private static StringBuilder sb = new StringBuilder(50);

	public MineralType GeologicalMap = MineralType.None;

	public MapMarkerType MarkerType;

	public Vector3 CamPosition;

	private static List<TileObject> ObjectsInArea = new List<TileObject>();

	private static int PROMPT_MarkerType = StringUtil.JenkinsHash("PROMPT_MarkerType");

	private List<InvaderIcon> InvaderIcons = new List<InvaderIcon>();

	private static int INFOPAGE_Map = StringUtil.JenkinsHash("INFOPAGE_Map");

	public override void OnAwake()
	{
		Instance = this;
		MapPanelImage = base.transform.Find("MapView").GetComponent<RawImage>();
		UnityGeologicalMapButtonsPanel = base.transform.Find("MapView/GeologicalMapButtons").gameObject;
		UnityGeologicalMapButtons = base.transform.Find("MapView/GeologicalMapButtons/Buttons").gameObject;
		UnityGeologicalMapText = base.transform.Find("MapView/GeologicalMapButtons/Text").GetComponent<TextMeshProUGUI>();
		UnityMarkerButtonsPanel = base.transform.Find("MapView/MarkerButtons").gameObject;
		UnityMarkerButtons = base.transform.Find("MapView/MarkerButtons/Buttons").gameObject;
		UnityMarkerButtonsText = base.transform.Find("MapView/MarkerButtons/Text").GetComponent<TextMeshProUGUI>();
		UnityInvadersPanel = base.gameObject.FindChild("Invaders").GetComponent<ScrollRect>();
		UnityInvadersObj = base.gameObject.FindChild("Invaders/Viewport/Content");
	}

	public MapPage Initialize()
	{
		return this;
	}

	public void OnStart()
	{
		MaxHeight = GameTerrain.Instance.Size / 2;
		Camera unityMapCamera = HudBehaviour.Instance.UnityMapCamera;
		unityMapCamera.farClipPlane = GameTerrain.Instance.Size * 2;
		unityMapCamera.transform.position = new Vector3(0f, MaxHeight, 0f);
		unityMapCamera.transform.LookAt(Vector3.zero, Vector3.back);
		MinimapCameraBehaviour = unityMapCamera.GetComponent<MinimapCameraBehaviour>();
		if (CamPosition.y > 0f)
		{
			unityMapCamera.transform.position = CamPosition;
		}
		else
		{
			CamPosition = unityMapCamera.transform.position;
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		HudBehaviour.Instance.UnityMapCameraObj.SetActive(value: true);
		HasGeologicalMap = !FogOfWar.DebugFogOfWarEnabled || Session.Instance.CommunityManager.PlayerCommunity.HasAnyInventoryItemsOfClass(typeof(GeologicalMap));
		MapMarkersEnabled = Session.Instance.FollowerCommandsEnabled;
		UnityGeologicalMapButtonsPanel.SetActive(HasGeologicalMap);
		UnityGeologicalMapButtons.DeleteAllChildren();
		UnityMineralButton.Clear();
		if (HasGeologicalMap)
		{
			for (int i = -1; i < 4; i++)
			{
				MineralType mineralType = (MineralType)i;
				EquipmentPrototype equipmentPrototype = ((mineralType == MineralType.None) ? EquipmentPrototype.WorldMap : EquipmentPrototype.MiningResources[i]);
				if (equipmentPrototype != null && equipmentPrototype.Tex != null)
				{
					RawImage component = UnityEngine.Object.Instantiate((GameObject)InfoScreen.MineralButtonPrefab, UnityGeologicalMapButtons.transform).GetComponent<RawImage>();
					component.transform.GetChild(0).GetComponent<RawImage>().texture = equipmentPrototype.Tex.GetAsset();
					component.color = GameTerrain.MinimapSettings.GetMineralCol(mineralType);
					CloseIconBehaviour component2 = component.GetComponent<CloseIconBehaviour>();
					component2.OnClicked.AddListener(delegate
					{
						GeologicalMap = mineralType;
					});
					UnityMineralButton.Add(component2);
				}
			}
		}
		UnityMarkerButtonsPanel.SetActive(MapMarkersEnabled);
		UnityMarkerButtons.DeleteAllChildren();
		UnityMarkerButton.Clear();
		if (MapMarkersEnabled)
		{
			for (int num = 0; num < 5; num++)
			{
				MapMarkerType markerType = (MapMarkerType)num;
				RawImage component3 = UnityEngine.Object.Instantiate((GameObject)InfoScreen.MineralButtonPrefab, UnityMarkerButtons.transform).GetComponent<RawImage>();
				component3.transform.GetChild(0).GetComponent<RawImage>().texture = (Texture2D)GameCursor.CurrentQuestIcon;
				component3.transform.GetChild(0).GetComponent<RawImage>().color = GetMapMarkerColor(markerType);
				CloseIconBehaviour component4 = component3.GetComponent<CloseIconBehaviour>();
				component4.OnClicked.AddListener(delegate
				{
					MarkerType = markerType;
				});
				UnityMarkerButton.Add(component4);
			}
		}
		HintManager.Instance.Hints[32].MarkPerformed();
	}

	public override void OnDeactivate()
	{
		HudBehaviour.Instance.UnityMapCameraObj.SetActive(value: false);
		Clicking = false;
		Dragging = false;
		AltClicking = false;
		AltDragging = false;
		AltMenuOpened = false;
		base.OnDeactivate();
	}

	public static Color GetMapMarkerColor(MapMarkerType markerType)
	{
		return markerType switch
		{
			MapMarkerType.Red => Color.red, 
			MapMarkerType.Blue => GameTerrain.MinimapSettings.AllyCol, 
			MapMarkerType.Yellow => GameTerrain.MinimapSettings.NeutralCol, 
			MapMarkerType.Green => GameTerrain.MinimapSettings.FriendCol, 
			MapMarkerType.Gray => GameTerrain.MinimapSettings.DeadCol, 
			_ => Color.white, 
		};
	}

	public Vector2 CalcWorldPosXZFromCursorPos(Vector2 mousePos)
	{
		Camera unityMapCamera = HudBehaviour.Instance.UnityMapCamera;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MapPanelImage.rectTransform, mousePos, null, out var localPoint);
		Vector3 position = MathUtil.ToXYZ(localPoint / MapPanelImage.rectTransform.rect.size, unityMapCamera.transform.position.y);
		return MathUtil.ToXZ(unityMapCamera.ViewportToWorldPoint(position));
	}

	public Vector2 CalcCursorPosOnScreen()
	{
		Vector3 vector = HudBehaviour.Instance.UnityMapCamera.WorldToViewportPoint(MathUtil.ToX0Y(CursorPosWorldXZ));
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(HudBehaviour.Instance.HudPanelRectTransform, MapPanelImage.rectTransform);
		return HudBehaviour.Instance.Centre + new Vector2(Mathf.LerpUnclamped(bounds.min.x, bounds.max.x, vector.x), Mathf.LerpUnclamped(bounds.min.y, bounds.max.y, vector.y));
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		Hud instance3 = Hud.Instance;
		if (IsMouseOverMap && !AltMenuOpened && !AltDragging && !instance3.DraggingZone && !Dragging && instance.FollowerCommandsEnabled)
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.MainAction, ButtonPromptBarBehaviour.PROMPT_GoToMapLocation);
		}
		MoveXZ = new Vector2(instance2.GetAxis(InputFunction.MoveHoriz), instance2.GetAxis(InputFunction.MoveVert));
		Zoom = instance2.GetAxis(InputFunction.MapZoom);
		if (MoveXZ.sqrMagnitude > 0f || Mathf.Abs(Zoom) != 0f)
		{
			SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
		}
		IsMouseOverMap = instance2.IsMouseOverPanel(MapPanelImage.rectTransform) && !instance2.IsMouseOverPanel((RectTransform)UnityGeologicalMapButtonsPanel.transform) && !instance2.IsMouseOverPanel((RectTransform)UnityMarkerButtonsPanel.transform);
		if ((IsMouseOverMap || instance3.DraggingZone || AltDragging || Dragging) && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor)
		{
			LastCursorPosWorldXZ = CalcWorldPosXZFromCursorPos(LastMousePos);
			CursorPosWorldXZ = CalcWorldPosXZFromCursorPos(instance2.GetMousePosition());
		}
		else if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
		{
			if (!WasUsingButtons)
			{
				Camera unityMapCamera = HudBehaviour.Instance.UnityMapCamera;
				Vector3 position = new Vector3(0.5f, 0.5f, CamPosition.y);
				CursorPosWorldXZ = MathUtil.ToXZ(unityMapCamera.ViewportToWorldPoint(position));
			}
			WasUsingButtons = true;
			if (instance2.IsMouseOverPanel((RectTransform)UnityInvadersPanel.transform))
			{
				float height = ((RectTransform)UnityInvadersPanel.transform).rect.height;
				float height2 = ((RectTransform)UnityInvadersPanel.content.transform).rect.height;
				float y = MoveXZ.y;
				float num = Mathf.Max(Mathf.Min(UnityInvadersPanel.content.anchoredPosition.y - y * GameImpl.UnscaledDeltaTime * SidebarScrollSpeed, height2 - height), 0f);
				if (num != UnityInvadersPanel.content.anchoredPosition.y)
				{
					MoveXZ.y = 0f;
				}
				UnityInvadersPanel.content.anchoredPosition = new Vector2(UnityInvadersPanel.content.anchoredPosition.x, num);
			}
		}
		HoveredCharacter = null;
		if (instance3.DraggingZone || Dragging || AltDragging)
		{
			return;
		}
		HoveredCharacter = HoveredObj as Character;
		if (HoveredObj is Building)
		{
			AvailableAction availableAction = instance3.Cursor.GetAvailableAction();
			if (availableAction.ActionType == CursorAction.CharacterNameAndIcon)
			{
				HoveredCharacter = availableAction.Actor;
			}
		}
		if (HoveredCharacter != null && HoveredCharacter.AliveAndNotZombie && HoveredCharacter.GetBaseObjectType() == BaseObjectType.Human && instance2.IsMapped(InputFunction.MapBrainScan) && instance2.IsJustPressed(InputFunction.MapBrainScan))
		{
			TileObject currentObject = InfoScreen.Instance.CurrentObject;
			InfoScreen.Instance.OnDeactivate();
			InfoScreen.Instance.ActivateBrainScan(HoveredCharacter, HoveredCharacter.BrainScanned, currentObject);
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		Session instance = Session.Instance;
		_ = StoryManager.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		GameTerrain instance3 = GameTerrain.Instance;
		Hud instance4 = Hud.Instance;
		Vector3 camPosition = CamPosition;
		camPosition.y -= Zoom * GameImpl.UnscaledDeltaTime * ZoomSpeed * CamPosition.y;
		if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor && (IsMouseOverMap || instance4.DraggingZone))
		{
			camPosition.y -= instance2.GetMouseAxis(MouseAxis.ScrollWheel) * GameImpl.UnscaledDeltaTime * ZoomSpeed * CamPosition.y;
			WasUsingButtons = false;
			if (!AltDragging)
			{
				if (Clicking)
				{
					if (instance2.IsPressed(InputFunction.MainAction))
					{
						ClickTime += GameImpl.UnscaledDeltaTime;
						Dragging |= LastMousePos != instance2.GetMousePosition();
						camPosition += MathUtil.ToX0Y(LastCursorPosWorldXZ - CursorPosWorldXZ);
					}
					else
					{
						if (!Dragging && ClickTime < QuickClickTime && instance.FollowerCommandsEnabled)
						{
							OnMapClicked(inputFrame);
						}
						Clicking = false;
						Dragging = false;
					}
				}
				else if (instance2.IsJustPressed(InputFunction.MainAction, capture: false))
				{
					instance2.Capture(InputFunction.MainAction, untilReleased: false);
					Clicking = true;
					ClickTime = 0f;
					Dragging = false;
				}
			}
			PickHoveredObject(CursorPosWorldXZ);
		}
		else if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
		{
			float num = SidebarWidthInWorld * CamPosition.y;
			float num2 = MaxHeight / MapCamAspect;
			float t = ((MaxHeight > num2) ? ((CamPosition.y - num2) / (MaxHeight - num2)) : 0f);
			float num3 = Mathf.Lerp(0f, Math.Max(0f, MapCamAspect - 1f), t) * instance3.HalfSize;
			CursorPosWorldXZ -= MoveXZ * GameImpl.UnscaledDeltaTime * ScrollSpeed * CamPosition.y;
			CursorPosWorldXZ.x = Mathf.Clamp(CursorPosWorldXZ.x, 0f - instance3.HalfSize - num3, instance3.HalfSize + num + num3);
			CursorPosWorldXZ.y = Mathf.Clamp(CursorPosWorldXZ.y, 0f - instance3.HalfSize, instance3.HalfSize);
			camPosition.x = CursorPosWorldXZ.x;
			camPosition.z = CursorPosWorldXZ.y;
			PickHoveredObject(CursorPosWorldXZ);
			if (!AltDragging)
			{
				if ((IsMouseOverMap || instance4.DraggingZone) && instance2.IsJustPressed(InputFunction.MainAction))
				{
					OnMapClicked(inputFrame);
				}
				else if (CloseIconBehaviour.Hovered != null && instance2.IsJustPressed(InputFunction.MainAction))
				{
					CloseIconBehaviour.Hovered.OnClicked.Invoke();
				}
			}
		}
		else
		{
			HoveredObj = null;
		}
		if (HoveredCharacter != null && HoveredCharacter.AliveAndNotZombie && HoveredCharacter.GetBaseObjectType() == BaseObjectType.Human)
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.MapBrainScan, HoveredCharacter.BrainScanned ? BrainScanPage.INFOPAGE_BrainScan : BrainScanPage.INFOPAGE_CharacterNotes);
		}
		if (!AltDragging && !instance4.DraggingZone && instance2.IsMapped(InputFunction.MapFindPlayer))
		{
			Character localPlayerCharacter = instance.GetLocalPlayerCharacter();
			if (localPlayerCharacter != null && instance2.IsJustPressed(InputFunction.MapFindPlayer, capture: true, ButtonPromptBarBehaviour.PROMPT_FindPlayer))
			{
				camPosition.y = Math.Min(64f, MaxHeight / 2f);
				camPosition.x = (CursorPosWorldXZ.x = localPlayerCharacter.Pos.x);
				camPosition.z = (CursorPosWorldXZ.y = localPlayerCharacter.Pos.z);
			}
		}
		float num4 = MaxHeight - camPosition.y;
		float num5 = Mathf.Max(0f, MaxHeight - camPosition.y * MapCamAspect);
		camPosition.x = Mathf.Clamp(camPosition.x, 0f - num5, num5);
		camPosition.z = Mathf.Clamp(camPosition.z, 0f - num4, num4);
		camPosition.y = Mathf.Clamp(camPosition.y, MinHeight, MaxHeight);
		CamPosition = camPosition;
		MinimapCameraBehaviour minimapCameraBehaviour = MinimapCameraBehaviour;
		minimapCameraBehaviour.HoveredObj = HoveredObj;
		minimapCameraBehaviour.ShowMineralType = GeologicalMap;
		minimapCameraBehaviour.CursorPosWorldXZ = CursorPosWorldXZ;
		LastMousePos = instance2.GetMousePosition();
		int num6 = IsInRangeOfMarker();
		if (IsMouseOverMap && !AltDragging && !instance4.DraggingZone && instance.FollowerCommandsEnabled && instance2.IsMapped(InputFunction.MapMarker))
		{
			if (instance2.IsJustPressed(InputFunction.MapMarker, capture: true, (num6 != -1) ? ButtonPromptBarBehaviour.PROMPT_ClearMapMarker : ButtonPromptBarBehaviour.PROMPT_SetMapMarker) && inputFrame != null)
			{
				TerrainCoord tileCoordForPosXZ = instance3.GetTileCoordForPosXZ(CursorPosWorldXZ);
				if (num6 == -1)
				{
					inputFrame.AddAction(InputAction.SetMapMarkerTile(MarkerType, tileCoordForPosXZ));
				}
				else
				{
					PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
					inputFrame.AddAction(InputAction.ClearMapMarkerTile(localPlayerRecord.MapMarkerLocations[num6].Type, localPlayerRecord.MapMarkerLocations[num6].Tile));
				}
			}
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.MapZoom, ButtonPromptBarBehaviour.PROMPT_Zoom);
		}
		if ((IsMouseOverMap || AltDragging) && !instance4.DraggingZone && instance.FollowerCommandsEnabled)
		{
			if (AltClicking)
			{
				if (instance2.IsPressed(InputFunction.AltAction))
				{
					bool addToSelection = instance2.IsPressed(InputFunction.AddToSelection, capture: true, INPUT_AddToSelection);
					AltClickTime += GameImpl.UnscaledDeltaTime;
					AltDragging |= (CursorPosWorldXZ - AltDragStartPosWorldXZ).sqrMagnitude >= 1f;
					if (AltDragging)
					{
						instance4.DragSelectCharactersInBox(AltDragStartPosWorldXZ, CursorPosWorldXZ, addToSelection, inputFrame);
					}
				}
				else
				{
					if (!AltDragging && AltClickTime < QuickClickTime)
					{
						AltMenuOpened = !AltMenuOpened;
						InfoScreen.Instance.HideActionMenu = false;
						if (AltMenuOpened)
						{
							SoundManager.PlayMenuSound(SoundManager.DialogOpenSound);
						}
						else
						{
							SoundManager.PlayMenuSound(SoundManager.CancelSound);
						}
					}
					AltClicking = false;
					AltDragging = false;
				}
			}
			else if (instance2.IsJustPressed(InputFunction.AltAction, capture: false))
			{
				instance2.Capture(InputFunction.AltAction, untilReleased: false);
				AltClicking = true;
				AltClickTime = 0f;
				AltDragging = false;
				AltDragStartPosWorldXZ = CursorPosWorldXZ;
			}
			if (!AltDragging)
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.AltAction, ButtonPromptBarBehaviour.PROMPT_Actions);
			}
		}
		if (HasGeologicalMap)
		{
			ShowSelectMineral = ((!instance2.IsCaptured(InputFunction.MapSelectMineral)) ? 1 : ((ShowSelectMineral == 2) ? 2 : 0));
			float justPressedAxis = instance2.GetJustPressedAxis(InputFunction.MapSelectMineral);
			if (justPressedAxis != 0f)
			{
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				GeologicalMap = (MineralType)((int)(GeologicalMap + 1 + 5 + (int)justPressedAxis) % 5 - 1);
				ShowSelectMineral = 2;
			}
		}
		if (MapMarkersEnabled)
		{
			ShowSelectMarkerType = ((!instance2.IsCaptured(InputFunction.MapSelectMarkerType)) ? 1 : ((ShowSelectMarkerType == 2) ? 2 : 0));
			float justPressedAxis2 = instance2.GetJustPressedAxis(InputFunction.MapSelectMarkerType);
			if (justPressedAxis2 != 0f)
			{
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				MarkerType = (MapMarkerType)((int)(MarkerType + 5 + (int)justPressedAxis2) % 5);
				ShowSelectMarkerType = 2;
			}
		}
	}

	public int IsInRangeOfMarker()
	{
		int result = -1;
		float num = MathUtil.Squared(CamPosition.y * MinimapCameraBehaviour.CursorSize);
		PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
		if (localPlayerRecord != null)
		{
			for (int i = 0; i < localPlayerRecord.MapMarkerLocations.Count; i++)
			{
				float sqrMagnitude = (GameTerrain.Instance.GetTileCentreXZ(localPlayerRecord.MapMarkerLocations[i].Tile) - CursorPosWorldXZ).sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					num = sqrMagnitude;
					result = i;
				}
			}
		}
		return result;
	}

	private void OnMapClicked(InputFrame inputFrame)
	{
		Hud instance = Hud.Instance;
		if (AltMenuOpened || instance.DraggingZone)
		{
			if (inputFrame != null)
			{
				if (instance.Cursor.IsCursorActionEnabled())
				{
					SoundManager.PlayMenuSound(SoundManager.InGameClickSound);
					bool captureAltAction = false;
					instance.OnSelectedAction(inputFrame, instance.Cursor.GetAvailableAction(), isDoubleClick: false, transferAll: false, ref captureAltAction);
					AltMenuOpened = false;
				}
				else
				{
					instance.OnSelectedDisabledAction(instance.Cursor.GetAvailableAction());
				}
			}
		}
		else
		{
			GoToPointOnMap(CursorPosWorldXZ, inputFrame);
		}
	}

	public void GoToQuestDestination()
	{
		if (Hud.Instance.LocalControlledCharacter == null)
		{
			return;
		}
		Vector2 vector2;
		Vector2 vector = (vector2 = Hud.Instance.LocalControlledCharacter.PosXZ);
		foreach (TileObject questDestination in StoryManager.Instance.QuestDestinations)
		{
			Vector2 posXZ = questDestination.PosXZ;
			vector2 = Vector2.Min(vector2, posXZ);
			vector = Vector2.Max(vector, posXZ);
		}
		Vector2 vector3 = (vector2 + vector) / 2f;
		Vector2 vector4 = (vector - vector2) / 2f;
		Vector3 camPosition = default(Vector3);
		camPosition.x = (CursorPosWorldXZ.x = vector3.x);
		camPosition.z = (CursorPosWorldXZ.y = vector3.y);
		camPosition.y = Math.Max(Math.Min(64f, MaxHeight / 2f), Math.Max(vector4.x, vector4.y) * 1.2f);
		float num = MaxHeight - camPosition.y;
		float num2 = Mathf.Max(0f, MaxHeight - camPosition.y * MapCamAspect);
		camPosition.x = Mathf.Clamp(camPosition.x, 0f - num2, num2);
		camPosition.z = Mathf.Clamp(camPosition.z, 0f - num, num);
		camPosition.y = Mathf.Clamp(camPosition.y, MinHeight, MaxHeight);
		CamPosition = camPosition;
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 173)
		{
			reflector.AddAfter(ref CamPosition, 168);
			if (reflector.IsDeserialising && reflector.Version < 168)
			{
				CamPosition = new Vector3(0f, GameTerrain.Instance.Size / 2, 0f);
			}
		}
	}

	private void GoToPointOnMap(Vector2 cursorPosWorldXZ, InputFrame inputFrame)
	{
		Session.Instance.GameCamera.SetFlyCamMode(on: true, snap: true, inputFrame);
		Session.Instance.GameCamera.Teleport(GameTerrain.Instance.ClampPosToSurface(MathUtil.ToX0Y(cursorPosWorldXZ)));
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		InfoScreen.Instance.WantClose = true;
	}

	private void PickHoveredObject(Vector2 cursorPosWorldXZ)
	{
		GameTerrain instance = GameTerrain.Instance;
		_ = HudBehaviour.Instance.UnityMapCamera;
		TileObject tileObject = null;
		if (IsMouseOverMap)
		{
			float num = CamPosition.y * HoverRangeFactor;
			float num2 = num * num;
			TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(cursorPosWorldXZ);
			instance.GetObjectsInRect(tileCoordForPosXZ - new TerrainCoord((int)num, (int)num), tileCoordForPosXZ + new TerrainCoord((int)num, (int)num), ObjectsInArea);
			foreach (TileObject item in ObjectsInArea)
			{
				if (item == null || item.GetUnderConstructionInfo() != null || (item.GetMaxInventoryWeight() == 0f && (item.GetMiningResourceType() == null || item.GetMiningResourceType() == EquipmentPrototype.Stone)))
				{
					continue;
				}
				Character character = item as Character;
				Building building = item as Building;
				if (character != null)
				{
					if (character.GetBaseObjectType() != BaseObjectType.Human || character.MapColor.a == 0)
					{
						continue;
					}
				}
				else
				{
					bool flag = false;
					if (building != null)
					{
						for (int i = 0; i < building.Inhabitants.Length; i++)
						{
							if (building.Inhabitants[i] != null && building.Inhabitants[i].MapColor.a != 0)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag && FogOfWar.DebugFogOfWarEnabled && !instance.FogOfWar.IsAnyTileInRectCornersExplored(item.GetMinTile(), item.GetMaxTile()))
					{
						continue;
					}
				}
				Vector2 vector = character?.PosXZ ?? instance.GetTileCentreXZ(item.GetNearestTileTo(tileCoordForPosXZ));
				float sqrMagnitude = (cursorPosWorldXZ - vector).sqrMagnitude;
				if (!(sqrMagnitude >= num2) && (building == null || building.GetInhabitantSlotDefs().Length == 0 || !building.GetInhabitantSlotDefs()[0].External || building.Inhabitants[0] == null))
				{
					tileObject = item;
					num2 = sqrMagnitude;
				}
			}
			ObjectsInArea.Clear();
			if (tileObject == null)
			{
				tileObject = Session.Instance.CommunityManager.GetVisitedTownForTile(tileCoordForPosXZ);
			}
		}
		HoveredObj = tileObject;
	}

	public override void Update()
	{
		base.Update();
		if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
		{
			Vector2 cursorPos = CalcCursorPosOnScreen() - HudBehaviour.Instance.Centre;
			InputFunctionManager.Instance.SetCursorPos(cursorPos);
		}
		if (HasGeologicalMap)
		{
			for (int i = -1; i < 4; i++)
			{
				bool flag = GeologicalMap == (MineralType)i;
				UnityMineralButton[i + 1].SetSelected(flag);
				if (flag)
				{
					EquipmentPrototype equipmentPrototype = ((GeologicalMap == MineralType.None) ? EquipmentPrototype.WorldMap : EquipmentPrototype.MiningResources[i]);
					sb.Length = 0;
					sb.Append(GameImpl.Translate(equipmentPrototype.NameHash));
					sb.Append(' ');
					sb.AppendButtonPromptString(InputFunction.MapSelectMineral);
					UnityGeologicalMapText.SetUnityTextIfDifferent(sb);
				}
			}
			UnityGeologicalMapText.alpha = ((ShowSelectMineral > 0) ? 1f : 0.5f);
		}
		if (MapMarkersEnabled)
		{
			PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
			for (int j = 0; j < 5; j++)
			{
				bool flag2 = MarkerType == (MapMarkerType)j;
				UnityMarkerButton[j].SetSelected(flag2);
				if (flag2)
				{
					sb.Length = 0;
					sb.Append(GameImpl.Translate(PROMPT_MarkerType));
					sb.Append(' ');
					sb.AppendButtonPromptString(InputFunction.MapSelectMarkerType);
					UnityMarkerButtonsText.SetUnityTextIfDifferent(sb);
				}
				UnityMarkerButton[j].UnityRawImage.color = ((localPlayerRecord != null && localPlayerRecord.IsMapMarkerTypeSet((MapMarkerType)j)) ? SkillDisplayBehaviour.StarColActive : Color.white);
			}
			UnityMarkerButtonsText.alpha = ((ShowSelectMarkerType > 0) ? 1f : 0.5f);
		}
		Camera unityMapCamera = HudBehaviour.Instance.UnityMapCamera;
		unityMapCamera.transform.position = CamPosition;
		int num = (int)MapPanelImage.rectTransform.rect.width;
		int num2 = (int)MapPanelImage.rectTransform.rect.height;
		if (MapRenderTexture != null && (MapRenderTexture.width != num || MapRenderTexture.height != num2))
		{
			MapRenderTexture.Release();
		}
		if (MapRenderTexture == null)
		{
			MapRenderTexture = new RenderTexture(num, num2, 24, RenderTextureFormat.ARGB32);
			MapRenderTexture.Create();
			MapPanelImage.texture = MapRenderTexture;
			unityMapCamera.targetTexture = MapRenderTexture;
			unityMapCamera.fieldOfView = 90f;
		}
		unityMapCamera.aspect = (float)num / (float)num2;
		MapCamAspect = unityMapCamera.aspect;
	}

	public override void Populate()
	{
		base.Populate();
		InvaderIcons.Clear();
		for (int i = 0; i < StoryManager.Instance.ActiveInvaders.Count; i++)
		{
			InvaderInstance invaderInstance = StoryManager.Instance.ActiveInvaders[i];
			if (invaderInstance.ShowOnMapSidebar())
			{
				InvaderIcon item = new InvaderIcon
				{
					InvaderInstance = invaderInstance,
					CreatedTime = invaderInstance.TriggeredTime,
					Index = i
				};
				InvaderIcons.Add(item);
			}
		}
		for (int j = 0; j < Session.Instance.CommunityManager.SquadsWithMapIcons.Count; j++)
		{
			InvaderIcon item2 = default(InvaderIcon);
			item2.Squad = Session.Instance.CommunityManager.SquadsWithMapIcons[j];
			item2.CreatedTime = item2.Squad.SquadCreatedTime;
			item2.Index = j;
			InvaderIcons.Add(item2);
		}
		InvaderIcons.Sort();
		for (int k = 0; k < InvaderIcons.Count; k++)
		{
			InvaderInstance invaderInstance2 = InvaderIcons[k].InvaderInstance;
			Squad squad = InvaderIcons[k].Squad;
			InvaderIconBehaviour invaderIconBehaviour = FindInvaderIconBehaviour(invaderInstance2, squad);
			if (invaderIconBehaviour == null)
			{
				invaderIconBehaviour = UnityEngine.Object.Instantiate(InfoScreen.InvaderIcon.GetAsset(), UnityInvadersObj.transform).GetComponent<InvaderIconBehaviour>();
				invaderIconBehaviour.Initialize(invaderInstance2, squad);
			}
			invaderIconBehaviour.transform.SetSiblingIndex(k);
			invaderIconBehaviour.Update();
		}
		for (int num = UnityInvadersObj.transform.childCount - 1; num >= 0; num--)
		{
			InvaderIconBehaviour component = UnityInvadersObj.transform.GetChild(num).GetComponent<InvaderIconBehaviour>();
			if ((component.InvaderInstance != null) ? (!StoryManager.Instance.ActiveInvaders.Contains(component.InvaderInstance)) : (!Session.Instance.CommunityManager.SquadsWithMapIcons.Contains(component.Squad)))
			{
				UnityEngine.Object.DestroyImmediate(component.gameObject);
			}
		}
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Map));
	}

	public InvaderIconBehaviour FindInvaderIconBehaviour(InvaderInstance invaderInstance, Squad squad)
	{
		for (int i = 0; i < UnityInvadersObj.transform.childCount; i++)
		{
			InvaderIconBehaviour component = UnityInvadersObj.transform.GetChild(i).GetComponent<InvaderIconBehaviour>();
			if (component != null && component.InvaderInstance == invaderInstance && component.Squad == squad)
			{
				return component;
			}
		}
		return null;
	}

	public override bool CanToggleActionMenuVisible()
	{
		if (Hud.Instance.DraggingZone || AltDragging)
		{
			return false;
		}
		if (HoveredObj == null && !AltMenuOpened && !(InvaderIconBehaviour.CurrentHovered != null))
		{
			return InfoScreen.Instance.HideActionMenu;
		}
		return true;
	}

	public override bool WantShowActionMenuOptionOnButtonPromptBar()
	{
		if (Hud.Instance.DraggingZone || AltDragging)
		{
			return false;
		}
		return AltMenuOpened;
	}
}
