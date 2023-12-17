using UnityEngine;

namespace InventorySystem
{
    public struct InventoryEventArgs
    {
        public InventoryItemType Type { get; }
        public int Amount { get; }
        public Vector2Int  InventorySlotPosition { get; }
        
        public InventoryEventArgs(InventoryItemType type, int amount, Vector2Int inventorySlotPosition)
        {
            Type = type;
            Amount = amount;
            InventorySlotPosition = inventorySlotPosition;
        }
        
    }
}
