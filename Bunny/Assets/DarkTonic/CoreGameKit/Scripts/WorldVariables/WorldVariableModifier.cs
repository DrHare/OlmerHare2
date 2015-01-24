using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class WorldVariableModifier {
    public string _statName;
    public KillerInt _modValueIntAmt = new KillerInt(0, int.MinValue, int.MaxValue);
    public KillerFloat _modValueFloatAmt = new KillerFloat(0f, float.MinValue, float.MaxValue);
    public WorldVariableTracker.VariableType _varTypeToUse = WorldVariableTracker.VariableType._integer;

    public WorldVariableModifier(string statName, WorldVariableTracker.VariableType vType) {
        _statName = statName;
        _varTypeToUse = vType;
    }
}
