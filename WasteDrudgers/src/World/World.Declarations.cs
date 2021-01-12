namespace WasteDrudgers
{
    public partial class World
    {
        private static void DeclareComponents(ManulECS.World ecs)
        {
            // Markers
            ecs.Declare<PlayerMarker>();
            ecs.Declare<PlayerInitiated>();

            // General components
            ecs.Declare<Identity>();
            ecs.Declare<Position>();
            ecs.Declare<Renderable>();
            ecs.Declare<Death>();

            // Intentions
            ecs.Declare<IntentionMove>();
            ecs.Declare<IntentionOperate>();
            ecs.Declare<IntentionAttack>();
            ecs.Declare<IntentionGetItem>();
            ecs.Declare<IntentionUseItem>();

            // Events
            ecs.Declare<EventMoved>();
            ecs.Declare<EventActed>();
            ecs.Declare<EventStatsUpdated>();
            ecs.Declare<EventInventoryUpdated>();
            ecs.Declare<EventEffectsUpdated>();
            ecs.Declare<EventStatusUpdated>();

            // Actor components
            ecs.Declare<Actor>();
            ecs.Declare<AI>();
            ecs.Declare<Player>();
            ecs.Declare<Turn>();
            ecs.Declare<Faction>();

            ecs.Declare<Stats>();
            ecs.Declare<Skills>();
            ecs.Declare<Pools>();
            ecs.Declare<Experience>();

            ecs.Declare<HungerClock>();

            ecs.Declare<Attack>();
            ecs.Declare<Defense>();

            // Item components
            ecs.Declare<Item>();
            ecs.Declare<Armor>();
            ecs.Declare<Shield>();

            ecs.Declare<Obfuscated>();
            ecs.Declare<InBackpack>();
            ecs.Declare<Equipped>();

            ecs.Declare<CastOnUse>();

            ecs.Declare<CastOnStrike>();
            ecs.Declare<NaturalAttack>();

            // Feature components
            ecs.Declare<Feature>();
            ecs.Declare<EntryTrigger>();
            ecs.Declare<Portal>();

            // Effect components
            ecs.Declare<VisualEffect>();
            ecs.Declare<Damage>();

            // TODO: Are these REALLY necessary? We could just include type in ActiveEffect
            // Is it worth having these filters or is going through all ActiveEffects fast enough?
            // I mean iterating through all ActiveEffects is just a span operation...
            ecs.Declare<IsTalent>();
            ecs.Declare<IsTrait>();

            ecs.Declare<ActiveEffect>();

            ecs.Declare<Duration>();

            ecs.Declare<Afflictions>();
        }
    }
}