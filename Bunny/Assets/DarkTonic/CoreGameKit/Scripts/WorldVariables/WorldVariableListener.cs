using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/World Variable Listener")]
public class WorldVariableListener : MonoBehaviour {
	public string variableName = "";
	public WorldVariableTracker.VariableType vType = WorldVariableTracker.VariableType._integer;
	public bool displayVariableName = false;
	public int decimalPlaces = 1;
	public bool useCommaFormatting = true;
	
	private int variableValue = 0;	
	private float variableFloatValue = 0f;

	private UILabel label;
	
	void Awake() {
		label = this.GetComponent<UILabel>();
	}
	
	public virtual void UpdateValue(int newValue) {
		variableValue = newValue;
		
		var valFormatted = string.Format("{0}{1}",
			displayVariableName ? variableName + ": " : "",
			variableValue.ToString("N0"));

		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		if (label == null || !SpawnUtility.IsActive(label.gameObject)) {
			return;
		}
		
		label.text = valFormatted;
	}

	public virtual void UpdateFloatValue(float newValue) {
		variableFloatValue = newValue;
		
		var valFormatted = string.Format("{0}{1}",
			displayVariableName ? variableName + ": " : "",
			variableFloatValue.ToString("N" + decimalPlaces));

		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		label.text = valFormatted;
	}
	
}
