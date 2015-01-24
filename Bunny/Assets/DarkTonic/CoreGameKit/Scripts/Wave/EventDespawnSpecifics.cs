using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class EventDespawnSpecifics {
    public bool eventEnabled = false;
    public bool useLayerFilter = false;
    public bool useTagFilter = false;
    public List<string> matchingTags = new List<string>() { "Untagged" };
    public List<int> matchingLayers = new List<int>() { 0 };
}
