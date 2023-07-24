using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NPresetData;

namespace NPresetData
{
    [System.Serializable]
    public class PresetData
    {
        public readonly Dictionary<string, string> presetData;

        public PresetData(Dictionary<string, string> _presetData)
        {
            presetData = _presetData;
        }
    }
}

public class CreateObjectFolder : EditorWindow
{
    private readonly string _presetPath = Application.dataPath + "/CreateObjectFolder/";

    private readonly List<string> _pathList = new();
    // .. json 프리셋의 형태로 저장
    private readonly Dictionary<string, string> _preset = new();

    // .. 프리셋 데이터를 저장할 스트링 ..
    private string _presetName = string.Empty;

    private Vector2 _scrollPosition = Vector2.zero;

    private string _myString = string.Empty;
    private string _addOrDeletePathString = string.Empty;

    private GUIStyle _boxStyle;

    [MenuItem("CreateFolder/CreateFolder")]
    private static void init()
    {
        CreateObjectFolder window = (CreateObjectFolder)EditorWindow.GetWindow(typeof(CreateObjectFolder));
        window.Show();
    }

    private void OnGUI()
    {
        float boxWidth = position.width - 20f;
        float boxHeight = 100f;

        drawCustomBox(boxHeight);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 80));
        GUILayout.BeginVertical(_boxStyle, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));

        _presetName = EditorGUILayout.TextField("", _presetName);

        if (GUILayout.Button("Save Preset"))
        {
            // .. 세이브 함수 호출 .. JSON 데이터로 저장
            savePreset(_preset);

        }

        foreach (string folderName in _pathList)
            GUILayout.Label(folderName);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        GUILayout.Space(-5f);

        GUILayout.Label("Create Object's name", EditorStyles.boldLabel);
        _myString = EditorGUILayout.TextField("", _myString);

        if (GUILayout.Button("CreateFolder"))
            createFolder(_myString);
    }
    private void savePreset(Dictionary<string, string> presetNames)
    {
        // .. 이미 경로상에 존재하는 폴더는 제외..
        var keyValuePair = presetNames.Where(currentPath => (!File.Exists(_presetPath + currentPath.Value)));
        PresetData presetData = new(keyValuePair.ToDictionary(pair => pair.Key, pair => pair.Value));

        string jsonData = JsonUtility.ToJson(presetData);

        foreach (var pathValue in presetData.presetData)
            File.WriteAllText(_presetPath + pathValue.Key, jsonData);

        if (presetData.presetData.Count == 0)
            Debug.Log("Failed save Preset!");
        else
            Debug.Log("Preset Saved!");
    }
    private void loadPreset()
    {

    }
    private void drawCustomBox(float boxHeight)
    {
        _boxStyle = GUI.skin.box;

        Rect addPathButton = new Rect(position.width - 80f, 2f, 70f, 20f);
        Rect deletePathButton = new Rect(position.width - 170f, 2f, 85f, 20f);

        _addOrDeletePathString = EditorGUILayout.TextField("", _addOrDeletePathString, GUILayout.Width(position.width - 180f));

        if (GUI.Button(addPathButton,       "AddPath"))
            addPath(_addOrDeletePathString);
        if (GUI.Button(deletePathButton, "DeletePath"))
            deletePath(_addOrDeletePathString);
    }

    private void addPath(string pathString)
    {
        if (checkStringNullOrWhiteSpace(pathString, "please, Input path!")) return;

        pathString = pathString.Replace(" ", "_");

        if (_pathList.Contains(pathString))
        {
            Debug.Log("this path already exist!");
            return;
        }

        _pathList.Add(pathString);
    }

    private bool checkStringNullOrWhiteSpace(string str, string log)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            Debug.Log(log);
            return true;
        }

        return false;
    }
    private void createFolder(string folderString)
    {
        if (checkStringNullOrWhiteSpace(folderString, "please, input object's name")) return;

        folderString = folderString.Replace(" ", "_");

        Debug.Log("create folder!");

        foreach (string path in _pathList)
        {
            string folderPath = "Assets/" + _myString + "/" + path;

            if (!File.Exists(folderPath))
                continue;

            File.Create(folderPath);
        }
        
        // File.Create("Assets/" + )
    }
    private void deletePath(string pathString)
    {
        if (checkStringNullOrWhiteSpace(pathString, "please, Input path!")) return;

        pathString = pathString.Replace(" ", "_");

        if (_pathList.Contains(pathString))
        {
            Debug.Log("delete!");
            _pathList.Remove(pathString);
        }
        else
            Debug.Log("not found!");
    }
}
