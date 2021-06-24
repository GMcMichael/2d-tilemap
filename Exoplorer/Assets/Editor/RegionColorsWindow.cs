using UnityEngine;
using UnityEditor;

public class RegionColorsWindow : EditorWindow
{

    public static MapGeneration.TerrainType[] regions;
    private static bool[] ShowRegions;

    private static MapGeneration mapGeneration;

    private static Gradient gradient = new Gradient();

    [MenuItem("Window/Region Colors")]
    public static void ShowWindow(MapGeneration _mapGeneration) {
        mapGeneration = _mapGeneration;
        regions = mapGeneration.GetRegions();
        ShowRegions = new bool[regions.Length];
        EditorWindow.GetWindow(typeof(RegionColorsWindow), false, "Region Colors", true);
    }

    void OnGUI() {
        for (int i = 0; i < regions.Length; i++)
        {
            ShowRegions[i] = EditorGUILayout.Foldout(ShowRegions[i], regions[i].name);
            if(ShowRegions[i]) {
                regions[i].name = EditorGUILayout.TextField("Name", regions[i].name);
                regions[i].height = EditorGUILayout.FloatField("Height", regions[i].height);
                regions[i].color = EditorGUILayout.ColorField("Color", regions[i].color);
            }
        }
    }

    void OnInspectorUpdate() {
        SendData();
    }

    private void SendData() {
        mapGeneration.GenerateMap();
    }
}
