using System.Collections.Generic;
using UnityEngine;

public class SelectScriptMenu : BaseMenu
{
	public override void OnActivate()
	{
		base.OnActivate();
		Populate();
	}

	public void Populate()
	{
		GameObject gameObject = base.gameObject.transform.Find("ScriptsList/Viewport/Content").gameObject;
		gameObject.DeleteAllChildren();
		int num = 0;
		foreach (KeyValuePair<string, Script> pair in GameImpl.Instance.GetCurrentlyEditingStory().Scripts)
		{
			AddMenuButton(gameObject, num, pair.Key, delegate
			{
				OnSelectScript(pair.Key);
			});
			num++;
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().Scripts.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string uniqueID)
	{
		if (!string.IsNullOrEmpty(uniqueID))
		{
			CheckUniqueID(ref uniqueID);
			Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
			Script script = new Script
			{
				UniqueID = uniqueID
			};
			currentlyEditingStory.Scripts[uniqueID] = script;
			OnSelectScript(script.UniqueID);
		}
	}

	public void OnCreateNewScript()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterScriptName"), "", multiline: false, readOnly: false);
	}

	public void OnSelectScript(string fileName)
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditScriptPanel"));
		(ChildMenu as ScriptEditor).OpenScript(fileName);
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}
}
