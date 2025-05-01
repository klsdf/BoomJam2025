using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Yarn.Unity.Example;

namespace BoomJam2025
{
    /// <summary>
    /// 视觉小说对话管理器
    /// 使用单例模式确保全局只有一个实例
    /// </summary>
    public class VNDialogueManager : MonoBehaviour
    {
        #region Singleton
        public static VNDialogueManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        [Header("对话组件")]
        [SerializeField] private VNManager vnManager;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private bool autoStartDialogue = true;
        [SerializeField] private string startNode = "VNDialogue";

        private bool isInitialized = false;
        private bool isDialogueRunning = false;

        private void Start()
        {
            if (vnManager == null)
            {
                vnManager = FindObjectOfType<VNManager>();
            }

            if (vnManager != null)
            {
                InitializeDialogueSystem();
            }
            else
            {
                Debug.LogError("VNManager not found in scene!");
            }
        }

        private void InitializeDialogueSystem()
        {
            if (isInitialized) return;

            // 初始化对话面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            isInitialized = true;

            // 自动开始对话
            if (autoStartDialogue)
            {
                StartCoroutine(StartDialogueDelayed());
            }
        }

        private IEnumerator StartDialogueDelayed()
        {
            // 等待一帧确保所有组件都初始化完成
            yield return null;
            StartDialogue(startNode);
        }

        /// <summary>
        /// 设置所有对话UI的启用状态
        /// </summary>
        /// <param name="active">是否启用</param>
        public void SetDialogueUIsActive(bool active)
        {
            // 设置主对话面板
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(active);
            }
        }

        /// <summary>
        /// 开始对话
        /// </summary>
        /// <param name="nodeName">对话节点名称</param>
        public void StartDialogue(string nodeName)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("Dialogue system is not initialized yet!");
                return;
            }

            if (isDialogueRunning)
            {
                Debug.LogWarning("Dialogue is already running!");
                return;
            }

            // 显示对话面板
            SetDialogueUIsActive(true);

            // 开始对话
            if (vnManager != null && vnManager.runner != null)
            {
                vnManager.runner.StartDialogue(nodeName);
                isDialogueRunning = true;
            }
            else
            {
                Debug.LogError("VNManager or DialogueRunner is not properly initialized!");
            }
        }

        /// <summary>
        /// 停止当前对话
        /// </summary>
        public void StopDialogue()
        {
            if (!isDialogueRunning) return;

            if (vnManager != null && vnManager.runner != null)
            {
                vnManager.runner.Stop();
                isDialogueRunning = false;
            }

            // 隐藏对话面板
            SetDialogueUIsActive(false);
        }

        /// <summary>
        /// 检查对话是否正在进行
        /// </summary>
        public bool IsDialogueRunning()
        {
            return isDialogueRunning;
        }

        /// <summary>
        /// 获取当前对话管理器实例
        /// </summary>
        public VNManager GetVNManager()
        {
            return vnManager;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
} 