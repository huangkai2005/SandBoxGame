using Sirenix.OdinInspector;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MoonFramework.FSM;
using UnityEngine.Rendering.PostProcessing;

namespace MoonFramework.GameTime
{
    //����
    public class MorningTime : BaseTimeState
    {
        public override void Entry()
        {
            RenderSettings.fog = true;
            Fog().Forget();
        }

        private async UniTaskVoid Fog()
        {
            while (RenderSettings.fog)
            {
                RenderSettings.fogDensity = 0.1f - (1 - ratio);
                await UniTask.Yield();
            }
        }

        public override void Exit()
        {
            RenderSettings.fog = false;
        }
    }

    //����
    public class NoonTime : BaseTimeState
    {
        public override void Entry()
        {
            
        }   

        public override void Exit()
        {
            
        }
    }

    //�ƻ�
    public class NightfallTime : BaseTimeState
    {
        public override void Entry()
        {
            
        }

        public override void Exit()
        {
            
        }
    }

    //����
    public class NightTime : BaseTimeState
    {
        public override void Entry()
        {
            
        }

        public override void Exit()
        {
            
        }
    }

    public abstract class BaseTimeState : IState
    {
        public TimeData timeData;
        protected float ratio;
        public TimeFSM fsmMachine;

        public void Init(TimeData timeData, TimeFSM timeFsm)
        {
            this.timeData = timeData;
            fsmMachine = timeFsm;
        }

        /// <summary>
        /// ��鲢�Ҽ���ʱ��
        /// </summary>
        /// <returns>�Ƿ��ڵ�ǰ״̬</returns>
        public bool CheckAndCalTime(float curTime, String nextStateName, out Quaternion rotation, out Color color, out float sunIntensity)
        {
            var nextState = fsmMachine.fsmStates[nextStateName] as BaseTimeState;
            // 0~1֮��
            ratio = 1f - (curTime / timeData.durationTime);
            rotation = Quaternion.Slerp(timeData.sunQuaternion, nextState.timeData.sunQuaternion, ratio);
            color = Color.Lerp(timeData.sunColor, nextState.timeData.sunColor, ratio);
            sunIntensity = Mathf.Lerp(timeData.sunIntensity, nextState.timeData.sunIntensity, ratio);
            // ���ʱ�����0���Ի��ڱ�״̬
            return curTime > 0;
        }

        public abstract void Entry();

        public abstract void Exit();
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
        
        public AudioClip BgAudioClip; //��������

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
                states[i].Init(((TimeManager)entity).timeDatas[i], this);
            }
            AddTransition(nameof(MorningTime), nameof(NoonTime));
            AddTransition(nameof(NoonTime), nameof(NightfallTime));
            AddTransition(nameof(NightfallTime), nameof(NightTime));
            AddTransition(nameof(NightTime), nameof(MorningTime));
            fsmStates[curState].Entry();
        }
    }
}