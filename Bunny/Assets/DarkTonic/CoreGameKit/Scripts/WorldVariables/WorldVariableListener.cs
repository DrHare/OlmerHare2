#if UNITY_4_6 || UNITY_5_0
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/World Variable Listener")]
public class WorldVariableListener : MonoBehaviour {
	public string variableName = "";
	public WorldVariableTracker.VariableType vType = WorldVariableTracker.VariableType._integer;
	public bool displayVariableName = false;
	public int decimalPlaces = 1;
	public bool useCommaFormatting = true;
	
	private int variableValue = 0;   
	private float variableFloatValue = 0f;
	
	private UnityEngine.UI.Text text;
	
	void Awake() {
		text = this.GetComponent<UnityEngine.UI.Text>();
	}
	
	public virtual void UpdateValue(int newValue) {
		variableValue = newValue;
		var valFormatted = string.Format("{0}{1}",
		                                 
		                                 displayVariableName ? variableName + ": " : "",
		                                 variableValue.ToString("N0"));
		
		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		if (text == null || !SpawnUtility.IsActive(text.gameObject)) {
			return;
		}
		
		text.text = valFormatted;
	}
	
	public virtual void UpdateFloatValue(float newValue) {
		variableFloatValue = newValue;
		var valFormatted = string.Format("{0}{1}",
		                                 displayVariableName ? variableName + ": " : "",
		                                 variableFloatValue.ToString("N" + decimalPlaces));
		
		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		text.text = valFormatted;
	}
}
#else
using UnityEngine;
using System.Collections;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/World Variable Listener")]
public class WorldVariableListener : MonoBehaviour {
	public string variableName = "";
	public WorldVariableTracker.VariableType vType = WorldVariableTracker.VariableType._integer;
	public int decimalPlaces = 1;
	public bool useCommaFormatting = true;
	
	private int variableValue = 0;
	private float variableFloatValue = 0f;
	
	public int xStart = 50; // ALSO delete this when you get rid of the OnGUI section. You won't need it.
	
	void Reset() {
		var src = this.GetComponent<WorldVariable>();
		if (src != null) {
			src.listenerPrefab = this;
			this.variableName = this.name;
		}
	}
	
	public virtual void UpdateValue(int newValue) {
		variableValue = newValue;
	}
	
	public virtual void UpdateFloatValue(float newValue) {
		variableFloatValue = newValue;
	}
	
	// This is just used for illustrative purposes. You might replace this with code to update a non-Unity GUI text element. If you use NGUI, please install the optional package "NGUI_CoreGameKit" to get an NGUI version of this script, replacing this one.
	void OnGUI() {
		var valFormatted = "";
		switch (vType) {
		case WorldVariableTracker.VariableType._integer:
			valFormatted = variableValue.ToString("N0");
			if (!useCommaFormatting) {
				valFormatted = valFormatted.Replace(",", "");
			}
			GUI.Label(new Rect(xStart, 120, 180, 40), variableName + ": " + valFormatted);
			break;
		case WorldVariableTracker.VariableType._float:
			valFormatted = variableFloatValue.ToString("N" + decimalPlaces);
			if (!useCommaFormatting) {
				valFormatted = valFormatted.Replace(",", "");
			}
			GUI.Label(new Rect(xStart, 120, 180, 40), variableName + ": " + valFormatted);
			break;
		default:
			LevelSettings.LogIfNew("Add code for varType: " + vType.ToString());
			break;
		}
	}
}
#endif