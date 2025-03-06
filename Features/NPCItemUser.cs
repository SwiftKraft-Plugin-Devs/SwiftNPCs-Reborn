using SwiftNPCs.Features.ItemBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftNPCs.Features
{
    public class NPCItemUser : NPCComponent
    {
        public ItemBehaviorBase CurrentItemBehavior
        {
            get => _currentItemBehavior;
            set
            {
                _currentItemBehavior?.End();
                _currentItemBehavior = value;
                _currentItemBehavior?.Begin();
            }
        }
        ItemBehaviorBase _currentItemBehavior;

        public override void Begin()
        {
            
        }

        public override void Tick()
        {
            CurrentItemBehavior?.Tick();
        }
    }
}
