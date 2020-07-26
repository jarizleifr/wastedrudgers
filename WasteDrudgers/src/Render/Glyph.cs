using WasteDrudgers.Entities;

namespace WasteDrudgers.Render
{
    public static class GlyphUtil
    {
        public static char GetItemGlyph(IContext ctx, ItemType type) => ctx.Config.Style < GraphicsStyle.Glyphs
            ? type switch
            {
                ItemType.ShortBlade => 'Γ',
                ItemType.LongBlade => 'Γ',
                ItemType.AxeMace => 'Γ',
                ItemType.Polearm => 'Γ',
                ItemType.Fencing => 'Γ',
                ItemType.Whip => 'Γ',
                ItemType.Shield => 'Θ',
                ItemType.Cloak => '¶',
                ItemType.Pauldron => 'µ',
                ItemType.Armor => '¥',
                ItemType.Undershirt => 'φ',
                ItemType.Belt => '═',
                ItemType.Greaves => '.',
                ItemType.Boots => 'π',
                ItemType.Gloves => '‼',
                ItemType.Bracers => '≈',
                ItemType.Earring => 'δ',
                ItemType.Amulet => '♀',
                ItemType.Ring => 'ô',
                ItemType.Scroll => '?',
                ItemType.Book => '"',
                ItemType.Food => '%',
                ItemType.Potion => '¿',
                ItemType.Helmet => 'ⁿ',
                _ => '.',
            }
            : type switch
            {
                ItemType.ShortBlade => (char)272,
                ItemType.LongBlade => (char)272,
                ItemType.AxeMace => (char)272,
                ItemType.Polearm => (char)272,
                ItemType.Fencing => (char)272,
                ItemType.Whip => (char)272,
                ItemType.Shield => (char)274,
                ItemType.Cloak => (char)275,
                ItemType.Pauldron => (char)276,
                ItemType.Armor => (char)277,
                ItemType.Undershirt => (char)278,
                ItemType.Belt => (char)279,
                ItemType.Greaves => (char)280,
                ItemType.Boots => (char)281,
                ItemType.Gloves => (char)282,
                ItemType.Bracers => (char)283,
                ItemType.Earring => (char)284,
                ItemType.Amulet => (char)285,
                ItemType.Ring => (char)286,
                ItemType.Scroll => (char)287,
                ItemType.Book => (char)288,
                ItemType.Food => (char)289,
                ItemType.Potion => (char)290,
                ItemType.Helmet => (char)291,
                _ => (char)273,
            };

    }
}