using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class LevelSpecifics {
    public LevelSettings.WaveOrder waveOrder = LevelSettings.WaveOrder.SpecifiedOrder;
    public List<LevelWave> WaveSettings = new List<LevelWave>();
    public bool isExpanded = true;
}
