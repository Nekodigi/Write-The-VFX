using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChatGPT;
using UnityEngine.UI;

public class ChatGPTParamControl : MonoBehaviour
{
    
    public IFieldController field;
    public IParticleController particle;
    public ITrailController trail;
    public InputField inputField;
    public Slider slider;
    // Start is called before the first frame update
    float ft, fsx, fsy, fm, ppr, pvr, ps, tl, tw;
    float nft, nfsx, nfsy, nfm, nppr, npvr, nps, ntl, ntw;
    void Start()
    {
        Capture();
    }

    void Capture()
    {
        ft = field.transition;
        fsx = field.scale.x;
        fsy = field.scale.y;
        fm = field.multiplier;
        ppr = particle.posRange;
        pvr = particle.velRange;
        ps = particle.sizeMax.x;
        tl = trail.life;
        tw = trail.width;
    }

    public void SendRequest()
    {
        string prompt = inputField.text;
        var req = new List<Message>() { new Message("system", "Decide parameter update factor based on prompt.") };
        req.Add(new Message("system", "Parameter: Field(transition, size, multiplier)/Particle(posRange, velRange)/Trail(life, width)"));
        req.Add(new Message("system", "Meaning: Field(transition:change over time, size:field size, multiplier:speed)"));
        req.Add(new Message("system", "Meaning: Particle(posRange:!=0 means spherical spawn range, velRange:!=0 means spherical spawn range)/Trail(life:trail length, width)"));
        req.Add(new Message("system", "Meaning: Trail(life:trail length, width)"));
        

        Capture();
        
        req.Add(new Message("system", $"Default Value: {ft},({fsx} {fsy}),{fm}/{ppr},{pvr},{ps}/{tl},{tw}"));
        req.Add(new Message("system", $"Must return in this format: transition,(size.x,size.y),multiplier)/posRange,velRange)/life,width"));
        req.Add(new Message("user", "もっと遅く"));
        req.Add(new Message("assistant", $"{ft},({fsx} {fsy}),{fm / 4}/{ppr},{pvr}/{tl},{tw}"));
        req.Add(new Message("user", "ダイナミックに変化"));
        req.Add(new Message("assistant", $"{ft*4},({fsx} {fsy}),{fm}/{ppr},{pvr}/{tl},{tw}"));
        req.Add(new Message("user", "立体的に放出"));
        req.Add(new Message("assistant", $"{ft},({fsx} {fsy}),{fm}/{0.01},{2}/{tl},{tw}"));
        req.Add(new Message("user", "全体から"));
        req.Add(new Message("assistant", $"{ft},({fsx} {fsy}),{fm}/{0},{0}/{tl},{tw}"));
        req.Add(new Message("user", prompt + "、それ以外はデフォルト"));

        Debug.Log(prompt);
        ChatGPTBase.Request(req, (res) => OnResponse(res));
    }

    void OnResponse(string res)
    {
        string[] datasList = res.Split("/");
        string[] fieldDatas = datasList[0].Split(",");
        string[] particleDatas = datasList[1].Split(",");
        string[] trailDatas = datasList[2].Split(",");

        if(!isValidData(fieldDatas, particleDatas, trailDatas))
        {
            Debug.LogError("Invalid return value:"+res);
            return;
        }
        nft = float.Parse(fieldDatas[0]);
        Vector2 fsize = parseVector2(fieldDatas[1]);
        nfsx = fsize.x;
        nfsy = fsize.y;
        nfm = float.Parse(fieldDatas[2]);
        nppr = float.Parse(particleDatas[0]);
        npvr = float.Parse(particleDatas[1]);
        //nps = float.Parse(particleDatas[2]);
        ntl = float.Parse(trailDatas[0]);
        ntw = float.Parse(trailDatas[1]);

        Debug.Log(res);
    }

    Vector2 parseVector2(string str)
    {
        string[] datas = str.Replace("(", "").Replace(")", "").Split(" ");
        return new Vector2(float.Parse(datas[0]), float.Parse(datas[1]));
    }

    bool isValidData(string[] fieldDatas, string[] particleDatas, string[] trailDatas)
    {
        return fieldDatas.Length == 3 && particleDatas.Length == 2 && trailDatas.Length == 2;
    }

    // Update is called once per frame
    void Update()
    {
        float fac = slider.value;
        field.transition = Mathf.Lerp(ft, nft, fac);
        field.scale = new Vector2(Mathf.Lerp(fsx, nfsx, fac), Mathf.Lerp(fsy, nfsy, fac));
        field.multiplier = Mathf.Lerp(fm, nfm, fac);
        particle.posRange = Mathf.Lerp(ppr, nppr, fac);
        particle.velRange = Mathf.Lerp(pvr, npvr, fac);
        //float size = Mathf.Lerp(ps, nps, fac);
        //particle.sizeMin = new Vector3(size, size, size);
        //particle.sizeMax = new Vector3(size, size, size);
        trail.life = Mathf.Lerp(tl, ntl, fac);
        trail.width = Mathf.Lerp(tw, ntw, fac);
    }
}
