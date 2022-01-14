using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class Shot : MonoBehaviour
{
    new public Rigidbody rigidbody;

    public float lifeTime = 10f;
    private float lifeTimestamp = -1f;

    public float explosionMagnitude = 10f;
    public float explosionRange = 5f;

    public float shakeDistanceValue = 50f;
    public GameObject impactParticles;
    public GameObject stoneParticles;
    public GameObject flareParticles;

    AudioSource impactSource;

    bool isDestroyed = false;

    // Start is called before the first frame update
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        impactSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        lifeTimestamp = Time.time + lifeTime;
    }

    private void Update()
    {
        if( isDestroyed && !impactSource.isPlaying ) {
            Destroy(gameObject);
            return;
        } else if( Time.time >= lifeTimestamp ) {
            Destroy(gameObject);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        float distance = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position);
        float intensity = explosionMagnitude * (shakeDistanceValue / distance);
        GameManager.Instance.activeCamera.CameraShake(0.5f, intensity);

        intensity = explosionMagnitude * 2f;
        ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        impactEmitter.Expand(intensity);
        ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        stoneEmitter.Expand(intensity);
        ParticleEmitter flareEmitter = Instantiate<GameObject>(flareParticles, collision.GetContact(0).point + flareParticles.transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        flareEmitter.Expand(intensity);

        if( impactSource != null ) {
            impactSource.pitch = Random.Range(0.2f, 2f);
            impactSource.volume = 1;
            impactSource.Stop();
            impactSource.Play();
        }

        if( Vector3.Distance(GameManager.Instance.player.transform.position, transform.position) < explosionRange ) {
            GameManager.Instance.player.Die();
        }

        SetToDestroy();
    }

    void SetToDestroy()
    {
        rigidbody.isKinematic = true;
        GetComponent<SphereCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<TrailRenderer>().enabled = false;

        isDestroyed = true;
    }
}
