using Sirenix.OdinInspector;
using System;
using UnityEngine;
using MoonFramework.FSM;

namespace MoonFramework.GameTime
{
    //����
    public class MorningTime : BaseTimeState
    {}

    //����
    public class NoonTime : BaseTimeState
    {}

    //�ƻ�
    public class NightfallTime : BaseTimeState
    {
    }

    //����
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
        /// ��鲢�Ҽ���ʱ��
        /// </summary>
        /// <returns>�Ƿ��ڵ�ǰ״̬</returns>
        public bool CheckAndCalTime(float curTime, BaseTimeState nextState, out Quaternion rotation, out Color color, out float sunIntensity)
        {
            // 0~1֮��
            float ratio = 1f - (curTime / timeData.durationTime);
            rotation = Quaternion.Slerp(this.timeData.sunQuaternion, nextState.timeData.sunQuaternion, ratio);
            color = Color.Lerp(this.timeData.sunColor, nextState.timeData.sunColor, ratio);
            sunIntensity = Mathf.Lerp(this.timeData.sunIntensity, nextState.timeData.sunIntensity, ratio);
            // ���ʱ�����0���Ի��ڱ�״̬
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
        //����ʱ��
        public float durationTime;

        //����ǿ��
        public float sunIntensity;

        //������ɫ
        public Color sunColor;

        //̫���ĽǶ�
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
    /// ʱ��״̬��
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