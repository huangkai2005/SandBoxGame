using System.Collections.Generic;
using UnityEngine;

namespace MoonFramework.Test
{
    public enum MapVertexType
    {
        Forest = 0, //森林
        Marsh //沼泽
    }

    //顶点类
    public class MapVertex
    {
        public Vector3 position;
        public MapVertexType vertexType;
    }

    //网格类
    public class MapCell
    {
        public Vector3 position;
        public int textureIndex;
    }

    /// <summary>
    ///     网格，主要包含顶点和网格
    /// </summary>
    public class MapGrid
    {
        public readonly Dictionary<Vector2Int, MapCell> cells = new();

        /// <summary>
        ///     顶点集合
        /// </summary>
        public readonly Dictionary<Vector2Int, MapVertex> vertexs = new();

        //构造函数，初始化地图
        public MapGrid(int mapHeight, int mapWidth, float cellSize)
        {
            MapHeight = mapHeight;
            MapWidth = mapWidth;
            CellSize = cellSize;

            //生成顶点数据
            for (var x = 1; x < mapWidth; x++)
            for (var z = 1; z < mapHeight; z++)
            {
                AddVertex(x, z);
                AddCell(x, z);
            }

            //生成一行一列
            for (var x = 1; x <= mapWidth; x++) AddCell(x, mapHeight);
            for (var z = 1; z < mapHeight; z++) AddCell(mapWidth, z);
        }

        //外部只能请求，不能修改
        public int MapHeight { get; }

        public int MapWidth { get; }
        public float CellSize { get; }

        /// <summary>
        ///     计算格子贴图的索引数字
        /// </summary>
        public void CalculateMapVertexType(float[,] noiseMap, float limit)
        {
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);

            for (var x = 1; x < width; x++)
            for (var z = 1; z < height; z++)
                // 基于噪声中的值确定这个顶点的类型
                // 大于边界是沼泽，否则是森林
                if (noiseMap[x, z] >= limit)
                    SetVertexType(x, z, MapVertexType.Marsh);
                else
                    SetVertexType(x, z, MapVertexType.Forest);
        }

        #region 顶点

        private void AddVertex(int x, int y)
        {
            vertexs.Add(new Vector2Int(x, y)
                , new MapVertex
                {
                    position = new Vector3(x * CellSize, 0, y * CellSize)
                });
        }

        /// <summary>
        ///     获取顶点，如果找不到返回Null
        /// </summary>
        public MapVertex GetVertex(Vector2Int index)
        {
            vertexs.TryGetValue(index, out var vertex);
            return vertex;
        }

        public MapVertex GetVertex(int x, int y)
        {
            return GetVertex(new Vector2Int(x, y));
        }

        /// <summary>
        ///     通过世界坐标获取顶点
        /// </summary>
        public MapVertex GetVertexByWorldPosition(Vector3 position)
        {
            var x = Mathf.Clamp(Mathf.RoundToInt(position.x / CellSize), 1, MapWidth);
            var y = Mathf.Clamp(Mathf.RoundToInt(position.z / CellSize), 1, MapHeight);
            return GetVertex(x, y);
        }

        /// <summary>
        ///     设置顶点类型
        /// </summary>
        private void SetVertexType(Vector2Int vertexIndex, MapVertexType mapVertexType)
        {
            var vertex = GetVertex(vertexIndex);
            if (vertex.vertexType != mapVertexType)
            {
                vertex.vertexType = mapVertexType;
                // 只有沼泽需要计算
                if (vertex.vertexType == MapVertexType.Marsh)
                {
                    // 计算附近的贴图权重

                    var tempCell = GetLeftBottomMapCell(vertexIndex);
                    if (tempCell != null) tempCell.textureIndex += 1;

                    tempCell = GetRightBottomMapCell(vertexIndex);
                    if (tempCell != null) tempCell.textureIndex += 2;

                    tempCell = GetLeftTopMapCell(vertexIndex);
                    if (tempCell != null) tempCell.textureIndex += 4;

                    tempCell = GetRightTopMapCell(vertexIndex);
                    if (tempCell != null) tempCell.textureIndex += 8;
                }
            }
        }

        /// <summary>
        ///     设置顶点类型
        /// </summary>
        private void SetVertexType(int x, int y, MapVertexType mapVertexType)
        {
            SetVertexType(new Vector2Int(x, y), mapVertexType);
        }

        #endregion 顶点

        #region 格子

        private void AddCell(int x, int y)
        {
            var offset = CellSize / 2;
            cells.Add(new Vector2Int(x, y),
                new MapCell
                {
                    position = new Vector3(x * CellSize - offset, 0, y * CellSize - offset)
                }
            );
        }

        /// <summary>
        ///     获取格子，如果没有找到会返回Null
        /// </summary>
        public MapCell GetCell(Vector2Int index)
        {
            cells.TryGetValue(index, out var cell);
            return cell;
        }

        public MapCell GetCell(int x, int y)
        {
            return GetCell(new Vector2Int(x, y));
        }

        /// <summary>
        ///     获取左下角格子
        /// </summary>
        public MapCell GetLeftBottomMapCell(Vector2Int vertexIndex)
        {
            return GetCell(vertexIndex);
        }

        /// <summary>
        ///     获取右下角格子
        /// </summary>
        public MapCell GetRightBottomMapCell(Vector2Int vertexIndex)
        {
            return GetCell(vertexIndex.x + 1, vertexIndex.y);
        }

        /// <summary>
        ///     获取左上角格子
        /// </summary>
        public MapCell GetLeftTopMapCell(Vector2Int vertexIndex)
        {
            return GetCell(vertexIndex.x, vertexIndex.y + 1);
        }

        /// <summary>
        ///     获取右上角格子
        /// </summary>
        public MapCell GetRightTopMapCell(Vector2Int vertexIndex)
        {
            return GetCell(vertexIndex.x + 1, vertexIndex.y + 1);
        }

        #endregion 格子
    }
}