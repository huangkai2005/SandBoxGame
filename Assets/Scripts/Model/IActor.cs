using Cysharp.Threading.Tasks;

namespace MoonFramework.Model
{
    public interface IActor
    {
        public abstract void Attack();
        public abstract void Idle();
        public abstract void Run();
        public abstract void Walk(UniTaskCompletionSource<int> completionSource);
        public abstract void Dead();
    }
}