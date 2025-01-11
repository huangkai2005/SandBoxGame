using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoonFramework.Template
{
	public class UIManager : BaseMonoManager<UIManager>
	{
		/// <summary>
		/// 元素资源库
		/// </summary>
		public Dictionary<Type, UIElement> UIElements { get { return GameRoot.Instance.GameSetting.UIElements; } }
		[SerializeField] private UILayer[] uiLayers;

		// 提示窗
		[SerializeField]
		private UITips UITips;

		public void AddTips(string info)
		{
			UITips.AddTips(info);
		}

		#region 内部类

		[Serializable]
		private class UILayer
		{
			public Transform root;
			public Image maskImage;
			private int count = 0;

			public void OnShow()
			{
				count++;
				Update();
			}

			public void OnClose()
			{
				count--;
				Update();
			}

			private void Update()
			{
				maskImage.raycastTarget = count != 0;
				int posIndex = root.childCount - 2;
				maskImage.transform.SetSiblingIndex(posIndex < 0 ? 0 : posIndex);
			}
		}

		#endregion 内部类
		public T Show<T>(int layer = -1)where T : BaseUIWindow
		{
			Type type = typeof(T);
			if(UIElements.TryGetValue(type , out var info))
			{
				int layerNum = layer == -1 ? info.layerNum : layer;

				// 实例化实例或者获取到实例，保证窗口实例存在
				if (info.objInstance != null)
				{
					info.objInstance.gameObject.SetActive(true);
					info.objInstance.transform.SetParent(uiLayers[layerNum].root);
					info.objInstance.transform.SetAsLastSibling();
					info.objInstance.OnShow();
				}
				else
				{
					BaseUIWindow window = Instantiate(info.prefab, uiLayers[layerNum].root).GetComponent<BaseUIWindow>();
					info.objInstance = window;
					window.Init();
					window.OnShow();
				}
				info.layerNum = layerNum;
				uiLayers[layerNum].OnShow();
				return info.objInstance as T;
			}
			return null;
		}

		/// <summary>
		/// 关闭窗口
		/// </summary>
		/// <typeparam name="T">窗口类型</typeparam>
		public void Close<T>()
		{
			Close(typeof(T));
		}

		/// <summary>
		/// 关闭窗口
		/// </summary>
		/// <typeparam name="Type">窗口类型</typeparam>
		public void Close(Type type)
		{
			if (UIElements.TryGetValue(type, out var info))
			{
				if (info.objInstance == null) return;

				// 缓存则隐藏
				if (info.isCache)
				{
					info.objInstance.gameObject.SetActive(false);
				}
				// 不缓存则销毁
				else
				{
					Destroy(info.objInstance);
					info.objInstance = null;
				}
				uiLayers[info.layerNum].OnClose();
			}
		}

		/// <summary>
		/// 关闭全部窗口
		/// </summary>
		public void CloseAll()
		{
			// 处理缓存中所有状态的逻辑
			var enumerator = UIElements.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.objInstance.Close();
			}
		}
	}
}