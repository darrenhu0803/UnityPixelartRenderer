using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CreateTexturesFeature: ScriptableRendererFeature {

    private abstract class TexturePass: ScriptableRenderPass {
        protected readonly Material material;
        protected readonly List<ShaderTagId> shaderTagIdList;

        protected RTHandle rtHandle;

        protected FilteringSettings filteringSettings;
        protected RenderTextureDescriptor desc;
        public TexturePass(RenderPassEvent renderPassEvent, FilteringSettings filter, string texName, RenderTextureDescriptor rtDesc, Color backgroundColor, string overrideShader = "Unlit/Color") {
            this.renderPassEvent = renderPassEvent;
            rtHandle = RTHandles.Alloc(texName, name: texName);
            shaderTagIdList = new List<ShaderTagId> {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            material = new Material(Shader.Find(overrideShader));

            filteringSettings = filter;
            desc = rtDesc;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            cmd.GetTemporaryRT(Shader.PropertyToID(rtHandle.name), desc, FilterMode.Point);

            ConfigureTarget(rtHandle);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(rtHandle.name));
        }
    }

    private class NormalTexturePass: TexturePass {
        public NormalTexturePass(RenderPassEvent renderPassEvent, LayerMask layerMask) :
            base(renderPassEvent,
                new FilteringSettings(RenderQueueRange.opaque, layerMask),
                "_ViewSpaceNormal",
                new RenderTextureDescriptor(MyConst.Resolution.x / MyConst.DownSamplingScale, MyConst.Resolution.y / MyConst.DownSamplingScale) {
                    depthBufferBits = 24,
                    enableRandomWrite = true,
                    graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
                    sRGB = true,
                    volumeDepth = 1,
                    msaaSamples = 1,
                    dimension = TextureDimension.Tex2D
                },
                Color.black,
                overrideShader: "Custom/NormalShader") {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

            if (!material) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ViewSpaceNormalTextureCreation"))) {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = material;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }


    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private LayerMask normalLayerMask;
    [SerializeField] private LayerMask uvOffsetLayerMask;

    private NormalTexturePass normalTexturePass;
    public override void Create() {
        normalTexturePass = new NormalTexturePass(renderPassEvent, normalLayerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(normalTexturePass);
    }
}
