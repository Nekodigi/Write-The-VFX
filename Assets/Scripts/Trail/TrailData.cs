//based on fuqunaga/GpuTrail
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


public class TrailData
{
    public float life = 1f;
    public float inputPerSec = 60f;

    public TrailData(int trailNum, float life, float inputPerSec)
    {
        this.life = life;
        this.inputPerSec = inputPerSec;
        this.TrailNum = trailNum;
        Init();
    }

    public int TrailNum { get; protected set; }
    public int NodeNumPerTrail { get; protected set; }
    public int NodeNumTotal => TrailNum * NodeNumPerTrail;

    public GraphicsBuffer TrailBuffer { get; protected set; }
    public GraphicsBuffer NodeBuffer { get; protected set; }

    public bool IsInitialized => TrailBuffer != null;



    public struct Trail
    {
        public float spawnTime;
        public int totalInputNum;
        public float life;
    }

    public void Init()
    {
        NodeNumPerTrail = Mathf.CeilToInt(life * inputPerSec);
        if (inputPerSec < Application.targetFrameRate)
        {
            Debug.LogWarning($"inputPerSec({inputPerSec}) < targetFps({Application.targetFrameRate}): Trai adds a node every frame, so running at TargetFrameRate will overflow the buffer.");
        }

        InitBuffer();
    }
    public void Dispose()
    {
        ReleaseBuffer();
    }

    protected virtual void InitBuffer()
    {
        TrailBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, TrailNum, Marshal.SizeOf<Trail>());
        TrailBuffer.Fill(default(Trail));

        NodeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, NodeNumTotal, Marshal.SizeOf<Particle>());
        NodeBuffer.Fill(default(Particle));
    }

    protected virtual void ReleaseBuffer()
    {
        TrailBuffer?.Release();
        NodeBuffer?.Release();
    }
}

public struct Vertex
{
    public Vector3 pos;
    public Vector4 customData;
    public Vector2 uv;
    public Color col;
}