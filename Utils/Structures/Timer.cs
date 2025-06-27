using SwiftNPCs.Utils.Interfaces;
using System;
using UnityEngine;

namespace SwiftNPCs.Utils.Structures
{
    public class Timer : IValue<float>, ITickable
    {
        [field: SerializeField]
        public float MaxValue { get; set; }

        public float PrevTickTime { get; private set; }

        public bool StartEnded;

        public virtual float CurrentValue
        {
            get
            {
                if (timeRemaining < 0f)
                    timeRemaining = 0f;
                return timeRemaining;
            }
            protected set
            {
                if (value > 0f)
                {
                    PrevTickTime = timeRemaining;
                    timeRemaining = value;
                }
                else
                    timeRemaining = 0f;
            }
        }

        public bool Ended
        {
            get => CurrentValue == 0f;
            set
            {
                if (!value || Ended)
                    return;

                CurrentValue = 0f;
                OnTimerEnd?.Invoke();
            }
        }

        public event Action OnTimerEnd;

        private float timeRemaining;

        public Timer() : this(0f) { }

        public Timer(float time, bool startEnded = true)
        {
            Reset(time);
            Ended = startEnded;
        }

        public virtual float Tick(float deltaTime)
        {
            if (Ended)
                return CurrentValue;

            CurrentValue -= deltaTime;

            if (CurrentValue == 0f)
                OnTimerEnd?.Invoke();

            return CurrentValue;
        }

        public bool IsPassingTime(float time) => CurrentValue == time || PrevTickTime >= time && CurrentValue <= time;

        public virtual void Reset(float time)
        {
            MaxValue = time;
            Reset();
        }

        public virtual void Reset() => CurrentValue = MaxValue;

        public float GetPercentage() => Mathf.InverseLerp(0f, MaxValue, CurrentValue);
    }
}
