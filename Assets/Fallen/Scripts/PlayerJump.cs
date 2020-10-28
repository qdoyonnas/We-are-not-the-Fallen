using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public bool isAlive = true;

    [Header("Controls")]
    public float airMove = 10f;
    public float maxAirMove = 50f;
    public float airMoveVerticalVelocityThreshold = 60f;

    [Header("Physics")]
    public float jumpFactor = 1f;
    public float dashFactor = 0.5f;
    public float dashMassFactor = 2f;
    public float forceMultiplier = 1000f;

    public float minDashSpeed = 100f;
    public float collisionDuration = 0.1f;
    float collisionTimeStamp = -1f;

    [Header("Friction")]
    public float airFriction = 0f;
    public float groundedFriction = 1f;

    [Header("Timing")]

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
    [HideInInspector] new public Rigidbody rigidbody;
    new CapsuleCollider collider;
    float defaultMass;
    Renderer glowSphere;

    // Grounding
    public bool grounded = false;
    public bool canDash = false;
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
        glowSphere = transform.Find("Glow").GetComponent<Renderer>();
    }

    private void FixedUpdate()
    {
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
                Jump();
            } else {
                ChangeMass(defaultMass * dashMassFactor);
                if( canDash ) {
                    Dash();
                }
            }
        }
        if( Input.GetMouseButtonUp(0) ) {
            ChangeMass();
        }

        if( !grounded ) {
            AirMove();
        }
    }

	#region Movement Methods

    private void Jump()
    {
        Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 jumpVector = (mousePos - transform.position).normalized;
        // Return if something is too close to jump
        Vector3 flattenedJumpVector = new Vector3(jumpVector.x, jumpVector.y, 0);
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.2f, flattenedJumpVector, 2f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
        if( hits.Length > 0 ) {
            return;
        }

        ChangeMass(defaultMass * dashMassFactor);

        rigidbody.useGravity = true;
        rigidbody.drag = airFriction;
        grounded = false;
        Destroy(anchor);

        mousePos.z = 0;

        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(jumpFactor * forceMultiplier * jumpVector);
    }

	private void Dash()
    {
        canDash = false;
                
        Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 jumpVector = -(transform.position - mousePos).normalized;

        Vector3 newVelocity = rigidbody.velocity;
        if (Mathf.Sign(newVelocity.x) != Mathf.Sign(jumpVector.x))
        {
            newVelocity.x = 0;
        }
        if (Mathf.Sign(newVelocity.y) != Mathf.Sign(jumpVector.y))
        {
            newVelocity.y = 0;
        }
        rigidbody.velocity = newVelocity;
        rigidbody.velocity = Vector3.zero;

        rigidbody.AddForce(jumpVector * dashFactor * forceMultiplier);
    }

    private void AirMove()
    {
        float adjustedMaxAirMove = maxAirMove;
        float adjustedAirMove = airMove;

        float absYVel = Mathf.Abs(rigidbody.velocity.y);
        if( absYVel < airMoveVerticalVelocityThreshold ) {
            float ratio = absYVel / airMoveVerticalVelocityThreshold;
            adjustedMaxAirMove = maxAirMove * ratio;
            adjustedAirMove = airMove * ratio;
        }
        if( Input.GetKey(KeyCode.A) ) {
            if( rigidbody.velocity.x > -adjustedMaxAirMove ) {
                rigidbody.AddForce(Vector3.left * adjustedAirMove);
            }
        }
        if( Input.GetKey(KeyCode.D) ) {
            if( rigidbody.velocity.x < adjustedMaxAirMove ) {
                rigidbody.AddForce(Vector3.right * adjustedAirMove);
            }
        }
        if( Input.GetKey(KeyCode.S) ) {
            if( rigidbody.velocity.y > -maxAirMove ) {
                rigidbody.AddForce(Vector3.down * airMove);
            }
        }
    }

	#endregion

	#region Utilities

    public void ChangeMass( float mass = -1f )
    {
        if( mass == -1 ) {
            rigidbody.mass = defaultMass;
            glowSphere.enabled = false;
        } else {
            rigidbody.mass = mass;
            glowSphere.enabled = true;
        }
    }

    public bool IsOnObject(GameObject gameObject)
    {
        if( anchor == null || anchor.transform.parent == null ) { return false; }
        return gameObject.transform == anchor.transform.parent;
    }

	#endregion

	private void OnCollisionEnter( Collision collision )
    {
        if( !isAlive ) { return; }

        Block block = collision.gameObject.GetComponent<Block>();
        if( block != null ) {
            if( block.blockTypes.Contains(GameManager.BlockType.hazard) ) {
                Die();
                return;
            }

            if( block.blockTypes.Contains(GameManager.BlockType.goal) ) {
                block.SetTypes(new List<GameManager.BlockType>());
                GameManager.Instance.ScoreGoal();
            }

            if( !grounded ) {
                grounded = true;
                canDash = true;
                rigidbody.drag = groundedFriction;
                rigidbody.useGravity = false;
            
                ContactPoint contact = collision.GetContact(0);
                transform.rotation = Quaternion.LookRotation(transform.forward, contact.normal);
                transform.position = contact.point + (transform.up * (transform.localScale.y * 0.5f));

                if( anchor == null ) { anchor = new GameObject("AnchorPoint"); }
                anchor.transform.position = contact.point + (contact.normal * (transform.localScale.y * 0.5f));
                anchor.transform.rotation = Quaternion.LookRotation(transform.forward, contact.normal);
                anchor.transform.parent = collision.gameObject.transform;

                float shakeFactor = 0.02f;
                GameManager.Instance.activeCamera.CameraShake(0.2f, storedVelocity * shakeFactor);
                if( effectSource != null ) {
                    effectSource.pitch = Random.Range(0.6f, 2f);
                    effectSource.volume = storedVelocity / impactSpeedValue;
                    effectSource.Stop();
                    effectSource.Play();
                }

                ParticleEmitter impactEmitter = Instantiate<GameObject>(impactParticles, transform.position, transform.rotation, GameManager.Instance.emitterContainer).GetComponent<ParticleEmitter>();
                impactEmitter.Expand( (storedVelocity / impactSpeedValue) * 0.5f );
                if( rigidbody.mass != defaultMass ) {
                    ParticleEmitter stoneEmitter = Instantiate<GameObject>(stoneParticles, transform.position, transform.rotation).GetComponent<ParticleEmitter>();
                    stoneEmitter.Expand( (storedVelocity / impactSpeedValue) * 0.5f );
                }
                
                ChangeMass();
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
        glowSphere.enabled = false;

        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = false;

        if( anchor == null ) { anchor = new GameObject("AnchorPoint"); }
        anchor.transform.position = transform.position;
        anchor.transform.parent = null;

        GameManager.Instance.savedScore = GameManager.Instance.GetTotalScore();

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
        Destroy(anchor);
        rigidbody.drag = airFriction;
        rigidbody.useGravity = true;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rigidbody.velocity = Vector3.zero;

        collisionTimeStamp = -1f;

        effectSource.clip = impactSound;
    }
}
