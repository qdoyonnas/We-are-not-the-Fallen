using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public float massFactor = 1f;
    public bool hazard = false;
    
    public float checkDestroyTime = 1f;
    public float destroyDistance = 30f;

    public float shakeDistanceValue = 50f;
    public GameObject impactParticles;
    public GameObject stoneParticles;

    new public Rigidbody rigidbody;
    float checkDestroyTimeStamp = -1f;

    AudioSource impactSource;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if( rigidbody == null ) {
            Debug.LogError(string.Format("Block {0} could not find attached rigidbody", name));
            return;
        }

        impactSource = GetComponent<AudioSource>();
    }

    public void SetScale( Vector3 scale )
    {
        transform.localScale = scale;
        rigidbody.mass = massFactor * (transform.localScale.x * transform.localScale.y);
    }

    private void Update()
    {
        if( Time.time >= checkDestroyTimeStamp ) {
            if( Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) > destroyDistance ) {
                Destroy(gameObject);
                return;
            }

            checkDestroyTimeStamp = Time.time + checkDestroyTime;
        }
    }

    private void OnCollisionEnter( Collision collision )
    {
        Block block = collision.gameObject.GetComponent<Block>();
        if( block != null ) {
            float distance = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position);
            float intensity = collision.impulse.magnitude * (shakeDistanceValue / distance) * 0.004f;
            GameManager.Instance.activeCamera.CameraShake(0.5f, intensity);

            intensity = collision.impulse.magnitude * 0.004f;
            ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, collision.GetContact(0).point, Quaternion.identity).GetComponent<ParticleEmitter>();
            impactEmitter.Expand(intensity);
            ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, collision.GetContact(0).point, Quaternion.identity).GetComponent<ParticleEmitter>();
            stoneEmitter.Expand(intensity);

            if( impactSource != null ) {
                impactSource.pitch = Random.Range(0.6f, 2f);
                impactSource.volume = intensity * 0.3f;
                impactSource.Stop();
                impactSource.Play();
            }
        }
    }
}
