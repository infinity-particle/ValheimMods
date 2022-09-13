using BepInEx;
using HarmonyLib;
using System.Diagnostics;
using UnityEngine;

namespace ContainerSort
{
    [BepInPlugin("null.ContainerSort", "Container Sort", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerSort : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("null.ContainerSort");

        void Awake()
        {
            harmony.PatchAll();
        }
    }
}
