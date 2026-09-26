using System;
using UnityEngine;

public class DebugMenuItemEquipment : DebugMenuItem
{
	public EquipmentPrototype Proto;

	private DebugMenuItemVariationSelected SelectedEvent;

	private int SelectedColorVariation;

	private int SelectedColorVariation2;

	private int SelectedColorVariation3;

	private int SelectedMaterialVariation;

	private int SelectedAmount = 1;

	private string SelectedAmountDisplayString;

	private const string Left = "<";

	private const string Right = ">";

	public DebugMenuItemEquipment(EquipmentPrototype proto, DebugMenuItemVariationSelected selectedEvent)
		: base(proto.NativeName)
	{
		Proto = proto;
		SelectedEvent = selectedEvent;
		SelectedAmountDisplayString = SelectedAmount.ToString();
		int num = CountEquipmentOfTypeInWorld(proto);
		Name = Name + " (" + num + ")";
	}

	private static int CountEquipmentOfTypeInWorld(EquipmentPrototype proto)
	{
		Session instance = Session.Instance;
		int num = 0;
		foreach (Prop allProp in instance.PropManager.AllProps)
		{
			num += allProp.Inventory.CountItemsOfType(proto);
		}
		foreach (Character character in instance.CharacterManager.Characters)
		{
			num += character.Inventory.CountItemsOfType(proto);
		}
		return num;
	}

	public override void Update(Vector2 pos)
	{
		if (GUIButton(new Rect(pos.x, pos.y, 250f, 30f), Name))
		{
			SelectedEvent(SelectedColorVariation, SelectedColorVariation2, SelectedColorVariation3, SelectedMaterialVariation, SelectedAmount);
		}
		pos.x += 260f;
		if (Proto.Tex != null && Proto.Tex.GetAsset() != null)
		{
			GUI.DrawTexture(new Rect(pos.x, pos.y - 5f, 40f, 40f), (Texture2D)Proto.Tex);
		}
		pos.x += 40f;
		if (Proto.MaterialVariations != null && Proto.MaterialVariations.Length != 0 && (Proto.ColorVariations == null || SelectedColorVariation >= Proto.ColorVariations.Length || Proto.ColorVariations[SelectedColorVariation].IsEqual(MathUtil.White)) && (Proto.ColorVariations3 == null || SelectedColorVariation3 >= Proto.ColorVariations3.Length || Proto.ColorVariations3[SelectedColorVariation3].IsEqual(MathUtil.White)))
		{
			pos.x += 20f;
			for (int i = 0; i < Proto.MaterialVariations.Length; i++)
			{
				string text = Proto.MaterialVariations[i];
				text = text.Substring(text.LastIndexOf('/') + 1);
				if (GUIToggle(new Rect(pos.x, pos.y, 75f, 30f), SelectedMaterialVariation == i, text))
				{
					SelectedMaterialVariation = i;
				}
				pos.x += 75f;
			}
		}
		if (Proto.ColorVariations != null && Proto.ColorVariations.Length != 0 && SelectedMaterialVariation == 0)
		{
			pos.x += 20f;
			for (int j = 0; j < Proto.ColorVariations.Length; j++)
			{
				GUI.color = Proto.ColorVariations[j];
				GUI.DrawTexture(new Rect(pos.x, pos.y, 30f, 30f), (Texture2D)GameCursor.WhiteTex);
				GUI.color = Color.white;
				if (GUIToggle(new Rect(pos.x + 8f, pos.y + 5f, 30f, 30f), SelectedColorVariation == j, ""))
				{
					SelectedColorVariation = j;
					SelectedMaterialVariation = 0;
				}
				pos.x += 30f;
			}
		}
		if (Proto.ColorVariations2 != null && Proto.ColorVariations2.Length != 0)
		{
			pos.x += 20f;
			for (int k = 0; k < Proto.ColorVariations2.Length; k++)
			{
				GUI.color = Proto.ColorVariations2[k];
				GUI.DrawTexture(new Rect(pos.x, pos.y, 30f, 30f), (Texture2D)GameCursor.WhiteTex);
				GUI.color = Color.white;
				if (GUIToggle(new Rect(pos.x + 8f, pos.y + 5f, 30f, 30f), SelectedColorVariation2 == k, ""))
				{
					SelectedColorVariation2 = k;
				}
				pos.x += 30f;
			}
		}
		if (Proto.ColorVariations3 != null && Proto.ColorVariations3.Length != 0 && SelectedMaterialVariation == 0)
		{
			pos.x += 20f;
			for (int l = 0; l < Proto.ColorVariations3.Length; l++)
			{
				GUI.color = Proto.ColorVariations3[l];
				GUI.DrawTexture(new Rect(pos.x, pos.y, 30f, 30f), (Texture2D)GameCursor.WhiteTex);
				GUI.color = Color.white;
				if (GUIToggle(new Rect(pos.x + 8f, pos.y + 5f, 30f, 30f), SelectedColorVariation3 == l, ""))
				{
					SelectedColorVariation3 = l;
					SelectedMaterialVariation = 0;
				}
				pos.x += 30f;
			}
		}
		if (Proto.CanBeCombined)
		{
			SelectedAmountDisplayString = GUI.TextField(new Rect(pos.x, pos.y, 100f, 30f), SelectedAmountDisplayString);
			SelectedAmount = StringUtil.ParseInt(SelectedAmountDisplayString, SelectedAmount);
			pos.x += 110f;
			if (GUIButton(new Rect(pos.x, pos.y, 30f, 30f), "<"))
			{
				SelectedAmount = Math.Max(1, SelectedAmount - 1);
				SelectedAmountDisplayString = SelectedAmount.ToString();
			}
			pos.x += 40f;
			if (GUIButton(new Rect(pos.x, pos.y, 30f, 30f), ">"))
			{
				SelectedAmount = Math.Min(1000, SelectedAmount + 1);
				SelectedAmountDisplayString = SelectedAmount.ToString();
			}
			pos.x += 40f;
		}
	}
}
