using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CopyOpaqueFeature: ScriptableRendererFeature {



    private class CopyOpaquePass: ScriptableRenderPass {

        private readonly Material material;
        private RenderTargetIdentifier temporaryBuffer;

        private RTHandle cameraColorRT;

        public CopyOpaquePass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            material = new Material(Shader.Find("Shader Graphs/CopyOpaque"));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("CopyOpaqueTex"))) {

                Blit(cmd, temporaryBuffer, cameraColorRT, material);


            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;
    private CopyOpaquePass screenDistortionPass;

    public override void Create() {
        screenDistortionPass = new CopyOpaquePass(renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(screenDistortionPass);
    }

}
