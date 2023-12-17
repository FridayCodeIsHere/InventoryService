using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Example
{
    public class InventoryExample : MonoBehaviour
    {
        private void Start()
        {
            InventoryConfig inventoryConfig = new InventoryConfig
            {
                InventorySize = new Vector2Int(3, 4),
                InventorySlotCapacity = 99
            };

            int inventorySize = inventoryConfig.InventorySize.x * inventoryConfig.InventorySize.y;
            InventoryData inventoryData = new InventoryData
            {
                Slots = new List<InventorySlotData>(inventorySize)
            };

            for (int i = 0; i < inventorySize; i++)
            {
                inventoryData.Slots.Add(new InventorySlotData());
            }

            InventoryService inventoryService = new InventoryService(inventoryData, inventoryConfig);
            inventoryService.PrintInventory();
            
            inventoryService.Add(InventoryItemType.Apple, 15);
            inventoryService.Add(InventoryItemType.Apple, 89);
            inventoryService.Add(InventoryItemType.Bread, 53);
            
            inventoryService.PrintInventory();

        }
    }
}