using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonFramework.Model;
using MoonFramework.Template;
using MoonFramework.View;

namespace MoonFramework.FSM
{
    public class WalkState : BaseState
    {

        private CancellationTokenSource _token;
        private UniTaskCompletionSource<int> _completion;
        
        public WalkState(BaseFSM fsmMachine) 
            : base(fsmMachine)
        {
            _token = new();
            _completion = new();
            AnimancerManager.Instance.Play((fsmMachine.entity as Actor)?.GetType().Name, "Walk");
        }

        public override void Entry()
        {
            Walk().Forget();
        }

        private async UniTaskVoid Walk()
        {
            while (true)
            {
                await UniTask.Yield(_token.Token);
                (fsmMachine.entity as Actor)?.Walk(_completion);
            }
        }

        public override void Exit()
        {
            _token.Cancel();
            _token = null;
        }
    }
}