using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenOutlineFeature : ScriptableRendererFeature {


   
    private class ScreenSpaceOutlinePass: ScriptableRenderPass {

        private readonly Material screenSpaceOutlineMaterial;
        private readonly Material screenSpaceOutlineBorderMaterial;
        private RenderTargetIdentifier temporaryBuffer;

        private RTHandle cameraColorRT;

        private int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

        public ScreenSpaceOutlinePass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            screenSpaceOutlineMaterial = new Material(Shader.Find("Shader Graphs/OutlineShader"));
            screenSpaceOutlineBorderMaterial = new Material(Shader.Find("Shader Graphs/OutlineShaderBorderOnly"));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines"))) {

                Blit(cmd, cameraColorRT, temporaryBuffer);
                Blit(cmd, temporaryBuffer, cameraColorRT, screenSpaceOutlineBorderMaterial);


            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;
    private ScreenSpaceOutlinePass screenSpaceOutlinePass;

    public override void Create() {
        screenSpaceOutlinePass = new ScreenSpaceOutlinePass(renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(screenSpaceOutlinePass);
    }
}
