using UnityEngine;

namespace BHR
{
    public static class UtilitiesFunctions
    {
        public static string TimeFormat(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int centiseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
            return string.Format("{0}:{1:00}:{2:00}", minutes, seconds, centiseconds);
        }
    }
}
