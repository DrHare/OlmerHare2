using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WorldVariableRange {
    public int _modValueIntMin;
    public int _modValueIntMax;

    public float _modValueFloatMin;
    public float _modValueFloatMax;
    public string _statName;

    public WorldVariableTracker.VariableType _varTypeToUse = WorldVariableTracker.VariableType._integer;

    public WorldVariableRange(string statName, WorldVariableTracker.VariableType vType) {
        _statName = statName;
        _varTypeToUse = vType;
    }
}
