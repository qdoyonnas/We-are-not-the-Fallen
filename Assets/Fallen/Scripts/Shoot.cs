using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject shotPrefab;
    public float shotSpeed = 60f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 target = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0;
            Vector3 vector = (target - transform.position).normalized * shotSpeed;
            SpawnShot(transform.position, vector);
        }
    }

    private GameObject SpawnShot(Vector3 spawnPosition, Vector3 velocity)
    {
        Shot shot = Instantiate<GameObject>( shotPrefab, spawnPosition, Quaternion.identity ).GetComponent<Shot>();
        shot.rigidbody.velocity = velocity;

        return shot.gameObject;
    }
}
