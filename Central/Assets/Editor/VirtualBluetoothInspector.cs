using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualBluetoothInterface))]
public class VirtualBluetoothInspector : Editor
{
    public override bool RequiresConstantRepaint()
    {
        var vbi = target as VirtualBluetoothInterface;
        if (vbi != null && vbi.virtualDice != null && vbi.virtualDice.Count > 0)
        {
            return true;
        }

        return false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var vbi = target as VirtualBluetoothInterface;

        if (vbi != null && vbi.virtualDice != null)
        {
            foreach (var die in vbi.virtualDice)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(die.name);
                if (GUILayout.Button("Disconnect"))
                {
                    vbi.RemoveDie(die);
                    GUILayout.EndHorizontal();
                    return;
                }
                GUILayout.EndHorizontal();

                // Then let the die show its own debug ui
                OnDieGUI(die);

                // Separator
                GUILayout.Box("", GUILayout.Height(1.0f), GUILayout.ExpandWidth(true));
            }

            // Draw buttons to add / remove dice
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add 6 Sided Die"))
            {
                vbi.AddDie(Die.DieType.SixSided);
            }
            if (GUILayout.Button("Add 20 Sided Die"))
            {
                vbi.AddDie(Die.DieType.TwentySided);
            }
            GUILayout.EndHorizontal();
        }
    }

    void OnDieGUI(VirtualDie die)
    {
        EditorGUILayout.LabelField("Type", die.dieType.ToString());
        EditorGUILayout.LabelField("State", die.currentState.ToString());
        EditorGUILayout.LabelField("Face", die.currentFace.ToString());
        if (GUILayout.Button("Roll"))
        {
            die.Roll();
        }
    }
}
