using System.Collections.Generic;
using Blaggard.Graphics;
using ManulECS;
using WasteDrudgers.Entities;

namespace WasteDrudgers
{
    public enum LoggingLevel
    {
        Never,
        Player,
        Visible,
        Global,
    }

    public static class LogArgs
    {
        public static ILogArg Actor(Entity entity) => new LogActor { Entity = entity };
        public static ILogArg Item(Entity entity) => new LogItem { Entity = entity };
        public static ILogArg Num(int num) => new LogNumber { Number = num };
    }

    public interface ILogArg
    {
        bool PlayerInvolved(Entity player) => false;
        ColoredString ToColoredString(World world);
    }

    public class LogActor : ILogArg
    {
        public Entity Entity { get; init; }

        public bool PlayerInvolved(Entity playerEntity) => Entity == playerEntity;

        public ColoredString ToColoredString(World world)
        {
            var renderable = world.ecs.GetRef<Renderable>(Entity);
            var identity = world.ecs.GetRef<Identity>(Entity);

            var arr = identity.name.ToCharArray();
            arr[0] = char.ToUpperInvariant(arr[0]);

            return new ColoredString(new string(arr), renderable.color);
        }
    }

    public class LogNumber : ILogArg
    {
        public int Number { get; init; }

        public ColoredString ToColoredString(World _) =>
            new ColoredString(Number.ToString(), Data.Colors.white);
    }

    public class LogItem : ILogArg
    {
        public Entity Entity { get; init; }

        public ColoredString ToColoredString(World world)
        {
            var renderable = world.ecs.GetRef<Renderable>(Entity);
            return new ColoredString(Items.GetFullName(world, Entity), renderable.color);
        }
    }

    public class Log
    {
        public string Description { get; set; }

        private Queue<ColoredTextSpan> messageQueue;
        private List<ColoredTextSpan[]> buffer;

        public Log()
        {
            messageQueue = new Queue<ColoredTextSpan>();
            buffer = new List<ColoredTextSpan[]>();
        }

        public void Clear()
        {
            Description = null;
            messageQueue.Clear();
            buffer.Clear();
        }

        public void Add(ColoredTextSpan message) =>
            messageQueue.Enqueue(message);

        public bool HasMessages() => messageQueue.Count > 0;
        public bool BufferHasMessages() => buffer.Count > 0;

        public List<ColoredTextSpan[]> GetBuffer() => buffer;

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

        private ColoredTextSpan[] GetMessage()
        {
            if (messageQueue.Count > 0)
            {
                // TODO: Get max from Config
                int max = 80;

                int lineLength = 0;
                List<ColoredTextSpan> line = new List<ColoredTextSpan>();
                ColoredTextSpan next = messageQueue.Peek();

                while (lineLength + next.Length + 1 < max && messageQueue.Count != 0)
                {
                    lineLength += next.Length;
                    line.Add(messageQueue.Dequeue());

                    if (messageQueue.Count != 0)
                    {
                        next = messageQueue.Peek();
                    }
                }
                return line.ToArray();
            }
            return null;
        }
    }
}