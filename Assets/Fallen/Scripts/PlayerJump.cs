using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public bool isAlive = true;

    [Header("Controls")]
    public bool canDash = true;
    public bool canSuperJumpDash = true;

    [Header("Physics")]
    private bool _superJump = false;
    public bool superJump {
        get {
            return _superJump;
        }
        set {
            if( value ) {
                superJumpLevel = GameManager.Instance.level;
            }
            if( superJump && !value ) {
                trail.material = trailColor;
                if( GameManager.Instance.goal != null ) {
                    Destroy(GameManager.Instance.goal.gameObject);
                    GameManager.Instance.goal = null;
                }
            }
            _superJump = value;
        }
    }
    public float jumpFactor = 1f;
    public float dashFactor = 0.5f;
    public float dashMassFactor = 2f;
    public float forceMultiplier = 1000f;

    public float collisionDuration = 0.1f;
    float collisionTimeStamp = -1f;

    [Header("Friction")]
    public float airFriction = 0f;
    public float groundedFriction = 1f;

    [Header("Timing")]
    public float dashCooldown = 0.5f;
    public float dashTime = 0f;
    public float superJumpDuration = 15f;
    public float superJumpTime = 0f;

    [Header("Sounds")]
    public AudioSource effectSource;
    public float impactSpeedValue = 10f;
    public AudioClip impactSound;
    public AudioClip squishSound;
    public AudioSource windSource;
    public float windSpeedValue = 10f;
    public AudioClip flapSound;

    [Header("Flight")]
    public float minGlideSpeed = 5f;
    public float glideDelta = 10f;
    public Material trailColor;
    public Material superTrailColor;

    float storedVelocity = 0f;

    GameObject impactParticles;
    GameObject stoneParticles;

    // Components
    [HideInInspector] new public Rigidbody rigidbody;
    new CapsuleCollider collider;
    float defaultMass;
    Renderer glowSphere;
    GameObject body;
    TrailRenderer trail;
    new Light light;
    int superJumpLevel = 0;

    // Grounding
    private bool _grounded = false;
    public bool grounded {
        get {
            return _grounded;
        }
        set {
            _grounded = value;
            if( value ) {
                rigidbody.drag = groundedFriction;
                rigidbody.useGravity = false;
                trail.emitting = false;
            } else {
                rigidbody.useGravity = true;
                rigidbody.drag = airFriction;
                trail.emitting = true;
            }
        }
    }
    [HideInInspector] public bool dashActive = false;
    public GameObject anchor;

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
        body = transform.Find("Body").gameObject;
        trail = body.GetComponent<TrailRenderer>();
        light = transform.Find("Point Light").GetComponent<Light>();
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
                grounded = false;
            }  else {
                transform.position = anchor.transform.position;
                transform.rotation = anchor.transform.rotation;
            }
        } else {
            Glide();
        }

        storedVelocity = rigidbody.velocity.magnitude;
        if( windSource != null ) {
            float windForce = (storedVelocity + 10f ) / windSpeedValue;
            windSource.volume = windForce;
            GameManager.Instance.activeCamera.CameraShake(0.01f, windForce);
        }
    }
    private void Update()
    {
        if( !isAlive ) { return; }

        if( !dashActive ) {
            if( Time.time >= dashTime ) {
                dashActive = true;
            }
        }

        if( superJump && GameManager.Instance.level > superJumpLevel ) {
            if( Time.time >= superJumpTime ) {
                superJump = false;
                grounded = grounded;
            }
        } else {
            superJumpTime = Time.time + superJumpDuration;
        }

        if( Input.GetMouseButtonDown(0) ) {
            if( grounded ) {
                Jump();
            } else {
                ChangeMass(defaultMass * dashMassFactor);
                if( dashActive && (canDash || (canSuperJumpDash && superJump)) ) {
                    Dash();
                }
            }
        }
        if( Input.GetMouseButtonUp(0) ) {
            ChangeMass();
        }

        if( !grounded ) {
            SetFacing();
        }
    }

	#region Movement Methods

    private void Jump()
    {
        Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 jumpVector = (mousePos - transform.position).normalized;
        // Return if angle is too wide (jumping into block)
        float angle = Vector3.Angle(transform.up, jumpVector);
        if( angle >= 90 ) { return; }

        ChangeMass(defaultMass * dashMassFactor);

        grounded = false;
        Destroy(anchor);

        if( superJump ) {
            rigidbody.useGravity = false;
            rigidbody.drag = 0;
            trail.material = superTrailColor;
        }

        mousePos.z = 0;

        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce((jumpFactor) * forceMultiplier * jumpVector);
    }

	private void Dash()
    {
        dashActive = false;
        dashTime = Time.time + dashCooldown;
                
        Vector3 mousePos = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 jumpVector = -(transform.position - mousePos).normalized;

        rigidbody.AddForce(jumpVector * dashFactor * forceMultiplier);

        if( effectSource != null ) {
            effectSource.clip = flapSound;
            effectSource.pitch = Random.Range(0.6f, 2f);
            effectSource.volume = 1f;
            effectSource.Stop();
            effectSource.Play();
        }
    }

    private void Glide()
    {
        Vector3 mousePosition = GameManager.Instance.activeCamera.camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 delta = (mousePosition - transform.position).normalized;
        delta.z = 0;

        float speedRatio = rigidbody.velocity.magnitude / minGlideSpeed;

        float angle = Vector3.SignedAngle(Vector3.up, rigidbody.velocity, Vector3.forward);
        float desiredAngle = Vector3.SignedAngle(Vector3.up, delta, Vector3.forward);
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angle, desiredAngle));

        float angleDeltaRatio = (glideDelta) / angleDelta;
        //angleDeltaRatio = angleDeltaRatio > 1 ? 1 : angleDeltaRatio;

        float newAngle = Mathf.LerpAngle(angle, desiredAngle, angleDeltaRatio) + 90f;
        Vector3 newVector = new Vector3(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad), 0);

        rigidbody.velocity = newVector * rigidbody.velocity.magnitude;
    }
    private void SetFacing()
    {
        float angle = Vector3.SignedAngle(Vector3.up, rigidbody.velocity, Vector3.forward);
        transform.localEulerAngles = new Vector3(0, 0, angle);
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
            if( !grounded ) {
                if( superJump && glowSphere.enabled ) {
                    block.DestroyBlock();
                } else {
                    LandOnBlock(collision);
                }

            } else if( collision.transform != anchor.transform.parent ) {
                if( superJump ) {
                    block.DestroyBlock();
                } else {
                    Die();
                }
            }

            if( block.blockTypes.Contains(GameManager.BlockType.goal) ) {
                block.SetTypes(new List<GameManager.BlockType>());
                superJump = true;
                GameManager.Instance.goal = null;
            }
        }
    }
    private void LandOnBlock( Collision collision )
    {
        grounded = true;
        dashActive = true;
            
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
            effectSource.clip = impactSound;
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
    }


    public void Die()
    {
        if( superJump ) { return; }

        isAlive = false;
        body.SetActive(false);
        glowSphere.enabled = false;
        light.color = Color.red;
        trail.material = trailColor;

        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = false;

        if( anchor == null ) { anchor = new GameObject("AnchorPoint"); }
        anchor.transform.position = transform.position;
        anchor.transform.parent = null;

        effectSource.clip = squishSound;
        effectSource.volume = 1f;
        effectSource.pitch = 1f;
        effectSource.Stop();
        effectSource.Play();
    }
    public void ResetPlayer()
    {
        isAlive = true;
        light.color = Color.white;

        grounded = false;
        dashActive = false;
        Destroy(anchor);

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        rigidbody.velocity = Vector3.zero;

        trail.Clear();
        body.SetActive(true);

        collisionTimeStamp = -1f;

        effectSource.clip = impactSound;
    }
}
