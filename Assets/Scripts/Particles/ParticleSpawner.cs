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
    public static void SpawnWithBounds(ParticleSystem particle, Vector3 pos, Quaternion rot, Bounds bounds)
    {
        ParticleSystem particleSys = Object.Instantiate(particle.gameObject, pos, rot).GetComponent<ParticleSystem>();

        ShapeModule shape = particleSys.shape;
        shape.scale = bounds.size; 
    }
}
