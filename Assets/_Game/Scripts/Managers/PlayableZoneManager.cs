using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

namespace BHR
{
    public class PlayableZoneManager : ManagerSingleton<PlayableZoneManager>
    {
        public bool CanCheck = true;
        public override void Awake()
        {
            SetInstance(false);
            CanCheck = true;
        }

        public void DisableCheckAndEnableAfterTime(float duration)
        {
            CanCheck = false;
            Invoke("EnableCheck", duration);
        }

        private void EnableCheck()
        {
            CanCheck = true;
            //transform.GetChild(0).GetComponent<PlayableZone>().ForceCheckWithVariable();
        }
    }
}
