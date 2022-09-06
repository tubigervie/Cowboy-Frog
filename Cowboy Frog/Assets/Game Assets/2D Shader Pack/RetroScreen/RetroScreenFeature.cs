using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ShaderPack2D
{
	public class RetroScreenFeature : ScriptableRendererFeature
	{
		public class CustomPass : ScriptableRenderPass
		{
			Material m_Mat;
			RenderTargetIdentifier m_Source;
			RenderTargetHandle m_TempColorTexture;
			string m_ProfilerTag;

			public CustomPass(string tag)
			{
				this.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
				m_ProfilerTag = tag;
				m_TempColorTexture.Init("_TemporaryColorTexture");
			}
			public void Setup(RenderTargetIdentifier source, Material mat)
			{
				m_Source = source;
				m_Mat = mat;
			}
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
				opaqueDesc.depthBufferBits = 0;

				CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
				cmd.GetTemporaryRT(m_TempColorTexture.id, opaqueDesc, FilterMode.Bilinear);

				Blit(cmd, m_Source, m_TempColorTexture.Identifier(), m_Mat, 0);
				Blit(cmd, m_TempColorTexture.Identifier(), m_Source);

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			public override void FrameCleanup(CommandBuffer cmd)
			{
				cmd.ReleaseTemporaryRT(m_TempColorTexture.id);
			}
		}
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public Material m_Mat;
		CustomPass m_Pass;

		public override void Create()
		{
			m_Pass = new CustomPass(name);
		}
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			RenderTargetIdentifier src = renderer.cameraColorTarget;
			if (m_Mat == null)
			{
				Debug.LogWarningFormat("Missing material. {0} pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}
			m_Pass.Setup(src, m_Mat);
			renderer.EnqueuePass(m_Pass);
		}
	}
}