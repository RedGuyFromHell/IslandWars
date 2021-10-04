using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public GamePhase phase = GamePhase.Idle;        //The General Phase of the game

    [SerializeField] GameObject islandPrefab;       //island model
    [SerializeField] public List<Fight> activeFights;       //active fights

    //Daca vrei sa generezi random astia sunt parametri. Functia e GenerateLevel din regiunea "Map Generation"
    //Dar inca este Work In Progress. insulele se suprapun si metoda e sloppy
    [SerializeField] int nrOfIslands = 5;       
    [SerializeField] int width;
    [SerializeField] int length;
    Vector3[,] positionsMatrix;     
    List<Vector3> availableSpots = new List<Vector3>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    GamePhase lastPhase;
    private void Update()
    {
        //Check if the Game Phase has changed
        if (phase != lastPhase)
        {
            switch (phase)
            {
                case GamePhase.Idle:
                    //change screen color and add external actors back
                    break;
                case GamePhase.Drawing:
                    //make screen gray and slow down time
                    break;
                case GamePhase.Invading:
                    //give screen battle theme and sound horn
                    break;
                case GamePhase.Finishing:
                    //decide level outcome
                    break;
            }
        }
        lastPhase = phase;
    }

    #region Map Generation
    void GenerateLevel ()
    {
        positionsMatrix = new Vector3[width, length];
        PopulatePositionsMatrix(10);
        PopulateMapWithIslands(nrOfIslands);
    }

    void PopulateMapWithIslands (int count)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(islandPrefab, GetRandomFreePosition(), Quaternion.identity);
        }
    }

    void PopulatePositionsMatrix(int step)
    {
        int minX = -width / 2 + 1;
        int minZ = -length / 2 + 1;
        for (int i = 0; i < width - 1; i++)
            for (int j = 0; j < length - 1; j++)
            {
                positionsMatrix[i, j] = new Vector3(minX + i * step, 2, minZ + j * step);
                availableSpots.Add(positionsMatrix[i, j]);
            }
    }

    Vector3 GetRandomFreePosition()
    {
        int randomIndex = Random.Range(0, availableSpots.Count);
        Vector3 randomPos = availableSpots[randomIndex];
        availableSpots.RemoveAt(randomIndex);
        return randomPos;
    }
    #endregion

    public void StartInvasion (Island invader, Island target, Vector3[] bridgePoints)
    {
        Debug.Log("it made it to invade Island");
        Vector3[] offsetBridge = new Vector3[bridgePoints.Length];
        for (int i = 0; i < bridgePoints.Length; i++)
            offsetBridge[i] = bridgePoints[i] + Vector3.up * 2f;

        Fight tempFight = new Fight(invader, target);
        activeFights.Add(tempFight);
        tempFight.InvadeIsland(offsetBridge);

        phase = GamePhase.Invading;
    }

    public void DecideInvadeOutcome(Island invader, Island target)
    {
        Debug.Log("Outcome is decided");
        Debug.Log(invader.unitsOnIsland.Count + " VS " + target.unitsOnIsland.Count);

        if (invader.unitsOnIsland.Count > target.unitsOnIsland.Count)
            ConquerIsland(invader, target);
        else if (invader.unitsOnIsland.Count < target.unitsOnIsland.Count)
            PlayLoseSequence();
        else
            PlayTieSequence();

        GameManager.Instance.phase = GamePhase.Idle;
    }

    void ConquerIsland (Island originIsland, Island completedIsland)
    {
        completedIsland.type = IslandType.player;

        completedIsland.islanders.Clear();
        foreach (Unit islander in completedIsland.unitsOnIsland)
        {
            completedIsland.islanders.Add(islander);
            originIsland.islanders.Remove(islander);
        }
    }

    void PlayLoseSequence ()
    {
        //you lost the game;
        Debug.Log("You lost the game! :(");
    }

    void PlayWinSequence ()
    {
        //you won the game;
        Debug.Log("You won the game! :)");
    }

    void PlayTieSequence ()
    {
        //Tie!
        Debug.Log("You got a tie! :|");
    }
}

public enum GamePhase 
{ 
    Idle,
    Drawing,
    Invading,
    Finishing
}

