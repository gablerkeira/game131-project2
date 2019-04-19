using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Selection_Rule : ScriptableObject
{
    public class SelectionRule
    {
        public string expression = string.Empty;
        public int depth = 0;

        public List<Actor> Apply(Actor initiator, List<Actor> availableTargets, DerivedStatList derivedStats)
        {
            string operation = null;

            if (expression.Contains(">=")) operation = ">=";
            else if (expression.Contains("<=")) operation = "<=";
            else if (expression.Contains("!=")) operation = "!=";
            else if (expression.Contains(">")) operation = ">";
            else if (expression.Contains("<")) operation = "<";
            else if (expression.Contains("=")) operation = "=";

            if (operation == null) throw new System.Exception("Invalid operator in SelectionRule expression [" + expression + "]. Only <, >, <=, >=, =, != are allowed.");

            string[] components = expression.Split(new string[] { operation }, System.StringSplitOptions.RemoveEmptyEntries);
            List<Actor> newAvailableTargets = new List<Actor>();
            for (int i = 0; i < availableTargets.Count; i++)
            {
                Actor candidateTarget = availableTargets[i];
                string[] left = components[0].ToLower().Split('.');
                string[] right = components[1].ToLower().Split('.');

                int leftValue, rightValue;
                if (left[0] == "my")
                {
                    initiator.TryEvaluate(left[1], derivedStats, out leftValue);
                }
                if (left[0] == "target")
                {
                    candidateTarget.TryEvaluate(left[1], derivedStats, out leftValue);
                }
                if (left[0] == "targets")
                {
                    int[] allValues = new int[availableTargets.Count];
                    for (int j = 0; j < allValues.Length; j++)
                    {
                        availableTargets[j].TryEvaluate(left[2], derivedStats, out allValues[j]);
                    }
                    int outcome = allValues[0];
                    for (int j = 1; j < allValues.Length; j++)
                    {
                        if (left[1] == "max" && allValues[j] > outcome) outcome = allValues[j];
                        if (left[1] == "min" && allValues[i] < outcome) outcome = allValues[j];
                    }
                    leftValue = outcome;
                }

                if (right[0] == "my")
                {

                    initiator.TryEvaluate(right[1], derivedStats, out rightValue);
                }
                if (right[0] == "target")
                {
                    candidateTarget.TryEvaluate(right[1], derivedStats, out rightValue);
                }
                if (right[0] == "targets")
                {
                    int[] allValues = new int[availableTargets.Count];
                    for (int j = 0; j < allValues.Length; j++)
                    {
                        availableTargets[j].TryEvaluate(right[2], derivedStats, out allValues[j]);
                    }
                    int outcome = allValues[0];
                    for (int j = 1; j < allValues.Length; j++)
                    {
                        if (right[1] == "max" && allValues[j] > outcome) outcome = allValues[j];
                        if (right[1] == "min" && allValues[i] < outcome) outcome = allValues[j];
                    }
                    rightValue = outcome;
                }
            }
        }
    }
}