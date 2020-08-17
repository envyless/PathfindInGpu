using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate Color MakeColorFromIndex(int index);
public delegate (string, Vector3) MakeStringFromIndex(int index);

public class ComputeBufferToTexture : MonoBehaviour
{
    List<Texture2D> texture;
    public static ComputeBufferToTexture Instance;

    public class RIWithTexture
    {
        public RawImage ri;
        public Texture2D tex2d;
    }

    Dictionary<Array, RIWithTexture> arrayTextureMap = new Dictionary<Array, RIWithTexture>();
    Canvas canvasTextures;
    Transform parentTexture;

    [RuntimeInitializeOnLoadMethod]
    public static void SetUp()
    {
        if(Instance == null)
        {
            var goInstance = new GameObject("Manager");
            Instance = goInstance.AddComponent<ComputeBufferToTexture>();
            goInstance.name = Instance.ToString();
            var originTextureUI = Resources.Load("TextureUI");
            Instance.canvasTextures = (GameObject.Instantiate(originTextureUI) as GameObject).GetComponentInChildren<Canvas>();
            Instance.parentTexture = Instance.canvasTextures.transform.Find("ParentTexture");
        }
    }

    public static void SetScaleTexture(float scale)
    {
        Instance.parentTexture.localScale = Vector2.one * scale;
    }

    public Texture2D SetTexture(string textureName, Array array, MakeColorFromIndex callback, int width = 0, int height = 0)
    {
        RIWithTexture rwTexture;
        
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
            var goRawImage = GameObject.Instantiate(rawImageOrigin, parentTexture) as GameObject;
            var rawImage = goRawImage.GetComponent<RawImage>();
            var name = goRawImage.GetComponentInChildren<Text>();
            name.text = textureName;

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

    private Dictionary<Array, List<Text>> arrayToText = new Dictionary<Array, List<Text>>();
    
    [Range(0,1000)]
    public int MaxNumOfText = 1000;
    
    /// <summary>
    /// when you need to show array atom info, use this. and MaxNumOfText will limit this count
    /// </summary>
    /// <param name="array"></param>
    /// <param name="callback">callback must return world position vector!</param>
    /// <param name="width"></param>
    //texture2label
    public static void SetText(Array array, MakeStringFromIndex callback, int width = 0, Color? color = null)
    {
        if (width == 0)
        {
            var sqrt = Mathf.Sqrt(array.Length);
            width = (int)sqrt;
        }

        Instance.arrayToText.TryGetValue(array, out var textList);
        if (textList == null)
        {
            int numArray = array.Length;
            textList = new List<Text>(numArray);
            Instance.arrayToText.Add(array, textList);

            foreach (var atom in array)
            {
                var a2t = Resources.Load("Array2Text");
                var instanced_a2t = GameObject.Instantiate(a2t, Instance.canvasTextures.transform) as GameObject;

                var text = instanced_a2t.GetComponentInChildren<Text>();
                textList.Add(text);
            }
        }

        int x = 0;
        int y = 0;
        int index = 0;
        int active_count = 0;
        foreach (var text in textList)
        {            
            do
            {
                bool isActive = Instance.MaxNumOfText > active_count;
                if (isActive == false)
                {
                    text.gameObject.SetActive(false);
                    break;
                }

                var info_and_position = callback(index);
                text.transform.position = Camera.main.WorldToScreenPoint(info_and_position.Item2);
                if (color != null)
                    text.color = color.Value;

                text.text = info_and_position.Item1;
                text.gameObject.SetActive(text.text != string.Empty);
                if (text.gameObject.activeInHierarchy)
                {
                    active_count++;                    
                }

            } while (false);
            
            //increase index
            ++x;
            ++index;
            if (x >= width)
            {
                x = 0;
                ++y;
            }
        }
    }
}
