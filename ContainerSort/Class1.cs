using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerSortPlugin
{
    [BepInPlugin("null.ContainerSort", "Container Sort", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerSortPlugin : BaseUnityPlugin
    {
        public static ContainerSortPlugin instance;
        private Harmony harmony; /*= new Harmony("null.ContainerSort");*/

        internal GameObject sortButton;
        internal GameObject containerButton;

        internal ConfigEntry<bool> ShouldAutoStack;
        internal ConfigEntry<string> GamepadJoystickSortKey;
        internal ConfigEntry<UnityEngine.KeyCode> SortKeyCode;

        int playerHeight = 0;
        int containerHeight = 0;

        const float BUTTON_WIDTH = 60f;
        const float BUTTON_HEIGHT = 20f;

        void Awake()
        {
            //harmony.PatchAll();
            instance = this;
            harmony = Harmony.CreateAndPatchAll(typeof(InventoryGuiPatch));
            Logger.LogInfo("Patched InventoryGui");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Destroy(containerButton);
        }

        void SortContainer()
        {
            Inventory inventory = InventoryGui.instance.m_container.GetComponentInChildren<InventoryGrid>().GetInventory();
            var toSort = inventory.GetAllItems().OrderBy((item) => item.m_shared.m_name);
        }

        GameObject GetSortButton()
        {
            if (sortButton != null)
                return sortButton;


            Logger.LogDebug("SortButton is null. Creating new button...");

            GameObject obj = Instantiate(InventoryGui.instance.m_takeAllButton).gameObject;
            if (obj is null)
            {
                Logger.LogError($"SortButton couldn't be instantiated.");
                return null;
            }

            Logger.LogDebug("Button created. Assigning text...");
            obj.name = "Sort";
            obj.GetComponentInChildren<Text>().text = "Sort";
            //obj.GetComponent<RectTransform>().sizeDelta = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

            Logger.LogDebug("Text assigned. Add listeners...");

            var btn = obj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.interactable = true;
            obj.SetActive(false);

            //var input = obj.GetComponent<UIGamePad>();
            //input.m_keyCode = SortKeyCode.Value;
            //input.m_zinputKey = GamepadJoystickSortKey.Value;
            //// TODO: Find a way to get text for the hint.
            //input.m_hint.SetActive(false);
            //input.m_hint = null;
            return obj;
        }

        GameObject MakeContainerButton()
        {
            var bkg = InventoryGui.instance.m_container.Find("Bkg");
            var takeAllButton = InventoryGui.instance.m_container.Find("TakeAll");

            GameObject obj = Instantiate(GetSortButton(), bkg);
            obj.transform.localPosition = new Vector3((obj.transform.parent as RectTransform).rect.xMax - BUTTON_WIDTH, (obj.transform.parent as RectTransform).rect.yMax - BUTTON_HEIGHT, 0);

            var btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(SortContainer);
            obj.SetActive(true);
            return obj;
        }

        public void RebuildContainerButton(Container container, bool force = false)
        {
            if (container is null)
                return;

            var newSize = container.GetInventory().GetHeight();

            if (newSize == containerHeight && !force)
                return;

            Logger.LogInfo(force ? "Container sort button being forcibly rebuilt." : $"Player opened container of height {newSize}, was {containerHeight}, rebuilding sort button.");

            if (containerButton != null)
                Destroy(containerButton);

            containerButton = MakeContainerButton();

            newSize = containerHeight;
        }
    }

    [HarmonyPatch(typeof(InventoryGui))]
    public class InventoryGuiPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Show")]
        internal static void PostfixShow(Container container) => ContainerSortPlugin.instance?.RebuildContainerButton(container);
    }
}
