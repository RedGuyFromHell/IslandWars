using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Fight : MonoBehaviour
{
    public Island attacker, defender;
    public int batchSize = 0;

    public Fight (Island attacker, Island defender)
    {
        this.attacker = attacker;
        this.defender = defender;

        batchSize = defender.maxPop + defender.unitsOnIsland.Count;
    }

    public void InvadeIsland (Vector3[] offsetBridge)
    {
        for (int i = 0; i < batchSize; i++)
        {
            attacker.unitsOnIsland[i].myFight = this;
            for (int j = 0; j < offsetBridge.Length; j++)
                attacker.unitsOnIsland[i].destinations.Add(offsetBridge[j]);

            if (i < defender.unitsOnIsland.Count)
            {
                attacker.unitsOnIsland[i].destinations.Add(defender.unitsOnIsland[i].transform.position);
                attacker.unitsOnIsland[i].curEnemy = defender.unitsOnIsland[i];
            }
            else
                attacker.unitsOnIsland[i].destinations.Add(defender.positionsList[i]);
        }

        defender.unitsOnIsland.AddRange(attacker.unitsOnIsland);
    }
}
