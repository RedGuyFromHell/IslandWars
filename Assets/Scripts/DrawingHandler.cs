using Dreamteck.Splines;
using System.Collections.Generic;
using UnityEngine;

public class DrawingHandler : MonoBehaviour
{
    static DrawingHandler _instance;
    public static DrawingHandler Instance { get{ return _instance; } }

    [SerializeField] bool freeHand = false;     //toggle freehand
    [SerializeField] float BridgeYHeight;       //the height of the bridge on the Y Axis
    [SerializeField] GameObject bridgeParent;       //the parent object of the bridge
    [SerializeField] GameObject bridgePrefab;       //the model of the bridge
    SplineComputer spline;      //spline of bridge
    SplineMesh splineMesh;      //mesh of spline

    public Island startIsland;      //island from which the bridge is drawn
    public Island endIsland;        //island where the bridge ends up

    List<SplinePoint> drawPoints = new List<SplinePoint>();     //List with 

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    void InitBridge ()
    {
        if (curBridge != null)
        {
            spline = curBridge.GetComponent<SplineComputer>();
            splineMesh = curBridge.GetComponent<SplineMesh>();
        }
    }

    SplinePoint startPoint;
    SplinePoint endPoint;

    void UpdateSplinePoints ()
    {
        startPoint = new SplinePoint();

        startPoint.position = startingPointPos;
        startPoint.normal = Vector3.up;
        startPoint.size = 2f;
        startPoint.color = Color.white;
        drawPoints.Add(startPoint);


        endPoint = new SplinePoint();

        endPoint.position = currentPointPos;
        endPoint.normal = Vector3.up;
        endPoint.size = 2f;
        endPoint.color = Color.green;
        drawPoints.Add(endPoint);
    }

    GameObject curBridge;
    Vector3 startingPointPos = new Vector3();
    Vector3 currentPointPos = new Vector3();
    Vector3 lastPos = new Vector3();

    public int fightingUntis = 0;
    private void Update()
    {
        if (!freeHand && fightingUntis == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (CheckForIslandSnap())
                {
                    curBridge = Instantiate(bridgePrefab, bridgeParent.transform);
                    InitBridge();

                    startingPointPos = GetRaycastObjFromScreenPos();

                    GameManager.Instance.phase = GamePhase.Drawing;
                }
            }

            if (GameManager.Instance.phase == GamePhase.Drawing)
            {
                if (Input.GetMouseButton(0))
                {
                    drawPoints.Clear();
                    currentPointPos = GetRaycastPointFromScreenPos();

                    UpdateSplinePoints();
                }

                Debug.Log(startPoint.position + " | " + endPoint.position);
                spline.SetPoints(drawPoints.ToArray(), SplineComputer.Space.World);
                spline.RebuildImmediate(true, true);

                if (Input.GetMouseButtonUp(0))
                {
                    drawPoints.Clear();
                    Debug.Log(CheckForIslandSnap());
                    if (!CheckForIslandSnap())
                        Destroy(curBridge);
                    else
                    {
                        currentPointPos = GetRaycastObjFromScreenPos();
                        UpdateSplinePoints();
                    }

                    GameManager.Instance.phase = GamePhase.Idle;
                }
            }
        }
        else if (fightingUntis == 0)
        {
            if (Input.GetMouseButtonDown(0) && CheckForIslandSnap() && GetRaycastIslandFromScreenPos() != null)
            {
                startIsland = GetRaycastIslandFromScreenPos();
                if (startIsland.type == IslandType.player)
                {
                    curBridge = Instantiate(bridgePrefab, bridgeParent.transform);
                    InitBridge();

                    GameManager.Instance.phase = GamePhase.Drawing;
                }
            }

            if (GameManager.Instance.phase == GamePhase.Drawing)
                UpdateBridge();
        }
    }

    void UpdateBridge ()
    {
        if (Input.GetMouseButton(0) && Vector3.Distance(GetRaycastPointFromScreenPos(), lastPos) > .65f)
        {
            SplinePoint newPoint = new SplinePoint();
            newPoint.position = GetRaycastPointFromScreenPos();
            newPoint.normal = Vector3.up;
            newPoint.size = 2f;
            newPoint.color = Color.white;
            drawPoints.Add(newPoint);

            lastPos = newPoint.position;
        }

        if (drawPoints.Count == 0)
            splineMesh.GetChannel(0).count = 1;
        else
            splineMesh.GetChannel(0).count = (int)(drawPoints.Count / 2.0f);

        spline.SetPoints(drawPoints.ToArray(), SplineComputer.Space.World);
        spline.RebuildImmediate();

        if (Input.GetMouseButtonUp(0))
        {
            if (!CheckForIslandSnap())
            {
                Destroy(curBridge);
                GameManager.Instance.phase = GamePhase.Idle;
            }
            else if (GetRaycastIslandFromScreenPos() != null)
            {
                endIsland = GetRaycastIslandFromScreenPos();
                endIsland.OrderIslandersByDistanceFrom(GetRaycastPointFromScreenPos());

                GameManager.Instance.StartInvasion(startIsland, endIsland, SplinePointArrayToVector3Array(drawPoints.ToArray()));
            }

            drawPoints.Clear();
        }
    }


    #region Feedback 
    //void SpawnParticles (int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        Vector3 startPos = new Vector3(0, 0.6f, -8);

    //        Vector3 randomDir = startPos + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    //        LeanTween.delayedCall(0.1f, () => FeedbackSequence(particlePrefab, startPos));
    //    }
    //}

    void FeedbackSequence(GameObject emoji, Vector3 emojiPos)
    {
        GameObject curEmoji = Instantiate(emoji, emojiPos, Quaternion.identity);

        Vector3 originalScale = curEmoji.transform.localScale;
        curEmoji.transform.localScale = Vector3.zero;

        LeanTween.scale(curEmoji, originalScale, 1.2f).setEaseOutCirc();
        LeanTween.move(curEmoji, GenerateRandomDirection(emojiPos), 0.5f).setEaseOutCirc().setDelay(Random.value).setOnComplete
            (
                () => LeanTween.delayedCall(0.4f, () => Destroy(curEmoji))
            );
    }

    Vector3 GenerateRandomDirection(Vector3 origin)
    {
        float randomDistance = Random.Range(0.3f, 0.8f);
        Vector3 randomDir = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), 0) * randomDistance;
        return origin + randomDir;
    }
    #endregion

    #region RayCasts
    Vector3 GetRaycastPointFromScreenPos()
    {
        LayerMask drawMask = LayerMask.GetMask("Draw Mask");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, drawMask))
            return new Vector3(hit.point.x, BridgeYHeight, hit.point.z);
        else
            return Vector3.zero;
    }

    Vector3 GetRaycastObjFromScreenPos()
    {
        LayerMask islandMask = LayerMask.GetMask("Island Mask");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, islandMask))
            return new Vector3(hit.transform.position.x, BridgeYHeight, hit.transform.position.z);
        else
            return Vector3.zero;
    }

    Island GetRaycastIslandFromScreenPos()
    {
        LayerMask islandMask = LayerMask.GetMask("Island Mask");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, islandMask))
            return hit.transform.parent.GetComponent<Island>();
        else
            return null;
    }

    bool CheckForIslandSnap ()
    {
        LayerMask islandMask = LayerMask.GetMask("Island Mask");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, Mathf.Infinity, islandMask);
    }

    bool CheckForBridgeIntersection ()
    {
        LayerMask bridgeMask = LayerMask.GetMask("Bridge Mask");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(ray, Mathf.Infinity, bridgeMask);
    }
    #endregion

    Vector3[] SplinePointArrayToVector3Array (SplinePoint[] array)
    {
        Vector3[] returnArray = new Vector3[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            returnArray[i] = array[i].position;
        }

        return returnArray;
    }
}
