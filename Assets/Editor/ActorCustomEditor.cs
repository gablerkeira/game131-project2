using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Actor myActor = target as Actor;

        #region Actor Name
        myActor.actorName = EditorGUILayout.TextField("Actor Name: ", myActor.actorName);
        #endregion

        #region Action Target
        myActor.actionTarget = (Actor.ActionTarget)EditorGUILayout.EnumPopup("Action Target: ", myActor.actionTarget);

        #endregion

        #region Action Effect
        myActor.actionEffect = (Actor.ActionEffect)EditorGUILayout.EnumPopup("Action Effect: ", myActor.actionEffect);
        #endregion

        #region AES & Immunities
        Actor.ActionSource[] sourceValues = Enum.GetValues(typeof(Actor.ActionSource)) as Actor.ActionSource[];
        string[] sourceNames = Enum.GetNames(typeof(Actor.ActionSource));

        SelectionList<Actor.ActionSource> sources = new SelectionList<Actor.ActionSource>(sourceValues, sourceNames);
        myActor.actionEffectSource = sources.RadioList("Action Source: ", myActor.actionEffectSource, 3);
        myActor.immunities = sources.CheckboxList("Immunities: ", myActor.immunities, 3);
        #endregion

        #region MaxHit Points
        string mhpLabel = "Max Hit Points: "; 
        int newMaxHitPoints = myActor.GetComponent<Actor>().maxHitPoints;
        newMaxHitPoints = EditorGUILayout.IntSlider(mhpLabel, newMaxHitPoints, 0, 1500);
        myActor.GetComponent<Actor>().maxHitPoints = newMaxHitPoints;
        #endregion

        #region Hit Points
        string hpLabel = "Hit Points: ";
        int newHitPoints = myActor.GetComponent<Actor>().hitPoints;
        newHitPoints = EditorGUILayout.IntSlider(hpLabel, newHitPoints, 1, 100);
        myActor.GetComponent<Actor>().hitPoints = newHitPoints;
        #endregion

        #region Initiative
        string iLabel = "Initiative: ";
        int newInitiative = myActor.GetComponent<Actor>().initiative;
        newInitiative = EditorGUILayout.IntSlider(iLabel, newInitiative, 10, 100);
        myActor.GetComponent<Actor>().initiative = newInitiative;

        #endregion

        #region Damage
        string dLabel = "Damage: ";
        int newDamage = myActor.GetComponent<Actor>().damage;
        newDamage = EditorGUILayout.IntSlider(dLabel, newDamage, 0, 180);
        myActor.GetComponent<Actor>().damage = newDamage;
        #endregion

        #region Percent Chance to Hit
        string pchLabel = "Percent Chance To Hit:  ";
        int newPcth = myActor.GetComponent<Actor>().percentChanceToHit;
        newPcth = EditorGUILayout.IntSlider(pchLabel, newPcth, 0, 100);
        myActor.GetComponent<Actor>().percentChanceToHit = newPcth;
        #endregion

        #region Board Position
        Actor.Position[] positionValues = Enum.GetValues(typeof(Actor.Position)) as Actor.Position[];
        string[] positionNames = Enum.GetNames(typeof(Actor.Position));
        SelectionList<Actor.Position> positions = new SelectionList<Actor.Position>(positionValues, positionNames);
        myActor.boardPosition = positions.PositionGrid("Board Position: ", myActor.boardPosition, 3);
        #endregion
    }

}

class SelectionList<T> where T : IComparable
{
    T[] _values;
    string[] _labels;
    T _selectedValue;

    #region Checkbox List
    public T[] CheckboxList(string label, T[] initialSelections, int itemsPerRow)
    {
        List<T> selectedValues = new List<T>();
        List<int> initialSelectedIndexes = new List<int>();
        for (int i = 0; i < _values.Length; i++)
        {
            for (int j = 0; j < initialSelections.Length; j++)
            {
                if (_values[i].CompareTo(initialSelections[j]) == 0) initialSelectedIndexes.Add(i);
            }
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

        EditorGUILayout.BeginHorizontal();
        for (int r = 0; r < _values.Length; r += itemsPerRow)
        {
            EditorGUILayout.BeginVertical();
            for (int i = r; i < r + itemsPerRow && i < _values.Length; i++)
            {
                if (GUILayout.Toggle(initialSelectedIndexes.Contains(i), _labels[i]))
                {
                    selectedValues.Add(_values[i]);
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        return selectedValues.ToArray();

    }
    #endregion

    #region RadioList
    public T RadioList(string label, T initialSelection, int itemsPerRow)
    {
        T originalSelectedValue = _selectedValue;
        _selectedValue = initialSelection;
        bool anyChecked = false;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

        EditorGUILayout.BeginHorizontal();
        for (int r = 0; r < _values.Length; r += itemsPerRow)
        {
            EditorGUILayout.BeginVertical();
            for (int i = r; i < r + itemsPerRow && i < _values.Length; i++)
            {
                if (_values[i].CompareTo(initialSelection) == 0) originalSelectedValue = initialSelection;
                if (GUILayout.Toggle(_values[i].CompareTo(_selectedValue) == 0, _labels[i]))
                {
                    _selectedValue = _values[i];
                    anyChecked = true;
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (!anyChecked) _selectedValue = originalSelectedValue;
        return _selectedValue;
    }
    #endregion

    #region SelectionList
    public SelectionList(T[] values, string[] labels)
    {
        _values = new T[values.Length];
        _labels = new string[labels.Length < values.Length ? values.Length : labels.Length];
        for (int i = 0; i < _values.Length; i++) _values[i] = values[i];
        for (int i = 0; i < _labels.Length; i++) _labels[i] = (i < labels.Length) ? labels[i] : values[i].ToString();
        _selectedValue = _values[0];
    }
    #endregion

    #region PositionGrid
    public T PositionGrid(string label, T initialSelection, int itemsPerRow)
    {
        T originalSelectedValue = _selectedValue;
        _selectedValue = initialSelection;
        bool anyChecked = false;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, GUILayout.MaxWidth(100));

        EditorGUILayout.BeginHorizontal();
        for (int r = 0; r < _values.Length; r += itemsPerRow)
        {
            EditorGUILayout.BeginVertical();
            for (int i = r; i < r + itemsPerRow && i < _values.Length; i++)
            {
                if (_values[i].CompareTo(initialSelection) == 0) originalSelectedValue = initialSelection;
                if (GUILayout.Toggle(_values[i].CompareTo(_selectedValue) == 0, _labels[i]))
                {
                    _selectedValue = _values[i];
                    anyChecked = true;
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        if (!anyChecked) _selectedValue = originalSelectedValue;
        return _selectedValue;
    }
    #endregion
}
