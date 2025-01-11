using Cysharp.Threading.Tasks;
using MoonFramework.Template;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoonFramework.Test
{
	public class MapCreator
	{
		private readonly int mapSize;        // 一行或者一列有多少个地图块
		private readonly int mapChunkSize;   // 一个地图块有多少个格子
		private readonly float cellSize;     // 一个格子多少米

		private readonly float noiseLacunarity;  // 噪音间隙
		private readonly int mapSeed;            // 地图种子
		private readonly int spawnSeed;          // 随时地图对象的种子
		private readonly float marshLimit;       // 沼泽的边界
		private MapGrid mapGrid;				// 地图逻辑网格、顶点数据
		private readonly Material mapMaterial;
		private Material marshMaterial;
		private Mesh chunkMesh;

		private readonly Texture2D[] marshTextures;
		private readonly Texture2D forestTexutre;
		private readonly Texture2D[] mapTexture = new Texture2D[16];
		private readonly Dictionary<MapVertexType, List<int>> spawnConfigs;
		private readonly Dictionary<int, int>[] RandValues = new Dictionary<int, int>[2];
		private readonly List<int> configIDSetForest;
		private readonly List<int> configIDSetMarsh;

		private int forestSpawnWeightTotal;
		private int marshSpawnWeightTotal;

		public MapCreator(int mapSize, int mapChunkSize, float cellSize, float noiseLacunarity, int mapSeed, int spawnSeed, float marshLimit, Material mapMaterial, Texture2D forestTexutre, Texture2D[] marshTextures, Dictionary<MapVertexType, List<int>> spawnConfigs)
		{
			this.mapSize = mapSize;
			this.mapChunkSize = mapChunkSize;
			this.cellSize = cellSize;
			this.noiseLacunarity = noiseLacunarity;
			this.mapSeed = mapSeed;
			this.spawnSeed = spawnSeed;
			this.marshLimit = marshLimit;
			this.mapMaterial = mapMaterial;
			this.forestTexutre = forestTexutre;
			this.marshTextures = marshTextures;
			this.spawnConfigs = spawnConfigs;
			configIDSetForest = new(spawnConfigs[MapVertexType.Forest]);
			configIDSetMarsh = new(spawnConfigs[MapVertexType.Marsh]);
			RandValues[0] = new();
			RandValues[1] = new();
			PreLoadTexture();
		}

		private async void PreLoadTexture()
		{
			for (int i = 0; i < mapTexture.Length; i++)
			{
				mapTexture[i] = await this.MoonGameObjGetPool($"Map/CellTexture_{i}") as Texture2D;
			}
		}

		public void CalConfigID()
		{
			configIDSetForest[0] = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configIDSetForest[0]).Probability;
			configIDSetMarsh[0] = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configIDSetMarsh[0]).Probability;

			for (int i = 1; i < Math.Max(configIDSetForest.Count, configIDSetMarsh.Count); i++)
			{
				if (i < configIDSetForest.Count)
				{
					int probability = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configIDSetForest[i]).Probability;
					configIDSetForest[i] = configIDSetForest[i - 1] + Math.Max(probability, 0);
				}

				if (i < configIDSetMarsh.Count)
				{
					int probability = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configIDSetMarsh[i]).Probability;
					configIDSetMarsh[i] = configIDSetMarsh[i - 1] + Math.Max(probability, 0);
				}
			}
		}

		public void CreateMapData()
		{
			//生成噪声图
			float[,] noiseMap = CreateNosieMap(mapSize * mapChunkSize, mapSize * mapChunkSize, noiseLacunarity);
			//应用地形种子
			Random.InitState(mapSeed);
			mapGrid = new MapGrid(mapSize * mapChunkSize, mapSize * mapChunkSize, cellSize);

			//确定网格 格子贴图索引
			mapGrid.CalculateMapVertexType(noiseMap, marshLimit);

			//初始化网格默认材质
			mapMaterial.mainTexture = forestTexutre;
			mapMaterial.SetTextureScale("_MainTex", cellSize * mapChunkSize * Vector2.one);

			// 实例化一个沼泽材质
			marshMaterial = new Material(mapMaterial);
			marshMaterial.SetTextureScale("_MainTex", Vector2.one);

			//使用随机种子来生成物品
			Random.InitState(spawnSeed);

			var tempList = spawnConfigs[MapVertexType.Forest];
			foreach (var item in tempList) forestSpawnWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, item).Probability;
			tempList = spawnConfigs[MapVertexType.Marsh];
			foreach (var item in tempList) marshSpawnWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, item).Probability;
			CalConfigID();
		}

		/// <summary>
		///生成地图块
		/// </summary>
		public BaseMapChunk CreateMapChunk(Vector2Int chunkIndex, Transform parent, Action callbackForMapTexture)
		{
			// 生成地图块物体
			GameObject mapChunkObj = new("Chunk_" + chunkIndex.ToString());
			BaseMapChunk mapChunk = mapChunkObj.AddComponent<BaseMapChunk>();

			if (chunkMesh == null)
				chunkMesh = this.MoonObjGetPool(() => CreateMesh(mapChunkSize, mapChunkSize, cellSize), nameof(Mesh));

			//生成Mesh
			mapChunkObj.AddComponent<MeshFilter>().mesh = chunkMesh;
			mapChunkObj.AddComponent<MeshCollider>();

			//生成地形块贴图
			Texture2D mapTexture;

			CreateMapTexture(chunkIndex, (tex, isAllForest) =>
			{
				// 如果完全是森林，没必要在实例化一个材质球
				if (isAllForest)
				{
					mapChunkObj.AddComponent<MeshRenderer>().sharedMaterial = mapMaterial;
				}
				else
				{
					mapTexture = tex;
					Material material = new(marshMaterial)
					{
						mainTexture = tex
					};
					mapChunkObj.AddComponent<MeshRenderer>().material = material;
				}
				callbackForMapTexture?.Invoke();
				// 确定坐标
				Vector3 position = mapChunkSize * cellSize * (Vector3.right * chunkIndex.x + Vector3.forward * chunkIndex.y);
				mapChunk.transform.position = position;
				mapChunkObj.transform.SetParent(parent);
				//生成场景配置
				List<MapChunkObjectModel> mapConfigModels = SpawnMapObject(chunkIndex);
				//初始化地图块
				mapChunk.Init(chunkIndex, position + ((mapChunkSize * cellSize) / 2) * (Vector3.right + Vector3.forward), isAllForest, mapConfigModels);
			});

			return mapChunk;
		}

		/// <summary>
		/// 生成地形渲染器Mesh
		/// </summary>
		/// <param name="mapHeight"></param>
		/// <param name="mapWidth"></param>
		/// <returns></returns>
		public Mesh CreateMesh(int mapHeight, int mapWidth, float cellSize)
		{
			Mesh mesh = new()
			{
				//确定顶点在哪
				vertices = new Vector3[]
				{
					Vector3.zero,
					cellSize * mapHeight * Vector3.forward,
					cellSize * (Vector3.forward * mapHeight + Vector3.right * mapWidth),
					cellSize * mapWidth * Vector3.right
				},

				//确定那些点构成三角形
				triangles = new int[]
				{
					0,1,2,
					0,2,3
				},

				uv = new Vector2[]
				{
					Vector2.zero,
					Vector2.up,
					Vector2.one,
					Vector2.right
				}
			};

			//法线信息
			mesh.RecalculateNormals();
			return mesh;
		}

		/// <summary>
		/// 生成噪音图(基于柏林噪音)
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="lacunarity"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private float[,] CreateNosieMap(int width, int height, float lacunarity)
		{
			lacunarity += 0.1f;

			//顶点噪音图
			float[,] nosieMap = new float[width - 1, height - 1];

			float offsetX = Random.Range(-1E5f, 1E5f);
			float offsetY = Random.Range(-1E5f, 1E5f);

			for (int x = 0; x < width - 1; x++)
			{
				for (int y = 0; y < height - 1; y++)
				{
					nosieMap[x, y] = Mathf.PerlinNoise(x * lacunarity + offsetX, y * lacunarity + offsetY);
				}
			}

			return nosieMap;
		}

		/// <summary>
		/// 地形贴图
		/// </summary>
		/// <param name="cellIndexMap">数值图</param>
		/// <param name="groundTexture">草地</param>
		/// <param name="marshTextures">沼泽</param>
		/// <returns></returns>
		private void CreateMapTexture(Vector2Int chunkIndex, Action<Texture2D, bool> callback)
		{
			bool isAllForest = true;
			int cellOffsetX = chunkIndex.x * mapChunkSize + 1;
			int cellOffsetY = chunkIndex.y * mapChunkSize + 1;

			// 检查是否只有森林类型的格子
			for (int y = 0; y < mapChunkSize; y++)
			{
				if (!isAllForest) break;
				for (int x = 0; x < mapChunkSize; x++)
				{
					MapCell cell = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY);
					if (cell != null && cell.textureIndex != 0)
					{
						isAllForest = false;
						break;
					}
				}
			}

			Texture2D mapTexture = null;
			if (!isAllForest)
			{
				int textureCellSize = forestTexutre.width; // 单元格纹理大小（假设所有单元格纹理大小一致）
				int textureSize = mapChunkSize * textureCellSize; // 大纹理的尺寸

				// 创建目标大纹理（注意：TextureFormat 与源纹理一致）
				mapTexture = this.MoonObjGetPool(() => new Texture2D(textureSize, textureSize, TextureFormat.DXT1, false), nameof(Texture2D));

				// 遍历每个格子，将预制好的单元格纹理“复制”到大纹理的对应位置
				for (int y = 0; y < mapChunkSize; y++)
				{
					for (int x = 0; x < mapChunkSize; x++)
					{
						int textureIndex = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY).textureIndex - 1;

						Texture2D cellTexture = (textureIndex < 0 || textureIndex >= this.mapTexture.Length)
							? this.mapTexture[0] // 默认森林纹理
							: this.mapTexture[textureIndex + 1];

						// 计算目标位置
						int targetX = x * textureCellSize;
						int targetY = y * textureCellSize;

						// 复制整块纹理到目标纹理
						Graphics.CopyTexture(cellTexture, 0, 0, 0, 0, textureCellSize, textureCellSize, mapTexture, 0, 0, targetX, targetY);
					}
				}
			}

			callback?.Invoke(mapTexture, isAllForest);
		}

		public int Lower_Bound(in List<int> t, int value)
		{
			int l = 0, r = t.Count - 1, mid;

			while (l < r)
			{
				mid = (l + r + 1) >> 1;
				if (t[mid] > value) r = mid - 1;
				else l = mid;
			}
			int res = t[r];

			l = 0;
			while (l < r)
			{
				mid = (l + r) >> 1;
				if (t[mid] >= res) r = mid;
				else l = mid + 1;
			}
			return Math.Clamp(l, 0, t.Count - 1);
		}

		private List<MapChunkObjectModel> SpawnMapObject(Vector2Int chunkIndex)
		{
			Random.InitState(spawnSeed);
			List<MapChunkObjectModel> mapObjectModels = new();

			int offsetX = chunkIndex.x * mapChunkSize;
			int offsetY = chunkIndex.y * mapChunkSize;

			for (int x = 1; x < mapChunkSize; x++)
			{
				for (int y = 1; y < mapChunkSize; y++)
				{
					MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);
					int Total = mapVertex.vertexType == MapVertexType.Forest ? forestSpawnWeightTotal : marshSpawnWeightTotal;

					int randValue = Random.Range(1, Total + 1);
					int spawnConfigIndex;

					if (RandValues[(int)mapVertex.vertexType].TryGetValue(randValue, out var index))
					{
						spawnConfigIndex = index;
					}
					else
					{
						if (mapVertex.vertexType == MapVertexType.Forest)
							spawnConfigIndex = Lower_Bound(in configIDSetForest, randValue);
						else
							spawnConfigIndex = Lower_Bound(in configIDSetMarsh, randValue);

						RandValues[(int)mapVertex.vertexType].TryAdd(randValue, spawnConfigIndex);
					}

					var configID = spawnConfigs[mapVertex.vertexType][spawnConfigIndex];
					var spawnConfigModel = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.MapObject, configID);

					if (!spawnConfigModel.isEmpty)
					{
						Vector3 pos = mapVertex.position + new Vector3(Random.Range(-cellSize / 2, cellSize / 2), 0f, Random.Range(-cellSize / 2, cellSize / 2));
						mapObjectModels.Add(new() { configID = configID, position = pos });
					}
				}
			}

			return mapObjectModels;
		}
	}
}