using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlaneMeshGenerator : EditorWindow
{
    const string path = "Assets/";
    const string extension = ".asset";
    string meshName = "MyMesh";
    Vector2Int divisions = Vector2Int.one;
    Vector2 size = Vector2.one;
    Vector2 center = Vector2.zero;
    Vector2 uvSize = Vector2.one;

    [MenuItem("Window/MeshGenerator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PlaneMeshGenerator));
    }

    void OnGUI() {
        meshName = EditorGUILayout.TextField("Mesh name: ", meshName);
        divisions = EditorGUILayout.Vector2IntField("Subdivisions: ", divisions);
        center = EditorGUILayout.Vector2Field("Center (0 to 1): ", center);
        size = EditorGUILayout.Vector2Field("Size: ", size);
        uvSize = EditorGUILayout.Vector2Field("UV size: ", uvSize);

        GUI.enabled = meshName.Length > 0;
        if (GUILayout.Button(GUI.enabled ? "Generate Plane" : "Please enter valid name")) {
            GeneratePlane();
        }
        if (GUILayout.Button(GUI.enabled ? "Generate Prism" : "Please enter valid name"))
        {
            GeneratePrysm();
        }
        if (GUILayout.Button(GUI.enabled ? "Generate Exact Side Prism" : "Please enter valid name"))
        {
            GeneratePrysm(false, true);
        }
        if (GUILayout.Button(GUI.enabled ? "Generate Sausage" : "Please enter valid name"))
        {
            GeneratePrysm(true);
        }
        if (GUILayout.Button(GUI.enabled ? "Generate On Side Sausage" : "Please enter valid name"))
        {
            GeneratePrysm(true, false, 2);
        }
    }

    void GeneratePlane()
    {
        int vertexCount = (divisions.x + 1) * (divisions.y + 1);
        int triangleCount = 2 * divisions.x * divisions.y * 3;

        Vector3[] vertex = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uv = new Vector2[vertexCount];

        Vector2Int lineCount = divisions + Vector2Int.one;
        Vector2Int div = divisions;
        for (int i = 0; i < lineCount.x; ++i)
        {
            for (int j = 0; j < lineCount.y; ++j)
            {
                vertex[i * lineCount.y + j] = new Vector3(i * size.x / div.x - center.x, j * size.y / div.y - center.y, 0);
                uv[i * lineCount.y + j] = new Vector2(i * uvSize.x / div.x, j * uvSize.y / div.y);

                if (i < div.x && j < div.y)
                {
                    triangles[(i * div.y + j) * 6 + 0] = i * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 1] = (i + 1) * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 2] = (i + 1) * lineCount.y + j + 1;
                    triangles[(i * div.y + j) * 6 + 3] = i * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 4] = (i + 1) * lineCount.y + j + 1;
                    triangles[(i * div.y + j) * 6 + 5] = i * lineCount.y + j + 1;
                }
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertex;
        m.triangles = triangles;
        m.uv = uv;

        AssetDatabase.CreateAsset(m, path + meshName + extension);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(m);
    }

    void GeneratePrysm(bool doSoft = false, bool exactSide = false, int softMask = 3)
    {
        int vertexCount = (divisions.x + 1) * (divisions.y + 1);
        int triangleCount = 2 * divisions.x * divisions.y * 3;

        Vector3[] vertex = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uv = new Vector2[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];

        float radius = size.x / 2f;
        if (exactSide)
        {
            radius = size.x / (2f * Mathf.Sin(Mathf.PI / divisions.x));
        }

        Vector2Int lineCount = divisions + Vector2Int.one;
        Vector2Int div = divisions;
        Vector3 c = new Vector3(0, center.y, center.x * radius);
        float deltaAngle = 2f * Mathf.PI / divisions.x;
        float innerAngle = Mathf.PI - deltaAngle;

        for (int i = 0; i < lineCount.x; ++i)
        {
            for (int j = 0; j < lineCount.y; ++j)
            {
                float angle = deltaAngle * i + innerAngle / 2f;
                float softFactor = 1;
                float yPos = j * size.y / div.y;
                if (doSoft) {
                    if ((softMask&1) > 0 && yPos < size.x * 0.5f)
                    {
                        softFactor = Easing.Circular.Out(yPos / (size.x * 0.5f));
                    }
                    else if ((softMask & 2) > 0 && yPos > size.y - size.x * 0.5f)
                    {
                        softFactor = Easing.Circular.Out((size.y - yPos) / (size.x * 0.5f));
                    }
                }
                vertex[i * lineCount.y + j] = new Vector3(Mathf.Sin(angle) * radius * softFactor, yPos - center.y, Mathf.Cos(angle) * radius * softFactor) + c;
                normals[i * lineCount.y + j] = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                uv[i * lineCount.y + j] = new Vector2((lineCount.x - i) * uvSize.x / div.x, j * uvSize.y / div.y);

                if (i < div.x && j < div.y)
                {
                    triangles[(i * div.y + j) * 6 + 0] = i * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 1] = (i + 1) * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 2] = (i + 1) * lineCount.y + j + 1;
                    triangles[(i * div.y + j) * 6 + 3] = i * lineCount.y + j;
                    triangles[(i * div.y + j) * 6 + 4] = (i + 1) * lineCount.y + j + 1;
                    triangles[(i * div.y + j) * 6 + 5] = i * lineCount.y + j + 1;
                }
            }
        }

        Mesh m = new Mesh();
        m.vertices = vertex;
        m.triangles = triangles;
        m.normals = normals;
        m.uv = uv;

        AssetDatabase.CreateAsset(m, path + meshName + extension);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(m);
    }
}
