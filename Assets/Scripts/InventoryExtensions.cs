namespace InventorySystem
{
    public static class InventoryExtensions
    {
        public static bool IsEmpty(this InventorySlotData slotData)
        {
            return slotData.Type == InventoryItemType.None || slotData.Amount == 0;
        }

        public static void Clean(this InventorySlotData slotData)
        {
            slotData.Amount = 0;
            slotData.Type = InventoryItemType.None;
        }
    }
}