using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer textureRenderer;
    private Material newMaterial;

    void SetMaterial() {
        newMaterial = new Material(textureRenderer.sharedMaterial);
        textureRenderer.sharedMaterial = newMaterial;
    }

    public void DrawTexture(Texture2D texture) {
        if(newMaterial == null) SetMaterial();
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
