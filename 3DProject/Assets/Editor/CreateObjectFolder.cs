using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class CreateObjectFolder : EditorWindow
{
    private const string PRESET_PATH = "/CreateObjectFolder";
    private const string JSON_FILE_NAME = "/Create_Folder_Preset.json";
    private Dictionary<string, List<string>> _presetList; // .. json 프리셋의 형태로 저장
    private List<string> _pathList = new();
    private Vector2 _scrollPosition = Vector2.zero;
    private string[] _presetKeys; // .. 매 프레임 딕셔너리에서 키 값만 받아오는 건 비효율적
    private string _addOrDeletePathString = string.Empty;
    private string _presetName = string.Empty;  /* .. 프리셋 데이터를 저장할 스트링 .. */
    private string _rootFolder = string.Empty;
    private int _selectedValueIndex = 0;

    private GUIStyle _boxStyle;

    [MenuItem("CreateFolder/CreateFolder")]
    private static void init()
    {
        CreateObjectFolder window = (CreateObjectFolder)EditorWindow.GetWindow(typeof(CreateObjectFolder));
        // .. 프리셋 로드
        window.Show();
    }

    private void OnEnable()
    {
        loadPreset(); // .. 프리셋 로드
    }
    private void OnDestroy()
    {
        // .. 창 종료시 기존에 있던 프리셋들의 세팅을 모두 저장
        onSavePreset();
    }
    private void OnDisable()
    {
        onSavePreset();
    }
    private void OnGUI()
    {
        _presetName = EditorGUILayout.TextField("Preset Name .. ", _presetName);

        _selectedValueIndex = EditorGUILayout.Popup(_selectedValueIndex, _presetKeys);

        if (_presetKeys.Length > 0)
            _pathList = _presetList[_presetKeys[_selectedValueIndex]];

        // .. 세이브 함수 호출 .. JSON 데이터로 저장
        if (GUILayout.Button("Save Preset"))
            savePreset(_presetName);
        if (GUILayout.Button("Remove Preset"))
            removePreset(_presetKeys.Length > 0 ? _presetKeys[_selectedValueIndex] : "");

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 80));
        GUILayout.BeginVertical();

        foreach (string folderName in _pathList)
            GUILayout.Label("[" + folderName + "]");

        drawCustomBox();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(-50f);

        GUILayout.Label("create root folder's name", EditorStyles.boldLabel);
        _rootFolder = EditorGUILayout.TextField("", _rootFolder);

        if (GUILayout.Button("CreateFolder"))
            createFolder(_rootFolder);
    }
    private void savePreset(string presetName)
    {
        if (checkStringNullOrWhiteSpace(presetName, "please, input preset name.") || ProcessingPresetMethod(ref presetName)) return;

        _presetList.Add(presetName, _pathList);

        onSavePreset();
    }
    private bool ProcessingPresetMethod(ref string presetName)
    {
        presetName = presetName.Replace(" ", "_");

        if (_presetList.ContainsKey(presetName))
        {
            Debug.Log("this key already included.");
            return false;
        }

        if (_pathList.Count == 0)
        {
            Debug.Log("noting in the pathList.");
            return false;
        }

        return true;
    }
    private void removePreset(string presetName)
    {
        if (checkStringNullOrWhiteSpace(presetName, "please, input preset name.") || ProcessingPresetMethod(ref presetName)) return;

        _presetList.Remove(presetName);
        _presetKeys = new List<string>(_presetList.Keys).ToArray();

        _pathList = new();
    }
    private void onSavePreset()
    {
        string folderPath = Application.dataPath + PRESET_PATH;

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string jsonData = DicJsonUtility.ToJson(_presetList);

        using (StreamWriter writer = new StreamWriter(folderPath + JSON_FILE_NAME, false))
            writer.Write(jsonData);
    }
    private void loadPreset()
    {
        string path = Application.dataPath + PRESET_PATH;
        string fileName = path + JSON_FILE_NAME;

        _presetList = Directory.Exists(path) && File.Exists(fileName) ? 
            DicJsonUtility.FromJson<string, List<string>>(File.ReadAllText(fileName)) : new();

        _presetKeys = new List<string>(_presetList.Keys).ToArray();
    }
    private void drawCustomBox()
    {
        _boxStyle = GUI.skin.box;

        _addOrDeletePathString = EditorGUILayout.TextField("Add/Remove .. ", _addOrDeletePathString);

        float width = position.width * 0.5f;

        if (GUILayout.Button("Add Path", GUILayout.Width(width)))
            addPath(_addOrDeletePathString);
        if (GUILayout.Button("Delete Path", GUILayout.Width(width)))
            deletePath(_addOrDeletePathString);
    }
    private void addPath(string pathString)
    {
        if (checkStringNullOrWhiteSpace(pathString, "please, input path!")) return;

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
            string folderPath = "Assets/" + _rootFolder + "/" + path;

            if (!File.Exists(folderPath))
                continue;

            Directory.CreateDirectory(folderPath);
        }
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
