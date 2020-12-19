using System.Collections.Generic;
using System.Linq;
using WasteDrudgers.Entities;

namespace WasteDrudgers.UI
{
    public class CharacterSheetData
    {
        public List<DBTalent> PickedTalents { get; private set; }

        public CharacterSheetData(World world)
        {
            var player = world.PlayerData.entity;
            var talents = Talents.GetOwnedTalentIds(world, player);

            PickedTalents = talents
                .Select((id) => Data.GetTalent(id))
                .ToList();
        }
    }
}