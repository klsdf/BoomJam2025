using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.U2D.Sprites;
using UnityEditor.AssetImporters;

public class SequenceAnimationTool : EditorWindow
{
    private const string RESOURCES_ROOT = "Assets/Resources";
    private const string ANIMATIONS_FOLDER = "Animations";
    private const string SPRITESHEETS_FOLDER = "SpriteSheets";
    
    private string folderPath = "Assets/";
    private bool showFolderPath = false;
    private string animationName = "NewSequenceAnimation";
    private bool createSpriteSheet = false;
    private int maxColumnCount = 8; // 每行最大图片数量
    private string resultMessage = "";

    // 从文件名中提取数字的正则表达式
    private static readonly Regex NumberRegex = new Regex(@"\d+", RegexOptions.Compiled);

    [MenuItem("Tools/序列帧动画/序列帧动画工具")]
    public static void ShowWindow()
    {
        GetWindow<SequenceAnimationTool>("序列帧动画工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("序列帧动画工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 文件夹路径输入
        showFolderPath = EditorGUILayout.Foldout(showFolderPath, "序列帧文件夹设置");
        if (showFolderPath)
        {
            // 路径输入行
            EditorGUILayout.BeginHorizontal();
            folderPath = EditorGUILayout.TextField("文件夹路径", folderPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("选择序列帧文件夹", folderPath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    if (newPath.StartsWith(Application.dataPath))
                    {
                        folderPath = "Assets" + newPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("错误", "请选择Assets文件夹下的目录！", "确定");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // 动画名称输入
            animationName = EditorGUILayout.TextField("动画名称", animationName);

            // Sprite Sheet设置
            createSpriteSheet = EditorGUILayout.Toggle("生成Sprite Sheet", createSpriteSheet);
            if (createSpriteSheet)
            {
                maxColumnCount = EditorGUILayout.IntSlider("每行最大图片数", maxColumnCount, 1, 16);
            }

            EditorGUILayout.HelpBox("请选择包含序列帧图片的文件夹，图片将按照文件名中的数字顺序加载。", MessageType.Info);
        }

        // 加载按钮
        EditorGUILayout.Space(5);
        if (GUILayout.Button("创建序列帧动画"))
        {
            CreateSequenceAnimation();
        }

        if (!string.IsNullOrEmpty(resultMessage))
        {
            EditorGUILayout.HelpBox(resultMessage, MessageType.Info);
        }
    }

    private void CreateSequenceAnimation()
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            resultMessage = "请选择有效的文件夹路径！";
            return;
        }

        // 设置Resources文件夹结构
        SetupResourcesFolders();

        // 获取文件夹中的所有图片文件
        string[] files = Directory.GetFiles(folderPath, "*.*")
            .Where(file => file.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
                          file.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
                          file.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => 
            {
                // 从文件名中提取数字
                string fileName = Path.GetFileNameWithoutExtension(file);
                Match match = NumberRegex.Match(fileName);
                if (match.Success && int.TryParse(match.Value, out int number))
                {
                    return number; // 按数字排序
                }
                return 0; // 如果没有数字，排在最前面
            })
            .ToArray();

        if (files.Length == 0)
        {
            resultMessage = "所选文件夹中没有找到图片文件！";
            return;
        }

        // 确保所有图片都可读
        bool needsRefresh = false;
        foreach (string file in files)
        {
            string relativePath = GetRelativePath(file);
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                needsRefresh = true;
            }
        }

        if (needsRefresh)
        {
            AssetDatabase.Refresh();
        }

        Sprite[] sprites;
        string spriteSheetPath = null;
        if (createSpriteSheet)
        {
            spriteSheetPath = $"{RESOURCES_ROOT}/{SPRITESHEETS_FOLDER}/{animationName}_SpriteSheet.png";
            sprites = CreateSpriteSheetFromFiles(files, spriteSheetPath);
        }
        else
        {
            sprites = LoadSpritesFromFiles(files);
        }

        if (sprites == null || sprites.Length == 0)
        {
            resultMessage = "没有成功加载任何图片！请确保图片已正确导入为Sprite。";
            return;
        }

        // 创建动画数据资源
        string savePath = $"{RESOURCES_ROOT}/{ANIMATIONS_FOLDER}/{animationName}.asset";
        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

        // 创建并保存动画数据
        SequenceAnimationData animationData = ScriptableObject.CreateInstance<SequenceAnimationData>();
        animationData.sprites = sprites;

        AssetDatabase.CreateAsset(animationData, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 创建新的GameObject并添加SequenceAnimation组件
        GameObject go = new GameObject(animationName);
        SequenceAnimation sequenceAnimation = go.AddComponent<SequenceAnimation>();
        sequenceAnimation.SetAnimationData(animationData);

        // 选中新创建的对象
        Selection.activeGameObject = go;

        resultMessage = $"已创建序列帧动画：\n" +
                       $"动画数据：{ANIMATIONS_FOLDER}/{Path.GetFileName(savePath)}";
        
        if (createSpriteSheet)
        {
            resultMessage += $"\nSprite Sheet：{SPRITESHEETS_FOLDER}/{Path.GetFileName(spriteSheetPath)}";
        }
    }

    private void SetupResourcesFolders()
    {
        // 确保Resources文件夹存在
        EnsureDirectoryExists(RESOURCES_ROOT);
        
        // 创建分类文件夹
        EnsureDirectoryExists($"{RESOURCES_ROOT}/{ANIMATIONS_FOLDER}");
        EnsureDirectoryExists($"{RESOURCES_ROOT}/{SPRITESHEETS_FOLDER}");
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private string GetRelativePath(string absolutePath)
    {
        absolutePath = absolutePath.Replace("\\", "/");
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        return absolutePath;
    }

    private Sprite[] LoadSpritesFromFiles(string[] files)
    {
        Sprite[] sprites = new Sprite[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            string relativePath = GetRelativePath(files[i]);
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);
            if (sprites[i] == null)
            {
                Debug.LogError($"无法加载图片: {relativePath}");
            }
        }
        return sprites;
    }

    private Sprite[] CreateSpriteSheetFromFiles(string[] files, string targetPath)
    {
        // 加载第一张图片来获取尺寸信息
        string firstImagePath = GetRelativePath(files[0]);
        Texture2D firstTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(firstImagePath);
        if (firstTexture == null)
        {
            Debug.LogError("无法加载第一张图片来获取尺寸信息");
            return null;
        }

        int frameWidth = firstTexture.width;
        int frameHeight = firstTexture.height;
        int frameCount = files.Length;

        // 计算行列数
        int columns = Mathf.Min(maxColumnCount, frameCount);
        int rows = Mathf.CeilToInt((float)frameCount / columns);

        // 创建大图
        int atlasWidth = frameWidth * columns;
        int atlasHeight = frameHeight * rows;
        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);

        // 填充透明背景
        Color[] clearColors = new Color[atlasWidth * atlasHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }
        atlas.SetPixels(clearColors);

        // 读取并复制每一帧到大图中
        for (int i = 0; i < frameCount; i++)
        {
            string imagePath = GetRelativePath(files[i]);
            Texture2D frameTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            if (frameTexture == null)
            {
                Debug.LogError($"无法加载图片: {imagePath}");
                continue;
            }

            // 计算在大图中的位置
            int row = i / columns;
            int col = i % columns;
            int x = col * frameWidth;
            int y = (rows - 1 - row) * frameHeight; // 从上到下填充

            // 复制像素
            Color[] framePixels = frameTexture.GetPixels();
            atlas.SetPixels(x, y, frameWidth, frameHeight, framePixels);
        }

        atlas.Apply();

        // 确保目标路径存在
        string directory = Path.GetDirectoryName(targetPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 保存大图
        byte[] pngData = atlas.EncodeToPNG();
        File.WriteAllBytes(targetPath, pngData);
        AssetDatabase.Refresh();

        // 设置图片导入设置
        TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;

            // 设置Sprite切分
            var spritesheet = new List<SpriteMetaData>();
            for (int i = 0; i < frameCount; i++)
            {
                int row = i / columns;
                int col = i % columns;
                int x = col * frameWidth;
                int y = (rows - 1 - row) * frameHeight;

                spritesheet.Add(new SpriteMetaData
                {
                    name = $"{animationName}_{i}",
                    rect = new Rect(x, y, frameWidth, frameHeight),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }

#pragma warning disable CS0618 // 抑制过时API警告
            importer.spritesheet = spritesheet.ToArray();
#pragma warning restore CS0618

            importer.SaveAndReimport();
        }

        // 加载切分后的Sprite
        return AssetDatabase.LoadAllAssetsAtPath(targetPath)
            .OfType<Sprite>()
            .OrderBy(s => int.Parse(s.name.Split('_').Last()))
            .ToArray();
    }
} 