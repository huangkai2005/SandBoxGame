using UnityEditor;
using UnityEngine;
using LoggerManager = MoonFramework.Tool.LoggerManager;

namespace MoonFramework.Template
{
    public class GameRoot : BaseGameMono<GameRoot> 
    {
        /// <summary>
        /// 框架设置
        /// </summary>
        [SerializeField]
        private GameConfigSetting gameSetting;
        public GameConfigSetting GameSetting { get { return gameSetting; } }

        protected override void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject); 
                return;
            }
            LoggerManager.RegisterLog("Map");
            base.Awake();
            DontDestroyOnLoad(gameObject);
            InitManager();
        }

        private void InitManager()
        {
            BaseMonoManager[] managers = GetComponents<BaseMonoManager>();
            for (int i = 0; i < managers.Length; i++)
            {
                managers[i].Init();
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void InitForEditor()
        {
            // 当前是否要进行播放或准备播放中
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (Instance == null && GameObject.Find("GameRoot") != null)
            {
                Instance = GameObject.Find("GameRoot").GetComponent<GameRoot>();
                // 清空事件
                EventCenter.Instance.Clear();
                //Instance.GameSetting.InitForEditor();
            }
        }
#endif
    }
}
