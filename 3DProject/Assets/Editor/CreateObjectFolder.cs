using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CreateObjectFolder : EditorWindow
{
    [SerializeField] private List<string> _pathList = new List<string>();

    [MenuItem("CreateFolder/CreateFolder")]
    static void Init()
    {
        CreateObjectFolder window = (CreateObjectFolder)EditorWindow.GetWindow(typeof(CreateObjectFolder));
        window.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("CreateFolder"))
            createFolder();    
    }
    private void createFolder()
    {
        Debug.Log("CreateFolder!");


    }
}
