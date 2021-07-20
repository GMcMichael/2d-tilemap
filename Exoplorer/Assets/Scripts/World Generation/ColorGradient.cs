using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorGradient
{

    public enum BlendMode{Linear, Discrete};
    public BlendMode blendMode;
    public bool randomizeColor;
    public bool changed;

    [SerializeField]
    private List<ColorKey> colorKeys;

    public ColorGradient() {
        ResetKeys();
    }

    public Color Evaluate(float time) {
        ColorKey leftColorKey = colorKeys[0];
        ColorKey rightColorKey = colorKeys[colorKeys.Count - 1];

        for(int i = 0; i < colorKeys.Count; i++) {
            if(colorKeys[i].Time <= time) {
                leftColorKey = colorKeys[i];
            }
            if(colorKeys[i].Time >= time) {
                rightColorKey = colorKeys[i];
                break;
            }
        }

        if(blendMode == BlendMode.Linear) {
            float blendTime = Mathf.InverseLerp(leftColorKey.Time, rightColorKey.Time, time);
            return Color.Lerp(leftColorKey.Color, rightColorKey.Color, blendTime);
        } else /*if(blendMode == blendMode.Discrete)*/ {
            return rightColorKey.Color;
        }
    }

    public Texture2D GetTextureVertical(int height) {
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

    public Texture2D GetTextureHorizontal(int width) {
        Texture2D texture = new Texture2D(width, 1);
        Color[] colors = new Color[width];
        for (int i = 0; i < width; i++)
        {
            colors[i] = Evaluate((float)i/(width-1));
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    public int AddColorKey(Color color, float time, string name) {
        ColorKey newColorKey = new ColorKey(color, time, name);
        for(int i = 0; i < colorKeys.Count; i++) {
            if(newColorKey.Time < colorKeys[i].Time) {
                colorKeys.Insert(i, newColorKey);
                changed = true;
                return i;
            }
        }
        colorKeys.Add(newColorKey);
        changed = true;
        return colorKeys.Count-1;
    }

    public int NumKeys() {
        return colorKeys.Count;
    }

    public ColorKey GetKey(int i) {
        if(i < 0) return new ColorKey(Color.white, 0, "");
        return colorKeys[i];
    }

    public void RemoveKey(int index) {
        if(colorKeys.Count >= 2)
            colorKeys.RemoveAt(index);
        changed = true;
    }

    public int UpdateKey(int index, Color color, float time, string name) {
        RemoveKey(index);
        changed = true;
        return AddColorKey(color, time, name);
    }

    public int UpdateKeyTime(int index, float time) {
        Color oldColor = colorKeys[index].Color;
        string oldName = colorKeys[index].Name;
        RemoveKey(index);
        changed = true;
        return AddColorKey(oldColor, time, oldName);
    }

    public void UpdateKeyColor(int index, Color color) {
        colorKeys[index] = new ColorKey(color, colorKeys[index].Time, colorKeys[index].Name);
        changed = true;
    }

    public void UpdateKeyName(int index, string name) {
        colorKeys[index] = new ColorKey(colorKeys[index].Color, colorKeys[index].Time, name);
    }

    public void ResetKeys() {
        colorKeys = new List<ColorKey>();
        AddColorKey(Color.white, 0, "start");
        AddColorKey(Color.black, 1, "end");
        changed = true;
    }

    public void UpdateBlendMode(BlendMode blendMode) {
        this.blendMode = blendMode;
        changed = true;
    }

    [System.Serializable]
    public struct ColorKey{
        [SerializeField]
        private Color color;
        [SerializeField]
        private float time;
        [SerializeField]
        private string name;

        public ColorKey(Color color, float time, string name) {
            this.color = color;
            this.time = time;
            this.name = name;
        }

        public Color Color {
            get {
                return color;
            }
        }

        public float Time {
            get {
                return time;
            }
        }

        public string Name {
            get {
                return name;
            }
        }
    }
}
