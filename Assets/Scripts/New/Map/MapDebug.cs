using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MapSearch
{
	public class MapDebug : MonoBehaviour
	{

        // 一维Byte数组
        [SerializeField] Chunk chunk;
        float cellSize = 1f;

        [SerializeField] bool isDrawChunk = false;

        private void OnDrawGizmos()
        {
            if (isDrawChunk)
            {
                Byte[] data = chunk.grids;
                Vector3 startPos = MapManager.Instance().Chunk2WorldPos(chunk.chunkIndex);
                // 遍历每一个数据点并根据其值设定颜色
                for (int i = 0; i < Chunk.ChunkEdgeLength; i++)
                {
                    for (int j = 0; j < Chunk.ChunkEdgeLength; j++)
                    {
                        // 从一维数组中获取对应的数据值
                        byte value = data[i * Chunk.ChunkEdgeLength + j];

                        // 将Byte值映射到0到1之间，用来控制颜色的深度
                        float normalizedValue = value / 255f;

                        // 设置颜色，根据数据值的深浅
                        Gizmos.color = new Color(normalizedValue, normalizedValue, normalizedValue);
                        Vector3 position = startPos + new Vector3(j * cellSize, i * cellSize, 0);
                        Gizmos.DrawCube(position, new Vector3(cellSize, cellSize,0.1f));
                    }
                }
            }
                

           
        }
    }
}