using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleSpawner
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="particle">Particle to spawn</param>
    /// <param name="pos">Particle Spawn Position</param>
    /// <param name="rot">Particle Spawn Rotation</param>
    /// <param name="bounds">Particle Bounds</param>
    /// <param name="scaleEmission">Scale emission rate based on bounds volume</param>
    /// <param name="emissionScale">Multiplier on top of volume-driven emission</param>
    public static ParticleSystem SpawnWithBounds(ParticleSystem particle, Vector3 pos, Quaternion rot, Bounds bounds, Transform parent = null,bool scaleEmission = false, float emissionScale = 1f)
    {
        ParticleSystem particleSys;

        if(parent == null)
        particleSys = Object.Instantiate(particle.gameObject, pos, rot).GetComponent<ParticleSystem>();
        else
        {
            particleSys = Object.Instantiate(particle.gameObject, parent).GetComponent<ParticleSystem>();
            particleSys.transform.localPosition = Vector3.zero;
            particleSys.transform.localRotation = Quaternion.identity;
        }

        ShapeModule shape = particleSys.shape;
        shape.scale = bounds.size;

        if (scaleEmission)
        {
            float volume = bounds.size.x * bounds.size.y * bounds.size.z;
            float sizeScale = Mathf.Pow(volume, 1f / 3f);

            EmissionModule emission = particleSys.emission;
            emission.rateOverTime = emission.rateOverTime.constant * sizeScale * emissionScale;
        }

        return particleSys;
    }
}