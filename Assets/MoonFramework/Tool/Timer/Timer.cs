using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MoonFramework.Tool
{
    /// <summary>
    ///     启动前需要重置Reset() Timer
    ///     定时器
    /// </summary>
    public class Timer
    {
        private float _elapsedTime;
        private bool _isPaused;
        private bool _isRunning;
        public float TimeLimit { get; private set; }

        public async UniTaskVoid Start(float timeLimit, Action timerCallback)
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _isPaused = false;
            TimeLimit = timeLimit;

            while (_elapsedTime < TimeLimit && _isRunning)
            {
                if (_isPaused)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                _elapsedTime += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            if (_isRunning)
                timerCallback?.Invoke();
            _isRunning = false;
        }

        /// <summary>
        ///     重置Timer
        /// </summary>
        public void Reset()
        {
            _elapsedTime = 0;
            _isPaused = false;
            _isRunning = false;
        }

        public void Change(float timeLimit)
        {
            TimeLimit = timeLimit;
        }

        /// <summary>
        ///     暂时停止Timer运行
        /// </summary>
        public void Pause()
        {
            if (_isRunning) _isPaused = true;
        }

        /// <summary>
        ///     恢复Timer运行
        /// </summary>
        public void Resume()
        {
            if (_isRunning && _isPaused) _isPaused = false;
        }
    }
}