using System;
using System.Collections.Generic;
using Blaggard.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;
using WasteDrudgers.State;

namespace WasteDrudgers
{
    public partial class World
    {
        public readonly ManulECS.World ecs;

        public readonly Log log;
        public readonly FOV fov;
        public readonly EntityTable spatial;
        public readonly TurnQueue queue;

        public readonly Queue<Command> repeatActionQueue;

        public int GameTicks { get; private set; }
        public bool ShouldRedraw { get; set; }

        public IRunState State { get; private set; }

        private ECSResource<PlayerData> playerData;
        public PlayerData PlayerData
        {
            get => playerData.Get(ecs);
            set => playerData.Set(ecs, value);
        }

        private ECSResource<Map> map;
        public Map Map
        {
            get => map.Get(ecs);
            set => map.Set(ecs, value);
        }

        private ECSResource<Calendar> calendar;
        public Calendar Calendar
        {
            get => calendar.Get(ecs);
            set => calendar.Set(ecs, value);
        }

        private ECSResource<ObfuscatedNames> obfuscatedNames;
        public ObfuscatedNames ObfuscatedNames
        {
            get => obfuscatedNames.Get(ecs);
            set => obfuscatedNames.Set(ecs, value);
        }

        public World()
        {
            ecs = new ManulECS.World();
            DeclareComponents(ecs);

            log = new Log();
            fov = new FOV();
            spatial = new EntityTable();
            queue = new TurnQueue();

            repeatActionQueue = new Queue<Command>();
        }

        public void SetState(IContext ctx, IRunState nextState)
        {
            // Always initialize and redraw on state change
            if (nextState != null && (State == null || State.GetType() != nextState.GetType()))
            {
                nextState.Initialize(ctx, this);
                ShouldRedraw = true;
            }
            State = nextState;
        }

        public void Tick(IEngineContext ctx)
        {
            State.Run(ctx, this);
            //System.Console.WriteLine(GC.GetTotalMemory(false) / 1024);
        }

        public void IncrementGameTicks()
        {
            GameTicks++;
            Calendar.PassTime(0, 0, 2);
        }

        public void WriteToLog(string key, Vec2 position, params LogItem[] items)
        {
            // Trigger redraw if log was updated
            if (log.Add(this, new LogMessage(Data.GetMessage(key), position, items)))
            {
                ShouldRedraw = true;
            }
        }

        public void Clear()
        {
            playerData.Clear(ecs);
            map.Clear(ecs);
            queue.Clear();
            calendar.Clear(ecs);
            obfuscatedNames.Clear(ecs);
            ecs.Clear();
        }
    }
}