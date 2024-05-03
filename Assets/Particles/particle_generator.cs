using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class particle_generator : MonoBehaviour
{
    [Header("Particle Attributes:")]
    public float scalingFactor = 0.1f; //scale = diameter of particles
    public Vector3 initialVelocity = new Vector3(0, -9.8f, 0); //Time.fixedDeltaTime = 0.02
    public float dampingFactor = 0.1f;
    public int planeRange = 5;
    public int numParticlesToAdd = 25;

    
    public GameObject prefab;
    public ParticlesList particlesList;
    public List<SandParticle> particles;
    List<GameObject> all_particles_go;
    Vector3[] vertices;
    int gridSize;
    float planeSize;
    int numParticles;
    int fixedUpdateCounter = 0;

    void Start()
    {
        // Initialize particles list from ParticlesList constructor
        particlesList = new ParticlesList(scalingFactor, initialVelocity, planeRange, numParticlesToAdd);
        particles = particlesList.particles;
 
        all_particles_go = new List<GameObject>();

        vertices = plane_generator.vertices;
        // Debug.Log(vertices);
        gridSize = plane_generator.globalGridSize;
        planeSize = plane_generator.globalPlaneSize;

        // Iterate through the list of particle objects
        foreach (SandParticle particle in particles)
        {
            GameObject particleGO = Instantiate(prefab, particle.position, Quaternion.identity);
            particleGO.transform.SetParent(transform); // Parent the particle to this GameObject
            particleGO.SetActive(true);


            // Set the size of the instantiated prefab
            particleGO.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
            all_particles_go.Add(particleGO);
        }
        numParticles = particles.Count;

        // RecalculateBatches()
    }


    void FixedUpdate()
    {
        //Update particle positions
        for (int i = 0; i < particles.Count; i++)
        {
            // Handle collisions
            particlesList.HandleCollisions(particles[i], vertices, gridSize, planeSize, dampingFactor);
            particles[i].position += particles[i].velocity/4 * Time.fixedDeltaTime;

            //Update game object position
            all_particles_go[i].transform.position = particles[i].position;
        }

        fixedUpdateCounter++;
        //Add more particles
        if (fixedUpdateCounter % 100 == 0)
        {
            int prevNumParticles = particles.Count;
            particlesList.AddParticles(5, scalingFactor, initialVelocity, planeRange, numParticlesToAdd);
            // Iterate through new particle objects
            foreach (SandParticle particle in particles.Skip(prevNumParticles))
            {
                GameObject particleGO = Instantiate(prefab, particle.position, Quaternion.identity);
                particleGO.transform.SetParent(transform); // Parent the particle to this GameObject
                
                // Set the size of the instantiated prefab
                particleGO.transform.localScale = new Vector3(scalingFactor, scalingFactor, scalingFactor);
                particleGO.SetActive(true);
                all_particles_go.Add(particleGO);
            }
            numParticles = particles.Count;
        }
    }
}

public class SandParticle
{
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 shape;
    public float radius;
    public float mass;

    //Constructor
    public SandParticle(Vector3 pos, Vector3 vel, Vector3 accel, float ms, float size)
    {
        position = pos;
        radius = size / 2 ;
        velocity = vel;
        acceleration = accel;
        mass = ms;
    }
}


public class ParticlesList
{
    public List<SandParticle> particles;
    public Vector3 center;
    public float spacing;
    // Constructor
    public ParticlesList(float size, Vector3 initial_velocity, int plane_range, int numParticlesToAdd)
    {
        particles = new List<SandParticle>();
        center = new Vector3(0, 20, 0); // Center of the square (vertical)

        for (int i = 0; i < numParticlesToAdd; i++)
        {
            // Calculate random x and z coordinates within specified ranges
            float randomX = UnityEngine.Random.Range(-plane_range, plane_range);
            float randomZ = UnityEngine.Random.Range(-plane_range, plane_range);

            // Calculate the position of the particle
            float x = center.x + randomX;
            float y = UnityEngine.Random.Range(10, 40); // Random y coordinate
            float z = center.z + randomZ;

            Vector3 position = new Vector3(x, y, z);

            // Set direction based on gravity
            float ms = 3;
            Vector3 accel = new Vector3(0, -0.08f, 0);

            // Add particle to the list
            particles.Add(new SandParticle(position, initial_velocity, accel, ms, size));
        }
    }

    public void AddParticles(float num, float size, Vector3 initial_velocity, int plane_range, int numParticlesToAdd)
    {
            for (int i = 0; i < numParticlesToAdd; i++) 
            {
                float randomX = UnityEngine.Random.Range(-plane_range, plane_range);
                float randomZ = UnityEngine.Random.Range(-plane_range, plane_range);

                // Calculate the position of the particle
                float x = center.x + randomX;
                float y = UnityEngine.Random.Range(10, 40); // Random y coordinate
                float z = center.z + randomZ;

                Vector3 position = new Vector3(x, y, z);

                // Set direction based on gravity
                float ms = 3;
                Vector3 accel = new Vector3(0, -0.08f, 0);

                // Add particle to the list
                particles.Add(new SandParticle(position, initial_velocity, accel, ms, size));
            }
    }

    public void HandleCollisions(SandParticle particle, Vector3[] vertices, int gridSize, float planeSize, float damping_factor)
    {
        
        for (int j = 0; j < particles.Count; j++)
            {
                if (particle != particles[j])
                {
                    // If there is a collision along all axes, the cubes are colliding
                    if (CheckCollision(particle, particles[j]))
                    {
                        //Jitter the particle slightly in a random direction to the side (x or z direction)
                        float addOrSubtractX = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1; // 0 means subtract, 1 means add
                        float addOrSubtractY = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1; // 0 means subtract, 1 means add
                        particle.position.x += addOrSubtractX * particle.radius;
                        particle.position.y += addOrSubtractY * particle.radius;

                        particles[j].position.x -= addOrSubtractX * particle.radius;
                        particles[j].position.y -= addOrSubtractY * particle.radius;
 
                        // Apply elastic collision forces
                        // ApplyElasticCollision(particle, particles[j]);
                    }
                }
            }

        //get the coordinates of the 4 surrounding vertices, these are the positions of the vertices
        int x_down = (int) Math.Floor(particle.position.x);
        int z_down = (int) Math.Floor(particle.position.z);
        int x_up = (int) Math.Ceiling(particle.position.x);
        int z_up = (int) Math.Ceiling(particle.position.z);

        float s = particle.position.x - x_down;
        float t = particle.position.z - z_down;

        float h00 = GetVertex(vertices, planeSize, gridSize, x_down, z_down).y;
        float h10 = GetVertex(vertices, planeSize, gridSize, x_up, z_down).y; ;
        float h01 = GetVertex(vertices, planeSize, gridSize, x_down, z_up).y; ;
        float h11 = GetVertex(vertices, planeSize, gridSize, x_up, z_up).y;

        // //do lerp 
        // float h0 = h00 + s * (h10 + (-1 * h00));
        // float h1 = h01 + s * (h11 + (-1 * h01));
        // float terrain_height = h0 + t * (h1 + (-1 * h0));
        float terrain_height = Mathf.Max(h00, h01, h10, h11);

        // Check for collisions with the terrain
        if (particle.position.y <= terrain_height + particle.radius)
        {
            particle.velocity.y = -damping_factor * particle.velocity.y; // Add a force in the opposite direction

            // if (particle.velocity.magnitude < 0.2) {
            //    particle.position.y = terrain_height + particle.radius + 0.001f; // Add a small offset 
            //    particle.velocity = Vector3.zero;
            // }
        
        }
    else
    {
        // Apply gravity only if the particle is in the air
        particle.velocity.y -= 9.8f * Time.fixedDeltaTime;
    }
    }

    Vector3 GetVertex(Vector3[] vertices, float planeSize, int gridSize, float xPos, float zPos)
    {
        int xIndex = Mathf.RoundToInt((xPos + planeSize / 2.0f) / planeSize * gridSize);
        int zIndex = Mathf.RoundToInt((zPos + planeSize / 2.0f) / planeSize * gridSize);
        int index = zIndex * (gridSize + 1) + xIndex;

        if (index >= 0 && index < vertices.Length)
        {
            return vertices[index];
        }
        else
        {
            return Vector3.zero;
        }
    }
        
    private Vector3 CalculateTangentPlaneNormal(Vector3 position, Vector3[] vertices, int gridSize)
    {   
        // Calculate the normal vector of the tangent plane at a given position on the terrain
        int x = Mathf.FloorToInt(position.x);
        int z = Mathf.FloorToInt(position.z);

        // Get the heights of the four surrounding vertices
        float h00 = vertices[z * (gridSize + 1) + x].y;
        float h10 = vertices[z * (gridSize + 1) + x + 1].y;
        float h01 = vertices[(z + 1) * (gridSize + 1) + x].y;
        float h11 = vertices[(z + 1) * (gridSize + 1) + x + 1].y;

        // Calculate the normal vector of the tangent plane using cross product
        Vector3 v1 = new Vector3(1, h10 - h00, 0);
        Vector3 v2 = new Vector3(0, h01 - h00, 1);
        Vector3 normal = Vector3.Cross(v1, v2).normalized;

        return normal;
    }

    public bool CheckCollision(SandParticle particle1, SandParticle particle2)
    {
        return Vector3.Distance(particle1.position, particle2.position) < (particle2.radius + particle1.radius);
    }

    private void ApplyElasticCollision(SandParticle particle1, SandParticle particle2)
    {
        // Calculate relative velocity
        Vector3 relativeVelocity = particle2.velocity - particle1.velocity;

        // Calculate impulse
        float impulse = Vector3.Dot(relativeVelocity, particle2.position - particle1.position) /
                        (particle1.mass + particle2.mass);

        // Calculate new velocities after collision
        Vector3 newVelocity1 = particle1.velocity + impulse * (particle2.position - particle1.position) / particle1.mass;
        Vector3 newVelocity2 = particle2.velocity - impulse * (particle2.position - particle1.position) / particle2.mass;

        // Update particle velocities
        particle1.velocity = newVelocity1;
        particle2.velocity = newVelocity2;
    }
}