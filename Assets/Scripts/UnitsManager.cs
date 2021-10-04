using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    static UnitsManager _instance;
    public static UnitsManager Instance { get { return _instance; } }

    [SerializeField] bool useJobs = true;

    public List<Unit> Agent = new List<Unit>(10000);
    public List<Unit> PlayerAgent = new List<Unit>(1000);
    List<Unit> movingPlayerAgent = new List<Unit>(1000);

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private void Update()
    {
        if (useJobs)
        {
            RefreshMovingUnitsList();

            ManageJobMovement();
        }
    }

    void RefreshMovingUnitsList ()
    {
        movingPlayerAgent.Clear();

        for (int i = 0; i < PlayerAgent.Count; i++)
            if (Agent.Contains(PlayerAgent[i]))
            {
                if (PlayerAgent[i].destinations.Count > 0)
                {
                    PlayerAgent[i].curDestination = PlayerAgent[i].destinations[PlayerAgent[i].destinationIndex];
                    movingPlayerAgent.Add(PlayerAgent[i]);
                }
            }
            else
                PlayerAgent.RemoveAt(i);
    }

    void ManageJobMovement ()
    {
        NativeArray<float3> positionsArr = new NativeArray<float3>(movingPlayerAgent.Count, Allocator.TempJob);
        NativeArray<float3> destinationsArr = new NativeArray<float3>(movingPlayerAgent.Count, Allocator.TempJob);
        NativeArray<float> speedArr = new NativeArray<float>(movingPlayerAgent.Count, Allocator.TempJob);

        for (int i = 0; i < movingPlayerAgent.Count; i++)
        {
            positionsArr[i] = movingPlayerAgent[i].transform.position;
            destinationsArr[i] = Vector3.Normalize(movingPlayerAgent[i].curDestination - movingPlayerAgent[i].transform.position);
            speedArr[i] = movingPlayerAgent[i].speed;
        }

        MoveUnitsJob moveJob = new MoveUnitsJob
        {
            deltaTime = Time.deltaTime,
            job_positionsArr = positionsArr,
            job_destinationsArr = destinationsArr,
            job_speedArr = speedArr
        };

        JobHandle jobHandle = moveJob.Schedule(movingPlayerAgent.Count, 10);
        jobHandle.Complete();

        for (int i = movingPlayerAgent.Count - 1; i >= 0; i--)
        {
            Unit curAgent = movingPlayerAgent[i];
            curAgent.transform.position = positionsArr[i];

            if (Vector3.Distance(curAgent.transform.position, curAgent.curDestination) <= 1f)
            {
                if (!curAgent.SetNextDestination())
                {
                    Debug.Log(curAgent.name + " has reached his final destination! :D");

                    movingPlayerAgent[i].destinations.Clear();
                    movingPlayerAgent[i].curDestination = Vector3.zero;
                    movingPlayerAgent.RemoveAt(i);

                    if (curAgent.curEnemy != null)
                        StartCoroutine(curAgent.FakeCombat(curAgent.curEnemy));
                }
            }
        }

        positionsArr.Dispose();
        destinationsArr.Dispose();
        speedArr.Dispose();
    }

    //JOB
    [BurstCompile]
    public struct MoveUnitsJob : IJobParallelFor 
    {
        public float deltaTime;

        public NativeArray<float3> job_positionsArr;
        public NativeArray<float3> job_destinationsArr;
        public NativeArray<float> job_speedArr;

        public void Execute (int index)
        {
            job_positionsArr[index] += job_destinationsArr[index] * job_speedArr[index] * deltaTime * .1f;
        }
    }
}
