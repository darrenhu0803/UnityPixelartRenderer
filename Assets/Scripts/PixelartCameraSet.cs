using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways, RequireComponent(typeof(Camera))]
public class PixelartCameraSet: MonoBehaviour {

    
    [SerializeField, HideInInspector]
    Camera m_PixelatedCamera;
    [SerializeField, HideInInspector]
    Camera m_PixelatedCamera2;
    [SerializeField, HideInInspector]
    Camera m_HighResCamera;
    [SerializeField, HideInInspector]
    Camera m_DistortionCamera;
    [SerializeField, HideInInspector]
    Camera m_ThisCamera;
    [SerializeField, HideInInspector]
    UniversalAdditionalCameraData m_PixelatedCameraData;
    [SerializeField, HideInInspector]
    UniversalAdditionalCameraData m_PixelatedCameraData2;
    [SerializeField, HideInInspector]
    UniversalAdditionalCameraData m_HighResCameraData;
    [SerializeField, HideInInspector]
    UniversalAdditionalCameraData m_DistortionCameraData;
    [SerializeField, HideInInspector]
    UniversalAdditionalCameraData m_ThisCameraData;
    [SerializeField, HideInInspector]
    RenderTexture m_PixelatedResult;
    RenderTexture m_PixelatedResult2;
    RenderTexture m_HighResResult;
    RenderTexture m_DistortionBuffer;


    void OnEnable() {
        Initialize();
        GetBuffers();

        m_PixelatedCamera.enabled = true;
        m_PixelatedCamera2.enabled = true;
        m_HighResCamera.enabled = true;
        m_DistortionCamera.enabled = true;
    }

    void OnDisable() {
        ReleaseBuffers();

        m_PixelatedCamera.enabled = false;
        m_PixelatedCamera2.enabled = false;
        m_HighResCamera.enabled = false;
        m_DistortionCamera.enabled = false;
    }

    void OnValidate() {
        GetBuffers();
    }

    private void Initialize() {
        InitializeThisCamera();
        InitializePixelatedCamera();
        InitializePixelatedCamera2();
        InitializeHighResCamera();
        InitializeDistortionCamera();
    }

    private void InitializeThisCamera() {
        if (m_ThisCamera == null) {
            m_ThisCamera = GetComponent<Camera>();
            m_ThisCameraData = m_ThisCamera.GetComponent<UniversalAdditionalCameraData>();
            if (m_ThisCameraData == null) m_ThisCameraData = m_ThisCamera.AddComponent<UniversalAdditionalCameraData>();
        }

        m_ThisCamera.clearFlags = CameraClearFlags.Nothing;
        m_ThisCamera.cullingMask = 0;
        m_ThisCamera.farClipPlane = 0.02f;
        m_ThisCamera.nearClipPlane = 0.01f;

        m_ThisCameraData.SetRenderer(1);
    }
    private void InitializePixelatedCamera() {
        if (m_PixelatedCamera == null) {
            GameObject pixelatedCameraObject = new GameObject("Pixelated Camera");
            pixelatedCameraObject.transform.SetParent(transform, false);
            m_PixelatedCamera = pixelatedCameraObject.AddComponent<Camera>();
            m_PixelatedCameraData = m_PixelatedCamera.GetComponent<UniversalAdditionalCameraData>();
            if (m_PixelatedCameraData == null) m_PixelatedCameraData = m_PixelatedCamera.AddComponent<UniversalAdditionalCameraData>();
            m_PixelatedCamera.orthographicSize = 6.125f;
        }

        m_PixelatedCamera.orthographic = true;
        m_PixelatedCamera.depth = -64;
        m_PixelatedCameraData.SetRenderer(2);
    }
    private void InitializePixelatedCamera2() {
        if (m_PixelatedCamera2 == null) {
            GameObject pixelatedCameraObject2 = new GameObject("Pixelated Camera 2");
            pixelatedCameraObject2.transform.SetParent(transform, false);
            m_PixelatedCamera2 = pixelatedCameraObject2.AddComponent<Camera>();
            m_PixelatedCameraData2 = m_PixelatedCamera2.GetComponent<UniversalAdditionalCameraData>();
            if (m_PixelatedCameraData2 == null) m_PixelatedCameraData2 = m_PixelatedCamera2.AddComponent<UniversalAdditionalCameraData>();
            m_PixelatedCamera2.orthographicSize = 6.125f;
        }

        m_PixelatedCamera2.orthographic = true;
        m_PixelatedCamera2.depth = -64;
        m_PixelatedCameraData2.SetRenderer(3);
    }
    private void InitializeHighResCamera() {
        if (m_HighResCamera == null) {
            GameObject HighResCameraObject = new GameObject("HighRes Camera");
            HighResCameraObject.transform.SetParent(transform, false);
            m_HighResCamera = HighResCameraObject.AddComponent<Camera>();
            m_HighResCameraData = m_HighResCamera.GetComponent<UniversalAdditionalCameraData>();
            if (m_HighResCameraData == null) m_HighResCameraData = m_HighResCamera.AddComponent<UniversalAdditionalCameraData>();
            m_HighResCamera.orthographicSize = 6.125f;
        }

        m_HighResCamera.orthographic = true;
        m_HighResCamera.depth = -64;
        m_HighResCameraData.SetRenderer(5);
    }
    private void InitializeDistortionCamera() {
        if (m_DistortionCamera == null) {
            GameObject DistortionCameraObject = new GameObject("Distortion Camera");
            DistortionCameraObject.transform.SetParent(transform, false);
            m_DistortionCamera = DistortionCameraObject.AddComponent<Camera>();
            m_DistortionCameraData = m_DistortionCamera.GetComponent<UniversalAdditionalCameraData>();
            if (m_DistortionCameraData == null) m_DistortionCameraData = m_DistortionCamera.AddComponent<UniversalAdditionalCameraData>();
            m_DistortionCamera.orthographicSize = 6.125f;
        }

        m_DistortionCamera.orthographic = true;
        m_DistortionCamera.depth = -64;
        m_DistortionCameraData.SetRenderer(4);
    }

    private void ReleaseBuffers() {

        if (m_PixelatedResult != null) {
            m_PixelatedCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(m_PixelatedResult);
            m_PixelatedResult = null;
        }

        if (m_PixelatedResult2 != null) {
            m_PixelatedCamera2.targetTexture = null;
            RenderTexture.ReleaseTemporary(m_PixelatedResult2);
            m_PixelatedResult2 = null;
        }

        if (m_DistortionBuffer != null) {
            m_DistortionCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(m_DistortionBuffer);
            m_DistortionBuffer = null;
        }

        if (m_HighResResult != null) {
            m_HighResCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(m_HighResResult);
            m_HighResResult = null;
        }
    }

    private void GetBuffers() {
        ReleaseBuffers();

        RenderTextureDescriptor resultDesc = new RenderTextureDescriptor(MyConst.Resolution.x / MyConst.DownSamplingScale, MyConst.Resolution.y / MyConst.DownSamplingScale) {
            depthBufferBits = 24,
            enableRandomWrite = true,
            graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
            sRGB = true,
            volumeDepth = 1,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D
        };

        m_PixelatedResult = RenderTexture.GetTemporary(resultDesc);
        m_PixelatedResult.filterMode = FilterMode.Point;
        m_PixelatedResult.Create();
        Shader.SetGlobalTexture("_Pixelated", m_PixelatedResult);

        RenderTextureDescriptor vfxDesc = new RenderTextureDescriptor(MyConst.Resolution.x / MyConst.DownSamplingScale, MyConst.Resolution.y / MyConst.DownSamplingScale) {
            depthBufferBits = 24,
            enableRandomWrite = true,
            graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
            sRGB = true,
            volumeDepth = 1,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D
        };

        m_PixelatedResult2 = RenderTexture.GetTemporary(vfxDesc);
        m_PixelatedResult2.filterMode = FilterMode.Point;
        m_PixelatedResult2.Create();
        Shader.SetGlobalTexture("_Pixelated2", m_PixelatedResult2);

        RenderTextureDescriptor distortionDesc = new RenderTextureDescriptor(MyConst.Resolution.x, MyConst.Resolution.y) {
            depthBufferBits = 24,
            enableRandomWrite = true,
            graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
            sRGB = true,
            volumeDepth = 1,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D
        };

        m_DistortionBuffer = RenderTexture.GetTemporary(distortionDesc);
        m_DistortionBuffer.filterMode = FilterMode.Point;
        m_DistortionBuffer.Create();
        Shader.SetGlobalTexture("_Distortion", m_DistortionBuffer);

        RenderTextureDescriptor highResDesc = new RenderTextureDescriptor(MyConst.Resolution.x, MyConst.Resolution.y) {
            depthBufferBits = 24,
            enableRandomWrite = true,
            graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
            sRGB = true,
            volumeDepth = 1,
            msaaSamples = 1,
            dimension = TextureDimension.Tex2D
        };

        m_HighResResult = RenderTexture.GetTemporary(highResDesc);
        m_HighResResult.filterMode = FilterMode.Point;
        m_HighResResult.Create();
        Shader.SetGlobalTexture("_HighRes", m_HighResResult);

        m_PixelatedCamera.targetTexture = m_PixelatedResult;

        m_PixelatedCamera2.targetTexture = m_PixelatedResult2;

        m_DistortionCamera.targetTexture = m_DistortionBuffer;

        m_HighResCamera.targetTexture = m_HighResResult;
    }

}

