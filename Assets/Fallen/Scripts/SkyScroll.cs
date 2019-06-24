using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyScroll : MonoBehaviour
{
    public float ySpeed = -1f;
    public float height = 80f;
    public int numberOfImages = 3;

    private void Update()
    {
        transform.Translate(0f, ySpeed, 0f);

        if( transform.localPosition.y < -height * (numberOfImages - 1) ) {
            transform.Translate(0f, height * numberOfImages, 0f);
        }
        if( transform.localPosition.y > height * (numberOfImages - 1) ) {
            transform.Translate(0f, -height * numberOfImages, 0f);
        }
    }
}
