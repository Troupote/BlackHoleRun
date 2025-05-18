using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BHR
{
    public static class UtilitiesFunctions
    {
        #region UI


        public static string TimeFormat(float timeInSeconds)
        {
            int minutes = Mathf.Clamp(Mathf.FloorToInt(timeInSeconds / 60f), 0, 99);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int centiseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
            return string.Format("{0}:{1:00}:{2:00}", minutes, seconds, centiseconds);
        }

        public static void DisplayMedals(int medalObtained, LevelDataSO data, Image[] sprites, TextMeshProUGUI[] timesText = null)
        {
            for(int i = 0; i < sprites.Length; i++)
            {
                sprites[i].color = medalObtained >= i+1 ? Color.white : ModuleManager.Instance.HideMedalColor;
                if(timesText != null)
                {
                    timesText[i].text = TimeFormat(data.Times[(MedalsType)i+1]);
                    timesText[i].color = ModuleManager.Instance.HideMedalTextColor;
                }
            }
        }
        #endregion

        public static bool IsVectorBetween(Vector3 vector, Vector3 min, Vector3 max)
        {
            return
            (vector.x >= Mathf.Min(min.x, max.x) && vector.x <= Mathf.Max(min.x, max.x)) &&
            (vector.y >= Mathf.Min(min.y, max.y) && vector.y <= Mathf.Max(min.y, max.y)) &&
            (vector.z >= Mathf.Min(min.z, max.z) && vector.z <= Mathf.Max(min.z, max.z));
        }

        public static string ToLowerWithFirstUpper(string text) => char.ToUpper(text[0]) + text.Substring(1);
    }
}
