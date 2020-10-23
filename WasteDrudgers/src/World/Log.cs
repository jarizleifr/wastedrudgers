using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;
using WasteDrudgers.Data;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers
{
    public enum LoggingLevel
    {
        Never,
        Player,
        Visible,
        Global,
    }

    public struct LogMessage
    {
        public readonly string text;
        public readonly LoggingLevel level;
        public readonly Vec2 position;
        public readonly LogItem[] items;

        public LogMessage(DBLogMessage raw, Vec2 position, LogItem[] items)
        {
            text = raw.Message;
            level = raw.LoggingLevel;
            this.position = position;
            this.items = items;
        }
    }

    public abstract class LogItem
    {
        public static LogItem Actor(Entity entity) => new LogActor(entity);
        public static LogItem Item(Entity entity) => new LogThing(entity);
        public static LogItem Num(int num) => new LogNumber(num);
        public virtual bool PlayerInvolved(Entity playerEntity) => false;
        public abstract ColoredString Fetch(World world);
    }

    public class LogActor : LogItem
    {
        private Entity entity;

        public LogActor(Entity entity) => this.entity = entity;

        public override bool PlayerInvolved(Entity playerEntity) => entity == playerEntity;

        public override ColoredString Fetch(World world)
        {
            var renderable = world.ecs.GetRef<Renderable>(entity);
            var identity = world.ecs.GetRef<Identity>(entity);

            var arr = identity.name.ToCharArray();
            arr[0] = char.ToUpperInvariant(arr[0]);

            return new ColoredString(new string(arr), renderable.color);
        }
    }

    public class LogNumber : LogItem
    {
        private int num;
        public LogNumber(int num) => this.num = num;
        public override bool PlayerInvolved(Entity playerEntity) => false;

        public override ColoredString Fetch(World world) => new ColoredString(num.ToString(), world.database.GetColor("c_white"));
    }

    public class LogThing : LogItem
    {
        private Entity entity;

        public LogThing(Entity entity) => this.entity = entity;

        public override bool PlayerInvolved(Entity playerEntity) => false;

        public override ColoredString Fetch(World world)
        {
            var renderable = world.ecs.GetRef<Renderable>(entity);
            return new ColoredString(Items.GetFullName(world, entity), renderable.color);
        }
    }

    public class Log
    {
        public string Description { get; set; }

        private Queue<ColoredString> messageQueue;
        private List<ColoredString> buffer;

        public Log()
        {
            messageQueue = new Queue<ColoredString>();
            buffer = new List<ColoredString>();
        }

        public void Clear()
        {
            Description = null;
            messageQueue.Clear();
            buffer.Clear();
        }

        public bool Add(World world, LogMessage message)
        {
            var map = world.Map;
            var playerData = world.PlayerData;

            if (ShouldLogMessage(message, map, playerData.entity))
            {
                messageQueue.Enqueue(ProcessLogMessage(world, message));
                return true;
            }
            return false;
        }

        public bool HasMessages() => messageQueue.Count > 0;

        private ColoredString ProcessLogMessage(World world, LogMessage msg) =>
            new ColoredStringBuilder(msg.text)
                    .WithParams(msg.items.Select(i => i.Fetch(world)))
                    .Build();

        private bool ShouldLogMessage(LogMessage msg, Map map, Entity player) => msg.level switch
        {
            LoggingLevel.Global => true,
            LoggingLevel.Visible => map[msg.position].Visibility == Visibility.Visible,
            LoggingLevel.Player => msg.items.Any(i => i.PlayerInvolved(player)),
            _ => false
        };

        public bool BufferHasMessages() => buffer.Count > 0;
        public IEnumerable<ColoredString> GetBuffer() => buffer;

        public void UpdateMessageBuffer()
        {
            buffer.Clear();
            if (messageQueue.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    var line = GetMessage();
                    if (line != null)
                    {
                        buffer.Add(line);
                    }
                }
            }
        }

        private ColoredString GetMessage()
        {
            if (messageQueue.Count > 0)
            {
                // TODO: Get max from Config
                int max = 80;
                ColoredString line = ColoredString.Empty;
                ColoredString next = messageQueue.Peek();

                while (line.Length + next.Length + 1 < max && messageQueue.Count != 0)
                {
                    line += line.Length == 0 ? messageQueue.Dequeue() : " " + messageQueue.Dequeue();

                    if (messageQueue.Count != 0)
                    {
                        next = messageQueue.Peek();
                    }
                }
                return line;
            }
            return null;
        }
    }
}