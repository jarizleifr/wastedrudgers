using Blaggard.Common;
using Blaggard.FileIO;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.State
{
    public static class RunState
    {
        public static readonly IRunState MoreMessages = new MoreMessagesState();
        public static readonly IRunState AwaitingInput = new AwaitingInputState();

        public static readonly IRunState Ticking = new TickingState();

        public static IRunState MainMenu(int selection) => new MainMenuState { selection = selection };
        public static IRunState Config(int selection) => new ConfigState { selection = selection };
        public static IRunState Chargen(int selection) => new ChargenState { selection = selection };
        public static IRunState EscapeMenu(int selection) => new EscapeMenuState { selection = selection };

        public static IRunState NewGame(Skills skills) => new NewGameState { skills = skills };
        public static IRunState SaveGame(string saveName, IRunState nextState) => new SaveGameState { saveName = saveName, nextState = nextState };
        public static IRunState LoadGame(int selection) => new LoadGameState { selection = selection };

        public static IRunState Look(Vec2 coords) => new LookState { coords = coords };
        public static IRunState Inventory(int selected, int offset) => new InventoryState { selected = selected, offset = offset };
        public static IRunState PickUp(int selected, int offset) => new PickUpState { selected = selected, offset = offset };
        public static IRunState CharacterSheet() => new CharacterSheetState { };

        public static IRunState LevelGeneration(string levelName, Map map) => new LevelGenerationState { levelName = levelName, map = map };
        public static IRunState LevelTransition(string levelName) => new LevelTransitionState { levelName = levelName };

        public static readonly IRunState GameOver = new GameOverState();
    }

    public interface IRunState
    {
        string[] InputDomains => null;
        void Initialize(IContext ctx, World world) { }
        void Run(IContext ctx, World world) { }
        void Draw(IContext ctx, World world) { }
    }
}