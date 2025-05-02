namespace BoomJam2025
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public class ButtonFirstClickManager : MonoBehaviour
    {
        private static ButtonFirstClickManager _instance;
        public static ButtonFirstClickManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ButtonFirstClickManager");
                    _instance = go.AddComponent<ButtonFirstClickManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 存储按钮组ID和是否首次点击的字典
        private Dictionary<string, bool> buttonGroupFirstClickStatus = new Dictionary<string, bool>();
        // 存储按钮组ID和对应的首次点击回调函数
        private Dictionary<string, Action> buttonGroupFirstClickCallbacks = new Dictionary<string, Action>();
        // 存储按钮ID到按钮组ID的映射
        private Dictionary<string, string> buttonToGroupMap = new Dictionary<string, string>();
        
        // 是否启用首次点击检测
        private bool isFirstClickDetectionEnabled = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 启用首次点击检测
        /// </summary>
        public void EnableFirstClickDetection()
        {
            isFirstClickDetectionEnabled = true;
        }

        /// <summary>
        /// 禁用首次点击检测
        /// </summary>
        public void DisableFirstClickDetection()
        {
            isFirstClickDetectionEnabled = false;
        }

        /// <summary>
        /// 注册按钮组及其首次点击回调函数
        /// </summary>
        /// <param name="groupId">按钮组的唯一标识符</param>
        /// <param name="callback">首次点击时要执行的回调函数</param>
        public void RegisterButtonGroup(string groupId, Action callback)
        {
            if (callback != null)
            {
                buttonGroupFirstClickCallbacks[groupId] = callback;
                // 在注册按钮组时初始化首次点击状态
                if (!buttonGroupFirstClickStatus.ContainsKey(groupId))
                {
                    buttonGroupFirstClickStatus[groupId] = true;
                    Debug.Log($"注册按钮组 {groupId}，初始化首次点击状态为 true");
                }
            }
        }

        /// <summary>
        /// 将按钮添加到按钮组
        /// </summary>
        /// <param name="buttonId">按钮的唯一标识符</param>
        /// <param name="groupId">按钮组的唯一标识符</param>
        public void AddButtonToGroup(string buttonId, string groupId)
        {
            buttonToGroupMap[buttonId] = groupId;
            Debug.Log($"添加按钮 {buttonId} 到组 {groupId}");
        }

        /// <summary>
        /// 检查按钮是否是首次点击，如果是则执行回调函数
        /// </summary>
        /// <param name="buttonId">按钮的唯一标识符</param>
        /// <returns>如果是首次点击返回true，否则返回false</returns>
        public bool CheckFirstClick(string buttonId)
        {
            if (!isFirstClickDetectionEnabled)
            {
                return false;
            }

            // 获取按钮所属的组
            if (!buttonToGroupMap.TryGetValue(buttonId, out string groupId))
            {
                Debug.LogWarning($"按钮 {buttonId} 未分配到任何组");
                return false;
            }

            // 确保按钮组状态已初始化
            if (!buttonGroupFirstClickStatus.ContainsKey(groupId))
            {
                buttonGroupFirstClickStatus[groupId] = true;
                Debug.Log($"初始化按钮组 {groupId} 的首次点击状态为 true");
            }

            bool isFirst = buttonGroupFirstClickStatus[groupId];
            if (isFirst)
            {
                buttonGroupFirstClickStatus[groupId] = false;
                Debug.Log($"按钮 {buttonId} 触发首次点击，组 {groupId} 状态更新为 false");
                // 如果是首次点击，执行回调函数
                if (buttonGroupFirstClickCallbacks.ContainsKey(groupId))
                {
                    buttonGroupFirstClickCallbacks[groupId]?.Invoke();
                }
            }
            return isFirst;
        }

        /// <summary>
        /// 重置所有按钮组的首次点击状态
        /// </summary>
        public void ResetAllButtonStatus()
        {
            buttonGroupFirstClickStatus.Clear();
        }

        /// <summary>
        /// 重置特定按钮组的首次点击状态
        /// </summary>
        /// <param name="groupId">按钮组的唯一标识符</param>
        public void ResetButtonGroupStatus(string groupId)
        {
            if (buttonGroupFirstClickStatus.ContainsKey(groupId))
            {
                buttonGroupFirstClickStatus[groupId] = true;
            }
        }

        /// <summary>
        /// 移除按钮组的首次点击回调函数
        /// </summary>
        /// <param name="groupId">按钮组的唯一标识符</param>
        public void RemoveButtonGroupCallback(string groupId)
        {
            if (buttonGroupFirstClickCallbacks.ContainsKey(groupId))
            {
                buttonGroupFirstClickCallbacks.Remove(groupId);
            }
        }

        /// <summary>
        /// 从按钮组中移除按钮
        /// </summary>
        /// <param name="buttonId">按钮的唯一标识符</param>
        public void RemoveButtonFromGroup(string buttonId)
        {
            if (buttonToGroupMap.ContainsKey(buttonId))
            {
                buttonToGroupMap.Remove(buttonId);
            }
        }
    }
} 