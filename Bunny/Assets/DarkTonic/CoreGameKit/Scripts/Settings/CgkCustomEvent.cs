using System;
using System.Collections;

[Serializable]
public class CgkCustomEvent {
	public string EventName;
	public string ProspectiveName;
	public bool eventExpanded = true;
	public LevelSettings.EventReceiveMode eventRcvMode = LevelSettings.EventReceiveMode.Always;
	public KillerFloat distanceThreshold = new KillerFloat (10, 0.1f, float.MaxValue);
    public int frameLastFired = -1;

	public CgkCustomEvent(string eventName) {
		EventName = eventName;
		ProspectiveName = eventName;
	}
}
