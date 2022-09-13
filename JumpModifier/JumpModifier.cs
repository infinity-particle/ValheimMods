using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace JumpModifier
{
    [BepInPlugin("null.JumpModifier", "Jump Modifier", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class JumpModifier : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("null.JumpModifier");

        void Awake()
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
        class Jump_Patch
        {
            static void Prefix(ref float ___m_jumpForce)
            {
                Debug.Log($"Jump force: {___m_jumpForce}");
                ___m_jumpForce = 15;
                Debug.Log($"Modified jump force: {___m_jumpForce}");
            }
        }
    }
}