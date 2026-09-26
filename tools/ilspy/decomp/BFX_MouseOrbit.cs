using System;
using UnityEngine;

public class BFX_MouseOrbit : MonoBehaviour
{
	public GameObject target;

	public float distance = 10f;

	public float xSpeed = 250f;

	public float ySpeed = 120f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	private float x;

	private float y;

	private float prevDistance;

	private void Start()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
	}

	private void LateUpdate()
	{
		if (distance < 2f)
		{
			distance = 2f;
		}
		distance -= Input.GetAxis("Mouse ScrollWheel") * 2f;
		if ((bool)target && Input.GetMouseButton(1))
		{
			Vector3 mousePosition = Input.mousePosition;
			float num = 1f;
			if (Screen.dpi < 1f)
			{
				num = 1f;
			}
			num = ((!(Screen.dpi < 200f)) ? (Screen.dpi / 200f) : 1f);
			if (mousePosition.x < 380f * num && (float)Screen.height - mousePosition.y < 250f * num)
			{
				return;
			}
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			Quaternion quaternion = Quaternion.Euler(y, x, 0f);
			Vector3 position = quaternion * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			base.transform.rotation = quaternion;
			base.transform.position = position;
		}
		else
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			if ((double)Time.timeScale < 0.5)
			{
				x += 1.6666667f;
			}
			else
			{
				x += Time.deltaTime * 3f;
			}
			Quaternion quaternion2 = Quaternion.Euler(y, x, 0f);
			Vector3 position2 = quaternion2 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			base.transform.rotation = quaternion2;
			base.transform.position = position2;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			if ((double)Time.timeScale < 0.5)
			{
				x -= 1.6666667f;
			}
			else
			{
				x -= Time.deltaTime * 3f;
			}
			Quaternion quaternion3 = Quaternion.Euler(y, x, 0f);
			Vector3 position3 = quaternion3 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			base.transform.rotation = quaternion3;
			base.transform.position = position3;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if ((double)Time.timeScale > 0.5)
			{
				Time.timeScale = 0f;
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
		if (Math.Abs(prevDistance - distance) > 0.001f)
		{
			prevDistance = distance;
			Quaternion quaternion4 = Quaternion.Euler(y, x, 0f);
			Vector3 position4 = quaternion4 * new Vector3(0f, 0f, 0f - distance) + target.transform.position;
			base.transform.rotation = quaternion4;
			base.transform.position = position4;
		}
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
