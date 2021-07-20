using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGeneration))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        MapGeneration mapGeneration = (MapGeneration)target;

        if(DrawDefaultInspector() && mapGeneration.autoUpdate) 
            mapGeneration.GenerateMap();

        if(mapGeneration.RegionsChanged()) 
            mapGeneration.GenerateMap();

        if(GUILayout.Button("Generate"))
            mapGeneration.GenerateMap();
    }
}
