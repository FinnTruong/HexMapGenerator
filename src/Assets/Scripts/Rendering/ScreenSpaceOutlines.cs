using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {

    }


    [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings;
    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private readonly RenderTargetHandle normals;
        public ViewSpaceNormalsTexturePass()
        {
            this.renderPassEvent = renderPassEvent;
            normals.Init("_SceneViewSpaceNormal");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(normals.id, cameraTextureDescriptor, FilterMode.Point);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
        }

    }

    private class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            throw new System.NotImplementedException();
        }
    }



    [SerializeField] private RenderPassEvent renderPassEvent;

    private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinePass screenSpaceOutlinePass;

    public override void Create()
    {
        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass();
        screenSpaceOutlinePass = new ScreenSpaceOutlinePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinePass);
    }


}
