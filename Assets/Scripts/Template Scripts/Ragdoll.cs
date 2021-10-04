using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private Collider[] colliders;
    [SerializeField] private Rigidbody[] rigidbodies;

    [SerializeField] private Rigidbody rootRigidbody;
    [SerializeField] private Collider rootCollider;
    [SerializeField] private Animator rootAnimator;
    [SerializeField] private Transform root;

    [SerializeField] private Vector3[] cachedPositions = new Vector3[11];
    [SerializeField] private Vector3[] cachedRotations = new Vector3[11];
    private void Start()
    {
        InitializeRagdoll();
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                cachedPositions[i] = rigidbodies[i].transform.localPosition;
                cachedRotations[i] = rigidbodies[i].transform.localEulerAngles;
            }
        }
    }

    public void EnableRagdoll(Vector3 force)
    {
        rootAnimator.enabled = false;
        rootRigidbody.isKinematic = true;
        rootCollider.enabled = false;
        
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = false;
            colliders[i].enabled = true;
            rigidbodies[i].AddForce(force, ForceMode.Impulse);
        }
    }
    
    public void DisableRagdoll()
    {            
        root.SetParent(null);
        rootCollider.transform.position = new Vector3(colliders[0].transform.position.x, rootCollider.transform.position.y, colliders[0].transform.position.z);
       // rootCollider.transform.eulerAngles = new Vector3(rootCollider.transform.eulerAngles.x,  UnwrapAngle(root.transform.localRotation.eulerAngles.y), rootCollider.transform.eulerAngles.z);
        root.SetParent(rootCollider.transform.GetChild(1));
        


        
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
            colliders[i].enabled = false;
            
            rigidbodies[i].transform.LeanMoveLocal(cachedPositions[i], 0.4f);
            LeanTween.rotateLocal(rigidbodies[i].gameObject, cachedRotations[i], 0.4f);
        }
        
        LeanTween.delayedCall(0.4f, () =>
        {
            rootAnimator.enabled = true;
            rootRigidbody.isKinematic = false;
            rootCollider.enabled = true;
        });
    }
    
    private static float UnwrapAngle(float angle)
    {
        if(angle >=0)
            return angle;
 
        angle = -angle%360;
 
        return 360-angle;
    }
    
    private void InitializeRagdoll()
    {
        rigidbodies = root.gameObject.GetComponentsInChildren<Rigidbody>();
        colliders = root.gameObject.GetComponentsInChildren<Collider>();
        
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
            colliders[i].enabled = false;
        }
    }
}
