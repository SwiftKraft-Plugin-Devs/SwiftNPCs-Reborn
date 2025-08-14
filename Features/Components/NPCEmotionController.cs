using LabApi.Events.Handlers;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;

namespace SwiftNPCs.Features.Components
{
    public class NPCEmotionController : NPCComponent
    {
        public EmotionSubcontroller Emotion { get; private set; }

        public override void Begin()
        {
            RefreshController();
            PlayerEvents.ChangedRole += OnPlayerRoleChanged;
        }

        private void OnPlayerRoleChanged(LabApi.Events.Arguments.PlayerEvents.PlayerChangedRoleEventArgs ev)
        {
            if (ev.Player != Core.NPC.WrapperPlayer)
                return;

            RefreshController();
        }

        public void RefreshController() => Emotion = Core.GetComponentInChildren<EmotionSubcontroller>();

        public override void Tick() { }

        public void SetEmotion(EmotionPresetType type)
        {
            if (Emotion == null)
                return;

            Emotion.SetPreset(type);
        }

        public void SetEmotion(EmotionPreset preset)
        {
            if (Emotion == null)
                return;

            Emotion.ResetWeights();
            preset.SetWeights(Emotion.SetWeight);
        }
    }
}
