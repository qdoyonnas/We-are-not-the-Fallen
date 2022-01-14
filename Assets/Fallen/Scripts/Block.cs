using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class Block : MonoBehaviour
{
    public float massFactor = 1f;
    public List<GameManager.BlockType> blockTypes;
    
    public float checkDestroyTime = 1f;
    public float destroyDistance = 240;

    public float killVelocity = 10f;

    public float shakeDistanceValue = 50f;
    public GameObject impactParticles;
    public GameObject stoneParticles;
    public GameObject flareParticles;

    new public Rigidbody rigidbody;
    float checkDestroyTimeStamp = -1f;

    public float collisionKillRangeModifier = 0.005f;

    public float baseDestructionThreshold = 5;
    private float destructionThreshold = 5;

    public float shakeIntensityMultiplier =  0.002f;
    public float impactIntensityMultiplier = 0.004f;

    public float blockGravity = 1f;

    AudioSource impactSource;

    public bool doDestroy = false;

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
        } else if( blockTypes.Contains(GameManager.BlockType.hazard) ) {
            prefabName = "Hazard";
        } else {
            prefabName = "Standard";
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if( prefab != null ) {
            Instantiate<GameObject>(prefab, transform);
        }
    }

	private void FixedUpdate()
	{
		rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y - blockGravity, 0);
	}
	private void Update()
    {
        if( !blockTypes.Contains(GameManager.BlockType.goal) && Time.time >= checkDestroyTimeStamp ) {
            if( doDestroy ) {
                Destroy(gameObject);
                return;
            }
            if( Vector3.Distance(transform.position, GameManager.Instance.player.transform.position) > destroyDistance ) {
                Destroy(gameObject);
                return;
            }

            checkDestroyTimeStamp = Time.time + checkDestroyTime;
        }

        if( blockTypes.Contains(GameManager.BlockType.rogue) && !blockTypes.Contains(GameManager.BlockType.hazard) ) {
            if( rigidbody.velocity.magnitude < killVelocity ) {
                SetTypes(new List<GameManager.BlockType>());
            }
        }
    }

    public float GetSize()
    {
        return transform.localScale.x * transform.localScale.y;
    }

    private void OnCollisionEnter( Collision collision )
    {
        PlayerJump player = collision.gameObject.GetComponent<PlayerJump>();
        if( player != null ) {
            if( blockTypes.Contains(GameManager.BlockType.hazard) || blockTypes.Contains(GameManager.BlockType.rogue) ) {
                if( blockTypes.Contains(GameManager.BlockType.rogue) ) { Debug.Log("Killed by rogue"); }
                player.Die();
            }
            return;
        }

        Block block = collision.gameObject.GetComponent<Block>();
        if( block != null ) {
            float distance = Vector3.Distance(GameManager.Instance.player.transform.position, transform.position);
            float intensity = collision.impulse.magnitude * (shakeDistanceValue / distance) * shakeIntensityMultiplier;
            GameManager.Instance.activeCamera.CameraShake(0.5f, intensity);

            float killDistance = SpawnExplosion(collision, intensity, block);

            if( Vector3.Distance(GameManager.Instance.player.transform.position, collision.GetContact(0).point) < killDistance ) {
                GameManager.Instance.player.Die();
            }
        }
    }
    private float SpawnExplosion(Collision collision, float intensity, Block block)
    {
        intensity = collision.impulse.magnitude * impactIntensityMultiplier;
        ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, collision.GetContact(0).point, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        impactEmitter.Expand(intensity);
        ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, collision.GetContact(0).point, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        stoneEmitter.Expand(intensity);
        ParticleEmitter flareEmitter = Instantiate<GameObject>(flareParticles, collision.GetContact(0).point + flareParticles.transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        flareEmitter.Expand(intensity);

        if( impactSource != null ) {
            impactSource.pitch = Random.Range(0.6f, 2f);
            impactSource.volume = intensity * 0.3f;
            impactSource.Stop();
            impactSource.Play();
        }

        float killDistance = (collision.impulse.magnitude * collisionKillRangeModifier);

        if( !blockTypes.Contains(GameManager.BlockType.goal) && collision.impulse.magnitude > destructionThreshold && GetSize() <= block.GetSize() ) {
            DestroyBlock();
            killDistance += (transform.localScale.x * transform.localScale.y) * 0.03f;
        }

        return killDistance;
    }
    public void DestroyBlock()
    {
        doDestroy = true;
        for( int i = 0; i < transform.childCount; i++ ) {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, Quaternion.identity, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
        stoneEmitter.Expand(transform.localScale.magnitude * 0.2f);

        if( impactSource != null ) {
            impactSource.pitch = Random.Range(0.6f, 2f);
            impactSource.volume = 0.8f;
            impactSource.Stop();
            impactSource.Play();
        }

        checkDestroyTimeStamp = Time.time + 2f;
    }
}
