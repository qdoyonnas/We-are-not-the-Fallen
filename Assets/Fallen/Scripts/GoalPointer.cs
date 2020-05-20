using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPointer : MonoBehaviour
{
    PlayerJump player;

    new MeshRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.Instance.player;
        renderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        renderer.enabled = false;

        if( GameManager.Instance.goal != null ) {
            MeshRenderer goalRenderer = GameManager.Instance.goal.GetComponentInChildren<MeshRenderer>();
            if( !goalRenderer.isVisible ) {
                renderer.enabled = true;

                Vector3 goalDirection = (GameManager.Instance.goal.transform.position - player.transform.position).normalized;

                float distance = GameManager.Instance.activeCamera.camera.orthographicSize * 0.8f;
                transform.position = player.transform.position + (goalDirection * distance);
            }
        }
    }
}
