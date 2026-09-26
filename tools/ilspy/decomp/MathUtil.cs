using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class MathUtil
{
	public const float Pi = MathF.PI;

	public const float PiOver2 = MathF.PI / 2f;

	public const float PiOver4 = MathF.PI / 4f;

	public const float PiOver8 = MathF.PI / 8f;

	public const float TwoPi = MathF.PI * 2f;

	public const float LbsToKg = 0.45359236f;

	public const float FlOzToLiter = 0.0295735f;

	public static Color TransparentBlackCol = new Color(0f, 0f, 0f, 0f);

	public static Color32 TransparentBlack = new Color32(0, 0, 0, 0);

	public static Color32 White = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static Action<Plane[], Matrix4x4> Internal_ExtractPlanes = null;

	public static CustomRandom NonDeterministicRand = new CustomRandom();

	private static Action<Plane[], Matrix4x4> ExtractPlanes
	{
		get
		{
			if (Internal_ExtractPlanes == null)
			{
				MethodInfo method = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(Plane[]),
					typeof(Matrix4x4)
				}, null);
				Internal_ExtractPlanes = Delegate.CreateDelegate(typeof(Action<Plane[], Matrix4x4>), method) as Action<Plane[], Matrix4x4>;
			}
			return Internal_ExtractPlanes;
		}
	}

	public static bool IsA(this Type type, Type otherType)
	{
		if (!(type == otherType))
		{
			return type.IsSubclassOf(otherType);
		}
		return true;
	}

	public static float CelsiusToFahrenheit(float v)
	{
		return v * 1.8f + 32f;
	}

	public static bool IsNaNorInfinity(float f)
	{
		if (!float.IsNaN(f))
		{
			return float.IsInfinity(f);
		}
		return true;
	}

	public static bool IsNaNorInfinity(Vector2 v)
	{
		if (!IsNaNorInfinity(v.x))
		{
			return IsNaNorInfinity(v.y);
		}
		return true;
	}

	public static bool IsNaNorInfinity(Vector3 v)
	{
		if (!IsNaNorInfinity(v.x) && !IsNaNorInfinity(v.y))
		{
			return IsNaNorInfinity(v.z);
		}
		return true;
	}

	public static bool IsNaNorInfinity(Quaternion q)
	{
		if (!IsNaNorInfinity(q.x) && !IsNaNorInfinity(q.y) && !IsNaNorInfinity(q.z))
		{
			return IsNaNorInfinity(q.w);
		}
		return true;
	}

	public static void CheckNaNorInfinity(float v)
	{
		if (IsNaNorInfinity(v))
		{
			Debug.LogWarning("float NaN: " + v);
		}
	}

	public static void CheckNaNorInfinity(Vector2 v)
	{
		if (IsNaNorInfinity(v))
		{
			Vector2 vector = v;
			Debug.LogWarning("Vector2 NaN: " + vector.ToString());
		}
	}

	public static void CheckNaNorInfinity(Vector3 v)
	{
		if (IsNaNorInfinity(v))
		{
			Vector3 vector = v;
			Debug.LogWarning("Vector3 NaN: " + vector.ToString());
		}
	}

	public static void CheckNaNorInfinity(Quaternion q)
	{
		if (IsNaNorInfinity(q))
		{
			Quaternion quaternion = q;
			Debug.LogWarning("Quaternion NaN: " + quaternion.ToString());
		}
	}

	public static void CheckNaNorInfinity(Trans t)
	{
		if (IsNaNorInfinity(t.Pos) || IsNaNorInfinity(t.Orientation) || IsNaNorInfinity(t.Scale))
		{
			string[] obj = new string[6] { "Trans NaN: ", null, null, null, null, null };
			Vector3 pos = t.Pos;
			obj[1] = pos.ToString();
			obj[2] = ", ";
			Quaternion orientation = t.Orientation;
			obj[3] = orientation.ToString();
			obj[4] = ", ";
			obj[5] = t.Scale.ToString();
			Debug.LogWarning(string.Concat(obj));
		}
	}

	public static void FixNaNorInfinity(ref float v)
	{
		if (IsNaNorInfinity(v))
		{
			Debug.LogWarning("float NaN: " + v);
			v = 0f;
		}
	}

	public static Vector3 Right(this Matrix4x4 mat)
	{
		return new Vector3(mat.m00, mat.m10, mat.m20);
	}

	public static Vector3 Up(this Matrix4x4 mat)
	{
		return new Vector3(mat.m01, mat.m11, mat.m21);
	}

	public static Vector3 Forward(this Matrix4x4 mat)
	{
		return new Vector3(mat.m02, mat.m12, mat.m22);
	}

	public static Vector3 Translation(this Matrix4x4 mat)
	{
		return new Vector3(mat.m03, mat.m13, mat.m23);
	}

	public static void SetRight(ref Matrix4x4 mat, Vector3 v)
	{
		mat.m00 = v.x;
		mat.m10 = v.y;
		mat.m20 = v.z;
	}

	public static void SetUp(ref Matrix4x4 mat, Vector3 v)
	{
		mat.m01 = v.x;
		mat.m11 = v.y;
		mat.m21 = v.z;
	}

	public static void SetForward(ref Matrix4x4 mat, Vector3 v)
	{
		mat.m02 = v.x;
		mat.m12 = v.y;
		mat.m22 = v.z;
	}

	public static void SetTranslation(ref Matrix4x4 mat, Vector3 v)
	{
		mat.m03 = v.x;
		mat.m13 = v.y;
		mat.m23 = v.z;
	}

	public static Matrix4x4 CreateTranslation(float x, float y, float z)
	{
		return Matrix4x4.TRS(new Vector3(x, y, z), Quaternion.identity, Vector3.one);
	}

	public static Matrix4x4 CreateTranslation(Vector3 pos)
	{
		return Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
	}

	public static Matrix4x4 CreateRotationX(float angle)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(angle * 57.29578f, 0f, 0f), Vector3.one);
	}

	public static Matrix4x4 CreateRotationY(float angle)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, angle * 57.29578f, 0f), Vector3.one);
	}

	public static Matrix4x4 CreateRotationZ(float angle)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle * 57.29578f), Vector3.one);
	}

	public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, float angle)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle * 57.29578f, axis), Vector3.one);
	}

	public static Matrix4x4 CreateScale(float scale)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, scale));
	}

	public static Matrix4x4 CreateScale(Vector3 scale)
	{
		return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
	}

	public static Bounds CreateBoundsMinMax(Vector3 min, Vector3 max)
	{
		return new Bounds(Vector3.Lerp(min, max, 0.5f), max - min);
	}

	public static Bounds CreateBoundsCentreExtents(Vector3 centre, Vector3 extents)
	{
		return new Bounds(centre, extents * 2f);
	}

	public static bool Intersects(this Bounds aabb, BoundingSphere sphere)
	{
		return aabb.SqrDistance(sphere.position) <= sphere.radius * sphere.radius;
	}

	public static float? IntersectsSphere(this Ray ray, BoundingSphere sphere)
	{
		Vector3 rhs = sphere.position - ray.origin;
		float sqrMagnitude = rhs.sqrMagnitude;
		float num = sphere.radius * sphere.radius;
		if (sqrMagnitude < num)
		{
			return 0f;
		}
		float num2 = Vector3.Dot(ray.direction, rhs);
		if (num2 < 0f)
		{
			return null;
		}
		float num3 = num + num2 * num2 - sqrMagnitude;
		if (num3 < 0f)
		{
			return null;
		}
		return num2 - (float)Math.Sqrt(num3);
	}

	public static Rect CreateRectCentreExtents(Vector2 centre, Vector2 extents)
	{
		return new Rect(centre - extents, extents * 2f);
	}

	public static Rect RectIntersection(Rect a, Rect b)
	{
		return Rect.MinMaxRect(Mathf.Max(a.xMin, b.xMin), Mathf.Max(a.yMin, b.yMin), Mathf.Min(a.xMax, b.xMax), Mathf.Min(a.yMax, b.yMax));
	}

	public static Rect RectExpand(Rect a, Rect b)
	{
		return Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
	}

	public static Color Color255(int r, int g, int b)
	{
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
	}

	public static bool IsEqual(this Color32 lhs, Color32 rhs)
	{
		if (lhs.r == rhs.r && lhs.g == rhs.g && lhs.b == rhs.b)
		{
			return lhs.a == rhs.a;
		}
		return false;
	}

	public static uint Color32ToUInt(Color32 color)
	{
		return (uint)((color.a << 24) | (color.r << 16) | (color.g << 8) | color.b);
	}

	public static Color32 UIntToColor32(uint color)
	{
		return new Color32((byte)(color >> 16), (byte)(color >> 8), (byte)color, (byte)(color >> 24));
	}

	public static Vector3 ToX0Y(Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}

	public static Vector3 ToXY0(Vector2 v)
	{
		return new Vector3(v.x, v.y, 0f);
	}

	public static Vector3 ToXYZ(Vector2 v, float z)
	{
		return new Vector3(v.x, v.y, z);
	}

	public static Vector3 ToXZY(Vector2 v, float z)
	{
		return new Vector3(v.x, z, v.y);
	}

	public static Vector2 ToXZ(Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector2 ToXY(Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector2 ToZY(Vector3 v)
	{
		return new Vector2(v.z, v.y);
	}

	public static Vector2 RightNormal(Vector2 v)
	{
		return new Vector2(v.y, 0f - v.x);
	}

	public static Vector2 LeftNormal(Vector2 v)
	{
		return new Vector2(0f - v.y, v.x);
	}

	public static TerrainCoord RightNormal(TerrainCoord v)
	{
		return new TerrainCoord(v.y, -v.x);
	}

	public static TerrainCoord LeftNormal(TerrainCoord v)
	{
		return new TerrainCoord(-v.y, v.x);
	}

	public static float Min(float a, float b, float c, float d)
	{
		return Math.Min(a, Math.Min(b, Math.Min(c, d)));
	}

	public static float Max(float a, float b, float c, float d)
	{
		return Math.Max(a, Math.Max(b, Math.Max(c, d)));
	}

	public static byte Min(byte a, byte b, byte c, byte d)
	{
		return Math.Min(a, Math.Min(b, Math.Min(c, d)));
	}

	public static byte Max(byte a, byte b, byte c, byte d)
	{
		return Math.Max(a, Math.Max(b, Math.Max(c, d)));
	}

	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T val = lhs;
		lhs = rhs;
		rhs = val;
	}

	public static float Squared(float x)
	{
		return x * x;
	}

	public static int Squared(int x)
	{
		return x * x;
	}

	public static float SquaredButKeepSign(float x)
	{
		return x * x * Mathf.Sign(x);
	}

	public static float Cubed(float x)
	{
		return x * x * x;
	}

	public static float WrapAngle(float a)
	{
		if (a < 0f)
		{
			a += MathF.PI * 2f * (float)((int)((0f - a) / (MathF.PI * 2f)) + 1);
		}
		a %= MathF.PI * 2f;
		if (a > MathF.PI)
		{
			a -= MathF.PI * 2f;
		}
		return a;
	}

	public static float WrapAngleDeg(float a)
	{
		if (a < 0f)
		{
			a += 180f * (float)((int)((0f - a) / 180f) + 1);
		}
		a %= 360f;
		if (a > MathF.PI)
		{
			a -= 180f;
		}
		return a;
	}

	public static float AngleStep(float cur, float dest, float step)
	{
		cur = WrapAngle(cur);
		dest = WrapAngle(dest);
		if (dest > cur)
		{
			if (dest - cur <= MathF.PI)
			{
				cur = Math.Min(cur + step, dest);
			}
			else
			{
				cur -= step;
				if (cur < -MathF.PI)
				{
					cur = Math.Max(cur + MathF.PI * 2f, dest);
				}
			}
		}
		else if (cur - dest <= MathF.PI)
		{
			cur = Math.Max(cur - step, dest);
		}
		else
		{
			cur += step;
			if (cur > MathF.PI)
			{
				cur = Math.Min(cur - MathF.PI * 2f, dest);
			}
		}
		return cur;
	}

	public static float AngleDiff(float a, float b)
	{
		a = WrapAngle(a);
		b = WrapAngle(b);
		float num = Math.Abs(a - b);
		if (num > MathF.PI)
		{
			return MathF.PI * 2f - num;
		}
		return num;
	}

	public static float SignedAngleDiff(float a, float b)
	{
		a = WrapAngle(a);
		b = WrapAngle(b);
		float num = a - b;
		if (num > MathF.PI)
		{
			return num - MathF.PI * 2f;
		}
		if (num < -MathF.PI)
		{
			return num + MathF.PI * 2f;
		}
		return num;
	}

	public static float AngleLerp(float a, float b, float t)
	{
		float num = AngleDiff(b, a);
		return AngleStep(a, b, num * t);
	}

	public static float GetAngleTo(Vector3 pos, Vector3 target, float fallback)
	{
		return GetAngleFromDir(new Vector2(target.x - pos.x, target.z - pos.z), fallback);
	}

	public static float GetAngleToXZ(Vector2 pos, Vector2 target, float fallback)
	{
		return GetAngleFromDir(target - pos, fallback);
	}

	public static float GetAngleFromDir(Vector2 toTarget, float fallback)
	{
		float magnitude = toTarget.magnitude;
		if (magnitude < 0.001f)
		{
			return fallback;
		}
		return GetAngleFromNormalizedDir(toTarget / magnitude);
	}

	public static float GetAngleFromNormalizedDir(Vector2 dirToTarget)
	{
		return (float)Math.Atan2(dirToTarget.x, dirToTarget.y);
	}

	public static Vector2 RotateVector2(Vector2 v, float angle)
	{
		float num = Mathf.Cos(angle);
		float num2 = Mathf.Sin(angle);
		return new Vector2(v.x * num - v.y * num2, v.x * num2 + v.y * num);
	}

	public static Vector3 RotateVectorXZ(Vector3 v, float angle)
	{
		float c = Mathf.Cos(angle);
		float s = Mathf.Sin(angle);
		return RotateVectorXZ(v, c, s);
	}

	public static Vector3 RotateVectorXZ(Vector3 v, float c, float s)
	{
		return new Vector3(v.x * c - v.z * s, v.y, v.x * s + v.z * c);
	}

	public static Vector2 GetDirFromAngle(float angle)
	{
		return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
	}

	public static float GetAngleBetweenNormalizedDirs(Vector2 a, Vector2 b)
	{
		return (float)Math.Acos(Mathf.Clamp(Vector2.Dot(a, b), 0f, 1f));
	}

	public static float GetAngleBetweenNormalizedDirs(Vector3 a, Vector3 b)
	{
		return (float)Math.Acos(Mathf.Clamp(Vector3.Dot(a, b), 0f, 1f));
	}

	public static float ClampAngleBetween0AndTwoPi(float angle)
	{
		return (angle + MathF.PI * 2f) % (MathF.PI * 2f);
	}

	public static int Clamp(int val, int min, int max)
	{
		return Math.Min(max, Math.Max(min, val));
	}

	public static int Sign(int val)
	{
		if (val >= 0)
		{
			if (val <= 0)
			{
				return 0;
			}
			return 1;
		}
		return -1;
	}

	public static Vector3 Clamp(Vector3 val, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(val.x, min.x, max.x), Mathf.Clamp(val.y, min.y, max.y), Mathf.Clamp(val.z, min.z, max.z));
	}

	public static float Delt(float cur, float target, float speed)
	{
		if (!(cur < target))
		{
			return Math.Max(cur - speed, target);
		}
		return Math.Min(cur + speed, target);
	}

	public static Vector2 SafeNormalize(Vector2 v, Vector2 fallback)
	{
		float magnitude = v.magnitude;
		if (magnitude < 0.001f)
		{
			return fallback;
		}
		return v / magnitude;
	}

	public static Vector3 SafeNormalize(Vector3 v, Vector3 fallback)
	{
		float magnitude = v.magnitude;
		if (magnitude < 0.001f)
		{
			return fallback;
		}
		return v / magnitude;
	}

	public static Vector3? GetPlaneIntersection(Plane a, Plane b, Plane c)
	{
		float num = Vector3.Dot(a.normal, Vector3.Cross(b.normal, c.normal));
		if (Math.Abs(num) < 0.001f)
		{
			return null;
		}
		return ((0f - a.distance) * Vector3.Cross(b.normal, c.normal) + (0f - b.distance) * Vector3.Cross(c.normal, a.normal) + (0f - c.distance) * Vector3.Cross(a.normal, b.normal)) / num;
	}

	public static Vector3? GetLinePlaneIntersection(Plane plane, Vector3 p0, Vector3 p1)
	{
		float distanceToPoint = plane.GetDistanceToPoint(p0);
		float distanceToPoint2 = plane.GetDistanceToPoint(p1);
		if (distanceToPoint * distanceToPoint2 >= 0f)
		{
			return null;
		}
		float t = Mathf.Abs(distanceToPoint) / (Mathf.Abs(distanceToPoint) + Mathf.Abs(distanceToPoint2));
		return Vector3.Lerp(p0, p1, t);
	}

	public static bool GetLineIntersection(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out float a, out float b)
	{
		float num = b1.y - b0.y;
		float num2 = b1.x - b0.x;
		float num3 = a1.y - a0.y;
		float num4 = a1.x - a0.x;
		float num5 = num * num4 - num2 * num3;
		if (Math.Abs(num5) > 0.001f)
		{
			float num6 = a0.x - b0.x;
			float num7 = a0.y - b0.y;
			float num8 = 1f / num5;
			a = (num2 * num7 - num * num6) * num8;
			b = (num4 * num7 - num3 * num6) * num8;
			return true;
		}
		a = 0f;
		b = 0f;
		return false;
	}

	public static Vector2? GetLineIntersectionPoint(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
	{
		if (GetLineIntersection(a0, a1, b0, b1, out var a2, out var _))
		{
			return a0 + a2 * (a1 - a0);
		}
		return null;
	}

	public static bool DoLinesIntersect(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
	{
		if (GetLineIntersection(a0, a1, b0, b1, out var a2, out var b2) && a2 >= 0f && a2 <= 1f && b2 >= 0f)
		{
			return b2 <= 1f;
		}
		return false;
	}

	public static bool GetRayLineIntersection(Vector2 start, Vector2 dir, Vector2 p0, Vector2 p1, out float t)
	{
		if (GetLineIntersection(start, start + dir, p0, p1, out t, out var b) && b >= 0f && b <= 1f && t >= 0f)
		{
			return true;
		}
		t = float.MaxValue;
		return false;
	}

	public static Vector2? GetRayRectIntersectionPoint(Vector2 start, Vector2 dir, Vector2 tl, Vector2 br)
	{
		float t = float.MaxValue;
		float t2 = float.MaxValue;
		float t3 = float.MaxValue;
		float t4 = float.MaxValue;
		GetRayLineIntersection(start, dir, new Vector2(tl.x, tl.y), new Vector2(br.x, tl.y), out t);
		GetRayLineIntersection(start, dir, new Vector2(br.x, tl.y), new Vector2(br.x, br.y), out t2);
		GetRayLineIntersection(start, dir, new Vector2(br.x, br.y), new Vector2(tl.x, br.y), out t3);
		GetRayLineIntersection(start, dir, new Vector2(tl.x, br.y), new Vector2(tl.x, tl.y), out t4);
		return start + dir * Math.Min(Math.Min(t, t2), Math.Min(t3, t4));
	}

	public static Vector3 GetClosestPointOnLineToPoint(Vector3 start, Vector3 end, Vector3 p)
	{
		Vector3 vector = start - end;
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			return start;
		}
		vector /= magnitude;
		float num = Vector3.Dot(p - start, vector);
		return start + num * vector;
	}

	public static float GetDistSqFromLineToPoint(Vector3 start, Vector3 end, Vector3 p)
	{
		Vector3 closestPointOnLineToPoint = GetClosestPointOnLineToPoint(start, end, p);
		return (p - closestPointOnLineToPoint).sqrMagnitude;
	}

	public static Vector3 GetClosestPointOnSegmentToPoint(Vector3 start, Vector3 end, Vector3 p, out float t)
	{
		Vector3 vector = end - start;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < 1E-08f)
		{
			t = 0f;
			return start;
		}
		t = Vector3.Dot(p - start, vector) / sqrMagnitude;
		t = Mathf.Clamp01(t);
		return start + t * vector;
	}

	public static float GetDistSqFromSegmentToPoint(Vector3 start, Vector3 end, Vector3 p, out float t)
	{
		Vector3 closestPointOnSegmentToPoint = GetClosestPointOnSegmentToPoint(start, end, p, out t);
		return (p - closestPointOnSegmentToPoint).sqrMagnitude;
	}

	public static Vector3 GetClosestPointOnRayToPoint(Vector3 start, Vector3 dir, Vector3 p)
	{
		float num = Vector3.Dot(p - start, dir);
		return start + num * dir;
	}

	public static Vector2 GetClosestPointOnLineToPoint(Vector2 start, Vector2 end, Vector2 p)
	{
		Vector2 vector = start - end;
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			return start;
		}
		vector /= magnitude;
		float num = Vector2.Dot(p - start, vector);
		return start + num * vector;
	}

	public static float GetDistSqFromLineToPoint(Vector2 start, Vector2 end, Vector2 p)
	{
		Vector2 closestPointOnLineToPoint = GetClosestPointOnLineToPoint(start, end, p);
		return (p - closestPointOnLineToPoint).sqrMagnitude;
	}

	public static Vector2 GetClosestPointOnSegmentToPoint(Vector2 start, Vector2 end, Vector2 p, out float t)
	{
		Vector2 vector = end - start;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude < 1E-08f)
		{
			t = 0f;
			return start;
		}
		t = Vector2.Dot(p - start, vector) / sqrMagnitude;
		t = Mathf.Clamp01(t);
		return start + t * vector;
	}

	public static float GetDistSqFromSegmentToPoint(Vector2 start, Vector2 end, Vector2 p, out float t)
	{
		Vector2 closestPointOnSegmentToPoint = GetClosestPointOnSegmentToPoint(start, end, p, out t);
		return (p - closestPointOnSegmentToPoint).sqrMagnitude;
	}

	public static float GetDistSqFromSegmentToPoint(Vector2 start, Vector2 end, Vector2 p)
	{
		float t;
		Vector2 closestPointOnSegmentToPoint = GetClosestPointOnSegmentToPoint(start, end, p, out t);
		return (p - closestPointOnSegmentToPoint).sqrMagnitude;
	}

	public static Vector2 GetClosestPointOnRayToPoint(Vector2 start, Vector2 dir, Vector2 p)
	{
		float num = Vector2.Dot(p - start, dir);
		return start + num * dir;
	}

	public static float GetPointDistFromRect(Vector2 pos, Vector2 tl, Vector2 br)
	{
		if (pos.x < tl.x)
		{
			if (pos.y < tl.y)
			{
				return (pos - tl).magnitude;
			}
			if (pos.y > br.y)
			{
				return (pos - new Vector2(tl.x, br.y)).magnitude;
			}
			return tl.x - pos.x;
		}
		if (pos.x > br.x)
		{
			if (pos.y < tl.y)
			{
				return (pos - new Vector2(br.x, tl.y)).magnitude;
			}
			if (pos.y > br.y)
			{
				return (pos - br).magnitude;
			}
			return pos.x - br.x;
		}
		if (pos.y < tl.y)
		{
			return tl.y - pos.y;
		}
		if (pos.y > br.y)
		{
			return pos.y - br.y;
		}
		return 0f;
	}

	public static Vector2 GetShortestVectorFromPointToRect(Vector2 pos, Vector2 tl, Vector2 br)
	{
		if (pos.x < tl.x)
		{
			if (pos.y < tl.y)
			{
				return tl - pos;
			}
			if (pos.y > br.y)
			{
				return new Vector2(tl.x, br.y) - pos;
			}
			return new Vector2(tl.x - pos.x, 0f);
		}
		if (pos.x > br.x)
		{
			if (pos.y < tl.y)
			{
				return new Vector2(br.x, tl.y) - pos;
			}
			if (pos.y > br.y)
			{
				return br - pos;
			}
			return new Vector2(br.x - pos.x, 0f);
		}
		if (pos.y < tl.y)
		{
			return new Vector2(0f, tl.y - pos.y);
		}
		if (pos.y > br.y)
		{
			return new Vector2(0f, br.y - pos.y);
		}
		return Vector2.zero;
	}

	public static bool DoesCircleIntersectRect(Vector2 pos, float radius, Vector2 tl, Vector2 br, out Vector2 normal, out float penetration)
	{
		if (pos.x < tl.x)
		{
			if (pos.y < tl.y)
			{
				Vector2 vector = pos - tl;
				float magnitude = vector.magnitude;
				normal = vector / magnitude;
				penetration = radius - magnitude;
				return penetration > 0f;
			}
			if (pos.y > br.y)
			{
				Vector2 vector2 = pos - new Vector2(tl.x, br.y);
				float magnitude2 = vector2.magnitude;
				normal = vector2 / magnitude2;
				penetration = radius - magnitude2;
				return penetration > 0f;
			}
			normal = new Vector2(-1f, 0f);
			penetration = radius - (tl.x - pos.x);
			return penetration > 0f;
		}
		if (pos.x > br.x)
		{
			if (pos.y < tl.y)
			{
				Vector2 vector3 = pos - new Vector2(br.x, tl.y);
				float magnitude3 = vector3.magnitude;
				normal = vector3 / magnitude3;
				penetration = radius - magnitude3;
				return penetration > 0f;
			}
			if (pos.y > br.y)
			{
				Vector2 vector4 = pos - br;
				float magnitude4 = vector4.magnitude;
				normal = vector4 / magnitude4;
				penetration = radius - magnitude4;
				return penetration > 0f;
			}
			normal = new Vector2(1f, 0f);
			penetration = radius - (pos.x - br.x);
			return penetration > 0f;
		}
		if (pos.y < tl.y)
		{
			normal = new Vector2(0f, -1f);
			penetration = radius - (tl.y - pos.y);
			return penetration > 0f;
		}
		if (pos.y > br.y)
		{
			normal = new Vector2(0f, 1f);
			penetration = radius - (pos.y - br.y);
			return penetration > 0f;
		}
		Vector2 vector5 = (tl + br) * 0.5f;
		Vector2 vector6 = pos - vector5;
		if (Math.Abs(vector6.x) >= Math.Abs(vector6.y))
		{
			if (vector6.x < 0f)
			{
				normal = new Vector2(-1f, 0f);
				penetration = radius + (pos.x - tl.x);
			}
			else
			{
				normal = new Vector2(1f, 0f);
				penetration = radius + (br.x - pos.x);
			}
		}
		else if (vector6.y < 0f)
		{
			normal = new Vector2(0f, -1f);
			penetration = radius + (pos.y - tl.y);
		}
		else
		{
			normal = new Vector2(0f, 1f);
			penetration = radius + (br.y - pos.y);
		}
		return true;
	}

	public static bool IsPointInRect(Vector2 p, Vector2 tl, Vector2 br)
	{
		if (p.x >= tl.x && p.y >= br.y && p.x <= br.x)
		{
			return p.y <= br.y;
		}
		return false;
	}

	public static bool IsTileInPolygon(TerrainCoord p, List<TerrainCoord> polygon)
	{
		bool flag = false;
		int num = 0;
		int index = polygon.Count - 1;
		while (num < polygon.Count)
		{
			if (polygon[num].y > p.y != polygon[index].y > p.y && p.x < (polygon[index].x - polygon[num].x) * (p.y - polygon[num].y) / (polygon[index].y - polygon[num].y) + polygon[num].x)
			{
				flag = !flag;
			}
			index = num++;
		}
		return flag;
	}

	public static bool IsTileOnPerimeterEdge(TerrainCoord p, List<TerrainCoord> polygon)
	{
		for (int i = 0; i < polygon.Count; i++)
		{
			TerrainCoord terrainCoord = polygon[i];
			TerrainCoord terrainCoord2 = polygon[(i + 1) % polygon.Count];
			if (terrainCoord.x == terrainCoord2.x && p.x == terrainCoord.x)
			{
				if (p.y >= Math.Min(terrainCoord.y, terrainCoord2.y) && p.y <= Math.Max(terrainCoord.y, terrainCoord2.y))
				{
					return true;
				}
			}
			else if (terrainCoord.y == terrainCoord2.y && p.y == terrainCoord.y)
			{
				if (p.x >= Math.Min(terrainCoord.x, terrainCoord2.x) && p.x <= Math.Max(terrainCoord.x, terrainCoord2.x))
				{
					return true;
				}
			}
			else if (terrainCoord2.x - terrainCoord.x == terrainCoord2.y - terrainCoord.y && p.x - terrainCoord.x == p.y - terrainCoord.y && p.x >= Math.Min(terrainCoord.x, terrainCoord2.x) && p.x <= Math.Max(terrainCoord.x, terrainCoord2.x) && p.y >= Math.Min(terrainCoord.y, terrainCoord2.y) && p.y <= Math.Max(terrainCoord.y, terrainCoord2.y))
			{
				return true;
			}
		}
		return false;
	}

	public static bool DoesSegmentIntersectRect(Vector2 a0, Vector2 a1, Vector2 tl, Vector2 br)
	{
		Vector2 vector = new Vector2(br.x, tl.y);
		Vector2 vector2 = new Vector2(tl.x, br.y);
		if (IsPointInRect(a0, tl, br))
		{
			return true;
		}
		if (IsPointInRect(a1, tl, br))
		{
			return true;
		}
		if (DoLinesIntersect(a0, a1, tl, vector))
		{
			return true;
		}
		if (DoLinesIntersect(a0, a1, vector, br))
		{
			return true;
		}
		if (DoLinesIntersect(a0, a1, br, vector2))
		{
			return true;
		}
		if (DoLinesIntersect(a0, a1, vector2, tl))
		{
			return true;
		}
		return false;
	}

	public static bool DoesSegmentIntersectRect(Vector2 a0, Vector2 a1, Vector2 tl, Vector2 br, ref float t)
	{
		Vector2 vector = new Vector2(br.x, tl.y);
		Vector2 vector2 = new Vector2(tl.x, br.y);
		if (IsPointInRect(a0, tl, br))
		{
			t = 0f;
			return true;
		}
		if (GetLineIntersection(a0, a1, tl, vector, out var a2, out var b) && a2 >= 0f && a2 < t && b >= 0f && b <= 1f)
		{
			t = a2;
		}
		if (GetLineIntersection(a0, a1, vector, br, out a2, out b) && a2 >= 0f && a2 < t && b >= 0f && b <= 1f)
		{
			t = a2;
		}
		if (GetLineIntersection(a0, a1, br, vector2, out a2, out b) && a2 >= 0f && a2 < t && b >= 0f && b <= 1f)
		{
			t = a2;
		}
		if (GetLineIntersection(a0, a1, vector2, tl, out a2, out b) && a2 >= 0f && a2 < t && b >= 0f && b <= 1f)
		{
			t = a2;
		}
		return false;
	}

	public static Vector3 InverseTransform(Vector3 position, ref Matrix4x4 matrix)
	{
		return InverseTransformNormal(position - matrix.Translation(), ref matrix);
	}

	public static Vector3 InverseTransformNormal(Vector3 normal, ref Matrix4x4 matrix)
	{
		Matrix4x4 result = Matrix4x4.zero;
		Matrix4x4.Inverse3DAffine(matrix, ref result);
		return result.MultiplyVector(normal);
	}

	public static Ray InverseTransform(this Ray ray, ref Matrix4x4 matrix)
	{
		Matrix4x4 result = Matrix4x4.zero;
		Matrix4x4.Inverse3DAffine(matrix, ref result);
		return new Ray(result.MultiplyVector(ray.origin - matrix.Translation()), result.MultiplyVector(ray.direction));
	}

	public static bool IntersectsOBB(this Ray ray, ref Matrix4x4 transform, Bounds box, out float dist)
	{
		Ray ray2 = ray.InverseTransform(ref transform);
		bool result = box.IntersectRay(ray2, out dist);
		dist *= transform.Forward().magnitude;
		return result;
	}

	public static bool IntersectsOBB(this Bounds box, ref Matrix4x4 transform, Bounds obb)
	{
		Vector3 rPos = transform.MultiplyPoint(obb.center) - box.center;
		Matrix4x4 transform2 = Matrix4x4.identity;
		if (!GetSeparatingPlane(rPos, transform2.Right(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, transform2.Up(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, transform2.Forward(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, transform.Right(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, transform.Up(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, transform.Forward(), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Right(), transform.Right()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Right(), transform.Up()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Right(), transform.Forward()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Up(), transform.Right()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Up(), transform.Up()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Up(), transform.Forward()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Forward(), transform.Right()), ref transform2, box, ref transform, obb) && !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Forward(), transform.Up()), ref transform2, box, ref transform, obb))
		{
			return !GetSeparatingPlane(rPos, Vector3.Cross(transform2.Forward(), transform.Forward()), ref transform2, box, ref transform, obb);
		}
		return false;
	}

	private static bool GetSeparatingPlane(Vector3 rPos, Vector3 planeNormal, ref Matrix4x4 transform1, Bounds box1, ref Matrix4x4 transform2, Bounds box2)
	{
		return Mathf.Abs(Vector3.Dot(rPos, planeNormal)) > Mathf.Abs(Vector3.Dot(transform1.Right() * box1.extents.x, planeNormal)) + Mathf.Abs(Vector3.Dot(transform1.Up() * box1.extents.y, planeNormal)) + Mathf.Abs(Vector3.Dot(transform1.Forward() * box1.extents.z, planeNormal)) + Mathf.Abs(Vector3.Dot(transform2.Right() * box2.extents.x, planeNormal)) + Mathf.Abs(Vector3.Dot(transform2.Up() * box2.extents.y, planeNormal)) + Mathf.Abs(Vector3.Dot(transform2.Forward() * box2.extents.z, planeNormal));
	}

	public static bool Intersects(this Plane plane, Bounds box)
	{
		bool side = plane.GetSide(box.center + box.extents);
		if (plane.GetSide(box.center - box.extents) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(box.extents.x, box.extents.y, 0f - box.extents.z)) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(box.extents.x, 0f - box.extents.y, 0f - box.extents.z)) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(0f - box.extents.x, box.extents.y, 0f - box.extents.z)) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(box.extents.x, 0f - box.extents.y, box.extents.z)) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(0f - box.extents.x, box.extents.y, box.extents.z)) != side)
		{
			return true;
		}
		if (plane.GetSide(box.center - new Vector3(0f - box.extents.x, 0f - box.extents.y, box.extents.z)) != side)
		{
			return true;
		}
		return false;
	}

	public static float? IntersectsTriangle(this Ray ray, Vector3 tri0, Vector3 tri1, Vector3 tri2, ref float barycentricU, ref float barycentricV)
	{
		Vector3 vector = tri1 - tri0;
		Vector3 vector2 = tri2 - tri0;
		Vector3 rhs = Vector3.Cross(ray.direction, vector2);
		float num = Vector3.Dot(vector, rhs);
		if (num < 0.0001f)
		{
			return null;
		}
		Vector3 lhs = ray.origin - tri0;
		barycentricU = Vector3.Dot(lhs, rhs);
		if (barycentricU < 0f || barycentricU > num)
		{
			return null;
		}
		Vector3 rhs2 = Vector3.Cross(lhs, vector);
		barycentricV = Vector3.Dot(ray.direction, rhs2);
		if (barycentricV < 0f || barycentricU + barycentricV > num)
		{
			return null;
		}
		float num2 = Vector3.Dot(vector2, rhs2);
		float num3 = 1f / num;
		float value = num2 * num3;
		barycentricU *= num3;
		barycentricV *= num3;
		return value;
	}

	public static float Norm(this Quaternion q)
	{
		return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
	}

	public static Quaternion Normalize(this Quaternion q)
	{
		float num = q.Norm();
		return new Quaternion(q.x / num, q.y / num, q.z / num, q.w / num);
	}

	public static void CalculateFrustumPlanes(Camera camera, Plane[] planeArray)
	{
		ExtractPlanes(planeArray, camera.projectionMatrix * camera.worldToCameraMatrix);
	}

	public static bool IsPointInFrustum(Vector3 p, Plane[] frustum)
	{
		for (int i = 0; i < frustum.Length; i++)
		{
			if (!frustum[i].GetSide(p))
			{
				return false;
			}
		}
		return true;
	}

	public static bool TestPlanesAABB(Plane[] planes, Bounds bounds)
	{
		for (int i = 0; i < planes.Length; i++)
		{
			Vector3 normal = planes[i].normal;
			if (Vector3.Dot(bounds.center + new Vector3(bounds.extents.x * Mathf.Sign(normal.x), bounds.extents.y * Mathf.Sign(normal.y), bounds.extents.z * Mathf.Sign(normal.z)), planes[i].normal) + planes[i].distance < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public static Vector3 WorldToScreenPoint(Vector3 wp, Matrix4x4 viewProjMatrix, int pixelWidth, int pixelHeight)
	{
		Vector4 vector = viewProjMatrix * new Vector4(wp.x, wp.y, wp.z, 1f);
		if (vector.w == 0f)
		{
			return Vector3.zero;
		}
		vector.x = (vector.x / vector.w + 1f) * 0.5f * (float)pixelWidth;
		vector.y = (vector.y / vector.w + 1f) * 0.5f * (float)pixelHeight;
		return new Vector3(vector.x, vector.y, wp.z);
	}

	public static uint JenkinsHash(uint a)
	{
		a = a + 2127912214 + (a << 12);
		a = a ^ 0xC761C23Cu ^ (a >> 19);
		a = a + 374761393 + (a << 5);
		a = (uint)((int)a + -744332180) ^ (a << 9);
		a = (uint)((int)a + -42973499) + (a << 3);
		a = a ^ 0xB55A4F09u ^ (a >> 16);
		return a;
	}

	public static uint WangHash(uint a)
	{
		a = a ^ 0x3D ^ (a >> 16);
		a += a << 3;
		a ^= a >> 4;
		a *= 668265261;
		a ^= a >> 15;
		return a;
	}

	public static int RandomInt(int seed, int maxValue)
	{
		return (int)(WangHash((uint)seed) % (uint)Math.Max(1, maxValue));
	}

	public static int RandomInt(int seed, int minValue, int maxValue)
	{
		return minValue + (int)(WangHash((uint)seed) % (uint)Math.Max(1, maxValue - minValue));
	}

	public static float RandomFloat(float seed)
	{
		return (float)(WangHash((uint)BitConverter.SingleToInt32Bits(seed)) % 1000001) * 1E-06f;
	}

	public static Vector2 RandomVec2InCircle(float seed)
	{
		return GetDirFromAngle(RandomFloat(seed) * (MathF.PI * 2f)) * RandomFloat(seed + 1000f);
	}

	public static Vector2 RandomVec2(Vector2 seed)
	{
		return new Vector2(RandomFloat(seed.x), RandomFloat(seed.y));
	}

	public static Vector3 RandomVec3(Vector3 seed)
	{
		return new Vector3(RandomFloat(seed.x), RandomFloat(seed.y), RandomFloat(seed.z));
	}

	public static bool RandomChoice(float seed, float probability)
	{
		return RandomFloat(seed) <= probability;
	}

	public static TerrainCoord RandomTile(int seed, TerrainCoord min, TerrainCoord max)
	{
		return new TerrainCoord(RandomInt(seed, min.x, max.x), RandomInt(seed + 1000, min.y, max.y));
	}

	public static float GetNormallyDistributedRand(float seed, int numSamples)
	{
		float num = 0f;
		for (int i = 0; i < numSamples; i++)
		{
			num += RandomFloat(seed + (float)(i * 17));
		}
		num /= (float)numSamples;
		return num;
	}

	public static TimeSpan FromSeconds(float seconds)
	{
		return TimeSpan.FromTicks((long)(10000000f * seconds));
	}

	public static TimeSpan FromMilliseconds(float milliseconds)
	{
		return TimeSpan.FromTicks((long)(10000f * milliseconds));
	}

	public static TimeSpan Min(TimeSpan a, TimeSpan b)
	{
		return TimeSpan.FromTicks(Math.Min(a.Ticks, b.Ticks));
	}

	public static TimeSpan Max(TimeSpan a, TimeSpan b)
	{
		return TimeSpan.FromTicks(Math.Max(a.Ticks, b.Ticks));
	}

	public static void CopyToList<T>(this List<T> from, List<T> to)
	{
		to.Clear();
		if (to.Capacity < from.Count)
		{
			to.Capacity = from.Count;
		}
		for (int i = 0; i < from.Count; i++)
		{
			to.Add(from[i]);
		}
	}

	public static void Resize<T>(this List<T> list, int size, T defaultVal)
	{
		while (list.Count < size)
		{
			list.Add(defaultVal);
		}
		if (list.Count > size)
		{
			list.RemoveRange(size, list.Count - size);
		}
	}

	public static void InsertionSort<T>(this IList<T> list, IComparer<T> comparer)
	{
		list.InsertionSort(0, list.Count, comparer);
	}

	public static void InsertionSort<T>(this IList<T> list, int start, int end, IComparer<T> comparer)
	{
		for (int i = start + 1; i < end; i++)
		{
			T val = list[i];
			int num = i - 1;
			while (num >= start && comparer.Compare(list[num], val) > 0)
			{
				list[num + 1] = list[num];
				num--;
			}
			list[num + 1] = val;
		}
	}

	public static float Gaussian(float x, float sigma)
	{
		return (float)Math.Exp((0f - x * x) / (2f * sigma * sigma)) / (float)Math.Sqrt(MathF.PI * 2f * sigma * sigma);
	}

	public static ushort EncodeLowPrecisionFloat(float value)
	{
		bool num = value < 0f;
		value = Math.Abs(value);
		value *= 32f;
		int val = (int)value;
		val = Math.Min(val, 32767);
		return (ushort)((num ? 32768 : 0) | val);
	}

	public static float DecodeLowPrecisionFloat(ushort value)
	{
		bool num = (value & 0x8000) != 0;
		float num2 = value & -32769;
		num2 /= 32f;
		if (num)
		{
			num2 = 0f - num2;
		}
		return num2;
	}

	public static DateTime UnixTimeStampToDateTime(uint unixTimeStamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
	}
}
