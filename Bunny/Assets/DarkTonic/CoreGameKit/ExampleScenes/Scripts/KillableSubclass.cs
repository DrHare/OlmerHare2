using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class KillableSubclass : Killable {
	public override void DestroyKillable (string scenarioName)
	{
		var enemyReachedTower = true; // change this to whatever your logic is to determine true or false
		
		if (enemyReachedTower) {
			scenarioName = "Reached Tower"; // change the scenario name to one of your others, instead of the default.
		}
		
		base.DestroyKillable (scenarioName);
	}
}
