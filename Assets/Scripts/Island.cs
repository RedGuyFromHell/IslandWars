using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Island : MonoBehaviour
{
    public int maxPop = 10;     //default maximum population. Is custom set from editor
    [SerializeField] public List<Unit> islanders = new List<Unit>(1000);    //total units from that island
    [SerializeField] public List<Unit> unitsOnIsland = new List<Unit>(1000);    //units actually on that island

    [SerializeField] public IslandType type;    //type of island(player, enemy, neutral, completed)

    [SerializeField] GameObject unitPrefab;    //model of units
    [SerializeField] int baseSize = 0;     //minimum size of an island. Is set from Editor

    int radius = 10;        //radius of the island
    Vector3[,] positionsMatrix;     //the matrix with the possible positions on the island
    public List<Vector3> positionsList = new List<Vector3>(1000);       //the above matrix in list form
    List<Vector3> availableSpots = new List<Vector3>(1000);     //the empty available spots on the island

    private void Start()
    {
        InitIsland();
    }

    void InitIsland()
    {
        GenerateIslandAttributes();
        PopulateIslandWithIslanders(availableSpots.Count / 2);
    }

    public void OrderIslandersByDistanceFrom(Vector3 point)
    {
        unitsOnIsland.OrderByDescending(x => Vector3.Distance(x.transform.position, point));
    }

    void GenerateIslandAttributes()
    {
        radius = baseSize + Random.Range(5, 20);

        transform.GetChild(0).transform.localScale = new Vector3(radius, 1, radius);
        positionsMatrix = new Vector3[radius, radius];
        PopulatePositionsMatrix();

        maxPop = availableSpots.Count;
    }

    void PopulateIslandWithIslanders (int count)
    {
        for (int i = 0; i < count; i ++)
        {
            Unit newUnit = Instantiate(unitPrefab, GetRandomFreePosition(), Quaternion.identity, transform).GetComponent<Unit>();
            newUnit.myIsland = this;

            islanders.Add(newUnit);
            unitsOnIsland.Add(newUnit);
        }

    }

    void PopulatePositionsMatrix()
    {
        int minX = -radius / 2;
        int minZ = -radius / 2;
        float mRadius = radius / 2 - 1;
        for (int i = 0; i < radius; i++)
        {
            float aSquared = (radius/2 - i) * (radius/2 - i);
            for (int j = 0; j < radius; j++)
            {
                float bSquared = (radius/2 - j) * (radius/2 - j);
                if ((aSquared + bSquared) < (mRadius * mRadius))
                {
                    positionsMatrix[i, j] = transform.position + new Vector3(minX + i, 1, minZ + j);
                    availableSpots.Add(positionsMatrix[i, j]);
                    positionsList.Add(positionsMatrix[i, j]);
                }
            }
        }
    }

    Vector3 GetRandomFreePosition ()
    {
        int randomIndex = Random.Range(0, availableSpots.Count);
        Vector3 randomPos = availableSpots[randomIndex];
        availableSpots.RemoveAt(randomIndex);
        return randomPos;
    }
}

public enum IslandType
{
    player,
    neutral,
    enemy,
    completed
}


