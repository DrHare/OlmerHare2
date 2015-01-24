using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class PoolBossItem {
    public Transform prefabTransform;
    public int instancesToPreload = 1;
    public bool isExpanded = true;
    public bool logMessages = false;
    public bool allowInstantiateMore = false;
    public int itemHardLimit = 10;
	public bool allowRecycle = false;
}
