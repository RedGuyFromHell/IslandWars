using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] public float speed;        //speed of unit
    [SerializeField] public int health = 100;

    [SerializeField] public Unit curEnemy = null;       //the current enemy. If null it has no enemy
    [SerializeField] public List<Vector3> destinations;     //list of destination the unit will go through
    [SerializeField] public Vector3 curDestination;     //the current destination of unit. If null it has no destinatin

    public Island myIsland;     //home island of unit
    public Fight myFight;       //fight in which unit is part of

    private void Start()
    {
        UnitsManager.Instance.Agent.Add(this);
        if (myIsland.type == IslandType.player)
            UnitsManager.Instance.PlayerAgent.Add(this);
    }

    public int destinationIndex = 0;        //index of curDestination in destinations list
    public bool SetNextDestination ()
    {
        if (curDestination != destinations[destinations.Count - 1])
        {
            curDestination = destinations[destinationIndex++];
            return true;
        }
        else
            return false;
    }

    void DestroyUnit(Unit unit)
    {
        UnitsManager.Instance.Agent.Remove(unit);
        myIsland.islanders.Remove(unit);
        unit.StopAllCoroutines();
        Destroy(unit.gameObject);
    }

    public IEnumerator FakeCombat (Unit target)
    {
        DrawingHandler.Instance.fightingUntis++;
        yield return new WaitForSeconds(2);
        DrawingHandler.Instance.fightingUntis--;

        myFight.batchSize--;
        if (myFight.batchSize == 0)
        {
            Debug.Log("fight got deleted");
            GameManager.Instance.activeFights.Remove(myFight);
            GameManager.Instance.DecideInvadeOutcome(myFight.attacker, myFight.defender);
        }

        int result = this.health - target.health;
        if (result < 0)
            DestroyUnit(this);
        else if (result == 0)
        {
            DestroyUnit(target);
            DestroyUnit(this);
        }
        else
        {
            DestroyUnit(target);
        }

        curEnemy = null;
    }
}
