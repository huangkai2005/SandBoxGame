using Cysharp.Threading.Tasks;

namespace MoonFramework.Model
{
    public interface IActor
    {
        public void Attack(UniTaskCompletionSource<bool> completionSource);
        public void Idle();
        public void Run();
        public void Walk();
        public void Dead();
    }
}