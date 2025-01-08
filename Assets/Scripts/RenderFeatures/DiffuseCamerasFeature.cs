using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DiffuseCamerasFeature : ScriptableRendererFeature {
    private class DiffuseCamerasPass: ScriptableRenderPass {

        private readonly Material material;
        private RenderTargetIdentifier cameraColorTarget;


        private RTHandle opaqueResult;

        public DiffuseCamerasPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            material = new Material(Shader.Find("Shader Graphs/VFXProcessShader"));
            opaqueResult = RTHandles.Alloc("_OpaqueResult", name: "_OpaqueResult");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

            cmd.GetTemporaryRT(Shader.PropertyToID(opaqueResult.name), cameraTextureDescriptor, FilterMode.Point);

            ConfigureTarget(opaqueResult);
            ConfigureClear(ClearFlag.All, Color.black);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("DiffuseCameras"))) {
                Blit(cmd, cameraColorTarget, opaqueResult, material);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(opaqueResult.name));
        }
    }

    private class DiffuseTransparentPass: ScriptableRenderPass {

        private readonly Material material;
        private RenderTargetIdentifier temporaryBuffer;


        private RTHandle finalResult;

        public DiffuseTransparentPass(RenderPassEvent renderPassEvent) {
            this.renderPassEvent = renderPassEvent;
            material = new Material(Shader.Find("Shader Graphs/DiffuseTransparent"));
            finalResult = RTHandles.Alloc("_FinalResult", name: "_FinalResult");
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

            cmd.GetTemporaryRT(Shader.PropertyToID(finalResult.name), cameraTextureDescriptor, FilterMode.Point);

            ConfigureTarget(finalResult);
            ConfigureClear(ClearFlag.All, Color.white);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("DiffuseTransparent"))) {
                Blit(cmd, temporaryBuffer, finalResult, material);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(finalResult.name));
        }
    }


    private DiffuseCamerasPass diffusePass;
    private DiffuseTransparentPass diffuseTransparentPass;

    public override void Create() {
        diffusePass = new DiffuseCamerasPass(RenderPassEvent.AfterRendering);
        diffuseTransparentPass = new DiffuseTransparentPass(RenderPassEvent.AfterRendering);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(diffusePass);
        renderer.EnqueuePass(diffuseTransparentPass);
    }
}
