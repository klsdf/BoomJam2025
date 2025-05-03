namespace BoomJam2025
{
    using UnityEngine;
    using TMPro;
    using Yarn.Unity;
    using System.Collections;
    using System;
    
    /// <summary>
    /// 对话管理器，负责管理游戏中的对话系统
    /// 使用单例模式确保全局只有一个实例
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        #region Singleton
        public static DialogueManager Instance { get; private set; }

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

        // 添加对话完成事件
        public event Action<string> OnDialogueNodeComplete;

        [Header("对话组件")]
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private YarnProject yarnProject;
        
        [Header("对话设置")]
        [SerializeField] private string startNode = "StreamerStart";
        [SerializeField] private bool autoStartDialogue = true;

        private bool isDialogueRunnerReady = false;
        private string currentNode;
        private bool isDialogueRunning = false;
        private Coroutine currentDialogueCoroutine;

        private void Start()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = FindObjectOfType<DialogueRunner>();
            }

            if (dialogueRunner != null)
            {
                // 确保Yarn项目被正确设置
                if (yarnProject != null)
                {
                    dialogueRunner.yarnProject = yarnProject;
                }
                else
                {
                    Debug.LogError("YarnProject is not assigned!");
                    return;
                }

                // 注册对话事件
                dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
                dialogueRunner.onNodeStart.AddListener(OnNodeStart);
                
                // 等待一帧确保所有组件都初始化完成
                StartCoroutine(InitializeDialogueSystem());
            }
            else
            {
                Debug.LogError("DialogueRunner not found in scene!");
            }
        }

        private IEnumerator InitializeDialogueSystem()
        {
            yield return null; // 等待一帧
            
            if (dialogueRunner.yarnProject != null)
            {
                isDialogueRunnerReady = true;
                
                // 自动开始对话
                if (autoStartDialogue)
                {
                    StartDialogue(startNode);
                }
            }
            else
            {
                Debug.LogError("YarnProject is not properly loaded!");
            }
        }

        /// <summary>
        /// 开始对话
        /// </summary>
        /// <param name="startNode">对话起始节点</param>
        public void StartDialogue(string startNode)
        {
            if (dialogueRunner != null && isDialogueRunnerReady)
            {
                if (dialogueRunner.NodeExists(startNode))
                {
                    currentNode = startNode;
                    isDialogueRunning = true;
                    dialogueRunner.StartDialogue(startNode);
                }
                else
                {
                    Debug.LogError($"Node {startNode} does not exist in the Yarn project!");
                }
            }
            else
            {
                Debug.LogWarning("DialogueRunner is not ready yet!");
            }
        }

        /// <summary>
        /// 等待当前对话完成后开始新的对话
        /// </summary>
        /// <param name="nextNode">下一个要开始的对话节点</param>
        /// <returns>协程</returns>
        public IEnumerator StartDialogueAfterCurrent(string nextNode)
        {
            // 等待当前对话完成（如果有的话）
            float startTime = Time.time;
            while (isDialogueRunning)
            {
                yield return null;
            }
            float waitTime = Time.time - startTime;
          //  Debug.Log($"等待当前对话完成耗时: {waitTime:F2}秒");

            // 开始新的对话
            bool dialogueCompleted = false;
            Action<string> onDialogueComplete = null;
            onDialogueComplete = (nodeName) => {
                if (nodeName == nextNode) {
                    dialogueCompleted = true;
                    OnDialogueNodeComplete -= onDialogueComplete;
                }
            };
            
            OnDialogueNodeComplete += onDialogueComplete;
          //  Debug.Log($"开始播放对话节点: {nextNode}");
            startTime = Time.time;
            StartDialogue(nextNode);
            
            // 等待对话完成
            while (!dialogueCompleted)
            {
                yield return null;
            }
            float dialogueTime = Time.time - startTime;
            Debug.Log($"对话节点 {nextNode} 播放完成，持续时间: {dialogueTime:F2}秒");
        }

        /// <summary>
        /// 停止当前对话
        /// </summary>
        public void StopDialogue()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.Stop();
            }
        }

        /// <summary>   
        /// 停止当前对话协程
        /// </summary>
        public void StopDialogueCoroutine()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.Stop();
                isDialogueRunning = false;
                
                // 停止当前正在运行的协程
                if (currentDialogueCoroutine != null)
                {
                    StopCoroutine(currentDialogueCoroutine);
                    currentDialogueCoroutine = null;
                }
            }
        }

        /// <summary>
        /// 当对话节点开始时调用
        /// </summary>
        private void OnNodeStart(string nodeName)
        {
            currentNode = nodeName;
            isDialogueRunning = true;
            Debug.Log($"开始对话节点: {nodeName}");
        }

        /// <summary>
        /// 当对话完成时调用
        /// </summary>
        private void OnDialogueComplete()
        {
            Debug.Log($"对话完成: {currentNode}");
            isDialogueRunning = false;
            OnDialogueNodeComplete?.Invoke(currentNode);
        }

        /// <summary>
        /// 检查是否有对话正在进行
        /// </summary>
        public bool IsDialogueRunning()
        {
            return isDialogueRunning;
        }

        private void OnDestroy()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
                dialogueRunner.onNodeStart.RemoveListener(OnNodeStart);
            }
        }
    }
} 