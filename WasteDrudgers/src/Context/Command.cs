namespace WasteDrudgers
{
    public enum Command
    {
        None,

        MoveSouthWest,
        MoveSouth,
        MoveSouthEast,
        MoveWest,
        MoveEast,
        MoveNorthWest,
        MoveNorth,
        MoveNorthEast,

        MoveSouthWestRepeat,
        MoveSouthRepeat,
        MoveSouthEastRepeat,
        MoveWestRepeat,
        MoveEastRepeat,
        MoveNorthWestRepeat,
        MoveNorthRepeat,
        MoveNorthEastRepeat,

        GetItem,
        Look,

        Inventory,
        CharacterSheet,
        Manual,
        GameMenu,

        Wait,
        WaitExtended,
        Operate,
        CastMagic,

        TacticVitals,
        TacticAggressive,
        TacticNormal,
        TacticDefensive,

        SelectA,
        SelectB,
        SelectC,
        SelectD,
        SelectE,
        SelectF,
        SelectG,
        SelectH,

        MenuDown,
        MenuLeft,
        MenuRight,
        MenuUp,
        MenuAccept,
        MenuBack,
        MenuSwitch,

        DropItem,

        ToggleASCII,
        Exit
    }
}