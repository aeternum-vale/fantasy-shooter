using UnityEngine;

namespace FantasyShooter
{
    public static class Constants
	{
		public const int TargetFrameRate = 60;

		public const int GroundLayer = 6;

		public static float DeltaTimeCorrection => Time.deltaTime * TargetFrameRate;

	}

}