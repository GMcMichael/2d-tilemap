using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GradientEditor : EditorWindow
{
    private ColorGradient gradient;
    private MapGeneration mapGeneration;
    const int borderSize = 10;
    const float keyWidth = 20;
    const float keyHeight = 10;
    bool verticalOrientation = true;

    Rect gradientPreview;
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
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
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

        borderRects = new Rect[mapGeneration.NumBorders()];
        for(int i = 0; i < borderRects.Length/2; i++) {
            MapGeneration.BorderInfo borderInfo = mapGeneration.GetBorder(i);
            Rect newBorderRect = new Rect(colorKeyRects[0].xMax + borderSize, gradientPreview.yMax - (gradientPreview.height * borderInfo.Time) - keyHeight/2f, keyWidth, keyHeight);
            if(!selectedKey && i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(borderRectMin.x-2, borderRectMin.y-2, borderRectMin.width+4, borderRectMin.height+4), Color.black);
                EditorGUI.DrawRect(new Rect(borderRectMax.x-2, borderRectMax.y-2, borderRectMax.width+4, borderRectMax.height+4), Color.black);
            }
            EditorGUI.DrawRect(borderRectMin, Color.grey);
            EditorGUI.DrawRect(borderRectMax, Color.grey);
            borderRects[i] = borderRectMin;
            borderRects[i+1] = borderRectMax;
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
            Rect colorKeyRect = new Rect(gradientPreview.x + (gradientPreview.width * colorKey.Time) + keyHeight/2, gradientPreview.yMax + borderSize, keyHeight, keyWidth);//keyWidth and keyHeight are switched to make key Rects vertical
            if(i == selectedKeyIndex) {
                EditorGUI.DrawRect(new Rect(colorKeyRect.x-2, colorKeyRect.y-2, colorKeyRect.width+4, colorKeyRect.height+4), Color.black);
            }
            EditorGUI.DrawRect(colorKeyRect, colorKey.Color);
            colorKeyRects[i] = colorKeyRect;
        }
        
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
        verticalOrientation = EditorGUILayout.Toggle("Vertical Orientation", verticalOrientation);
        if(selectedKeyIndex >= 0) {
            if(selectedKey) {
                EditorGUI.BeginChangeCheck();
                Color newColor = EditorGUILayout.ColorField(gradient.GetKey(selectedKeyIndex).Color);
                string newKeyName = EditorGUILayout.TextField("Name", gradient.GetKey(selectedKeyIndex).Name);
                float newKeyTime = EditorGUILayout.FloatField("Time", gradient.GetKey(selectedKeyIndex).Time);
                if(EditorGUI.EndChangeCheck()) {
                    selectedKeyIndex = gradient.UpdateKey(selectedKeyIndex, newColor, newKeyTime, newKeyName);
                }
            } else {
                EditorGUI.BeginChangeCheck();
                string newBorderName = EditorGUILayout.TextField("Name", mapGeneration.GetBorder(selectedKeyIndex).Name);
                float newBorderTime = EditorGUILayout.FloatField("Time", mapGeneration.GetBorder(selectedKeyIndex).Time);
                if(EditorGUI.EndChangeCheck()) {
                    selectedKeyIndex = mapGeneration.UpdateBorder(selectedKeyIndex, newKeyName, newKeyTime);
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
                    float colorKeyTime = GetTime(guiEvent.mousePosition);
                    Color keyColor = GetColor(colorKeyTime);
                    selectedKeyIndex = gradient.AddColorKey(keyColor, colorKeyTime, "");
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
                selectedKeyIndex = mapGeneration.UpdateBorderTime(selectedKeyIndex, newTime);
            }
            needsRepaint = true;
        }

        if(guiEvent.keyCode == KeyCode.Backspace && guiEvent.type == EventType.KeyDown && selectedKeyIndex >= 0) {
            if(selectedKey) {
                gradient.RemoveKey(selectedKeyIndex);
                if(selectedKeyIndex >= gradient.NumKeys()) selectedKeyIndex--;
            } else {
                mapGeneration.RemoveKey(selectedKeyIndex);
                if(selectedKeyIndex >= mapGeneration.NumBorders()) selectedKeyIndex--;
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
        this.gradient = mapGeneration.GetGradient();
    }

    private void OnEnable() {
        titleContent.text = "Gradient Editor";
        position.Set(position.x, position.y, 350, 400);//Set sizes when orientation is switched somehow
        minSize = new Vector2(350, 400);
        maxSize = new Vector2(1000, 1000);
    }

    private void OnDisable() {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private Color GetColor(float time = -1) {
        if(gradient.randomizeColor) return new Color(Random.value, Random.value, Random.value);
        if(time != -1) return gradient.Evaluate(time);
        return Color.white;
    }
}
