using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoonFramework
{
    public class UITips : MonoBehaviour
    {
        [SerializeField] private Text infoText;

        [SerializeField] private Animator animator;

        private readonly Queue<string> tipsQue = new();
        private bool isShow;

        /// <summary>
        ///     ������ʾ
        /// </summary>
        public void AddTips(string info)
        {
            tipsQue.Enqueue(info);
            ShowTips();
        }

        private void ShowTips()
        {
            if (tipsQue.Count > 0 && !isShow)
            {
                infoText.text = tipsQue.Dequeue();
                animator.Play("Show", 0, 0);
            }
        }

        #region �����¼�

        private void StartTips()
        {
            isShow = true;
        }

        private void EndTips()
        {
            isShow = false;
            ShowTips();
        }

        #endregion �����¼�
    }
}