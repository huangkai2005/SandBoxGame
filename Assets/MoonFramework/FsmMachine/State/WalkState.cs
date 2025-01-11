using System;
using Cysharp.Threading.Tasks;

namespace MoonFramework.FSM
{
    public class WalkState : BaseState
    {
        public WalkState(BaseFSM fsmMachine,ref AutoResetUniTaskCompletionSource uniTaskToken) 
            : base(fsmMachine,ref uniTaskToken)
        {
        }

        public override void Entry()
        {
            
        }

        public override void Exit()
        {
            
        }
    }
}