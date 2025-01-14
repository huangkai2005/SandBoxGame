using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoonFramework
{
	public class UITips : MonoBehaviour
	{
		[SerializeField]
		private Text infoText;
		[SerializeField]
		private Animator animator;
		private readonly Queue<string> tipsQueue = new();
		private bool isShow = false;

		/// <summary>
		/// 添加提示
		/// </summary>
		public void AddTips(string info)
		{
			tipsQueue.Enqueue(info);
			ShowTips();
		}

		private void ShowTips()
		{
			if (tipsQueue.Count > 0 && !isShow)
			{
				infoText.text = tipsQueue.Dequeue();
				animator.Play("Show", 0, 0);
			}
		}

		#region 动画事件

		private void StartTips()
		{
			isShow = true;
		}

		private void EndTips()
		{
			isShow = false;
			ShowTips();
		}

		#endregion 动画事件
	}
}