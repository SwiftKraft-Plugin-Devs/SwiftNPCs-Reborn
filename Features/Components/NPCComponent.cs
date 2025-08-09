namespace SwiftNPCs.Features.Components
{
    public abstract class NPCComponent
    {
        public NPCCore Core { get; set; }

        public float DeltaTime => Core.DeltaTime;

        public abstract void Begin();
        public abstract void Tick();
        public virtual void Frame() { }
        public virtual void Close() { }
    }
}
