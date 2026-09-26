using UnityEngine;

public class EquipmentSettings : MonoBehaviour
{
	public EquippedModelProperties[] EquippedModels;

	public IKPoint DrinkingIKPoint;

	public IKPoint EatingRightHandIKPoint;

	public IKPoint EatingLeftHandIKPoint;

	public IKPoint WateringLeftHandIKPoint;

	public void SaveAsXml(string fileName)
	{
		PrefabSettings prefabSettings = new PrefabSettings();
		EquippedModelProperties[] equippedModels = EquippedModels;
		foreach (EquippedModelProperties item in equippedModels)
		{
			prefabSettings.EquippedModels.Add(item);
		}
		PrefabSettings.SaveToFile(fileName, prefabSettings);
	}
}
