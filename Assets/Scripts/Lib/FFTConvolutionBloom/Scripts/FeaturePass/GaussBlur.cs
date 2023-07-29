using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussBlur
{
    private const string CommandBufferName = nameof(GaussBlurBorderRenderPass);

    private RenderTargetIdentifier _colorTarget;
    private FFTBlur _fftBlur = null;

    private Material mat;

    //PropertyToID関連
    private int _fftTempID1, _fftTempID2;

    public GaussBlur(Shader shader)
    {
        mat = CoreUtils.CreateEngineMaterial(shader);
        _fftTempID1 = Shader.PropertyToID("_fftTempID1");//FFTの入力
        _fftTempID2 = Shader.PropertyToID("_fftTempID2");//FFTの出力
    }

    public void Execute()
    {
        if (_fftBlur == null) return;

        var commandBuffer = CommandBufferPool.Get(CommandBufferName);

        commandBuffer.GetTemporaryRT(_fftTempID1, _fftBlur.Descriptor, FilterMode.Bilinear);//入力。xyサイズは_fFTBloom.Descriptorじゃなくても良い
        commandBuffer.GetTemporaryRT(_fftTempID2, _fftBlur.Descriptor, FilterMode.Bilinear);//出力


        // 現在のカメラ描画画像をRenderTextureにコピー
        // borderRatioを0.0より大きくすることで画面端に余白を持たせることができる。fft計算で端から端に回り込んでブラーがかかるための対処
        // convolution kernelの内容にあわせて調整を
        // _colorTargetのfilter moder=clampにすることで画面端を引き伸ばすことができる
        commandBuffer.SetGlobalFloat("_ScalingRatio", 1f / (1f - 2f * _fftBlur.borderRatio));
        commandBuffer.Blit(_fftBlur.target, _fftTempID1, mat);

        //ここでFFTConvolution実行
        _fftBlur.FFTConvolutionFromRenderPass(commandBuffer, _fftTempID1, _fftTempID2);

        // RenderTextureを現在のRenderTarget（カメラ）にコピー
        // 余白の分を考慮
        commandBuffer.SetGlobalFloat("_ScalingRatio", 1f - 2f * _fftBlur.borderRatio);

        
        commandBuffer.Blit(_fftTempID2, _fftBlur.target, mat);

        

        Graphics.ExecuteCommandBuffer(commandBuffer);

        CommandBufferPool.Release(commandBuffer);
    }

    public void SetFFT(FFTBlur fftBlur) 
    {
        _fftBlur = fftBlur;
    }
}