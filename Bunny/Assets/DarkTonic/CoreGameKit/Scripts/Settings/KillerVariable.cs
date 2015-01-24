using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class KillerVariable {
    public LevelSettings.VariableSource variableSource = LevelSettings.VariableSource.Self;
    public string worldVariableName = string.Empty;
    public ModMode curModMode = ModMode.Add;

    public enum ModMode {
        Set,
        Add,
        Sub,
        Mult
    }
}
