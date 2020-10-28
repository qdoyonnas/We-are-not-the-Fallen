using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(AudioSource))]
public class EnemyBase : MonoBehaviour
{
    public float accel = 1f;

    Rigidbody rigidbody;
    AudioSource audioSource;

    List<Block> trackedObjects = new List<Block>();

    Vector3 positionTarget;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        positionTarget = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 force = positionTarget - transform.position;
        force.z = 0;
        force.Normalize();
        force *= accel;

        rigidbody.AddForce(force);
    }

    private void OnTriggerEnter(Collider other)
    {
        Block block = other.GetComponentInParent<Block>();
        if( block == null ) { return; }

        trackedObjects.Add(block);
    }
    private void OnTriggerExit(Collider other)
    {
        Block block = other.GetComponentInParent<Block>();
        if( block == null ) { return; }
        if( !trackedObjects.Contains(block) ) { return; }

        trackedObjects.Remove(block);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(positionTarget, 1);

        Gizmos.color = Color.red;
        foreach( Block block in trackedObjects ) {
            Vector3 position = block.transform.position;
            position.z = -2;
            Gizmos.DrawCube(position, Vector3.one);
        }
    }
}
