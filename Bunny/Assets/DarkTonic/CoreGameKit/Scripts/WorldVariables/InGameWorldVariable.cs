using UnityEngine;
using System;
using System.Collections;
#if UNITY_WEBPLAYER || UNITY_WP8 || UNITY_METRO
	// can't compile this class
#else
using PlayerPrefs = PreviewLabs.PlayerPrefs;
#endif

/// <summary>
/// This class represents a World Variable in the current Scene at runtime. You can get, set or modify the value.
/// </summary>
[Serializable]
public class InGameWorldVariable {
    private const string PLAYER_PREF_STAT_TOKEN = "~KWStat_{0}~";

    private string _statName;
    private WorldVariable _sourceStat;
    private WorldVariableTracker.VariableType _varType;

    private string _tokenizedPrefsKey = string.Empty;

    public InGameWorldVariable(WorldVariable srcStat, string statName, WorldVariableTracker.VariableType varType) {
        _sourceStat = srcStat;
        _statName = statName;
        _varType = varType;
    }

    public WorldVariableTracker.VariableType VariableType {
        get {
            return _varType;
        }
    }

    /// <summary>
    /// This property returns the integer value that the World Variable currently holds.
    /// </summary>
    public int CurrentIntValue {
        get {
            if (!PlayerPrefs.HasKey(TokenizedPrefsKey)) {
                PlayerPrefs.SetInt(TokenizedPrefsKey, _sourceStat.startingValue);
            }

            return PlayerPrefs.GetInt(TokenizedPrefsKey);
        }
        set {
            if (_sourceStat.hasMaxValue && value > _sourceStat.intMaxValue) {
                value = _sourceStat.intMaxValue;
            }

            if (value < 0 && !_sourceStat.allowNegative) {
                value = 0;
            }

            PlayerPrefs.SetInt(TokenizedPrefsKey, value);

            if (_sourceStat.listenerPrefab != null) {
                _sourceStat.listenerPrefab.UpdateValue(value);
            }

            EndGameIfIntInRange();
        }
    }

    /// <summary>
    /// This property returns the float value that the World Variable currently holds.
    /// </summary>
    public float CurrentFloatValue {
        get {
            if (!PlayerPrefs.HasKey(TokenizedPrefsKey)) {
                PlayerPrefs.SetFloat(TokenizedPrefsKey, _sourceStat.startingValueFloat);
            }

            return PlayerPrefs.GetFloat(TokenizedPrefsKey);
        }
        set {
            if (_sourceStat.hasMaxValue && value > _sourceStat.floatMaxValue) {
                value = _sourceStat.floatMaxValue;
            }

            if (value < 0f && !_sourceStat.allowNegative) {
                value = 0f;
            }

            PlayerPrefs.SetFloat(TokenizedPrefsKey, value);

            if (_sourceStat.listenerPrefab != null) {
                _sourceStat.listenerPrefab.UpdateFloatValue(value);
            }

            EndGameIfFloatInRange();
        }
    }

    public void ModifyVariable(WorldVariableModifier mod) {
        switch (mod._varTypeToUse) {
            case WorldVariableTracker.VariableType._integer:
                var modVal = mod._modValueIntAmt.Value;

                switch (mod._modValueIntAmt.curModMode) {
                    case KillerVariable.ModMode.Add:
                        AddToIntValue(modVal);
                        break;
                    case KillerVariable.ModMode.Set:
                        SetIntValueIfAllowed(modVal);
                        AddToIntValue(0); // need to trigger game over if G.O. value reached
                        break;
                    case KillerVariable.ModMode.Sub:
                        AddToIntValue(-modVal);
                        break;
                    case KillerVariable.ModMode.Mult:
                        MultiplyByIntValue(modVal);
                        break;
                    default:
                        LevelSettings.LogIfNew("Add code for modMode: " + mod._modValueIntAmt.curModMode.ToString());
                        break;
                }

                break;
            case WorldVariableTracker.VariableType._float:
                var modFloatVal = mod._modValueFloatAmt.Value;

                switch (mod._modValueFloatAmt.curModMode) {
                    case KillerVariable.ModMode.Add:
                        AddToFloatValue(modFloatVal);
                        break;
                    case KillerVariable.ModMode.Set:
                        SetFloatValueIfAllowed(modFloatVal);
                        AddToFloatValue(0f);
                        break;
                    case KillerVariable.ModMode.Sub:
                        AddToFloatValue(-modFloatVal);
                        break;
                    case KillerVariable.ModMode.Mult:
                        MultiplyByFloatValue(modFloatVal);
                        break;
                    default:
                        LevelSettings.LogIfNew("Add code for modMode: " + mod._modValueIntAmt.curModMode.ToString());
                        break;
                }

                break;
            default:
                LevelSettings.LogIfNew("Add code for varType: " + mod._varTypeToUse.ToString());
                break;
        }
    }

    private void EndGameIfIntInRange() {
        if (_sourceStat.canEndGame) {
            if (CurrentIntValue >= _sourceStat.endGameMinValue && CurrentIntValue <= _sourceStat.endGameMaxValue) {
                LevelSettings.IsGameOver = true;
            }
        }
    }

    private void EndGameIfFloatInRange() {
        if (_sourceStat.canEndGame) {
            if (CurrentFloatValue >= _sourceStat.endGameMinValue && CurrentFloatValue <= _sourceStat.endGameMaxValue) {
                LevelSettings.IsGameOver = true;
            }
        }
    }

    public void SetIntValueIfAllowed(int newVal) {
        switch (_sourceStat.changeMode) {
            case WorldVariable.VariableChangeMode.Any:
                break;
            case WorldVariable.VariableChangeMode.OnlyDecrease:
                if (newVal > CurrentIntValue) {
                    return; // not allowed
                }
                break;
            case WorldVariable.VariableChangeMode.OnlyIncrease:
                if (newVal < CurrentIntValue) {
                    return; // not allowed
                }
                break;
        }

        CurrentIntValue = newVal;
    }

    public void SetFloatValueIfAllowed(float newVal) {
        switch (_sourceStat.changeMode) {
            case WorldVariable.VariableChangeMode.Any:
                break;
            case WorldVariable.VariableChangeMode.OnlyDecrease:
                if (newVal > CurrentFloatValue) {
                    return; // not allowed
                }
                break;
            case WorldVariable.VariableChangeMode.OnlyIncrease:
                if (newVal < CurrentFloatValue) {
                    return; // not allowed
                }
                break;
        }

        CurrentFloatValue = newVal;
    }

    private void AddToIntValue(int valueToAdd) {
        SetIntValueIfAllowed(CurrentIntValue + valueToAdd);

        EndGameIfIntInRange();

        if (_sourceStat.listenerPrefab != null) {
            _sourceStat.listenerPrefab.UpdateValue(CurrentIntValue);
        }
    }

    private void MultiplyByIntValue(int valueToMultiplyBy) {
        SetIntValueIfAllowed(CurrentIntValue * valueToMultiplyBy);

        EndGameIfIntInRange();

        if (_sourceStat.listenerPrefab != null) {
            _sourceStat.listenerPrefab.UpdateValue(CurrentIntValue);
        }
    }

    private void AddToFloatValue(float valueToAdd) {
        SetFloatValueIfAllowed(CurrentFloatValue + valueToAdd);

        EndGameIfFloatInRange();

        if (_sourceStat.listenerPrefab != null) {
            _sourceStat.listenerPrefab.UpdateFloatValue(CurrentFloatValue);
        }
    }

    private void MultiplyByFloatValue(float valueToMultiplyBy) {
        SetFloatValueIfAllowed(CurrentFloatValue * valueToMultiplyBy);

        EndGameIfFloatInRange();

        if (_sourceStat.listenerPrefab != null) {
            _sourceStat.listenerPrefab.UpdateFloatValue(CurrentFloatValue);
        }
    }

    private string TokenizedPrefsKey {
        get {
            if (_tokenizedPrefsKey == string.Empty) {
                _tokenizedPrefsKey = GetTokenPrefsKey(_statName);
            }

            return _tokenizedPrefsKey;
        }
    }

    public static string GetTokenPrefsKey(string myStatName) {
        return string.Format(PLAYER_PREF_STAT_TOKEN, myStatName);
    }
}