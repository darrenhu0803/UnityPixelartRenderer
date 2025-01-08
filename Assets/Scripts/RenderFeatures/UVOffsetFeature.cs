using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UVOffsetFeature: ScriptableRendererFeature {

    private class UVOffsetPass: ScriptableRenderPass {
        protected readonly List<ShaderTagId> shaderTagIdList;

        protected RTHandle rtHandle;

        protected FilteringSettings filteringSettings;
        public UVOffsetPass(RenderPassEvent renderPassEvent, LayerMask layerMask) {
            this.renderPassEvent = renderPassEvent;
            rtHandle = RTHandles.Alloc("_UVOffset", name: "_UVOffset");
            shaderTagIdList = new List<ShaderTagId> {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("SRPDefaultUnlit")
            };


            filteringSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            cmd.GetTemporaryRT(Shader.PropertyToID(rtHandle.name), cameraTextureDescriptor, FilterMode.Point);

            ConfigureTarget(rtHandle);
            ConfigureClear(ClearFlag.All, new Color(0.5f,0.5f,0.5f, 0f));
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("UVOffsetTextureCreation"))) {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(rtHandle.name));
        }
    }



    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private LayerMask normalLayerMask;

    private UVOffsetPass uvOffPass;
    public override void Create() {
        uvOffPass = new UVOffsetPass(renderPassEvent, normalLayerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(uvOffPass);
    }
}
