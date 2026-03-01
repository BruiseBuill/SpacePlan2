using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;
using System;

namespace MapSearch
{
	public class MapDebug : Single<MapDebug>
	{
        float cellSize = 1f;

        [Header("DebugChunkInfo")]
        [SerializeField] bool isDebugChunk = false;
        [SerializeField] List<Chunk> chunkList;
        

        [SerializeField] bool isDrawGrid = false;
        [SerializeField] bool isDrawPathFinderCounter = false;

        [Header("DebugPath")]
        [SerializeField] bool isDebugPath = false;
        List<FullGrid> path;

        public void DebugPath(List<FullGrid> path)
        {
            this.path = path;
        }
        private void OnDrawGizmos()
        {
            if (isDebugChunk)
            {
                for(int k = 0; k < chunkList.Count; k++)
                {
                    Byte[] data = chunkList[k].grids;
                    Byte[] pathCounter = chunkList[k].pathFindCounter;
                    Vector3 startPos = MapManager.Instance().Chunk2WorldPos(chunkList[k].chunkIndex);
                    // 遍历每一个数据点并根据其值设定颜色
                    for (int i = 0; i < Chunk.ChunkEdgeLength; i++)
                    {
                        for (int j = 0; j < Chunk.ChunkEdgeLength; j++)
                        {
                            byte value = 0;
                            // 将Byte值映射到0到1之间，用来控制颜色的深度
                            float normalizedValue = 0f;
                            if (isDrawGrid)
                            {
                                value = data[i * Chunk.ChunkEdgeLength + j];
                                normalizedValue = value / 255f;
                            }
                            else if (isDrawPathFinderCounter)
                            {
                                value = pathCounter[i * Chunk.ChunkEdgeLength + j];
                                normalizedValue = (PathFinder.Instance().searchCounter - value) / 3f;
                            }
                            // 设置颜色，根据数据值的深浅
                            Gizmos.color = new Color(normalizedValue, normalizedValue, normalizedValue, 0.6f);
                            Vector3 position = startPos + new Vector3(j * cellSize, i * cellSize, 0);
                            Gizmos.DrawCube(position, new Vector3(cellSize, cellSize, 0.1f));
                        }
                    }
                }
            }
            if (isDebugPath && path != null) 
            {
                for(int i = 0; i < path.Count; i++)
                {
                    Gizmos.color = new Color(1,1,1, 0.6f);
                    Vector3 position = MapManager.Instance().FullGrid2WorldPos(path[i]);
                    Gizmos.DrawCube(position, new Vector3(cellSize, cellSize, 0.1f));
                }
            }
        }
    }
}