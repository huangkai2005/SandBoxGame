using MoonFramework.Template;
using Serilog;

namespace MoonFramework.Tool
{
    public static class TimerManager
    {
        // 初始化 Serilog 日志记录器
        static TimerManager()
        {
            LoggerManager.RegisterLog("Timer");
        }

        /// <summary>
        ///     分配一个新的计时器。
        /// </summary>
        /// <param name="timerCallback">计时器回调方法。</param>
        /// <param name="dueTime">首次执行的延迟时间（毫秒）。</param>
        /// <param name="period">执行间隔时间（毫秒）。</param>
        /// <returns>配置好的计时器实例。</returns>
        public static Timer AllocateTimer()
        {
            return ObjPoolManager.Instance.Pop("Timer", () => new Timer());
        }


        /// <summary>
        ///     将计时器归还到对象池，并停止其操作。
        /// </summary>
        /// <param name="timer">要归还的计时器实例。</param>
        public static void PushTimer(Timer timer)
        {
            if (timer == null)
            {
                Log.Warning("尝试归还空的计时器实例。");
                return;
            }

            // 停止计时器
            timer.Reset();
            Log.Information("计时器已停止。");

            // 将计时器归还到对象池
            ObjPoolManager.Instance.Push("Timer", timer);
            Log.Information("计时器已归还到对象池。");
        }
    }
}