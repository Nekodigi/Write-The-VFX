using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[ExecuteInEditMode]
public class FromImage : IFieldController
{
    //video player attatched = video, target selected = image, other = webcam
    public Texture2D target_;
    RenderTexture target;
    public VideoPlayer video;

    WebCamTexture webcamTexture;
    public int width = 1920, height = 1080, fps = 30;
    public ComputeShader cs;

    FFTBlur fftBlur;

    enum ImageType
    {
        img, video, webcam
    }
    ImageType imageType;


    void Start()
    {
        video = GetComponent<VideoPlayer>();
        fftBlur = GetComponent<FFTBlur>();
        

        if (video != null)
        {
            imageType = ImageType.video;
            video.targetTexture = new RenderTexture((int)video.clip.width, (int)video.clip.height, 0, RenderTextureFormat.ARGBFloat);
        }
        else if(target_ != null)
        {
            imageType = ImageType.img;

            target = new RenderTexture((int)target_.width / 2, (int)target_.height / 2, 0, RenderTextureFormat.ARGBFloat);
            target.enableRandomWrite = true;
            target.Create();

            /*int kernelDownscaleId = cs.FindKernel("Downscale");
            int kernelVertId = cs.FindKernel("GaussianBlurVertical");
            int kernelHoriId = cs.FindKernel("GaussianBlurHorizontal");
            int kernelBoxId = cs.FindKernel("BoxBlur");
            


            cs.SetVector("_Size", new Vector2(1000, 1000));
            cs.SetFloat("_Amount", 2);
            cs.SetTexture(kernelDownscaleId, "_Source", target_);
            cs.SetTexture(kernelDownscaleId, "_Dest", target);
            cs.Dispatch(kernelDownscaleId, target_.width/32, target_.height / 32, 1);

            cs.SetTexture(kernelVertId, "_Dest", target);
            cs.SetTexture(kernelHoriId, "_Dest", target);

            for (int i = 0; i < 16; i++)
            {
                cs.Dispatch(kernelVertId, target_.width / 32, target_.height / 32, 1);
                cs.Dispatch(kernelHoriId, target_.width / 32, target_.height / 32, 1);

            }*/
            Graphics.Blit(target_, target);
            Debug.Log("BLUR");
        }
        else
        {
            imageType = ImageType.webcam;
            SetWebCamTexture(0);

            target = new RenderTexture((int)webcamTexture.width / 2, (int)webcamTexture.height / 2, 0, RenderTextureFormat.ARGBFloat);
            target.enableRandomWrite = true;
            target.Create();
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

                /*int kernelDownscaleId = cs.FindKernel("Downscale");
                int kernelVertId = cs.FindKernel("GaussianBlurVertical");
                int kernelHoriId = cs.FindKernel("GaussianBlurHorizontal");
                int kernelBoxId = cs.FindKernel("BoxBlur");


                cs.SetVector("_Size", new Vector2(1000, 1000));
                cs.SetFloat("_Amount", 2);
                cs.SetTexture(kernelDownscaleId, "_Source", webcamTexture);
                cs.SetTexture(kernelDownscaleId, "_Dest", target);
                cs.Dispatch(kernelDownscaleId, webcamTexture.width / 32, webcamTexture.height / 32, 1);

                cs.SetTexture(kernelVertId, "_Dest", target);
                cs.SetTexture(kernelHoriId, "_Dest", target);

                for (int i = 0; i < 16; i++)
                {
                    cs.Dispatch(kernelVertId, webcamTexture.width / 32, webcamTexture.height / 32, 1);
                    cs.Dispatch(kernelHoriId, webcamTexture.width / 32, webcamTexture.height / 32, 1);

                }*/

                Graphics.Blit(webcamTexture, target);
                target = fftBlur.Execute(target);
                Graphics.Blit(target, source);


                base.Update();
                Graphics.Blit(target, dest);


                break;
        }

        Graphics.Blit(destVec, sourceVec);
        Graphics.Blit(dest, source);
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
