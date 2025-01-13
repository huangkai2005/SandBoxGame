using Cysharp.Threading.Tasks;

namespace MoonFramework.Model
{
    public interface IActor
    {
        public void Attack();
        public void Idle();
        public void Run();
        public void Walk(UniTaskCompletionSource<int> completionSource);
        public void Dead();
    }
}