using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WorldVariable : MonoBehaviour {
    public bool isExpanded = true;
    public WorldVariableTracker.VariableType varType = WorldVariableTracker.VariableType._integer;
    public bool allowNegative = false;
    public bool canEndGame = false;
    public bool hasMaxValue = false;

    public int startingValue = 0;
    public int endGameMinValue = 0;
    public int endGameMaxValue = 0;
    public int prospectiveValue = 0;
    public int intMaxValue = 100;

    public float startingValueFloat = 0f;
    public float endGameMinValueFloat = 0f;
    public float endGameMaxValueFloat = 0f;
    public float prospectiveFloatValue = 0f;
    public float floatMaxValue = 100f;

    public StatPersistanceMode persistanceMode = StatPersistanceMode.ResetToStartingValue;
    public WorldVariableListener listenerPrefab;
    public VariableChangeMode changeMode = VariableChangeMode.Any;

    void Awake() {
        this.useGUILayout = false;
    }

    void Start() {
        if (this.listenerPrefab != null) {
            var variable = WorldVariableTracker.GetWorldVariable(this.name);
            if (variable == null) {
                return;
            }

            var curVal = variable.CurrentIntValue;
            listenerPrefab.UpdateValue(curVal);
        }
    }

    public enum StatPersistanceMode {
        ResetToStartingValue,
        KeepFromPrevious
    }

    public enum VariableChangeMode {
        OnlyIncrease,
        OnlyDecrease,
        Any
    }
}
