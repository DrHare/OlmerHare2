using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This class is used to hold any float field in Core GameKit's Inspector's. You can either type a float value or choose a WorldVariable.
/// </summary>
[Serializable]
public class KillerFloat : KillerVariable {
    public float selfValue;
    public float minimum = float.MinValue;
    public float maximum = float.MaxValue;

    private bool isValid = true;

    public KillerFloat(float startingValue) : this(startingValue, float.MinValue, float.MaxValue) { }

    public KillerFloat(float startingValue, float min, float max) {
        selfValue = startingValue;
        minimum = min;
        maximum = max;
    }

    public float LogIfInvalid(Transform trans, string fieldName, int? levelNum = null, int? waveNum = null, string trigEventName = null) {
        var val = Value; // trigger Valid or not evaluation

        if (isValid) {
            return val;
        }

        WorldVariableTracker.LogIfInvalidWorldVariable(worldVariableName, trans, fieldName, levelNum, waveNum, trigEventName);

        return val;
    }

    /// <summary>
    /// This will get or set the value of a Killer Float, which is either the value of the selected World Variable or the entered float. If this field is set to a World Variable, you cannot set it.
    /// </summary>
    public float Value {
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

                    varVal = variable.CurrentFloatValue;
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

    public static float DefaultValue {
        get {
            return default(float);
        }
    }
}