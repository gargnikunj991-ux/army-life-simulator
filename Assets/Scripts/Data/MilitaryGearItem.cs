using System;

namespace ArmyLife.Data
{
    public enum GearCategory
    {
        Armor,          // Reduces incoming wound damage & mortality risk
        Weapon,         // Grants combat bonus & special tactical dilemma choices
        Medical,        // Consumables to patch wounds or save comrades
        Morale          // Consumables or lucky charms to boost morale & squad respect
    }

    [Serializable]
    public class MilitaryGearItem
    {
        public string id;
        public string itemName;
        public string description;
        public GearCategory category;
        public long price;
        public float casualtyReductionPercent; // e.g. 0.15f for 15% lower casualty chance
        public float combatBonus;              // e.g. +8 combat skill
        public bool isConsumable;
        public int count = 1;

        public MilitaryGearItem() {}

        public MilitaryGearItem(string id, string name, string desc, GearCategory cat, long cost, float casualtyReduction = 0f, float combatBonus = 0f, bool consumable = false, int count = 1)
        {
            this.id = id;
            this.itemName = name;
            this.description = desc;
            this.category = cat;
            this.price = cost;
            this.casualtyReductionPercent = casualtyReduction;
            this.combatBonus = combatBonus;
            this.isConsumable = consumable;
            this.count = count;
        }
    }
}
