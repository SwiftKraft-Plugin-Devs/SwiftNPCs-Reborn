using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using SwiftNPCs.Commands;
using SwiftNPCs.Features;
using System;
using System.Collections.Generic;

namespace SwiftNPCs
{
    public class Core : Plugin
    {
        public static Version CurrentVersion = new(2, 0, 0, 0);

        public static List<IEvents> Events = [];

        public static Core Instance { get; private set; }

        public override string Name => "SwiftNPCs Reborn";

        public override string Description => "A vanilla friendly NPCs mod for LabAPI.";

        public override string Author => "SwiftKraft";

        public override Version Version => CurrentVersion;

        public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

        public override LoadPriority Priority => LoadPriority.Highest;

        public override void Enable()
        {
            Instance = this;
            NPCParentCommand.SetPrompt();
        }

        public override void Disable()
        {
            foreach (IEvents e in Events)
                e.Unsubscribe();

            NPCManager.RemoveAll();
        }
    }

    public interface IEvents
    {
        void Unsubscribe();
    }

    public abstract class EventClassBase : IEvents
    {
        public EventClassBase() => Core.Events.Add(this);

        public abstract void Unsubscribe();

        public void Remove() => Core.Events.Remove(this);
    }
}
