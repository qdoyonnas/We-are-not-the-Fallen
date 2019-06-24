using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    ParticleSystem _particleSystem;
    new public ParticleSystem particleSystem {
         get {
            return _particleSystem;
        }
    }

    #region Modules

    public ParticleSystemRenderer particleRenderer;
    public ParticleSystem.MainModule mainModule;
    public ParticleSystem.CollisionModule collisionModule;
    public ParticleSystem.ColorBySpeedModule colorBySpeedModule;
    public ParticleSystem.ColorOverLifetimeModule colorOverTimeModule;
    public ParticleSystem.CustomDataModule customDataModule;
    public ParticleSystem.EmissionModule emissionModule;
    public ParticleSystem.ExternalForcesModule externalForcesModule;
    public ParticleSystem.ForceOverLifetimeModule forceOverTimeModule;
    public ParticleSystem.InheritVelocityModule inheritVelocityModule;
    public ParticleSystem.LightsModule lightsModule;
    public ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverTimeModule;
    public ParticleSystem.NoiseModule noiseModule;
    public ParticleSystem.RotationBySpeedModule rotationBySpeedModule;
    public ParticleSystem.SizeBySpeedModule sizeBySpeedModule;
    public ParticleSystem.SizeOverLifetimeModule sizeOverTimeModule;
    public ParticleSystem.TrailModule trailModule;
    public ParticleSystem.TriggerModule triggerModule;
    public ParticleSystem.ShapeModule shapeModule;
    public ParticleSystem.VelocityOverLifetimeModule velOverTimeModule;

    #endregion

    float lifeStamp;

    private void Awake()
    {
        GetParticleSystem();
    }
    protected virtual void GetParticleSystem()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        particleRenderer = GetComponentInChildren<ParticleSystemRenderer>();

        // Modules - for ease of access
        mainModule = _particleSystem.main;
        collisionModule = _particleSystem.collision; 
        colorBySpeedModule = _particleSystem.colorBySpeed;
        colorOverTimeModule = _particleSystem.colorOverLifetime;
        customDataModule = _particleSystem.customData;
        emissionModule = _particleSystem.emission;
        externalForcesModule = _particleSystem.externalForces;
        forceOverTimeModule = _particleSystem.forceOverLifetime;
        inheritVelocityModule = _particleSystem.inheritVelocity;
        lightsModule = _particleSystem.lights;
        limitVelocityOverTimeModule = _particleSystem.limitVelocityOverLifetime;
        noiseModule = _particleSystem.noise;
        rotationBySpeedModule = _particleSystem.rotationBySpeed;
        sizeBySpeedModule = _particleSystem.sizeBySpeed;
        sizeOverTimeModule = _particleSystem.sizeOverLifetime;
        trailModule = _particleSystem.trails;
        triggerModule = _particleSystem.trigger;
        shapeModule = _particleSystem.shape;
        velOverTimeModule = _particleSystem.velocityOverLifetime;
    }

    private void Update()
    {
        if( !_particleSystem.IsAlive() ) {
            Destroy(gameObject);
            return;
        }
    }

    #region Modication Methods

    public ParticleEmitter Expand( float multiplier )
    {
        shapeModule.radius *= multiplier;
        shapeModule.boxThickness *= multiplier;

        emissionModule.rateOverTime = MultiplyCurve(emissionModule.rateOverTime, multiplier);
        emissionModule.rateOverDistance = MultiplyCurve(emissionModule.rateOverTime, multiplier);

        for( int i = 0; i < emissionModule.burstCount; i++ ) {
            ParticleSystem.Burst burst = emissionModule.GetBurst(i);
            burst.count = MultiplyCurve(burst.count, multiplier);
            emissionModule.SetBurst(i, burst);
        }

        mainModule.startSize = MultiplyCurve( mainModule.startSize, Mathf.Pow(multiplier, 0.6f) );
        mainModule.startLifetime = MultiplyCurve( mainModule.startLifetime, Mathf.Pow(multiplier, 0.3f) );

        return this;
    }

    public ParticleEmitter Accelerate( float multiplier )
    {
        velOverTimeModule.x = MultiplyCurve( velOverTimeModule.x, multiplier );
        velOverTimeModule.y = MultiplyCurve( velOverTimeModule.y, multiplier );
        velOverTimeModule.z = MultiplyCurve( velOverTimeModule.z, multiplier );

        return this;
    }

    protected virtual ParticleSystem.MinMaxCurve MultiplyCurve( ParticleSystem.MinMaxCurve curve, float multiplier )
    {
        curve.constantMin *= multiplier;
        curve.constantMax *= multiplier;

        curve.curveMultiplier *= multiplier;

        return curve;
    }

    #endregion
}