using System.Collections.Generic;
using UnityEngine;

public class HelicopterBehaviour : MonoBehaviour
{
	private Helicopter Owner;

	private List<GameObject> Rotors = new List<GameObject>();

	private List<GameObject> TailRotors = new List<GameObject>();

	private List<Quaternion> Rot = new List<Quaternion>();

	private List<Quaternion> TailRot = new List<Quaternion>();

	private bool VTOL;

	private static float FlyToHeight = 100f;

	public void Init(Helicopter owner)
	{
		Owner = owner;
		SetupRotors(base.gameObject, vtol: false);
	}

	private void SetupRotors(GameObject obj, bool vtol)
	{
		if (obj.name.Contains("tail_rotor"))
		{
			TailRotors.Add(obj);
			TailRot.Add(obj.transform.localRotation);
		}
		else if (obj.name.Contains("rotor"))
		{
			Rotors.Add(obj);
			Rot.Add(obj.transform.localRotation);
			VTOL |= vtol;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			SetupRotors(obj.transform.GetChild(i).gameObject, obj.name.Contains("motor"));
		}
	}

	private void Update()
	{
		if (Owner == null)
		{
			return;
		}
		if (Owner.CurrentHelicopterAnimState >= HelicopterAnimState.RotorSpinning)
		{
			for (int i = 0; i < Rotors.Count; i++)
			{
				GameObject obj = Rotors[i];
				if (VTOL)
				{
					Rot[i] *= Quaternion.Euler(Time.deltaTime * Owner.RotorSpeed, 0f, 0f);
				}
				else
				{
					Rot[i] *= Quaternion.Euler(0f, Time.deltaTime * Owner.RotorSpeed, 0f);
				}
				obj.transform.localRotation = Rot[i];
			}
			for (int j = 0; j < TailRotors.Count; j++)
			{
				GameObject obj2 = TailRotors[j];
				TailRot[j] *= Quaternion.Euler(0f, 0f, Time.deltaTime * Owner.RotorSpeed);
				obj2.transform.localRotation = TailRot[j];
			}
		}
		if (Owner.CurrentHelicopterAnimState == HelicopterAnimState.TakingOff)
		{
			Matrix4x4 customModelTransform = Owner.GetCustomModelTransform();
			Vector3 vector = customModelTransform.Translation();
			Quaternion rotation = customModelTransform.rotation;
			float num = Mathf.Clamp01((float)(Session.Instance.PlayTime - Owner.TakeOffStartTime).TotalSeconds / Helicopter.TakeOffTime);
			float t = Mathf.Clamp01((float)(Session.Instance.PlayTime - Owner.TakeOffStartTime).TotalSeconds / 1f);
			base.gameObject.transform.position = vector + Vector3.up * num * num * FlyToHeight;
			base.gameObject.transform.rotation = Quaternion.Slerp(rotation, Quaternion.Euler(0f, rotation.eulerAngles.y, 0f), t);
		}
	}
}
