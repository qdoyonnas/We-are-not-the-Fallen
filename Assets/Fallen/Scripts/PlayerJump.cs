using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public bool isAlive = true;

    [Header("Physics")]
    public float jumpFactor = 1f;
    public float dashFactor = 0.5f;
    public float dashMassFactor = 2f;
    public float collisionDuration = 0.1f;
    float collisionTimeStamp = -1f;

    [Header("Friction")]
    public float airFriction = 0f;
    public float groundedFriction = 1f;

    [Header("Timing")]
    public float dashDuration = 1f;
    float dashTimeStamp = -1f;

    [Header("Sounds")]
    public AudioSource effectSource;
    public float impactSpeedValue = 10f;
    public AudioClip impactSound;
    public AudioClip squishSound;
    public AudioSource windSource;
    public float windSpeedValue = 10f;

    float storedVelocity = 0f;

    GameObject impactParticles;
    GameObject stoneParticles;

    // Components
    new public Rigidbody rigidbody;
    new CapsuleCollider collider;
    float defaultMass;

    // Grounding
    bool grounded = false;
    bool canDash = false;
    GameObject anchor;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if( rigidbody == null ) {
            Debug.LogError(string.Format("PlayerJump {0} could not find attached rigidbody", name));
            return;
        }

        defaultMass = rigidbody.mass;

        collider = GetComponent<CapsuleCollider>();
        if( rigidbody == null ) {
            Debug.LogError(string.Format("PlayerJump {0} could not find attached CapsuleCollider", name));
            return;
        }

        impactParticles = Resources.Load<GameObject>("Particles/Impact");
        stoneParticles = Resources.Load<GameObject>("Particles/StoneImpact");
    }

    private void FixedUpdate()
    {
        if( dashTimeStamp != -1 ) {
            if( Time.time >= dashTimeStamp ) {
                rigidbody.mass = defaultMass;
                dashTimeStamp = -1f;
            }
        }
        if( collisionTimeStamp != -1 ) {
            if( Time.time >= collisionTimeStamp ) {
                collider.enabled = true;
                collisionTimeStamp = -1f;
            }
        }

        if( grounded ) {
            if( anchor == null ) {
                rigidbody.useGravity = true;
                rigidbody.drag = airFriction;
                grounded = false;
            }  else {
                transform.position = anchor.transform.position;
                transform.rotation = anchor.transform.rotation;
            }
        }

        storedVelocity = rigidbody.velocity.magnitude;
        if( windSource != null ) {
            windSource.volume = (storedVelocity + 30f ) / windSpeedValue;
        }
    }
    private void Update()
    {
        if( !isAlive ) { return; }

        if( Input.GetMouseButtonDown(0) ) {

            if( grounded ) {
                rigidbody.useGravity = true;
                rigidbody.drag = airFriction;
                grounded = false;
                Destroy(anchor);

                Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                Vector3 jumpVector = -(transform.position - mousePos).normalized;

                rigidbody.velocity = Vector3.zero;
                rigidbody.AddForce(jumpFactor * jumpVector);

            } else if( canDash ) {
                canDash = false;

                Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                Vector3 jumpVector = -(transform.position - mousePos).normalized;
                rigidbody.AddForce(jumpVector * dashFactor);
                
                rigidbody.mass *= dashMassFactor;

                dashTimeStamp = Time.time + dashDuration;
            }
        }
    }

    private void OnCollisionEnter( Collision collision )
    {
        if( !isAlive ) { return; }

        Block block = collision.gameObject.GetComponent<Block>();
        if( block != null ) {
            if( block.blockTypes.Contains(GameManager.BlockType.hazard) ) {
                Die();
                return;
            }

            if( !grounded ) {
                grounded = true;
                canDash = true;
                rigidbody.drag = groundedFriction;
                rigidbody.useGravity = false;
            
                ContactPoint contact = collision.GetContact(0);
                transform.rotation = Quaternion.LookRotation(transform.forward, contact.normal);
                transform.position = contact.point + (transform.up * (transform.localScale.y * 0.5f));

                anchor = new GameObject("AnchorPoint");
                anchor.transform.position = contact.point + (contact.normal * (transform.localScale.y * 0.5f));
                anchor.transform.rotation = Quaternion.LookRotation(transform.forward, contact.normal);
                anchor.transform.parent = collision.gameObject.transform;

                rigidbody.mass = defaultMass;

                float shakeFactor = dashTimeStamp != -1f ? 0.03f : 0.01f;
                GameManager.Instance.activeCamera.CameraShake(0.2f, storedVelocity * shakeFactor);
                if( effectSource != null ) {
                    effectSource.pitch = Random.Range(0.6f, 2f);
                    effectSource.volume = storedVelocity / impactSpeedValue;
                    effectSource.Stop();
                    effectSource.Play();
                }

                ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, transform.position, transform.rotation, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
                impactEmitter.Expand( (storedVelocity / impactSpeedValue) * 0.5f );
                if( dashTimeStamp != -1f ) {
                    ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, transform.rotation).GetComponent<ParticleEmitter>();
                    stoneEmitter.Expand( (storedVelocity / impactSpeedValue) * 0.5f );
                }

                dashTimeStamp = -1f;
                storedVelocity = 0f;

            } else if( collision.transform != anchor.transform.parent ) {
                Die();
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        GetComponent<MeshRenderer>().enabled = false;

        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = false;

        effectSource.clip = squishSound;
        effectSource.volume = 1f;
        effectSource.pitch = 1f;
        effectSource.Stop();
        effectSource.Play();
    }
    public void ResetPlayer()
    {
        isAlive = true;
        GetComponent<MeshRenderer>().enabled = true;

        grounded = false;
        canDash = false;
        anchor = null;
        rigidbody.drag = airFriction;
        rigidbody.useGravity = true;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rigidbody.velocity = Vector3.zero;

        collisionTimeStamp = -1f;

        effectSource.clip = impactSound;
    }
}
