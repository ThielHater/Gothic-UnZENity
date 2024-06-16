using System;
using System.Linq;
using GUZ.Core.Globals;
using UnityEngine;
using GUZ.Core;
using UnityEngine.SceneManagement;

namespace GUZ.Core.Manager
{
    public class XRDeviceSimulatorManager
    {
        [Obsolete] public static XRDeviceSimulatorManager I;

        private readonly bool _featureEnable;

        public XRDeviceSimulatorManager(GameConfiguration config)
        {
            I = this;
            _featureEnable = config.enableDeviceSimulator;
        }

        public void Init()
        {
            GlobalEventDispatcher.GeneralSceneLoaded.AddListener(delegate(GameObject playerGo)
            {
                AddXRDeviceSimulator();
            });
            GlobalEventDispatcher.MainMenuSceneLoaded.AddListener(AddXRDeviceSimulator);
        }

        public void AddXRDeviceSimulator()
        {
            if (!_featureEnable) return;

            var simulator = ResourceLoader.TryGetPrefabObject(PrefabType.XRDeviceSimulator);
            simulator.name = "XRDeviceSimulator - XRIT";
            SceneManager.GetActiveScene().GetRootGameObjects().Append(simulator);
        }
    }
}
