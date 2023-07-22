using UnityEngine;

public struct Particle 
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 rot;
    public Vector3 rotVel;
    public Vector3 size;//customData0, customData1
    public Vector3 sizeDest;
    public Color col;
    public Color colDest;
    public Vector4 customData;
    public float lifeTime;
    public float spawnTime;
    public int disable;
}