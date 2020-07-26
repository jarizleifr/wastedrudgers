using System;

namespace WasteDrudgers.Entities
{
    public enum ItemType
    {
        Misc,
        ShortBlade,
        LongBlade,
        AxeMace,
        Polearm,
        Fencing,
        Whip,

        Shield,
        Helmet,
        Face,
        Cloak,
        Pauldron,
        Armor,
        Undershirt,
        Belt,
        Greaves,
        Boots,

        Gloves,
        Bracers,
        Earring,
        Amulet,
        Ring,

        Scroll,
        Book,
        Food,
        Potion
    }

    public static class ItemTypeExtensions
    {
        public static Slot GetEquipmentSlot(this ItemType type) => type switch
        {
            ItemType.ShortBlade => Slot.MainHand,
            ItemType.LongBlade => Slot.MainHand,
            ItemType.AxeMace => Slot.MainHand,
            ItemType.Polearm => Slot.MainHand,
            ItemType.Fencing => Slot.MainHand,
            ItemType.Whip => Slot.MainHand,

            ItemType.Shield => Slot.OffHand,
            ItemType.Helmet => Slot.Helmet,
            ItemType.Cloak => Slot.Cloak,
            ItemType.Armor => Slot.Armor,
            ItemType.Undershirt => Slot.Undershirt,
            ItemType.Belt => Slot.Belt,
            ItemType.Boots => Slot.Boots,
            ItemType.Gloves => Slot.Gloves,
            ItemType.Bracers => Slot.Bracers,
            ItemType.Earring => Slot.Earring,
            ItemType.Amulet => Slot.Amulet,
            _ => throw new Exception("Error, trying to equip an item of invalid type")
        };

        public static bool IsEquipable(this ItemType type) => type.IsWeapon() || type.IsApparel();

        public static bool IsWeapon(this ItemType type)
        {
            switch (type)
            {
                case ItemType.ShortBlade:
                case ItemType.LongBlade:
                case ItemType.AxeMace:
                case ItemType.Polearm:
                case ItemType.Fencing:
                case ItemType.Whip:
                case ItemType.Shield:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUseable(this ItemType type)
        {
            switch (type)
            {
                case ItemType.Potion:
                case ItemType.Food:
                case ItemType.Scroll:
                case ItemType.Book:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMisc(this ItemType type)
        {
            switch (type)
            {
                case ItemType.Misc:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsApparel(this ItemType type)
        {
            if (type.IsArmor()) return true;

            switch (type)
            {
                case ItemType.Amulet:
                case ItemType.Ring:
                case ItemType.Earring:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsArmor(this ItemType type)
        {
            switch (type)
            {
                case ItemType.Helmet:
                case ItemType.Cloak:
                case ItemType.Armor:
                case ItemType.Undershirt:
                case ItemType.Belt:
                case ItemType.Boots:
                case ItemType.Gloves:
                case ItemType.Bracers:
                    return true;
                default:
                    return false;
            }
        }

        public static SkillType GetWeaponSkill(this ItemType type) => type switch
        {
            ItemType.ShortBlade => SkillType.ShortBlade,
            ItemType.LongBlade => SkillType.LongBlade,
            ItemType.AxeMace => SkillType.AxeMace,
            ItemType.Polearm => SkillType.Polearm,
            ItemType.Fencing => SkillType.Fencing,
            ItemType.Whip => SkillType.Whip,
            _ => throw new Exception("Not a weapon!")
        };
    }

    public enum IdentificationStatus
    {
        Unknown,
        Examined,
        Identified,
    }
}