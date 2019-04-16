using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{
    private static GUIStyle errorBoxStyle = null;

    private bool showDerivedProperties = false;

    private static void InitializeStyles()
    {
        errorBoxStyle = new GUIStyle(EditorStyles.textField);

        errorBoxStyle.normal.background = Resources.Load<Texture2D>("Textures/txErrorBackground");

    }

    private DerivedStatList derivedStats;

    public override void OnInspectorGUI()
    {
        #region Actor Stats
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
        #endregion

        #region AI Authoring
        if (errorBoxStyle == null) InitializeStyles();

        showDerivedProperties = EditorGUILayout.Foldout(showDerivedProperties, new GUIContent("Derived Properties", "Properties based on static unit stats."));
        if (showDerivedProperties)
        {
            derivedStats = AssetDatabase.LoadAssetAtPath("Assets/DerivedProperties.asset", typeof(DerivedStatList)) as DerivedStatList;
            if (derivedStats == null)
            {
                UnityEngine.MonoBehaviour.print("Nope, not there");
                derivedStats = ScriptableObject.CreateInstance<DerivedStatList>();
                AssetDatabase.CreateAsset(derivedStats, "Assets/DerivedProperties.asset");
                AssetDatabase.SaveAssets();
            }

            int nameFieldWidth = 80;
            for (int i = 0; i < derivedStats.Length; i++)
            {
                int statNameWidth = (int)EditorStyles.textField.CalcSize(new GUIContent(derivedStats.list[i].statName + " ")).x;
                if (statNameWidth > nameFieldWidth) nameFieldWidth = statNameWidth;
            }

            bool derivedPropEquationChanged = false;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(nameFieldWidth));
            EditorGUILayout.LabelField("Expression");
            EditorGUILayout.LabelField("Current Value", GUILayout.MaxWidth(90));
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < (derivedStats != null ? derivedStats.Length : 0); i++)
            {
                EditorGUILayout.BeginHorizontal();

                // TODO: Watch out for repeats
                derivedStats.list[i].statName = EditorGUILayout.TextField(derivedStats.list[i].statName, GUILayout.Width(nameFieldWidth));

                int derivedValue = 0;
                string derivedEquationErrorMessage = string.Empty;

                // How to detect expression errors vs. circular definitions?
                if (!derivedStats.list[i].TryEvaluate((target as Actor), derivedStats, out derivedValue)) derivedEquationErrorMessage = "Invalid expression.";

                string newDerivedPropEquation = EditorGUILayout.TextField(derivedStats.list[i].expression, derivedEquationErrorMessage.Length == 0 ? EditorStyles.textField : errorBoxStyle);
                if (derivedEquationErrorMessage.Length > 0)
                    GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, derivedEquationErrorMessage));
                else
                    EditorGUILayout.LabelField(derivedValue.ToString(), GUILayout.MaxWidth(90));

                if (newDerivedPropEquation != derivedStats.list[i].expression) derivedPropEquationChanged = true;

                derivedStats.list[i].expression = newDerivedPropEquation;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            bool added = false;
            if (GUILayout.Button("Add"))
            {
                added = true;
                if (derivedStats == null) derivedStats = new DerivedStatList();

                List<DerivedStat> newDerivedStats;
                if (derivedStats.list != null)
                    newDerivedStats = new List<DerivedStat>(derivedStats.list);
                else
                    newDerivedStats = new List<DerivedStat>();
                newDerivedStats.Add(new DerivedStat());

                derivedStats.list = newDerivedStats.ToArray();
                this.Repaint();
            }

            if (GUI.changed || added)
            {
                EditorUtility.SetDirty(derivedStats);
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }

            if (derivedPropEquationChanged) this.Repaint();

        }
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
