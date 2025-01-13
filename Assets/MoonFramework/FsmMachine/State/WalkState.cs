using System.Threading;
using Cysharp.Threading.Tasks;
using MoonFramework.Model;
using MoonFramework.View;
using UnityEngine;

namespace MoonFramework.FSM
{
    public class WalkState : BaseState
    {
        private UniTaskCompletionSource<int> _completion;
        private bool _isRun;

        public WalkState(BaseFSM fsmMachine)
            : base(fsmMachine)
        {
        }

        public override void Entry()
        {
            _isRun = true;
            _completion = new UniTaskCompletionSource<int>();
            AnimancerManager.Instance.Play((fsmMachine.entity as Actor)?.GetType().Name, "Walk");
            Walk().Forget();
        }

        private async UniTaskVoid Walk()
        {
            while (_isRun)
            {
                (fsmMachine.entity as Actor)?.Walk(_completion);
                await UniTask.Yield();
            }
        }

        public override void Exit()
        {
            _isRun = false;
        }
    }
}