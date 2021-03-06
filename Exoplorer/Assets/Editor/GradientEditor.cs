using UnityEngine;
using UnityEditor;

public class GradientEditor : EditorWindow
{
    private ColorGradient gradient;
    private MapGeneration mapGeneration;
    const int borderSize = 10;
    const float keyWidth = 20;
    const float keyHeight = 10;
    bool verticalOrientation = true;

    Rect gradientPreview;
    Rect borderPreview;
    Rect borderRange;
    Rect settingsRect;
    Rect[] colorKeyRects;
    Rect[] borderRects;
    bool mouseIsDownOverKey;
    int selectedKeyIndex;
    bool selectedKey;
    bool needsRepaint;

    void OnGUI() {
        if(verticalOrientation) Draw();
        else DrawHorizontal();
        HandleInput();

        if(needsRepaint) {
            needsRepaint = false;
            GUI.FocusControl(null);
            Undo.RecordObject(this, "Gradient Editor Chnaged");
            Repaint();
        }
    }

    private void Draw() {
        gradientPreview = new Rect(borderSize, borderSize, 25, position.height-borderSize*2);
        GUI.DrawTexture(gradientPreview, gradient.GetTextureVertical((int)gradientPreview.height));
        
        colorKeyRects = new Rect[gradient.NumKeys()];
        for(int i = 0; i < colorKeyRects.Length; i++) {
            ColorGradient.ColorKey colorKey = gradient.GetKey(i);
            Rect colorKeyRect = new Rect(gradientPreview.xMax + borderSize, gradientPreview.yMax - (gradientPreview.height * colorKey.Time) - keyHeight/2f, keyWidth, keyHeight);
            if(selectedKey && i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(colorKeyRect.x-2, colorKeyRect.y-2, colorKeyRect.width+4, colorKeyRect.height+4), Color.black);
            }
            EditorGUI.DrawRect(colorKeyRect, colorKey.Color);
            colorKeyRects[i] = colorKeyRect;
        }
        
        borderRects = new Rect[gradient.NumBorders()];
        for(int i = 0; i < borderRects.Length; i++) {
            ColorGradient.BorderInfo borderInfo = gradient.GetBorder(i);
            Rect newBorderRect = new Rect(colorKeyRects[0].xMax + borderSize, gradientPreview.yMax - (gradientPreview.height * borderInfo.Time) - keyHeight/2f, keyWidth, keyHeight);
            if(!selectedKey && i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(newBorderRect.x-2, newBorderRect.y-2, newBorderRect.width+4, newBorderRect.height+4), Color.black);
            }
            EditorGUI.DrawRect(newBorderRect, gradient.EvaluateColor(gradient.GetBorder(i).Time));
            borderRects[i] = newBorderRect;
        }
        
        if(borderRects.Length > 0 )
            settingsRect = new Rect(borderRects[0].xMax + borderSize, borderSize, position.width-borderRects[0].xMax-borderSize*2, position.height-borderSize*2);
        else
            settingsRect = new Rect(colorKeyRects[0].xMax + borderSize, borderSize, position.width-colorKeyRects[0].xMax-borderSize*2, position.height-borderSize*2);
        DrawSettings();
    }

    private void DrawHorizontal() {
        gradientPreview = new Rect(borderSize, borderSize, position.width-borderSize*2, 25);
        GUI.DrawTexture(gradientPreview, gradient.GetTextureHorizontal((int)gradientPreview.width));

        colorKeyRects = new Rect[gradient.NumKeys()];
        for(int i = 0; i < gradient.NumKeys(); i++) {
            ColorGradient.ColorKey colorKey = gradient.GetKey(i);
            Rect colorKeyRect = new Rect(gradientPreview.x + (gradientPreview.width * colorKey.Time) - keyHeight/2, gradientPreview.yMax + borderSize, keyHeight, keyWidth);//keyWidth and keyHeight are switched to make key Rects vertical
            if(i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(colorKeyRect.x-2, colorKeyRect.y-2, colorKeyRect.width+4, colorKeyRect.height+4), Color.black);
            }
            EditorGUI.DrawRect(colorKeyRect, colorKey.Color);
            colorKeyRects[i] = colorKeyRect;
        }

        float[] borderRectSettings = {colorKeyRects[0].yMax + borderSize, 5, position.width-borderSize*2};
        float[] borderPos = gradient.GetBorderRanges();
        for (int i = 0; i < borderPos.Length; i++)
        {
            borderRange = new Rect(gradientPreview.x + (gradientPreview.width * gradient.GetBorder(i).Time), borderRectSettings[0], -(gradientPreview.width * borderPos[i]), borderRectSettings[1]);
            EditorGUI.DrawRect(borderRange, gradient.EvaluateColor(gradient.GetBorder(i).Time));
        }
        borderRects = new Rect[gradient.NumBorders()];
        for(int i = 0; i < borderRects.Length; i++) {
            ColorGradient.BorderInfo borderInfo = gradient.GetBorder(i);
            Rect newBorderRect = new Rect(gradientPreview.x + (gradientPreview.width * borderInfo.Time) - keyHeight/2f, borderRange.yMax + borderSize, keyHeight, keyWidth);
            if(!selectedKey && i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(newBorderRect.x-2, newBorderRect.y-2, newBorderRect.width+4, newBorderRect.height+4), Color.black);
            }
            EditorGUI.DrawRect(newBorderRect, gradient.EvaluateColor(gradient.GetBorder(i).Time));
            borderRects[i] = newBorderRect;
        }
        
        if(borderRects.Length > 0 )
            settingsRect = new Rect(borderSize, borderRects[0].yMax + borderSize, position.width-borderSize*2, position.height-borderRects[0].yMax-borderSize*2);
        else
            settingsRect = new Rect(borderSize, colorKeyRects[0].yMax + borderSize, position.width-borderSize*2, position.height-colorKeyRects[0].yMax-borderSize*2);
        DrawSettings();
    }

    private void DrawSettings() {
        GUILayout.BeginArea(settingsRect);
        EditorGUI.BeginChangeCheck();
        ColorGradient.BlendMode blendMode = (ColorGradient.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", gradient.blendMode);
        if(EditorGUI.EndChangeCheck()) {
            gradient.UpdateBlendMode(blendMode);
        }

        gradient.randomizeColor = EditorGUILayout.Toggle("Randomize New Color", gradient.randomizeColor);
        gradient.snapBorders = EditorGUILayout.Toggle("Snap Borders", gradient.snapBorders);
        verticalOrientation = EditorGUILayout.Toggle("Vertical Orientation", verticalOrientation);
        if(GUILayout.Button("Reset Borders")) gradient.ResetBorders();
        if(selectedKeyIndex >= 0) {
            if(selectedKey) {
                EditorGUI.BeginChangeCheck();
                Color newColor = EditorGUILayout.ColorField(gradient.GetKey(selectedKeyIndex).Color);
                string newKeyName = EditorGUILayout.TextField("Name", gradient.GetKey(selectedKeyIndex).Name);
                float newKeyTime = EditorGUILayout.FloatField("Time", gradient.GetKey(selectedKeyIndex).Time);
                if(EditorGUI.EndChangeCheck()) {
                    selectedKeyIndex = gradient.UpdateKey(selectedKeyIndex, newColor, newKeyTime, newKeyName);
                }
            } else if(gradient.NumBorders() != 0){
                if(selectedKeyIndex >= gradient.NumBorders()) {
                    selectedKeyIndex = gradient.NumBorders()-1;
                }
                EditorGUI.BeginChangeCheck();
                string newBorderName = EditorGUILayout.TextField("Name", gradient.GetBorder(selectedKeyIndex).Name);
                float newBorderTime = EditorGUILayout.FloatField("Time", gradient.GetBorder(selectedKeyIndex).Time);
                if(EditorGUI.EndChangeCheck()) {
                    selectedKeyIndex = gradient.UpdateBorder(selectedKeyIndex, newBorderName, newBorderTime);
                }
            }
        }
        GUILayout.EndArea();
    }

    private void HandleInput() {
        Event guiEvent = Event.current;

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 0) {

            for(int i = 0; i < colorKeyRects.Length; i++) {
                if(colorKeyRects[i].Contains(guiEvent.mousePosition)) {
                    mouseIsDownOverKey = true;
                    selectedKeyIndex = i;
                    selectedKey = true;
                    break;
                }
            }

            for(int i = 0; i < borderRects.Length; i++) {
                if(borderRects[i].Contains(guiEvent.mousePosition)) {
                    mouseIsDownOverKey = true;
                    selectedKeyIndex = i;
                    selectedKey = false;
                    break;
                }
            }
            
            if(!mouseIsDownOverKey) {
                if(settingsRect.Contains(guiEvent.mousePosition)) {
                    selectedKeyIndex = -1;
                } else {
                    Rect TimeRect;
                    if(verticalOrientation) {
                        TimeRect = new Rect(0, 0, colorKeyRects[0].xMax, position.height);
                    } else {
                        TimeRect = new Rect(0, 0, position.width, colorKeyRects[0].yMax);
                    }
                    float newTime = GetTime(guiEvent.mousePosition);
                    if(TimeRect.Contains(guiEvent.mousePosition)) {
                        Color keyColor = GetColor(newTime);
                        selectedKeyIndex = gradient.AddColorKey(keyColor, newTime, "");
                    } else {
                        selectedKeyIndex = gradient.AddBorder("", newTime);
                    }
                    mouseIsDownOverKey = true;
                }
            }
            needsRepaint = true;
        }

        if(guiEvent.type == EventType.MouseUp && guiEvent.button == 0) {
            mouseIsDownOverKey = false;
        }

        if(mouseIsDownOverKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && selectedKeyIndex >= 0) {
            float newTime = GetTime(guiEvent.mousePosition);
            if(selectedKey) {
                selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, newTime);
            } else {
                if(gradient.snapBorders) {
                    for (int i = 0; i < gradient.NumKeys(); i++)
                    {
                        float colorKeyTime = gradient.GetKey(i).Time;
                        if(Mathf.Abs((newTime-colorKeyTime)) <= 0.005f) {
                            newTime = colorKeyTime;
                            break;
                        }
                    }
                }
                selectedKeyIndex = gradient.UpdateBorderTime(selectedKeyIndex, newTime);
            }
            needsRepaint = true;
        }

        if(guiEvent.keyCode == KeyCode.Backspace && guiEvent.type == EventType.KeyDown && selectedKeyIndex >= 0) {
            if(selectedKey) {
                gradient.RemoveKey(selectedKeyIndex);
                if(selectedKeyIndex >= gradient.NumKeys()) selectedKeyIndex--;
            } else {
                gradient.RemoveBorder(selectedKeyIndex);
                if(selectedKeyIndex >= gradient.NumBorders()) selectedKeyIndex--;
            }
            needsRepaint = true;
        }

        if(guiEvent.type == EventType.MouseDown && guiEvent.button == 1) {
            gradient.ResetKeys();
            needsRepaint = true;
        }
    }

    private float GetTime(Vector2 mousePos) {
        if(verticalOrientation) return Mathf.InverseLerp(gradientPreview.yMax, gradientPreview.y, mousePos.y);
        else return Mathf.InverseLerp(gradientPreview.x, gradientPreview.xMax, mousePos.x);
    }

    public void SetGradient(ColorGradient gradient) {
        this.gradient = gradient;
        //this.mapGeneration = gradient.GetMapGeneration();
    }

    public void SetMapGeneration(MapGeneration mapGeneration) {
        this.mapGeneration = mapGeneration;
        this.gradient = mapGeneration.GetRegionInfo();
    }

    private void OnEnable() {
        titleContent.text = "Gradient Editor";
        position.Set(position.x, position.y, 350, 400);//Set sizes when orientation is switched somehow
        minSize = new Vector2(350, 400);
        maxSize = new Vector2(1000, 1000);
    }

    private void OnDisable() {
        Undo.RecordObject(this, "Gradient Editor Chnaged");
    }

    private Color GetColor(float time = -1) {
        if(gradient.randomizeColor) return new Color(Random.value, Random.value, Random.value);
        if(time != -1) return gradient.EvaluateColor(time);
        return Color.white;
    }
}
