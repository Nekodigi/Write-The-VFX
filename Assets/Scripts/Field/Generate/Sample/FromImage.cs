using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[ExecuteInEditMode]
public class FromImage : IFieldController
{
    //video player attatched = video, target selected = image, other = webcam
    public Texture2D target;
    public VideoPlayer video;

    WebCamTexture webcamTexture;
    public int width = 1920, height = 1080, fps = 30;

    enum ImageType
    {
        img, video, webcam
    }
    ImageType imageType;


    void Start()
    {
        video = GetComponent<VideoPlayer>();
        if(video != null)
        {
            imageType = ImageType.video;
            video.targetTexture = new RenderTexture((int)video.clip.width, (int)video.clip.height, 0, RenderTextureFormat.ARGBFloat);
        }
        else if(target != null)
        {
            imageType = ImageType.img;
        }
        else
        {
            imageType = ImageType.webcam;
            SetWebCamTexture(0);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (imageType)
        {
            case ImageType.img:
                Graphics.Blit(target, source);
                base.Update();
                Graphics.Blit(target, source);
                break;
            case ImageType.video:
                Graphics.Blit(video.targetTexture, source);
                base.Update();
                Graphics.Blit(video.targetTexture, source);
                break;
            case ImageType.webcam:
                Graphics.Blit(webcamTexture, source);
                base.Update();
                Graphics.Blit(webcamTexture, source);
                break;
        }

        Graphics.Blit(destVec, sourceVec);
    }

    void SetWebCamTexture(int index)
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();
        WebCamDevice[] devices = WebCamTexture.devices;
        try
        {
            webcamTexture = new WebCamTexture(devices[index].name, this.width, this.height, this.fps);
        }
        catch (System.Exception e)
        {
            webcamTexture = new WebCamTexture(devices[0].name, this.width, this.height, this.fps);
        }
        webcamTexture.Play();
    }
}
