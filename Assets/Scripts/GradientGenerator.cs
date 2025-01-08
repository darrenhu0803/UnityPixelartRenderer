using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class GradientGenerator: MonoBehaviour {
    public Gradient gradient;
    public string path = "/Textures/";

    public float width = 128f;

    private Texture2D gradientTex;
    private Texture2D tempTex;

    Texture2D GenerateGradientTex(Gradient grad) {
        if (tempTex == null) {
            tempTex = new Texture2D((int) width, 1);
        }

        for (int i = 0; i < width; i++) {
            Color color = grad.Evaluate(i / width);
            tempTex.SetPixel(i, 0, color);
        }

        tempTex.wrapMode = TextureWrapMode.Clamp;
        tempTex.Apply();
        return tempTex;
    }

    public void BakeGradientTexture() {
        gradientTex = GenerateGradientTex(gradient);
        byte[] _bytes = gradientTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + path + "GradientTexture.png", _bytes);
    }


}
