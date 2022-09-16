using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerSortPlugin
{
    [BepInPlugin("null.ContainerSort", "Container Sort", "1.0.0")]
    [BepInProcess("valheim.exe")]

    public class ContainerSortPlugin : BaseUnityPlugin
    {
        public static ContainerSortPlugin instance;
        private Harmony harmony;

        internal Button sortButton;

        void Awake()
        {
            if(CreateContainerSortButton())
            {
                instance = this;
                harmony = Harmony.CreateAndPatchAll(typeof(InventoryGuiPatch));
                Logger.LogInfo("Patched InventoryGui");
            }
            else
            {
                Logger.LogError("Can't create sort button!");
            }
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Destroy(sortButton);
        }

        void SortContainer()
        {
            Inventory inventory = InventoryGui.instance.m_container.GetComponentInChildren<InventoryGrid>().GetInventory();
            List<ItemDrop.ItemData> sortedItemList = inventory.GetAllItems().OrderBy((item) => item.m_shared.m_name).ToList();

            inventory.RemoveAll();

            foreach (var item in sortedItemList)
            {
                inventory.AddItem(item);
            }
        }

        bool CreateContainerSortButton()
        {
            Button takeAllButton = InventoryGui.instance.m_takeAllButton;
            sortButton = Instantiate(takeAllButton, takeAllButton.transform.parent);

            if (sortButton)
            {
                sortButton.name = "Sort";
                sortButton.GetComponentInChildren<Text>().text = "Sort";

                sortButton.onClick.AddListener(SortContainer);
                sortButton.interactable = true;

                sortButton.gameObject.SetActive(true);
            }
            else
            {
                Logger.LogError($"SortButton couldn't be instantiated.");
            }

            return !(sortButton is null);
        }

        void UpdateSortButton()
        {
            RectTransform sortButtonRect = (RectTransform)sortButton.transform;
            var x = (sortButton.transform.parent as RectTransform).rect.width - sortButtonRect.rect.width / 2;

            sortButton.transform.localPosition = new Vector3(x, sortButton.transform.localPosition.y, sortButton.transform.localPosition.z);
        }

        public void RebuildContainerButton(Container container)
        {
            if (container is null)
                return;

            UpdateSortButton();
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
