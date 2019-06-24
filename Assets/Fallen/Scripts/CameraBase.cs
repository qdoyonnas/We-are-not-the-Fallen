using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBase : MonoBehaviour
{
    public Transform target;
    public float distance;

    private float cameraShakeDuration = -1f;
    private float cameraShakeTimeStamp = -1f;
    private float shakeIntensity = 0;

    private Camera _camera;
    new public Camera camera {
        get {
            return _camera;
        }
    }

    private void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        if( _camera == null ) {
            Debug.LogError( string.Format("CameraBase {0} could not find Camera in children", name) );
            return;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = target.position + (-transform.forward * distance);
    }
    private void Update()
    {
        if( cameraShakeTimeStamp != -1 ) {
            if( Time.time >= cameraShakeTimeStamp ) {
                camera.transform.localPosition = Vector3.zero;
                cameraShakeTimeStamp = -1f;
            } else {
                float currentShakeIntensity = shakeIntensity * ( (cameraShakeTimeStamp - Time.time)/cameraShakeDuration );
                camera.transform.localPosition = new Vector3(Random.Range(-currentShakeIntensity, currentShakeIntensity),
                                                        Random.Range(-currentShakeIntensity, currentShakeIntensity), 0);
            }
        }
    }

    public void CameraShake( float duration, float intensity )
    {
        cameraShakeDuration = duration;
        cameraShakeTimeStamp = Time.time + duration;
        shakeIntensity = intensity;
    }
}
