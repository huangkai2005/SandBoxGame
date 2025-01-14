using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoonFramework;
using MoonFramework.Template;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
namespace MoonFramework.Audio
{
    public class AudioManager : BaseMonoManager<AudioManager>
    {
        [SerializeField]
        private AudioSource BGAudioSource;

        // 场景中生效的所有特效音乐播放器
        private List<AudioSource> audioPlayList = new List<AudioSource>();

        #region 音量、播放控制
        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateAllAudioPlay")]
        private float globalVolume;
        public float GlobalVolume
        {
            get => globalVolume;
            set
            {
                if (globalVolume == value) return;
                globalVolume = value;
                UpdateAllAudioPlay();
            }
        }

        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateBgAudioPlay")]
        private float bgVolume;
        public float BGVolume
        {
            get => bgVolume;
            set
            {
                if (bgVolume == value) return;
                bgVolume = value;
                UpdateBgAudioPlay();
            }
        }

        [SerializeField]
        [Range(0, 1)]
        [OnValueChanged("UpdateEffectAudioPlay")]
        private float effectVolume;
        public float EffectlVolume
        {
            get => effectVolume;
            set
            {
                if (effectVolume == value) return;
                effectVolume = value;
                UpdateEffectAudioPlay();
            }
        }

        [SerializeField]
        [OnValueChanged("UpdateMute")]
        private bool isMute = false;
        public bool IsMute
        {
            get => isMute;
            set
            {
                if (isMute == value) return;
                isMute = value;
                UpdateMute();
            }
        }

        [SerializeField]
        [OnValueChanged("UpdateLoop")]
        private bool isLoop = true;
        public bool IsLoop
        {
            get => isLoop;
            set
            {
                if (isLoop == value) return;
                isLoop = value;
                UpdateLoop();
            }
        }

        private bool _isPause = false;
        public bool IsPause
        {
            get => _isPause;
            set
            {
                if (_isPause == value) return;
                _isPause = value;
                if (_isPause)
                {
                    BGAudioSource.Pause();
                }
                else
                {
                    BGAudioSource.UnPause();
                }
                UpdateEffectAudioPlay();
            }
        }

        /// <summary>
        /// 更新全部播放器类型
        /// </summary>
        private void UpdateAllAudioPlay()
        {
            UpdateBgAudioPlay();
            UpdateEffectAudioPlay();
        }

        /// <summary>
        /// 更新背景音乐
        /// </summary>
        private void UpdateBgAudioPlay()
        {
            BGAudioSource.volume = bgVolume * globalVolume;
        }

        /// <summary>
        /// 更新特效音乐播放器
        /// </summary>
        private void UpdateEffectAudioPlay()
        {
            // 倒序遍历
            for (int i = audioPlayList.Count - 1; i >= 0; i--)
            {
                if (audioPlayList[i] != null)
                {
                    SetEffectAudioPlay(audioPlayList[i]);
                }
                else
                {
                    audioPlayList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 设置特效音乐播放器
        /// </summary>
        private void SetEffectAudioPlay(AudioSource audioPlay, float spatial = -1)
        {
            audioPlay.mute = isMute;
            audioPlay.volume = effectVolume * globalVolume;
            if (!Mathf.Approximately(spatial, -1))
            {
                audioPlay.spatialBlend = spatial;
            }
            if (_isPause)
            {
                audioPlay.Pause();
            }
            else
            {
                audioPlay.UnPause();
            }
        }

        /// <summary>
        /// 更新背景音乐静音情况
        /// </summary>
        private void UpdateMute()
        {
            BGAudioSource.mute = isMute;
            UpdateEffectAudioPlay();
        }

        /// <summary>
        /// 更新背景音乐循环
        /// </summary>
        private void UpdateLoop()
        {
            BGAudioSource.loop = isLoop;

        }
        #endregion

        public override void Init()
        {
            base.Init();
            UpdateAllAudioPlay();
        }

        #region 背景音乐
        public void PlayBgAudio(AudioClip clip, bool loop = true, float volume = -1)
        {
            BGAudioSource.clip = clip;
            IsLoop = loop;
            if (!Mathf.Approximately(volume, -1))
            {
                BGVolume = volume;
            }
            BGAudioSource.Play();
        }
        public async UniTaskVoid PlayBGAudio(string clipPath, bool loop = true, float volume = -1)
        {
            AudioClip clip = await ResourceManager.Instance.LoadAsync<AudioClip>(clipPath);
            PlayBgAudio(clip, loop, volume);
        }
        #endregion

        #region 特效音乐

        /// <summary>
        /// 获取音乐播放器
        /// </summary>
        /// <returns></returns>
        private async UniTask<AudioSource> GetAudioPlay(bool is3D = true)
        {
            // 从对象池中获取播放器
            AudioSource audioSource = await GameObjPoolManager.Instance.Pop("") as AudioSource;
            SetEffectAudioPlay(audioSource, is3D ? 1f : 0f);
            audioPlayList.Add(audioSource);
            return audioSource;
        }

        /// <summary>
        /// 回收播放器
        /// </summary>
        private void RecycleAudioPlay(AudioSource audioSource, AudioClip clip, UnityAction callBak, float time)
        {
            DoRecycleAudioPlay(audioSource, clip, callBak, time).Forget();
        }

        private async UniTaskVoid DoRecycleAudioPlay(AudioSource audioSource, AudioClip clip, UnityAction callBak, float time)
        {
            // 延迟 Clip的长度（秒）
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length));
            // 放回池子
            if (audioSource)
            {
                //TODO:路径进行统一
                this.MoonGameObjPushPool(audioSource, "");
                // 回调 延迟 time（秒）时间
                await UniTask.Delay(TimeSpan.FromSeconds(time));
                callBak?.Invoke();
            }
        }

        /// <summary>
        /// 播放一次特效音乐
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量 0-1</param>
        /// <param name="is3d">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后执行</param>
        /// <param name="callBacKTime">回调函数在音乐播放完成后执行的延迟时间</param>
        public async void PlayOnShot(AudioClip clip, Component component, float volumeScale = 1, bool is3d = true, UnityAction callBack = null, float callBacKTime = 0)
        {
            // 初始化音乐播放器
            AudioSource audioSource = await GetAudioPlay(is3d);
            audioSource.transform.SetParent(component.transform);
            audioSource.transform.localPosition = Vector3.zero;

            // 播放一次音效
            audioSource.PlayOneShot(clip, volumeScale);

            // 播放器回收以及回调函数
            RecycleAudioPlay(audioSource, clip, callBack, callBacKTime);
        }

        /// <summary>
        /// 播放一次特效音乐
        /// </summary>
        /// <param name="clip">音效片段</param>
        /// <param name="position">播放的位置</param>
        /// <param name="volumeScale">音量 0-1</param>
        /// <param name="is3d">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后执行</param>
        /// <param name="callBacKTime">回调函数在音乐播放完成后执行的延迟时间</param>
        public async void PlayOnShot(AudioClip clip, Vector3 position, float volumeScale = 1, bool is3d = true, UnityAction callBack = null, float callBacKTime = 0)
        {
            // 初始化音乐播放器
            AudioSource audioSource = await GetAudioPlay(is3d);
            audioSource.transform.position = position;

            // 播放一次音效
            audioSource.PlayOneShot(clip, volumeScale);
            // 播放器回收以及回调函数
            RecycleAudioPlay(audioSource, clip, callBack, callBacKTime);
        }

        /// <summary>
        /// 播放一次特效音乐
        /// </summary>
        /// <param name="clipPath">音效路径</param>
        /// <param name="component">挂载组件</param>
        /// <param name="volumeScale">音量 0-1</param>
        /// <param name="is3d">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后执行</param>
        /// <param name="callBacKTime">回调函数在音乐播放完成后执行的延迟时间</param>
        public async UniTaskVoid PlayOnShot(string clipPath, Component component, float volumeScale = 1, bool is3d = true, UnityAction callBack = null, float callBacKTime = 0)
        {
            AudioClip audioClip = await ResourceManager.Instance.LoadAsync<AudioClip>(clipPath);
            if (audioClip != null) PlayOnShot(audioClip, component, volumeScale, is3d, callBack, callBacKTime);
        }

        /// <summary>
        /// 播放一次特效音乐
        /// </summary>
        /// <param name="clipPath">音效路径</param>
        /// <param name="position">播放的位置</param>
        /// <param name="volumeScale">音量 0-1</param>
        /// <param name="is3d">是否3D</param>
        /// <param name="callBack">回调函数-在音乐播放完成后执行</param>
        /// <param name="callBacKTime">回调函数在音乐播放完成后执行的延迟时间</param>
        public async UniTaskVoid PlayOnShot(string clipPath, Vector3 position, float volumeScale = 1, bool is3d = true, UnityAction callBack = null, float callBacKTime = 0)
        {
            AudioClip audioClip = await ResourceManager.Instance.LoadAsync<AudioClip>(clipPath);
            if (audioClip != null) PlayOnShot(audioClip, position, volumeScale, is3d, callBack, callBacKTime);
        }
        #endregion
    }
}