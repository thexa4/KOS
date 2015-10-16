﻿using UnityEngine;

namespace kOS.AddOns.InfernalRobotics
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class IRHandler : MonoBehaviour
    {
        public void Start()
        {
            IRWrapper.InitWrapper();
            if (IRWrapper.APIReady)
            {
                
            }
        }

        public void OnDestroy()
        {
            if (IRWrapper.APIReady)
            {
            }
        }
    }
}