using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CreateObjectFolder : EditorWindow
{
    private List<string> _pathList = new();
    private Dictionary<string, string> _preset = new();

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

        DrawCustomBox(boxHeight);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 80));
        GUILayout.BeginVertical(_boxStyle, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));

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

    private void DrawCustomBox(float boxHeight)
    {
        _boxStyle = GUI.skin.box;

        Rect addPathButton = new Rect(position.width - 80f, 2f, 70f, 20f);
        Rect deletePathButton = new Rect(position.width - 170f, 2f, 85f, 20f);

        _addOrDeletePathString = EditorGUILayout.TextField("", _addOrDeletePathString, GUILayout.Width(position.width - 180f));

        if (GUI.Button(addPathButton, "AddPath"))
            addPath(_addOrDeletePathString);
        if (GUI.Button(deletePathButton, "DeletePath"))
            deletePath(_addOrDeletePathString);
    }
    private void deletePath(string pathString)
    {
        if (CheckStringNullOrWhiteSpace(pathString, "please, Input path!")) return;

        if (_pathList.Contains(pathString))
        {
            Debug.Log("delete!");
            _pathList.Remove(pathString);
        }
        else
            Debug.Log("not found!");
    }
    private void addPath(string pathString)
    {
        if (CheckStringNullOrWhiteSpace(pathString, "please, Input path!")) return;

        pathString = pathString.Replace(" ", "_");

        if (_pathList.Contains(pathString))
        {
            Debug.Log("this path already exist!");
            return;
        }

        _pathList.Add(pathString);
    }

    private bool CheckStringNullOrWhiteSpace(string str, string log)
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
        if (CheckStringNullOrWhiteSpace(folderString, "please, input object's name")) return;

        folderString = folderString.Replace(" ", "_");

        Debug.Log("create folder!");

        foreach (string path in _pathList)
        {
            string folderPath = "Assets/" + _myString + "/" + path;


        }
        
        // File.Create("Assets/" + )
    }
}
