using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate Color MakeColorFromIndex(int index);

public class ComputeBufferToTexture 
{
    List<Texture2D> texture;
    public static ComputeBufferToTexture Instance;

    public class RIWithTexture
    {
        public RawImage ri;
        public Texture2D tex2d;
    }

    Dictionary<Array, RIWithTexture> arrayTextureMap = new Dictionary<Array, RIWithTexture>();
    Canvas CanvasTextures;

    [RuntimeInitializeOnLoadMethod]
    public static void SetUp()
    {
        if(Instance == null)
        {
            Instance = new ComputeBufferToTexture();
            var originTextureUI = Resources.Load("TextureUI");
            Instance.CanvasTextures = (GameObject.Instantiate(originTextureUI) as GameObject).GetComponentInChildren<Canvas>();
        }
    }

    public Texture2D SetTexture(Array array, MakeColorFromIndex callback, int width = 0, int height = 0)
    {
        RIWithTexture rwTexture;
        Debug.LogError("as");
        
        if(width == 0 && height == 0)
        {
            var sqrt = Mathf.Sqrt(array.Length);
            width = (int)sqrt;
            height = (int)sqrt;
        }

        if(!arrayTextureMap.TryGetValue(array, out rwTexture))
        {
            rwTexture = new RIWithTexture();
            arrayTextureMap.Add(array, rwTexture);
            rwTexture.tex2d = new Texture2D(width, height);

            var rawImageOrigin = Resources.Load("RawImage");
            var goRawImage = GameObject.Instantiate(rawImageOrigin, CanvasTextures.transform) as GameObject;
            var rawImage = goRawImage.GetComponent<RawImage>();
            rwTexture.ri = rawImage;
            rwTexture.ri.texture = rwTexture.tex2d;
        }

        int x = 0;
        int y = 0;
        int index = 0;
        foreach(var atom in array)
        {            
            Color color = callback(index);
            rwTexture.tex2d.SetPixel(x, y, color);

            ++x;
            ++index;

            if(x >= width)
            {
                x = 0;
                ++y;
            }
        }
        rwTexture.tex2d.Apply();
        rwTexture.ri.texture = rwTexture.tex2d;
        return rwTexture.tex2d;
    }
}
