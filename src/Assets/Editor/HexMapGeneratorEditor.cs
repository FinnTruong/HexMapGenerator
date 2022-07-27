using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexMapGenerator))]
public class HexMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexMapGenerator generator = target as HexMapGenerator;
        if(DrawDefaultInspector())
        {
            generator.GenerateMap();
        }

        if(GUILayout.Button("Generate Map"))
        {
            generator.GenerateMap();
        }
    }
}
