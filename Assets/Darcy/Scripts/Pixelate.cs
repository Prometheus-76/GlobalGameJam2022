using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pixelate : MonoBehaviour
{
    public enum PixelScreenMode { Resize, Scale }

    [Header("Screen Scaling Settings")]
    public Vector2Int screenSize;

    [Header("Display")]
    public RawImage outputImage;

    [Header("Components")]
    public GameObject canvasCamera;

    private Camera renderCamera;
    private RenderTexture renderTexture;

    public void Init()
    {
        if (!renderCamera) renderCamera = GetComponent<Camera>();

        if (screenSize.x < 1) screenSize.x = 1;
        if (screenSize.y < 1) screenSize.y = 1;

        int width = (int)screenSize.x;
        int height = (int)screenSize.y;

        renderTexture = new RenderTexture(width, height, 24)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };

        renderCamera.targetTexture = renderTexture;
        outputImage.texture = renderTexture;

        canvasCamera.SetActive(true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }
}
