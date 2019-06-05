using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualBluetoothInterface))]
public class VirtualBluetoothInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var vbi = target as VirtualBluetoothInterface;

        if (vbi != null && vbi.virtualDice != null)
        {
            foreach (var die in vbi.virtualDice)
            {
                EditorGUILayout.LabelField(die.name);
            }

            // Draw buttons to add / remove dice
            if (GUILayout.Button("Add Die"))
            {
                vbi.AddDie();
            }
        }
    }
}
