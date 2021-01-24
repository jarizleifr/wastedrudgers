using Blaggard.Common;
using WasteDrudgers.Level;

namespace WasteDrudgers.State
{
    public static class RunState
    {
        public static readonly Scene MoreMessages = new MoreMessagesState();
        public static readonly Scene AwaitingInput = new AwaitingInputState();

        public static readonly Scene Ticking = new TickingState();

        public static Scene MainMenu(int selection) => new MainMenuState { selection = selection };
        public static Scene Config(int selection) => new ConfigState { selection = selection };
        public static Scene Chargen => new ChargenState { };
        public static Scene EscapeMenu(int selection) => new EscapeMenuState { selection = selection };
        public static readonly Scene Manual = new ManualState { };

        public static Scene NewGame => new NewGameState { };
        public static Scene SaveGame(string saveName, Scene nextState) => new SaveGameState { saveName = saveName, nextState = nextState };
        public static Scene LoadGame(int selection) => new LoadGameState { selection = selection };

        public static Scene Look(Vec2 coords) => new LookState { coords = coords };
        public static Scene Inventory(int selected, int offset) => new InventoryState { selected = selected, offset = offset };
        public static Scene PickUp(int selected, int offset) => new PickUpState { selected = selected, offset = offset };
        public static Scene CharacterSheet() => new CharacterSheetState { };

        public static Scene RestMenu => new RestMenuState { };
        public static Scene CharacterUpgrade => new CharacterUpgradeState { };

        public static Scene LevelGeneration(string levelName, Map map, bool newGame = false) => new LevelGenerationState { levelName = levelName, map = map, newGame = newGame };
        public static Scene LevelTransition(string levelName) => new LevelTransitionState { levelName = levelName };

        public static readonly Scene GameOver = new GameOverState();
    }
}