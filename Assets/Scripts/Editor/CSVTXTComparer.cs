using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class CSVTXTComparer : EditorWindow
{
    private TextAsset csvFile;
    private TextAsset txtFile;
    private string resultMessage = "";

    [MenuItem("Tools/CSV-TXT 字符比较器")]
    public static void ShowWindow()
    {
        GetWindow<CSVTXTComparer>("CSV-TXT 字符比较器");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV-TXT 字符比较工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV文件", csvFile, typeof(TextAsset), false);
        txtFile = (TextAsset)EditorGUILayout.ObjectField("TXT文件", txtFile, typeof(TextAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("比较并更新"))
        {
            if (csvFile == null || txtFile == null)
            {
                resultMessage = "请选择CSV和TXT文件！";
                return;
            }

            CompareAndUpdate();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(resultMessage, MessageType.Info);
    }

    private void CompareAndUpdate()
    {
        try
        {
            // 读取CSV文件内容
            string csvContent = csvFile.text;
            HashSet<char> csvChars = new HashSet<char>();
            foreach (char c in csvContent)
            {
                if (!char.IsWhiteSpace(c))
                {
                    csvChars.Add(c);
                }
            }

            // 读取TXT文件内容
            string txtContent = txtFile.text;
            HashSet<char> txtChars = new HashSet<char>();
            foreach (char c in txtContent)
            {
                if (!char.IsWhiteSpace(c))
                {
                    txtChars.Add(c);
                }
            }

            // 找出CSV中有但TXT中没有的字符
            HashSet<char> newChars = new HashSet<char>(csvChars);
            newChars.ExceptWith(txtChars);

            if (newChars.Count == 0)
            {
                resultMessage = "TXT文件中已包含所有CSV文件中的字符。";
                return;
            }

            // 将新字符添加到TXT文件末尾
            StringBuilder newContent = new StringBuilder(txtContent);
            foreach (char c in newChars)
            {
                newContent.Append(c);
            }

            // 保存更新后的TXT文件
            string txtPath = AssetDatabase.GetAssetPath(txtFile);
            File.WriteAllText(txtPath, newContent.ToString());
            AssetDatabase.Refresh();

            resultMessage = $"成功添加了 {newChars.Count} 个新字符到TXT文件中。";
        }
        catch (System.Exception e)
        {
            resultMessage = $"发生错误：{e.Message}";
        }
    }
} 