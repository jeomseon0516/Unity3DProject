#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WayPoint))]
public class WayPointEditor : EditorWindow
{
    [field:Header("부모 Node")]
    private GameObject Parent { get; set; }

    [MenuItem("CustomEditor/WayPoint")]
    public static void ShowWindows()
    {
        GetWindow<WayPointEditor>("WayPoint");
    }

    private void OnGUI()
    {
        Parent = (GameObject)EditorGUILayout.ObjectField("부모 Node", Parent, typeof(GameObject), true);

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Node", GUILayout.Width(250), GUILayout.Height(25),
                                             GUILayout.MaxWidth(350), GUILayout.MaxHeight(30)))
            {
                if (ReferenceEquals(Parent, null) || !Parent)
                    Parent = new GameObject("NAKZI");

                GameObject nodeObject = new GameObject("Node");
                nodeObject.AddComponent<Node>();

                nodeObject.transform.SetParent(Parent.transform);
                nodeObject.transform.position = new Vector3(
                    Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f));
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif