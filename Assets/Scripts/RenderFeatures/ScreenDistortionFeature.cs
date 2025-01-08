using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenDistortionFeature: ScriptableRendererFeature {



    private class ScreenDistortionPass: ScriptableRenderPass {

        private readonly Material material;
        private RenderTargetIdentifier temporaryBuffer;

        private RTHandle cameraColorRT;

        public ScreenDistortionPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            material = new Material(Shader.Find("Shader Graphs/ScreenDistortion"));
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenDistortion"))) {

                Blit(cmd, cameraColorRT, temporaryBuffer);
                Blit(cmd, temporaryBuffer, cameraColorRT, material);


            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;
    private ScreenDistortionPass screenDistortionPass;

    public override void Create() {
        screenDistortionPass = new ScreenDistortionPass(renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(screenDistortionPass);
    }

}
