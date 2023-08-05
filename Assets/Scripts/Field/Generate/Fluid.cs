using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluidSim2DProject;

[ExecuteInEditMode]
public class Fluid : IFieldController
{
    FluidSimCore fluid;

    //impluse
    Vector2 implusePos = new Vector2(0.5f, 0.0f);
    public float impulseTemperature = 10.0f;
    public float impulseDensity = 1.0f;
    public float impluseRadius = 0.1f;
    public float mouseImpluseRadius = 0.05f;
    //obstacle
    Vector2 obstaclePos = new Vector2(0.5f, 0.5f);
    public float obstacleRadius = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        fluid = GetComponent<FluidSimCore>();
        fluid.Init(resolution.x, resolution.y);
    }

    /*[ImageEffectOpaque]
    //based on this site https://github.com/SebLague/Ray-Marching/blob/master/Assets/Scripts/SDF/Master.cs
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(fluid.m_result, destination);

    }*/

    // Update is called once per frame
    protected override void Update()
    {
        //obstacle
        fluid.AddObstacles(obstaclePos, obstacleRadius);//Obstacles only need to be added once unless changed.

        Color c = Color.HSVToRGB((Time.realtimeSinceStartup / 10f) % 1, 1, 1) * impulseDensity;//color
        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition / new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
            //Vector2 pos = Input.mousePosition / new Vector2(Camera.main.pixelHeight, Camera.main.pixelHeight);

            //pos.x -= 1.0f*(Camera.main.pixelWidth - Camera.main.pixelHeight)/ Camera.main.pixelHeight/2.0f;


            //pos.y -= fluid.m_rect.yMin;

            //pos.x /= fluid.m_rect.width;
            //pos.y /= fluid.m_rect.height;


            c.a = impulseDensity;//simulation density
            Vector4 velocity = new Vector4(Mathf.Cos(Time.realtimeSinceStartup) * 10000, Mathf.Sin(Time.realtimeSinceStartup) * 10000, 0, 0);

            fluid.ApplyImpulse(fluid.m_densityTex, pos, mouseImpluseRadius, c);//alpha used as density other rgb are color

            fluid.ApplyImpulse(fluid.m_velocityTex, pos, mouseImpluseRadius / 3, velocity);//add velocity
        }

        //impluse
        fluid.ApplyImpulse(fluid.m_temperatureTex, implusePos, impluseRadius, impulseTemperature);//temperature
        fluid.ApplyImpulse(fluid.m_densityTex, implusePos, impluseRadius, c);//density //new Vector4(1, 1, 1, impulseDensity)
        //generate
        fluid.Generate();

        base.Update();

        Graphics.Blit(fluid.m_densityTex[1], source);
        Graphics.Blit(fluid.m_velocityTex[1], sourceVec);

        Graphics.Blit(fluid.m_densityTex[1], sourceVec4);


    }
}
