using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem
{
    public class InventoryService
    {
        public event Action<InventoryEventArgs> OnItemsAdded;
        public event Action<InventoryEventArgs> OnItemsRemoved;
        public event Action<InventoryItemType, int> OnItemsDropped;
        
        private readonly InventoryData inventoryData;
        private readonly InventoryConfig inventoryConfig;

        public InventoryService(InventoryData inventoryData, InventoryConfig inventoryConfig)
        {
            this.inventoryData = inventoryData;
            this.inventoryConfig = inventoryConfig;
        }

        public void Add(InventoryItemType itemType, int amount = 1)
        {
            int remainingAmount = amount;
            
            this.AddToSlotsWithSameItems(itemType, remainingAmount, out remainingAmount);
            if (remainingAmount <= 0)
            {
                return;
            }
            
            this.AddToFirstAvailableSlot(itemType, remainingAmount, out remainingAmount);
            if (remainingAmount > 0)
            {
                this.OnItemsDropped?.Invoke(itemType, remainingAmount);
            }
        }

        public void Add(Vector2Int slotPosition, InventoryItemType itemType, int amount = 1)
        {
            int rowLength = this.inventoryConfig.InventorySize.x;
            int slotIndex = slotPosition.x + rowLength * slotPosition.y;
            InventorySlotData slotData = this.inventoryData.Slots[slotIndex];
            int newValue = slotData.Amount + amount;
            if (slotData.IsEmpty())
            {
                slotData.Type = itemType;
            }

            if (newValue > this.inventoryConfig.InventorySlotCapacity)
            {
                int remainingItems = newValue - this.inventoryConfig.InventorySlotCapacity;
                int itemsToAddAmount = this.inventoryConfig.InventorySlotCapacity - slotData.Amount;
                slotData.Amount = this.inventoryConfig.InventorySlotCapacity;
                this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, itemsToAddAmount, slotPosition));
                this.Add(itemType, remainingItems);
            }
            else
            {
                slotData.Amount = newValue;
                this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, amount, slotPosition));
            }
        }

        public bool Remove(InventoryItemType itemType, int amount = 1, bool invokeDrop = true)
        {
            if (!Has(itemType, amount))
            {
                return false;
            }

            int amountToRemove = amount;
            Vector2Int size = this.inventoryConfig.InventorySize;
            int rowLength = size.x;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int slotPosition = new Vector2Int(i, j);
                    InventorySlotData slotData = inventoryData.Slots[slotPosition.x + rowLength * slotPosition.y];
                    if (slotData.Type != itemType)
                    {
                        continue;
                    }

                    if (amountToRemove > slotData.Amount)
                    {
                        amountToRemove -= slotData.Amount;
                        this.Remove(slotPosition, itemType, slotData.Amount, invokeDrop);
                    }
                    else
                    {
                        Remove(slotPosition, itemType, amountToRemove, invokeDrop);
                        return true;
                    }
                }
            }
            return true;
        }
        
        public bool Remove(Vector2Int slotPosition, InventoryItemType itemType, int amount = 1, bool invokeDrop = true)
        {
            Vector2Int size = this.inventoryConfig.InventorySize;
            int rowLength = size.x;
            InventorySlotData slotData = this.inventoryData.Slots[slotPosition.x + rowLength * slotPosition.y];
            
            if (slotData.IsEmpty() || slotData.Type != itemType || slotData.Amount < amount)
            {
                return false;
            }

            slotData.Amount -= amount;
            if (slotData.Amount == 0)
            {
                slotData.Clean();
            }
            
            this.OnItemsRemoved?.Invoke(new InventoryEventArgs(itemType, amount, slotPosition));
            if (invokeDrop)
            {
                this.OnItemsDropped?.Invoke(itemType, amount);
            }

            return true;
        }

        public bool Has(InventoryItemType itemType, int amount = 1)
        {
            IEnumerable<InventorySlotData> slots = this.inventoryData.Slots.Where(slot => slot.Type == itemType);
            int sumExists = 0;
            foreach (InventorySlotData slotData in slots)
            {
                sumExists += slotData.Amount;
            }

            return sumExists >= amount;
        }

        public void PrintInventory()
        {
            string line = "";
            Vector2Int size = this.inventoryConfig.InventorySize;
            int rowLength = size.x;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int position = new Vector2Int(i, j);
                    InventorySlotData slotData = this.inventoryData.Slots[position.x + rowLength * position.y];

                    line += $"Slot ({i}, {j}): ItemType = {slotData.Type}, amount: {slotData.Amount} ";
                }

                line += "\n";
            }
            
            Debug.Log(line);
        }

        private void AddToSlotsWithSameItems(InventoryItemType itemType, int amount, out int remainingAmount)
        {
            Vector2Int size = this.inventoryConfig.InventorySize;
            int rowLength = size.x;
            remainingAmount = amount;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int slotPosition = new Vector2Int(i, j);
                    InventorySlotData slot = this.inventoryData.Slots[slotPosition.x + rowLength * slotPosition.y];

                    if (slot.IsEmpty())
                    {
                        continue;
                    }

                    if (slot.Amount >= this.inventoryConfig.InventorySlotCapacity)
                    {
                        continue;
                    }

                    if (slot.Type != itemType)
                    {
                        continue;
                    }

                    int newValue = slot.Amount + remainingAmount;
                    if (newValue > this.inventoryConfig.InventorySlotCapacity)
                    {
                        remainingAmount = newValue - this.inventoryConfig.InventorySlotCapacity;
                        int itemsToAddAmount = this.inventoryConfig.InventorySlotCapacity - slot.Amount;
                        slot.Amount = this.inventoryConfig.InventorySlotCapacity;
                        this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, itemsToAddAmount, slotPosition));
                    }
                    else
                    {
                        slot.Amount = newValue;
                        int itemsToAddAmount = remainingAmount;
                        remainingAmount = 0;
                        this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, itemsToAddAmount, slotPosition));
                        return;
                    }
                }
            }
            
        }

        private void AddToFirstAvailableSlot(InventoryItemType itemType, int amount, out int remainingAmount)
        {
            Vector2Int size = this.inventoryConfig.InventorySize;
            int rowLength = size.x;
            remainingAmount = amount;

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    Vector2Int slotPosition = new Vector2Int(i, j);
                    InventorySlotData slotData = this.inventoryData.Slots[slotPosition.x + rowLength * slotPosition.y];

                    if (!slotData.IsEmpty())
                    {
                        continue;
                    }

                    slotData.Type = itemType;
                    int newValue = remainingAmount;
                    if (newValue > this.inventoryConfig.InventorySlotCapacity)
                    {
                        remainingAmount = newValue - this.inventoryConfig.InventorySlotCapacity;
                        int itemsToAddAmount = this.inventoryConfig.InventorySlotCapacity;
                        slotData.Amount = this.inventoryConfig.InventorySlotCapacity;
                        
                        this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, itemsToAddAmount, slotPosition));
                    }
                    else
                    {
                        slotData.Amount = newValue;
                        int itemsToAddAmount = remainingAmount;
                        remainingAmount = 0;
                        this.OnItemsAdded?.Invoke(new InventoryEventArgs(itemType, itemsToAddAmount, slotPosition));
                        return;
                    }

                }
            }
            
        }
        
    }
}