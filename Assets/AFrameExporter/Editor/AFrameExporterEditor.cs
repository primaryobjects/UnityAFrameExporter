﻿using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AFrameExporter))]
public class AFrameExporterEditor : Editor {

    public override void OnInspectorGUI()
    {
        AFrameExporter myExporter = (AFrameExporter)target;
        if (GUILayout.Button("Export"))
        {
            myExporter.Export();
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Clear Exported Files"))
        {
            myExporter.ClearExport();
        }
    }
}
