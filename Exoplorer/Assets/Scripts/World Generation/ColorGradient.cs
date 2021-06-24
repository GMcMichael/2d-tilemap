using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorGradient
{
    public Color Evaluate(float time) {
        return Color.Lerp(Color.white, Color.black, time);
    }

    public Texture2D GetTexture(int height) {
        Texture2D texture = new Texture2D(1, height);
        Color[] colors = new Color[height];
        for (int i = 0; i < height; i++)
        {
            colors[i] = Evaluate((float)i/(height-1));
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
}
