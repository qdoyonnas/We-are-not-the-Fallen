using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public float massFactor = 1f;
    public List<GameManager.BlockType> blockTypes;
    
    public float checkDestroyTime = 1f;
    public float destroyDistance = 240;

    public float shakeDistanceValue = 50f;
    public GameObject impactParticles;
    public GameObject stoneParticles;

    new public Rigidbody rigidbody;
    float checkDestroyTimeStamp = -1f;

    public float collisionKillRangeModifier = 0.005f;

    public float baseDestructionThreshold = 5;
    private float destructionThreshold = 5;

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
        rigidbody.mass = massFactor * GetSize();
        destructionThreshold = baseDestructionThreshold * GetSize();
    }
    public void SetTypes( List<GameManager.BlockType> types )
    {
        blockTypes = types;

        Destroy(transform.GetChild(0).gameObject);

        string prefabName;
        if( blockTypes.Contains(GameManager.BlockType.goal) ) {
            prefabName = "Goal";
        } else {
            prefabName = "Standard";
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if( prefab != null ) {
            Instantiate<GameObject>(prefab, transform);
        }
    }

    private void Update()
    {
        if( !blockTypes.Contains(GameManager.BlockType.goal) && Time.time >= checkDestroyTimeStamp ) {
            if( Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) > destroyDistance ) {
                Destroy(gameObject);
                return;
            }

            checkDestroyTimeStamp = Time.time + checkDestroyTime;
        }
    }

    public float GetSize()
    {
        return transform.localScale.x * transform.localScale.y;
    }

    private void OnCollisionEnter( Collision collision )
    {
        Block block = collision.gameObject.GetComponent<Block>();
        if( block != null ) {
            float distance = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position);
            float intensity = collision.impulse.magnitude * (shakeDistanceValue / distance) * 0.002f;
            GameManager.Instance.activeCamera.CameraShake(0.5f, intensity);

            intensity = collision.impulse.magnitude * 0.004f;
            ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, collision.GetContact(0).point, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
            impactEmitter.Expand(intensity);
            ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, collision.GetContact(0).point, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
            stoneEmitter.Expand(intensity);

            if( impactSource != null ) {
                impactSource.pitch = Random.Range(0.6f, 2f);
                impactSource.volume = intensity * 0.3f;
                impactSource.Stop();
                impactSource.Play();
            }

            float killDistance = (collision.impulse.magnitude * collisionKillRangeModifier);

            

            if( !blockTypes.Contains(GameManager.BlockType.goal) && collision.impulse.magnitude > destructionThreshold && GetSize() <= block.GetSize() ) {
                Destroy(gameObject);

                stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
                stoneEmitter.Expand(transform.localScale.magnitude * 0.2f);

                killDistance += (transform.localScale.x * transform.localScale.y) * 0.03f;
            }

            if( Vector3.Distance(GameManager.Instance.player.transform.position, collision.GetContact(0).point) < killDistance ) {
                GameManager.Instance.player.Die();
            }
        }
    }
}
