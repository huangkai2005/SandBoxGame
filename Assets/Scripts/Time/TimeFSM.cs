using Sirenix.OdinInspector;
using System;
using UnityEngine;
using MoonFramework.FSM;

namespace MoonFramework.GameTime
{
    //早上
    public class MorningTime : BaseTimeState
    {}

    //中午
    public class NoonTime : BaseTimeState
    {}

    //黄昏
    public class NightfallTime : BaseTimeState
    {
    }

    //晚上
    public class NightTime : BaseTimeState
    {}

    public abstract class BaseTimeState : IState
    {
        public TimeData timeData;

        public void Init(TimeData timeData)
        {
            this.timeData = timeData;
        }

        /// <summary>
        /// 检查并且计算时间
        /// </summary>
        /// <returns>是否还在当前状态</returns>
        public bool CheckAndCalTime(float curTime, BaseTimeState nextState, out Quaternion rotation, out Color color, out float sunIntensity)
        {
            // 0~1之间
            float ratio = 1f - (curTime / timeData.durationTime);
            rotation = Quaternion.Slerp(this.timeData.sunQuaternion, nextState.timeData.sunQuaternion, ratio);
            color = Color.Lerp(this.timeData.sunColor, nextState.timeData.sunColor, ratio);
            sunIntensity = Mathf.Lerp(this.timeData.sunIntensity, nextState.timeData.sunIntensity, ratio);
            // 如果时间大于0所以还在本状态
            return curTime > 0;
        }

        public void Entry()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public struct TimeData
    {
        //持续时间
        public float durationTime;

        //阳光强度
        public float sunIntensity;

        //阳光颜色
        public Color sunColor;

        //太阳的角度
        [OnValueChanged(nameof(SetRotation))]
        public Vector3 sunRotation;
        [HideInInspector]
        public Quaternion sunQuaternion;

        private void SetRotation()
        {
            sunQuaternion = Quaternion.Euler(sunRotation);
        }
    }

    /// <summary>
    /// 时间状态机
    /// </summary>
    public class TimeFSM : BaseFSM
    {
        public string curState = "MorningTime";

        public TimeFSM(string curState, TimeManager time) : base(curState, time)
        {
            BaseTimeState[] states = new BaseTimeState[]
            {
                AllocateState("MorningTime", ()=>new MorningTime()) as BaseTimeState, 
                AllocateState("NoonTime", ()=> new NoonTime())as BaseTimeState,
                AllocateState("NightfallTime", ()=> new NightfallTime()) as BaseTimeState,
                AllocateState("NightTime", ()=> new NightTime()) as BaseTimeState
            };
            for (int i = 0; i < states.Length; i++)
            {
                states[i].timeData = ((TimeManager)entity).timeDatas[i];
            }
            AddTransition(nameof(MorningTime), nameof(NoonTime));
            AddTransition(nameof(NoonTime), nameof(NightfallTime));
            AddTransition(nameof(NightfallTime), nameof(NightTime));
            AddTransition(nameof(NightTime), nameof(MorningTime));
        }
    }
}