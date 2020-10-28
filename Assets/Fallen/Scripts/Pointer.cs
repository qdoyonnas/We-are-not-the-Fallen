using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    PlayerJump player;
    Renderer indicator;

    private void Start()
    {
        player = GameManager.Instance.player;
        indicator = transform.Find("Indicator").GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        indicator.enabled = player.canDash;

        Vector3 point = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        point.z = -1;
        transform.position = point;
    }
}
