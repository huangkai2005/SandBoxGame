using UnityEngine;

namespace MoonFramework.Template
{
    public class MusicManager : BaseManager<MusicManager>
    {
        private readonly float musicVolum = 1;

        //背景音乐
        private AudioSource backgroundMusic;

        //音效
        //private 

        public MusicManager()
        {
            MonoManager.Instance.SendUpdateCommand(Update);
        }

        private void Update()
        {
        }

        public async void PlayBackgroundMusic(string name)
        {
            if (backgroundMusic == null)
            {
                GameObject obj = new()
                {
                    name = "BackgroundMusic"
                };
                backgroundMusic = obj.AddComponent<AudioSource>();
            }

            //异步加载背景音乐
            await ResourceManager.Instance.LoadAsync<AudioClip>($"Music/Background{name}", clip =>
            {
                backgroundMusic.clip = clip;
                backgroundMusic.volume = musicVolum;
                backgroundMusic.Play();
            });
        }

        /// <summary>
        ///     改变音量大小
        /// </summary>
        /// <param name="v"></param>
        public void ChangeBackgroundMusic(float v)
        {
            if (backgroundMusic == null)
                return;
            backgroundMusic.volume = v;
        }

        /// <summary>
        ///     暂停播放音乐
        /// </summary>
        public void PauseBackgroundMusic()
        {
            if (backgroundMusic == null)
                return;
            backgroundMusic.Pause();
        }

        /// <summary>
        ///     停止播放音乐
        /// </summary>
        public void StopBackgroudMusic()
        {
            if (backgroundMusic == null)
                return;
            backgroundMusic.Stop();
        }
    }
}