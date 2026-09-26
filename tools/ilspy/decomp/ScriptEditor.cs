using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScriptEditor : BaseMenu, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
	public static ScriptEditor Instance;

	private bool HasUnsavedChanges;

	public Script EditingScript;

	public GameObject UnityContentPane;

	public GameObject UnityContextMenu;

	public GameObject SelectionBox;

	public ScriptConnectionsGraphic ConnectionsGraphic;

	public List<ScriptObjectBehaviour> SelectedObjects = new List<ScriptObjectBehaviour>();

	public List<ScriptObjectItemBehaviour> SelectedItems = new List<ScriptObjectItemBehaviour>();

	public ScriptObjectItemBehaviour ClickedItem;

	public float ZoomScale = 1f;

	private static float MouseWheelSensitivity = 0.05f;

	private static float MinZoomScale = 0.1f;

	private static float MaxZoomScale = 1f;

	private bool DrawingSelectionBox;

	private Vector2 SelectionBoxStart;

	private Vector2 SelectionBoxEnd;

	private List<ScriptObjectBehaviour> ObjectsAddedToSelection = new List<ScriptObjectBehaviour>();

	public static float KeysScrollSpeed = 10f;

	public override void AwakeImpl()
	{
		Instance = this;
		UnityContentPane = base.gameObject.FindChild("Scroll View/Viewport/Content");
		UnityContextMenu = UnityContentPane.FindChild("ContextMenu");
		UnityContextMenu.SetActive(value: false);
		SelectionBox = UnityContentPane.FindChild("SelectionBox");
		ConnectionsGraphic = UnityContentPane.GetComponent<ScriptConnectionsGraphic>();
		PaperTextureAmount = 0f;
	}

	public override void OnDeactivate(bool popped)
	{
		DeselectAllItems();
		DeselectAll();
		base.OnDeactivate(popped);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
			DeselectAllItems();
			DeselectAll();
			CloseContextMenu();
			break;
		case PointerEventData.InputButton.Right:
			OpenContextMenu(null);
			break;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)UnityContentPane.transform, eventData.position, null, out var localPoint);
			SelectionBoxStart = (SelectionBoxEnd = localPoint);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && eventData.dragging)
		{
			DrawingSelectionBox = true;
			ObjectsAddedToSelection.Clear();
		}
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (DrawingSelectionBox)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)UnityContentPane.transform, eventData.position, null, out var localPoint);
			SelectionBoxEnd = localPoint;
		}
	}

	public static bool IsUIInputFieldActive()
	{
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null)
		{
			if (current.currentSelectedGameObject.GetComponent<InputField>() != null)
			{
				return true;
			}
			if (current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (IsUIInputFieldActive())
		{
			instance.Capture(InputFunction.MapZoom, untilReleased: true);
			instance.Capture(InputFunction.MoveVert, untilReleased: true);
			instance.Capture(InputFunction.MoveHoriz, untilReleased: true);
		}
		if (instance.IsJustPressed(InputFunction.Back) && (!HasUnsavedChanges || SaveScript()))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
		float y = Input.mouseScrollDelta.y;
		y += instance.GetAxis(InputFunction.MapZoom);
		if (y != 0f)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)UnityContentPane.transform, instance.GetMousePosition(), null, out var localPoint);
			ZoomScale = Mathf.Clamp(ZoomScale + y * MouseWheelSensitivity * ZoomScale, MinZoomScale, MaxZoomScale);
			UnityContentPane.transform.localScale = Vector3.one * ZoomScale;
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)UnityContentPane.transform, instance.GetMousePosition(), null, out var localPoint2);
			UnityContentPane.transform.localPosition += MathUtil.ToXY0(localPoint2 - localPoint) * ZoomScale;
		}
		float axis = instance.GetAxis(InputFunction.MoveHoriz);
		float axis2 = instance.GetAxis(InputFunction.MoveVert);
		if (axis != 0f || axis2 != 0f)
		{
			UnityContentPane.transform.localPosition += new Vector3(0f - axis, 0f - axis2, 0f) * ZoomScale * KeysScrollSpeed;
		}
		if (!DrawingSelectionBox)
		{
			return;
		}
		DeselectAllItems();
		if (instance.IsKeyPressed(KeyCode.LeftControl) || instance.IsKeyPressed(KeyCode.RightControl))
		{
			foreach (ScriptObjectBehaviour item in ObjectsAddedToSelection)
			{
				Deselect(item);
			}
		}
		else
		{
			DeselectAll();
		}
		Vector2 vector = Vector2.Min(SelectionBoxStart, SelectionBoxEnd);
		Vector2 vector2 = Vector2.Max(SelectionBoxStart, SelectionBoxEnd);
		Rect other = new Rect(vector, vector2 - vector);
		ObjectsAddedToSelection.Clear();
		for (int i = 0; i < UnityContentPane.transform.childCount; i++)
		{
			GameObject gameObject = UnityContentPane.transform.GetChild(i).gameObject;
			ScriptObjectBehaviour component = gameObject.GetComponent<ScriptObjectBehaviour>();
			if (component != null)
			{
				RectTransform rectTransform = (RectTransform)gameObject.transform;
				if (new Rect(MathUtil.ToXY(rectTransform.localPosition) - new Vector2(0f, rectTransform.sizeDelta.y), rectTransform.sizeDelta).Overlaps(other) && Select(component))
				{
					ObjectsAddedToSelection.Add(component);
				}
			}
		}
		if (!Input.GetMouseButton(1))
		{
			DrawingSelectionBox = false;
		}
	}

	public void OpenScript(string uniqueID)
	{
		for (int i = 0; i < UnityContentPane.transform.childCount; i++)
		{
			GameObject gameObject = UnityContentPane.transform.GetChild(i).gameObject;
			if (gameObject.name == "ContextMenu")
			{
				gameObject.SetActive(value: false);
			}
			else if (gameObject.name == "SelectionBox")
			{
				gameObject.SetActive(value: false);
			}
			else
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		EditingScript = currentlyEditingStory.Scripts[uniqueID];
		ZoomScale = Mathf.Clamp(EditingScript.EditorZoomScale, MinZoomScale, MaxZoomScale);
		UnityContentPane.transform.localScale = Vector3.one * ZoomScale;
		UnityContentPane.transform.localPosition = MathUtil.ToXY0(EditingScript.EditorScrollPos);
		if (GameImpl.Instance.Settings.Language != Language.English)
		{
			foreach (Speech speech in EditingScript.Speeches)
			{
				speech.LoadTranslatedText(currentlyEditingStory);
			}
			foreach (Quest quest in EditingScript.Quests)
			{
				quest.LoadTranslatedText(currentlyEditingStory);
			}
			foreach (QuestGroup questGroup in EditingScript.QuestGroups)
			{
				questGroup.LoadTranslatedText(currentlyEditingStory);
			}
			foreach (Template template in EditingScript.Templates)
			{
				template.LoadTranslatedText(currentlyEditingStory);
			}
			foreach (Invader invader in EditingScript.Invaders)
			{
				invader.LoadTranslatedText(currentlyEditingStory);
			}
		}
		foreach (Speech speech2 in EditingScript.Speeches)
		{
			CreateUnityScriptObject(speech2);
		}
		foreach (Trigger trigger in EditingScript.Triggers)
		{
			CreateUnityScriptObject(trigger);
		}
		foreach (ConditionBlock conditionBlock in EditingScript.ConditionBlocks)
		{
			CreateUnityScriptObject(conditionBlock);
		}
		foreach (Quest quest2 in EditingScript.Quests)
		{
			CreateUnityScriptObject(quest2);
		}
		foreach (QuestGroup questGroup2 in EditingScript.QuestGroups)
		{
			CreateUnityScriptObject(questGroup2);
		}
		foreach (Template template2 in EditingScript.Templates)
		{
			CreateUnityScriptObject(template2);
		}
		foreach (Invader invader2 in EditingScript.Invaders)
		{
			CreateUnityScriptObject(invader2);
		}
		Instance.ConnectionsGraphic.SetVerticesDirty();
	}

	public bool SaveScript()
	{
		EditingScript.EditorScrollPos = MathUtil.ToXY(UnityContentPane.transform.localPosition);
		EditingScript.EditorZoomScale = ZoomScale;
		string fileName = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Scripts/" + EditingScript.UniqueID + ".xml";
		if (EditingScript.SaveToFile(fileName))
		{
			GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
			HasUnsavedChanges = false;
			return true;
		}
		return false;
	}

	public void OpenContextMenu(ScriptObjectItemBehaviour clickedItem)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)UnityContentPane.transform, InputFunctionManager.Instance.GetMousePosition(), null, out var localPoint);
		UnityContextMenu.SetActive(value: true);
		UnityContextMenu.transform.SetAsLastSibling();
		UnityContextMenu.transform.localPosition = MathUtil.ToXY0(localPoint);
		ClickedItem = clickedItem;
		UpdateContextMenuButtons();
	}

	public void CloseContextMenu()
	{
		UnityContextMenu.SetActive(value: false);
		ClickedItem = null;
	}

	public void OnNewSpeech()
	{
		Speech speech = new Speech();
		do
		{
			speech.UniqueID = EditingScript.UniqueID + "_Speech" + EditingScript.NextFreeSpeechID;
			EditingScript.NextFreeSpeechID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(speech.UniqueID) != null);
		speech.OnUniqueIDChanged();
		EditingScript.Speeches.Add(speech);
		CreateScriptObject(speech);
	}

	public void OnNewTrigger()
	{
		Trigger trigger = new Trigger();
		do
		{
			trigger.UniqueID = EditingScript.UniqueID + "_Trigger" + EditingScript.NextFreeTriggerID;
			EditingScript.NextFreeTriggerID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(trigger.UniqueID) != null);
		EditingScript.Triggers.Add(trigger);
		CreateScriptObject(trigger);
	}

	public void OnNewConditionBlock()
	{
		ConditionBlock conditionBlock = new ConditionBlock();
		do
		{
			conditionBlock.UniqueID = EditingScript.UniqueID + "_ConditionBlock" + EditingScript.NextFreeConditionBlockID;
			EditingScript.NextFreeConditionBlockID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(conditionBlock.UniqueID) != null);
		EditingScript.ConditionBlocks.Add(conditionBlock);
		CreateScriptObject(conditionBlock);
	}

	public void OnNewQuest()
	{
		Quest quest = new Quest();
		do
		{
			quest.UniqueID = EditingScript.UniqueID + "_Quest" + EditingScript.NextFreeQuestID;
			EditingScript.NextFreeQuestID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(quest.UniqueID) != null);
		quest.OnUniqueIDChanged();
		EditingScript.Quests.Add(quest);
		CreateScriptObject(quest);
	}

	public void OnNewQuestGroup()
	{
		QuestGroup questGroup = new QuestGroup();
		do
		{
			questGroup.UniqueID = EditingScript.UniqueID + "_QuestGroup" + EditingScript.NextFreeQuestGroupID;
			EditingScript.NextFreeQuestGroupID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(questGroup.UniqueID) != null);
		questGroup.OnUniqueIDChanged();
		EditingScript.QuestGroups.Add(questGroup);
		CreateScriptObject(questGroup);
	}

	public void OnNewTemplate()
	{
		Template template = new Template();
		do
		{
			template.UniqueID = EditingScript.UniqueID + "_Template" + EditingScript.NextFreeTemplateID;
			EditingScript.NextFreeTemplateID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(template.UniqueID) != null);
		template.OnUniqueIDChanged();
		EditingScript.Templates.Add(template);
		CreateScriptObject(template);
	}

	public void OnNewInvader()
	{
		Invader invader = new Invader();
		do
		{
			invader.UniqueID = EditingScript.UniqueID + "_Invader" + EditingScript.NextFreeInvaderID;
			EditingScript.NextFreeInvaderID++;
		}
		while (GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(invader.UniqueID) != null);
		invader.OnUniqueIDChanged();
		EditingScript.Invaders.Add(invader);
		CreateScriptObject(invader);
	}

	public void OnFind()
	{
		GameImpl.Instance.ShowInputBox(OnFindText, "Find", "", multiline: false, readOnly: false);
	}

	private void MatchText(BaseScriptObject scriptObject, string txt, ref BaseScriptObject result)
	{
		if (result == null && scriptObject.MatchText(txt))
		{
			result = scriptObject;
		}
	}

	public void OnFindText(InputFrame inputFrame, string txt)
	{
		txt = txt.ToLower();
		BaseScriptObject result = null;
		foreach (Speech speech in EditingScript.Speeches)
		{
			MatchText(speech, txt, ref result);
		}
		foreach (Trigger trigger in EditingScript.Triggers)
		{
			MatchText(trigger, txt, ref result);
		}
		foreach (ConditionBlock conditionBlock in EditingScript.ConditionBlocks)
		{
			MatchText(conditionBlock, txt, ref result);
		}
		foreach (Quest quest in EditingScript.Quests)
		{
			MatchText(quest, txt, ref result);
		}
		foreach (QuestGroup questGroup in EditingScript.QuestGroups)
		{
			MatchText(questGroup, txt, ref result);
		}
		foreach (Template template in EditingScript.Templates)
		{
			MatchText(template, txt, ref result);
		}
		foreach (Invader invader in EditingScript.Invaders)
		{
			MatchText(invader, txt, ref result);
		}
		if (result != null)
		{
			ScriptObjectBehaviour scriptObjectBehaviour = GetScriptObjectBehaviour(result);
			if (scriptObjectBehaviour != null)
			{
				Rect rect = ((RectTransform)base.transform).rect;
				Rect rect2 = ((RectTransform)scriptObjectBehaviour.transform).rect;
				UnityContentPane.transform.localPosition = new Vector3((0f - result.x) * ZoomScale + rect.width * 0.5f - rect2.width * 0.5f, (0f - result.y) * ZoomScale - rect.height * 0.5f + rect2.height * 0.5f, 0f);
				DeselectAllItems();
				DeselectAll();
				Select(scriptObjectBehaviour);
			}
		}
		else
		{
			GameImpl.Instance.PopDialog();
			GameImpl.Instance.ShowMessageBox("Couldn't find \"" + txt + "\"");
		}
	}

	public void OnCopy()
	{
		Script script = new Script();
		foreach (ScriptObjectBehaviour selectedObject in SelectedObjects)
		{
			BaseScriptObject scriptObject = selectedObject.ScriptObject;
			script.AddScriptObject(scriptObject);
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (TextWriter textWriter = new StringWriter(stringBuilder))
		{
			new XmlSerializer(typeof(Script)).Serialize(textWriter, script);
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
		UpdateContextMenuButtons();
	}

	public void OnCut()
	{
		OnCopy();
		OnDeleteSelection();
	}

	public void OnDeleteSelection()
	{
		while (SelectedObjects.Count > 0)
		{
			DeleteScriptObject(SelectedObjects[0]);
		}
	}

	public void OnPaste()
	{
		DeselectAllItems();
		DeselectAll();
		using TextReader textReader = new StringReader(GUIUtility.systemCopyBuffer);
		Script script = null;
		try
		{
			script = (Script)new XmlSerializer(typeof(Script)).Deserialize(textReader);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
			return;
		}
		Dictionary<string, string> newUniqueIDs = new Dictionary<string, string>();
		List<BaseScriptObject> list = new List<BaseScriptObject>();
		foreach (Speech speech in script.Speeches)
		{
			list.Add(PasteScriptObject(speech, newUniqueIDs));
		}
		foreach (Trigger trigger in script.Triggers)
		{
			list.Add(PasteScriptObject(trigger, newUniqueIDs));
		}
		foreach (ConditionBlock conditionBlock in script.ConditionBlocks)
		{
			list.Add(PasteScriptObject(conditionBlock, newUniqueIDs));
		}
		foreach (Quest quest in script.Quests)
		{
			list.Add(PasteScriptObject(quest, newUniqueIDs));
		}
		foreach (QuestGroup questGroup in script.QuestGroups)
		{
			list.Add(PasteScriptObject(questGroup, newUniqueIDs));
		}
		foreach (Template template in script.Templates)
		{
			list.Add(PasteScriptObject(template, newUniqueIDs));
		}
		foreach (Invader invader in script.Invaders)
		{
			list.Add(PasteScriptObject(invader, newUniqueIDs));
		}
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		foreach (BaseScriptObject item in list)
		{
			item.FixupAfterXmlLoad(EditingScript, currentlyEditingStory, newUniqueIDs);
		}
		foreach (BaseScriptObject item2 in list)
		{
			ScriptObjectBehaviour obj = CreateUnityScriptObject(item2);
			Select(obj);
		}
		if (SelectedObjects.Count > 0)
		{
			Rect a = SelectedObjects[0].GetRect();
			for (int i = 1; i < SelectedObjects.Count; i++)
			{
				a = MathUtil.RectExpand(a, SelectedObjects[i].GetRect());
			}
			Vector2 v = MathUtil.ToXY(UnityContextMenu.transform.localPosition) - a.center;
			foreach (ScriptObjectBehaviour selectedObject in SelectedObjects)
			{
				selectedObject.transform.localPosition += MathUtil.ToXY0(v);
				selectedObject.ScriptObject.EditorPos = MathUtil.ToXY(selectedObject.transform.localPosition);
			}
		}
		OnFieldChanged();
	}

	public void OnCopyItem()
	{
		if (SelectedItems.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (TextWriter output = new StringWriter(stringBuilder))
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.OmitXmlDeclaration = true;
			using XmlWriter writer = XmlWriter.Create(output, xmlWriterSettings);
			SelectedItems[0].CopyFunc(writer, SelectedItems);
		}
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
		UpdateContextMenuButtons();
	}

	public void OnCutItem()
	{
		OnCopyItem();
		OnDeleteSelectedItems();
	}

	public void OnDeleteSelectedItems()
	{
		for (int num = SelectedItems.Count - 1; num >= 0; num--)
		{
			SelectedItems[num].DeleteFunc();
		}
	}

	public void OnPasteItem()
	{
		if (!(ClickedItem != null))
		{
			return;
		}
		using TextReader input = new StringReader(GUIUtility.systemCopyBuffer);
		XmlReaderSettings settings = new XmlReaderSettings();
		using XmlReader reader = XmlReader.Create(input, settings);
		ClickedItem.PasteFunc(reader);
	}

	private int ParseCopyNumber(string str)
	{
		int num = str.Length - 1;
		while (num >= 0 && str[num] == ' ')
		{
			num--;
		}
		if (num >= 0 && str[num] == ')')
		{
			num--;
			while (num >= 0 && str[num] >= '0' && str[num] <= '9')
			{
				num--;
			}
			if (num >= 0 && str[num] == '(')
			{
				num--;
				while (num >= 0 && str[num] == ' ')
				{
					num--;
				}
				return num;
			}
			return -1;
		}
		return -1;
	}

	public BaseScriptObject PasteScriptObject(BaseScriptObject obj, Dictionary<string, string> newUniqueIDs)
	{
		string uniqueID = obj.UniqueID;
		int num = 0;
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		while (currentlyEditingStory.FindScriptObjectByUniqueID(obj.UniqueID) != null)
		{
			num++;
			int num2 = ParseCopyNumber(obj.UniqueID);
			if (num2 != -1)
			{
				obj.UniqueID = obj.UniqueID.Substring(0, num2);
			}
			obj.UniqueID = obj.UniqueID + " (" + num + ")";
		}
		newUniqueIDs[uniqueID] = obj.UniqueID;
		EditingScript.AddScriptObject(obj);
		currentlyEditingStory.RegisterScriptObject(obj);
		return obj;
	}

	public void OnSave()
	{
		SaveScript();
	}

	public void OnExit()
	{
		if (!HasUnsavedChanges || SaveScript())
		{
			WantPop = true;
		}
	}

	private ScriptObjectBehaviour CreateUnityScriptObject(BaseScriptObject obj)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(BaseMenu.ScriptObjectPrefab.GetAsset(), UnityContentPane.transform);
		ScriptObjectBehaviour scriptObjectBehaviour = gameObject.AddComponent<ScriptObjectBehaviour>();
		scriptObjectBehaviour.ScriptObject = obj;
		scriptObjectBehaviour.UpdateFields();
		gameObject.transform.localPosition = MathUtil.ToXY0(obj.EditorPos);
		return scriptObjectBehaviour;
	}

	public void CreateScriptObject(BaseScriptObject obj)
	{
		GameImpl.Instance.GetCurrentlyEditingStory().RegisterScriptObject(obj);
		obj.EditorPos = MathUtil.ToXY(UnityContextMenu.transform.localPosition);
		CreateUnityScriptObject(obj);
		OnFieldChanged();
		CloseContextMenu();
	}

	public void DeleteScriptObject(ScriptObjectBehaviour unityObj)
	{
		Deselect(unityObj);
		UnityEngine.Object.Destroy(unityObj.gameObject);
		BaseScriptObject scriptObject = unityObj.ScriptObject;
		GameImpl.Instance.GetCurrentlyEditingStory().UnregisterScriptObject(scriptObject);
		if (scriptObject is Speech)
		{
			EditingScript.Speeches.Remove(scriptObject as Speech);
		}
		if (scriptObject is Trigger)
		{
			EditingScript.Triggers.Remove(scriptObject as Trigger);
		}
		if (scriptObject is ConditionBlock)
		{
			EditingScript.ConditionBlocks.Remove(scriptObject as ConditionBlock);
		}
		if (scriptObject is Quest)
		{
			EditingScript.Quests.Remove(scriptObject as Quest);
		}
		if (scriptObject is QuestGroup)
		{
			EditingScript.QuestGroups.Remove(scriptObject as QuestGroup);
		}
		if (scriptObject is Template)
		{
			EditingScript.Templates.Remove(scriptObject as Template);
		}
		if (scriptObject is Invader)
		{
			EditingScript.Invaders.Remove(scriptObject as Invader);
		}
		OnFieldChanged();
	}

	public void OnFieldChanged()
	{
		HasUnsavedChanges = true;
		ConnectionsGraphic.SetVerticesDirty();
	}

	public ScriptObjectBehaviour GetScriptObjectBehaviour(BaseScriptObject obj)
	{
		for (int i = 0; i < UnityContentPane.transform.childCount; i++)
		{
			ScriptObjectBehaviour component = UnityContentPane.transform.GetChild(i).gameObject.GetComponent<ScriptObjectBehaviour>();
			if (component != null && component.ScriptObject == obj)
			{
				return component;
			}
		}
		return null;
	}

	public bool Select(ScriptObjectBehaviour obj)
	{
		if (!SelectedObjects.Contains(obj))
		{
			SelectedObjects.Add(obj);
			Outline component = obj.GetComponent<Outline>();
			component.effectColor = Color.red;
			component.effectDistance = new Vector2(4f, 4f);
			UpdateContextMenuButtons();
			return true;
		}
		return false;
	}

	public void Deselect(ScriptObjectBehaviour obj)
	{
		for (int num = SelectedItems.Count - 1; num >= 0; num--)
		{
			ScriptObjectItemBehaviour scriptObjectItemBehaviour = SelectedItems[num];
			if (scriptObjectItemBehaviour.Owner == obj)
			{
				DeselectItem(scriptObjectItemBehaviour);
			}
		}
		int num2 = SelectedObjects.IndexOf(obj);
		if (num2 != -1)
		{
			SelectedObjects.RemoveAt(num2);
			Outline component = obj.GetComponent<Outline>();
			component.effectColor = Color.black;
			component.effectDistance = new Vector2(2f, 2f);
			UpdateContextMenuButtons();
		}
	}

	public void DeselectAll()
	{
		while (SelectedObjects.Count > 0)
		{
			Deselect(SelectedObjects[0]);
		}
	}

	public void SelectItem(ScriptObjectItemBehaviour item)
	{
		for (int num = SelectedItems.Count - 1; num >= 0; num--)
		{
			if (SelectedItems[num].Owner != item.Owner || SelectedItems[num].FieldName != item.FieldName)
			{
				DeselectItem(SelectedItems[num]);
			}
		}
		if (!SelectedItems.Contains(item))
		{
			SelectedItems.Add(item);
			SelectedItems.Sort();
			item.GetComponent<RawImage>().color = ((item.ListIndex % 2 == 0) ? ScriptObjectBehaviour.SelectedItemCol : ScriptObjectBehaviour.SelectedItemAltCol);
			UpdateContextMenuButtons();
		}
	}

	public void DeselectItem(ScriptObjectItemBehaviour item)
	{
		int num = SelectedItems.IndexOf(item);
		if (num != -1)
		{
			SelectedItems.RemoveAt(num);
			if (item != null)
			{
				item.GetComponent<RawImage>().color = ((item.ListIndex % 2 == 0) ? ScriptObjectBehaviour.ItemCol : ScriptObjectBehaviour.ItemAltCol);
			}
			UpdateContextMenuButtons();
		}
	}

	public void DeselectAllItems()
	{
		while (SelectedItems.Count > 0)
		{
			DeselectItem(SelectedItems[0]);
		}
	}

	public void UpdateContextMenuButtons()
	{
		for (int i = 0; i < UnityContextMenu.transform.childCount; i++)
		{
			GameObject gameObject = UnityContextMenu.transform.GetChild(i).gameObject;
			if (gameObject.name == "CopyButton" || gameObject.name == "CutButton")
			{
				gameObject.GetComponent<Button>().interactable = SelectedObjects.Count > 0;
			}
			if (gameObject.name == "PasteButton")
			{
				gameObject.GetComponent<Button>().interactable = !string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
			}
			if (gameObject.name == "CopyItemButton")
			{
				gameObject.gameObject.SetActive(SelectedItems.Count > 0);
				if (SelectedItems.Count > 0)
				{
					gameObject.transform.GetChild(0).GetComponent<Text>().SetUnityText("Copy " + SelectedItems[0].GetTypeName() + ((SelectedItems.Count > 1) ? "s" : ""));
				}
			}
			if (gameObject.name == "CutItemButton")
			{
				gameObject.gameObject.SetActive(SelectedItems.Count > 0);
				if (SelectedItems.Count > 0)
				{
					gameObject.transform.GetChild(0).GetComponent<Text>().SetUnityText("Cut " + SelectedItems[0].GetTypeName() + ((SelectedItems.Count > 1) ? "s" : ""));
				}
			}
			if (gameObject.name == "PasteItemButton")
			{
				gameObject.gameObject.SetActive(ClickedItem != null);
				gameObject.GetComponent<Button>().interactable = !string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
				if (ClickedItem != null)
				{
					gameObject.transform.GetChild(0).GetComponent<Text>().SetUnityText("Paste " + ClickedItem.GetTypeName() + "s");
				}
			}
			if (gameObject.name == "ItemSeparator")
			{
				gameObject.gameObject.SetActive(SelectedItems.Count > 0 || ClickedItem != null);
			}
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		UpdateActiveObjects();
	}

	public void UpdateActiveObjects()
	{
		Rect rect = ((RectTransform)base.transform).rect;
		rect.position = (-MathUtil.ToXY(UnityContentPane.transform.localPosition) - new Vector2(0f, rect.height)) / ZoomScale;
		rect.size /= ZoomScale;
		for (int i = 0; i < UnityContentPane.transform.childCount; i++)
		{
			RectTransform rectTransform = (RectTransform)UnityContentPane.transform.GetChild(i);
			if (rectTransform.gameObject == UnityContextMenu)
			{
				continue;
			}
			if (rectTransform.gameObject == SelectionBox)
			{
				SelectionBox.SetActive(DrawingSelectionBox);
				if (SelectionBox.activeSelf)
				{
					Vector2 vector = Vector2.Min(SelectionBoxStart, SelectionBoxEnd);
					Vector2 vector2 = Vector2.Max(SelectionBoxStart, SelectionBoxEnd);
					rectTransform.localPosition = MathUtil.ToXY0(vector);
					rectTransform.sizeDelta = vector2 - vector;
					float num = 2f / ZoomScale;
					for (int j = 0; j < rectTransform.childCount; j++)
					{
						rectTransform.GetChild(j).GetComponent<RawImage>().rectTransform.sizeDelta = ((j < 2) ? new Vector2(num, 0f) : new Vector2(0f, num));
					}
				}
			}
			else
			{
				Rect rect2 = rectTransform.rect;
				rect2.position = MathUtil.ToXY(rectTransform.localPosition) - new Vector2(0f, rect2.height);
				rectTransform.gameObject.SetActive(rect.Overlaps(rect2));
			}
		}
	}
}
