using System.Collections;
using UnityEngine;

namespace FantasyShooter
{
	public static class VectorExtensions
	{
		public static Vector2 WithX(this Vector2 vector, float x)
		{
			return new Vector2(x, vector.y);
		}

		public static Vector2 WithY(this Vector2 vector, float y)
		{
			return new Vector2(vector.x, y);
		}

		public static Vector3 WithX(this Vector3 vector, float x)
		{
			return new Vector3(x, vector.y, vector.z);
		}

		public static Vector3 WithY(this Vector3 vector, float y)
		{
			return new Vector3(vector.x, y, vector.z);
		}

		public static Vector3 WithZ(this Vector3 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}
		public static Vector2 WithIncX(this Vector2 vector, float xInc)
		{
			return new Vector2(vector.x + xInc, vector.y);
		}
		public static Vector2 WithIncY(this Vector2 vector, float yInc)
		{
			return new Vector2(vector.x, vector.y + yInc);
		}
		public static Vector3 WithIncX(this Vector3 vector, float xInc)
		{
			return new Vector3(vector.x + xInc, vector.y, vector.z);
		}
		public static Vector3 WithIncY(this Vector3 vector, float yInc)
		{
			return new Vector3(vector.x, vector.y + yInc, vector.z);
		}
		public static Vector3 WithIncZ(this Vector3 vector, float zInc)
		{
			return new Vector3(vector.x, vector.y, vector.z + zInc);
		}
	}
}