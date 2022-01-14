using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBase : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float cameraPanDistance = 10f;

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
        Vector3 mousePosition = Input.mousePosition;
        float screenWidth = GameManager.Instance.activeCamera.camera.scaledPixelWidth / 2;
        float screenHeight = GameManager.Instance.activeCamera.camera.scaledPixelHeight / 2;
        Vector3 centeredMousePosition = new Vector3(mousePosition.x - screenWidth, mousePosition.y - screenHeight, 0);
        Vector3 screenRatio = new Vector3(centeredMousePosition.x / screenWidth, centeredMousePosition.y / screenHeight, 0);
        transform.position = target.position + new Vector3(screenRatio.x * cameraPanDistance, screenRatio.y * cameraPanDistance, 0);
    }
    private void Update()
    {
        if( cameraShakeTimeStamp != -1 ) {
            if( Time.time >= cameraShakeTimeStamp ) {
                camera.transform.localPosition = new Vector3(0, 0, -distance);
                cameraShakeTimeStamp = -1f;
            } else {
                float currentShakeIntensity = shakeIntensity * ( (cameraShakeTimeStamp - Time.time)/cameraShakeDuration );
                camera.transform.localPosition = new Vector3(Random.Range(-currentShakeIntensity, currentShakeIntensity),
                                                        Random.Range(-currentShakeIntensity, currentShakeIntensity), -distance);
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
