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
    public bool snapBorders;

    [SerializeField]
    private List<ColorKey> colorKeys;
    [SerializeField]
    private List<BorderInfo> regionBorders;

    private float[,] borderMap;

    public ColorGradient() {
        ResetKeys();
        ResetBorders();
    }

    public Color EvaluateColor(float time) {
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
            colors[i] = EvaluateColor((float)i/(height-1));
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
            colors[i] = EvaluateColor((float)i/(width-1));
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    #region Color Keys
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
    #endregion

    #region Borders
    [System.Serializable]
    public struct BorderInfo {
        [SerializeField]
        private string name;
        [SerializeField]
        private float time;

        public BorderInfo(string name, float time) {
            this.name = name;
            this.time = time;
        }

        public string Name {
            get {
                return name;
            }
        }

        public float Time {
            get {
                return time;
            }
        }
    }

    public float[,] GenerateBorderMap(float[,] noiseMap) {//still need to make pathways between borders
        borderMap = new float[noiseMap.GetLength(0), noiseMap.GetLength(1)];
        int[,,] tempMap = new int[borderMap.GetLength(0), borderMap.GetLength(1), regionBorders.Count-1];

        //Check if there are borders
        if(regionBorders.Count == 0) {
            Debug.Log("No Borders Found");
            return borderMap;
        }
        //add borders at the ends (0 and 1) if they don't exist
        if(regionBorders[0].Time != 0) regionBorders.Insert(0, new BorderInfo("Start", 0));
        if(regionBorders[regionBorders.Count-1].Time != 1) regionBorders.Add(new BorderInfo("End", 1));

        for (int i = 0; i < regionBorders.Count-1; i++)//go between each set of borders and take the perimiter and set it to blockage
        {
            float leftBorder = regionBorders[i].Time;
            float rightBorder = regionBorders[i+1].Time;

            for (int x = 0; x < noiseMap.GetLength(0); x++) {
                for (int y = 0; y < noiseMap.GetLength(1); y++) {
                    if(noiseMap[x,y] >= leftBorder && noiseMap[x,y] <= rightBorder) tempMap[x,y,i] = 1;//if it should be blocked, set it to 1 since default on creation is 0
                }
            }
        }
        
        for (int i = 0; i < tempMap.GetLength(2); i++)
        {
            //all the layers are set, loop through the layers and get the perimeter. If it is part of the perimeter set blockageMap[x,y] to 1
            for (int x = 0; x < noiseMap.GetLength(0); x++) {
                for (int y = 0; y < noiseMap.GetLength(1); y++) {//may want to check corners
                    if(tempMap[x,y,i] != 1) continue;
                    if(y-1 >= 0) {//check up
                        if(tempMap[x,y-1,i] == 0) {
                            borderMap[x,y] = 1;
                            continue;
                        }
                    }
                    if(y+1 < tempMap.GetLength(1)) {//check down
                        if(tempMap[x,y+1,i] == 0) {
                            borderMap[x,y] = 1;
                            continue;
                        }
                    }
                    if(x-1 >= 0) {//check left
                        if(tempMap[x-1,y,i] == 0) {
                            borderMap[x,y] = 1;
                            continue;
                        }
                    }
                    if(x+1 < tempMap.GetLength(0)) {//check right
                        if(tempMap[x+1,y,i] == 0) {
                            borderMap[x,y] = 1;
                            continue;
                        }
                    }
                }
            }
        }
        for (int x = 0; x < borderMap.GetLength(0); x++)//invert the map so blocked = 0, unblocked = 1
        {
            for (int y = 0; y < borderMap.GetLength(1); y++)
            {
                if(borderMap[x,y] == 0) borderMap[x,y] = 1;
                else borderMap[x,y] = 0;
            }
        }
        return borderMap;
    }

    public void ResetBorders() {
        regionBorders = new List<BorderInfo>{
            new BorderInfo("Start", 0),
            new BorderInfo("End", 1)
        };
    }

    public float[] GetBorderRanges() {
        float[] ranges = new float[regionBorders.Count];

        float lastHeight = 0;
        for(int i = 0; i < regionBorders.Count; i++) {
            ranges[i] = regionBorders[i].Time-lastHeight;
            lastHeight = regionBorders[i].Time;
        }

        return ranges;
    }

    public int NumBorders() {
        if(regionBorders == null) ResetBorders();
        return regionBorders.Count;
    }

    public BorderInfo GetBorder(int index) {
        return regionBorders[index];
    }

    public int AddBorder(string name, float time) {
        changed = true;
        BorderInfo newBorder = new BorderInfo(name, time);
        for(int i = 0; i < regionBorders.Count; i++) {
            if(newBorder.Time < regionBorders[i].Time) {
                regionBorders.Insert(i, newBorder);
                return i;
            }
        }
        regionBorders.Add(newBorder);
        return regionBorders.Count-1;
    }

    public void RemoveBorder(int index) {
        if(regionBorders.Count >= 1)
            regionBorders.RemoveAt(index);
        changed = true;
    }

    public int UpdateBorder(int index, string name, float time) {
        RemoveBorder(index);
        changed = true;
        return AddBorder(name, time);
    }

    public int UpdateBorderTime(int index, float newTime) {
        return UpdateBorder(index, regionBorders[index].Name, newTime);
    }

    public void UpdateBorderName(int index, string name) {
        regionBorders[index] = new BorderInfo(name, regionBorders[index].Time);
    }
    #endregion
}
