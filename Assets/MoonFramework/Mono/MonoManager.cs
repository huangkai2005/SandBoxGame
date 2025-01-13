using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Mono的管理者
/// 1.声明周期函数
/// 2.事件
/// 3.协程
/// </summary>
namespace MoonFramework.Template
{
    public class MonoManager : BaseManager<MonoManager>
    {
        private readonly MonoController controller;

        public MonoManager()
        {
            //保证了MonoController对象的唯一性
            GameObject obj = new("MonoController");
            controller = obj.AddComponent<MonoController>();
        }

        /// <summary>
        ///     提供给外部 用于外部帧更新事件函数
        /// </summary>
        /// <param name="_fun"></param>
        public void SendUpdateCommand(Action _fun)
        {
            controller.SendCommand(_fun);
        }

        /// <summary>
        ///     提供给外部 用于移除外部帧更新事件函数
        /// </summary>
        /// <param name="_fun"></param>
        public void ReturnUpdateCommand(Action _fun)
        {
            controller.ReturnCommand(_fun);
        }

        /// <summary>
        ///     开启协程
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return controller.StartCoroutine(routine);
        }

        public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
        {
            return controller.StartCoroutine(methodName, value);
        }

        public Coroutine StartCoroutine(string methodName)
        {
            return controller.StartCoroutine(methodName);
        }
    }
}