using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This class is used to hold any integer field in Core GameKit's Inspector's. You can either type an int value or choose a WorldVariable.
/// </summary>
[Serializable]
public class KillerInt : KillerVariable {
    public int selfValue;
    public int minimum = int.MinValue;
    public int maximum = int.MaxValue;

    private bool isValid = true;

    public KillerInt(int startingValue) : this(startingValue, int.MinValue, int.MaxValue) { }

    public KillerInt(int startingValue, int min, int max) {
        selfValue = startingValue;
        minimum = min;
        maximum = max;
    }

    public int LogIfInvalid(Transform trans, string fieldName, int? levelNum = null, int? waveNum = null, string trigEventName = null) {
        var val = Value; // trigger Valid or not evaluation

        if (isValid) {
            return val;
        }

        WorldVariableTracker.LogIfInvalidWorldVariable(worldVariableName, trans, fieldName, levelNum, waveNum, trigEventName);

        return val;
    }

    /// <summary>
    /// This will get or set the value of a Killer Int, which is either the value of the selected World Variable or the entered int. If this field is set to a World Variable, you cannot set it.
    /// </summary>
    public int Value {
        get {
            var varVal = DefaultValue;
            isValid = true;

            switch (variableSource) {
                case LevelSettings.VariableSource.Self:
                    varVal = selfValue;
                    break;
                case LevelSettings.VariableSource.Variable:
                    if (LevelSettings.illegalVariableNames.Contains(worldVariableName)) {
                        isValid = false;
                        break;
                    }
                    var variable = WorldVariableTracker.GetWorldVariable(worldVariableName);
                    if (variable == null) {
                        isValid = false;
                        break;
                    }

                    varVal = variable.CurrentIntValue;
                    break;
                default:
                    LevelSettings.LogIfNew("Unknown VariableSource: " + variableSource.ToString());
                    break;
            }

            return Math.Min(varVal, maximum);
        }
        set {
            switch (variableSource) {
                case LevelSettings.VariableSource.Self:
                    var newVal = Math.Min(value, maximum);
                    newVal = Math.Max(newVal, minimum);
                    selfValue = newVal;
                    break;
                default:
                    LevelSettings.LogIfNew("Cannot set KillerInt with source of: " + variableSource.ToString());
                    break;
            }
        }
    }

    public static int DefaultValue {
        get {
            return default(int);
        }
    }
}