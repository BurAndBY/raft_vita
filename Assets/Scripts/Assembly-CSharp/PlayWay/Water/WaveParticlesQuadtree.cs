using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace PlayWay.Water
{
	public sealed class WaveParticlesQuadtree : Quadtree<WaveParticle>
	{
		private Mesh mesh;

		private Vector3[] vertices;

		private Vector4[] tangentsPack;

		private WaveParticlesGroup[] particleGroups;

		private int numParticleGroups;

		private int lastGroupIndex = -1;

		private float stress = 1f;

		private bool tangentsPackChanged;

		private Stopwatch stopwatch;

		private int lastUpdateIndex;

		private new WaveParticlesQuadtree a;

		private new WaveParticlesQuadtree b;

		private new WaveParticlesQuadtree c;

		private new WaveParticlesQuadtree d;

		private new WaveParticlesQuadtree root;

		public WaveParticlesQuadtree(Rect rect, int maxElementsPerNode, int maxTotalElements)
			: base(rect, maxElementsPerNode, maxTotalElements)
		{
			root = this;
			particleGroups = new WaveParticlesGroup[maxElementsPerNode >> 3];
			CreateMesh();
		}

		private WaveParticlesQuadtree(WaveParticlesQuadtree root, Rect rect, int maxElementsPerNode)
			: this(rect, maxElementsPerNode, 0)
		{
			this.root = root;
		}

		public void Render(Rect renderRect)
		{
			if (!rect.Overlaps(renderRect))
			{
				return;
			}
			if (a != null)
			{
				a.Render(renderRect);
				b.Render(renderRect);
				c.Render(renderRect);
				d.Render(renderRect);
			}
			else if (numElements != 0)
			{
				mesh.vertices = vertices;
				if (tangentsPackChanged)
				{
					mesh.tangents = tangentsPack;
					tangentsPackChanged = false;
				}
				Graphics.DrawMeshNow(mesh, Matrix4x4.identity, 0);
			}
		}

		public void UpdateSimulation(float time, float maxExecutionTimeExp)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (stopwatch == null)
			{
				stopwatch = new Stopwatch();
			}
			stopwatch.Reset();
			stopwatch.Start();
			UpdateSimulation(time);
			float num = (float)stopwatch.ElapsedTicks / 10000f;
			if (num > 50f)
			{
				num = 50f;
			}
			stress = stress * 0.98f + (Mathf.Exp(num) - maxExecutionTimeExp) * 0.04f;
			if (stress < 1f)
			{
				stress = 1f;
			}
			if (!(stress < 20f))
			{
				stress = 20f;
			}
		}

		public void UpdateSimulation(float time)
		{
			if (a != null)
			{
				a.UpdateSimulation(time);
				b.UpdateSimulation(time);
				c.UpdateSimulation(time);
				d.UpdateSimulation(time);
			}
			else if (numElements != 0)
			{
				UpdateParticles(time);
			}
		}

		private void UpdateParticles(float time)
		{
			List<WaterCamera> enabledWaterCameras = WaterCamera.EnabledWaterCameras;
			int count = enabledWaterCameras.Count;
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				if (rect.Overlaps(enabledWaterCameras[i].LocalMapsRect))
				{
					flag = true;
					break;
				}
			}
			int num;
			int num2;
			int num3;
			if (!flag)
			{
				num = lastUpdateIndex;
				num2 = lastUpdateIndex + 8;
				num3 = num << 2;
				if (num2 >= elements.Length)
				{
					num2 = elements.Length;
					lastUpdateIndex = 0;
				}
				else
				{
					lastUpdateIndex = num2;
				}
			}
			else
			{
				num = 0;
				num2 = elements.Length;
				num3 = 0;
			}
			WaveParticlesQuadtree quadtree = ((!flag) ? null : root);
			float num4 = ((!flag) ? 1.5f : 0.01f);
			float num5 = ((!flag) ? 8f : 0.4f);
			bool flag2 = false;
			num4 *= root.stress;
			num5 *= root.stress;
			for (int j = 0; particleGroups != null && j < particleGroups.Length; j++)
			{
				WaveParticlesGroup waveParticlesGroup = particleGroups[j];
				if (waveParticlesGroup == null)
				{
					continue;
				}
				if (!waveParticlesGroup.leftParticle.isAlive)
				{
					numParticleGroups--;
					particleGroups[j] = null;
				}
				else
				{
					if (!(time >= waveParticlesGroup.lastUpdateTime + num4))
					{
						continue;
					}
					if (time >= waveParticlesGroup.lastCostlyUpdateTime + num5 && !flag2)
					{
						if (!RectContainsParticleGroup(waveParticlesGroup))
						{
							numParticleGroups--;
							particleGroups[j] = null;
							continue;
						}
						waveParticlesGroup.CostlyUpdate(quadtree, time);
						flag2 = true;
					}
					waveParticlesGroup.Update(time);
				}
			}
			if (elements == null)
			{
				return;
			}
			for (int k = num; k < num2; k++)
			{
				WaveParticle waveParticle = elements[k];
				if (waveParticle != null)
				{
					if (waveParticle.isAlive)
					{
						if (marginRect.Contains(waveParticle.position))
						{
							Vector3 vertexData = waveParticle.VertexData;
							Vector4 packedParticleData = waveParticle.PackedParticleData;
							vertices[num3] = vertexData;
							tangentsPack[num3++] = packedParticleData;
							vertices[num3] = vertexData;
							tangentsPack[num3++] = packedParticleData;
							vertices[num3] = vertexData;
							tangentsPack[num3++] = packedParticleData;
							vertices[num3] = vertexData;
							tangentsPack[num3++] = packedParticleData;
							tangentsPackChanged = true;
						}
						else
						{
							base.RemoveElementAt(k);
							vertices[num3++].x = float.NaN;
							vertices[num3++].x = float.NaN;
							vertices[num3++].x = float.NaN;
							vertices[num3++].x = float.NaN;
							root.AddElement(waveParticle);
						}
					}
					else
					{
						base.RemoveElementAt(k);
						vertices[num3++].x = float.NaN;
						vertices[num3++].x = float.NaN;
						vertices[num3++].x = float.NaN;
						vertices[num3++].x = float.NaN;
						waveParticle.AddToCache();
					}
				}
				else
				{
					num3 += 4;
				}
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			if (mesh != null)
			{
				mesh.Destroy();
				mesh = null;
			}
			vertices = null;
			tangentsPack = null;
		}

		private bool HasParticleGroup(WaveParticlesGroup group)
		{
			for (int i = 0; i < particleGroups.Length; i++)
			{
				if (particleGroups[i] == group)
				{
					return true;
				}
			}
			return false;
		}

		private void AddParticleGroup(WaveParticlesGroup group)
		{
			if (particleGroups.Length == numParticleGroups)
			{
				Array.Resize(ref particleGroups, numParticleGroups << 1);
			}
			lastGroupIndex++;
			while (lastGroupIndex < particleGroups.Length)
			{
				if (particleGroups[lastGroupIndex] == null)
				{
					numParticleGroups++;
					particleGroups[lastGroupIndex] = group;
					return;
				}
				lastGroupIndex++;
			}
			for (lastGroupIndex = 0; lastGroupIndex < particleGroups.Length; lastGroupIndex++)
			{
				if (particleGroups[lastGroupIndex] == null)
				{
					numParticleGroups++;
					particleGroups[lastGroupIndex] = group;
					break;
				}
			}
		}

		private bool RectContainsParticleGroup(WaveParticlesGroup group)
		{
			WaveParticle waveParticle = group.leftParticle;
			if (!waveParticle.isAlive)
			{
				return false;
			}
			do
			{
				if (marginRect.Contains(waveParticle.position))
				{
					return true;
				}
				waveParticle = waveParticle.rightNeighbour;
			}
			while (waveParticle != null);
			return false;
		}

		protected override void AddElementAt(WaveParticle particle, int index)
		{
			base.AddElementAt(particle, index);
			if (!HasParticleGroup(particle.group))
			{
				AddParticleGroup(particle.group);
			}
		}

		protected override void RemoveElementAt(int index)
		{
			base.RemoveElementAt(index);
			int num = index << 2;
			vertices[num++].x = float.NaN;
			vertices[num++].x = float.NaN;
			vertices[num++].x = float.NaN;
			vertices[num].x = float.NaN;
		}

		protected override void SpawnChildNodes()
		{
			mesh.Destroy();
			mesh = null;
			float width = rect.width * 0.5f;
			float height = rect.height * 0.5f;
			base.a = (a = new WaveParticlesQuadtree(root, new Rect(rect.xMin, center.y, width, height), elements.Length));
			base.b = (b = new WaveParticlesQuadtree(root, new Rect(center.x, center.y, width, height), elements.Length));
			base.c = (c = new WaveParticlesQuadtree(root, new Rect(rect.xMin, rect.yMin, width, height), elements.Length));
			base.d = (d = new WaveParticlesQuadtree(root, new Rect(center.x, rect.yMin, width, height), elements.Length));
			vertices = null;
			tangentsPack = null;
			particleGroups = null;
			numParticleGroups = 0;
		}

		private void CreateMesh()
		{
			int num = elements.Length << 2;
			vertices = new Vector3[num];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].x = float.NaN;
			}
			tangentsPack = new Vector4[num];
			Vector2[] array = new Vector2[num];
			int num2 = 0;
			while (num2 < array.Length)
			{
				array[num2++] = new Vector2(0f, 0f);
				array[num2++] = new Vector2(0f, 1f);
				array[num2++] = new Vector2(1f, 1f);
				array[num2++] = new Vector2(1f, 0f);
			}
			int[] array2 = new int[num];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = j;
			}
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			mesh.name = "Wave Particles";
			mesh.vertices = vertices;
			mesh.uv = array;
			mesh.tangents = tangentsPack;
			mesh.SetIndices(array2, MeshTopology.Quads, 0);
		}
	}
}
