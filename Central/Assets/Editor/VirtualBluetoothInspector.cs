using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            // Draw buttons to add 1 dice
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

            // Draw buttons to add 3 dice at a time
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add 3x 6 Sided Die"))
            {
                vbi.AddDie(Die.DieType.SixSided);
                vbi.AddDie(Die.DieType.SixSided);
                vbi.AddDie(Die.DieType.SixSided);
            }
            if (GUILayout.Button("Add 3x 20 Sided Die"))
            {
                vbi.AddDie(Die.DieType.TwentySided);
                vbi.AddDie(Die.DieType.TwentySided);
                vbi.AddDie(Die.DieType.TwentySided);
            }
            GUILayout.EndHorizontal();

            // Draw buttons to rools all dices of same type
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Roll All"))
            {
                foreach (var vd in vbi.virtualDice.Where(d => d.dieType == Die.DieType.SixSided))
                {
                    vd.Roll();
                }
            }
            if (GUILayout.Button("Roll All"))
            {
                foreach (var vd in vbi.virtualDice.Where(d => d.dieType == Die.DieType.TwentySided))
                {
                    vd.Roll();
                }
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
