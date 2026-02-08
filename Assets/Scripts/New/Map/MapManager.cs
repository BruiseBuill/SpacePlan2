using BF;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSearch
{
	public class MapManager : Single<MapManager>
	{
		[SerializeField] List<Chunk> chunkList;
        Dictionary<Vector2Int, Chunk> chunkDic = new Dictionary<Vector2Int, Chunk>();

		[SerializeField] Vector2Int chunkMapSize;

		[ContextMenu("CreateMap")]
		public void CreateMap()
		{
			int chunkIndex = 0;
			if (chunkDic.Count > 0)
				chunkDic.Clear();
			for (int i = -chunkMapSize.x / 2; i < chunkMapSize.x / 2; i++)
			{
				for (int j = -chunkMapSize.y / 2; j < chunkMapSize.y / 2; j++)
				{
					Chunk chunk = chunkList[chunkIndex++];
					chunk.chunkIndex = new Vector2Int(i, j);
					chunk.grids = new byte[Chunk.ChunkEdgeLength * Chunk.ChunkEdgeLength];
					chunk.costs = new byte[Chunk.ChunkEdgeLength * Chunk.ChunkEdgeLength];
					CreateOneChunkFromWorldData(chunk);
					chunkDic.Add(chunk.chunkIndex, chunk);
				}
            }
        }
		void CreateOneChunkFromWorldData(Chunk chunk)
		{
			Vector3 start = Chunk2WorldPos(chunk.chunkIndex);

			for (int i = 0; i < Chunk.ChunkEdgeLength; i++)
			{
				for (int j = 0; j < Chunk.ChunkEdgeLength; j++)
				{
					Vector3 worldPos = start + new Vector3(i, j, 0);
					//byte gridData = WorldDataManager.Instance.GetGridData(worldPos);
					//chunk.grids[i + j * Chunk.ChunkEdgeLength] = gridData;
					var col = Physics2D.OverlapPoint(worldPos);
					if (col == null)
					{
						chunk.grids[i + j * Chunk.ChunkEdgeLength] = 0b0;
						chunk.costs[i + j * Chunk.ChunkEdgeLength] = Chunk.DefaultGridCost;
                    }
					else
					{
						chunk.grids[i + j * Chunk.ChunkEdgeLength] = 0b11000000;
                        chunk.costs[i + j * Chunk.ChunkEdgeLength] = Chunk.DefaultObstacleCost;
                    }
                }
            }
		}
		public void RefreshGrid(Vector2Int start, Vector2Int end)
		{
			int x1 = start.x / Chunk.ChunkEdgeLength;
			int y1 = start.y / Chunk.ChunkEdgeLength;
			int x2 = end.x / Chunk.ChunkEdgeLength;
			int y2 = end.y / Chunk.ChunkEdgeLength;
			for (int i = x1; i <= x2; i++)
			{
				for (int j = y1; j <= y2; j++)
				{
					var chunk = ChunkPos2Chunk(i, j);
					if (chunk != null)
					{
						CreateOneChunkFromWorldData(chunk);
                    }
                }
			}
        }

		public FullGrid FullGridOffset(FullGrid start,Vector2Int offset)
		{
			Vector2Int a = (start.grid + offset + Vector2Int.one * Chunk.ChunkEdgeLength) / Chunk.ChunkEdgeLength - Vector2Int.one;
			if (a == Vector2Int.zero)
			{
				return new FullGrid(start.grid + offset, start.chunk);
			}
			else
			{
				return new FullGrid(start.grid + offset - a * Chunk.ChunkEdgeLength, ChunkPos2Chunk(start.chunk.chunkIndex + a));
			}
		}
        public void FullGridMove(FullGrid start, Vector2Int offset)
        {
            Vector2Int a = (start.grid + offset + Vector2Int.one * Chunk.ChunkEdgeLength) / Chunk.ChunkEdgeLength - Vector2Int.one;
            if (a == Vector2Int.zero)
            {
				start.grid += offset;
            }
            else
            {
				start.grid += offset - a * Chunk.ChunkEdgeLength;
				start.chunk = ChunkPos2Chunk(start.chunk.chunkIndex + a);
            }
        }
        Chunk ChunkPos2Chunk(Vector2Int index)
		{
			return chunkDic[index];
        }
        Chunk ChunkPos2Chunk(int i,int j)
        {
            return chunkDic[new Vector2Int(i, j)];
        }
		/// <param name="worldPos"></param>
		/// <returns>!Return GridPos in Chunk</returns>
		Vector2Int World2GridPos(Vector3 worldPos)
		{
			int a = worldPos.x >= 0 ? Mathf.FloorToInt(worldPos.x) : Mathf.FloorToInt(worldPos.x) - 1;
			int b = worldPos.y >= 0 ? Mathf.FloorToInt(worldPos.y) : Mathf.FloorToInt(worldPos.y) - 1;
			return new Vector2Int(a > 0 ? a % Chunk.ChunkEdgeLength : a % Chunk.ChunkEdgeLength + Chunk.ChunkEdgeLength, b > 0 ? b % Chunk.ChunkEdgeLength : b % Chunk.ChunkEdgeLength + Chunk.ChunkEdgeLength);
        }
		Chunk World2Chunk(Vector3 worldPos)
		{
			int a = worldPos.x >= 0 ? Mathf.FloorToInt(worldPos.x / Chunk.ChunkEdgeLength) : Mathf.FloorToInt((worldPos.x + 1) / Chunk.ChunkEdgeLength) - 1;
			int b = worldPos.y >= 0 ? Mathf.FloorToInt(worldPos.y / Chunk.ChunkEdgeLength) : Mathf.FloorToInt((worldPos.y + 1) / Chunk.ChunkEdgeLength) - 1;
			return ChunkPos2Chunk(a, b);
		}
		public FullGrid World2FullGrid(Vector3 worldPos)
		{
			return new FullGrid(World2GridPos(worldPos), World2Chunk(worldPos));
        }
        public Vector3 Chunk2WorldPos(Vector2Int chunkPos)
        {
            return new Vector3(chunkPos.x * Chunk.ChunkEdgeLength, chunkPos.y * Chunk.ChunkEdgeLength, 0);
        }
		public Vector3 FullGrid2WorldPos(FullGrid fullGrid)
		{
			return Chunk2WorldPos(fullGrid.chunk.chunkIndex) + new Vector3(fullGrid.grid.x, fullGrid.grid.y, 0);
        }
		public bool IfWithinMap(Vector3 pos)
		{
			return pos.x >= -chunkMapSize.x / 2 * Chunk.ChunkEdgeLength &&
				   pos.x < chunkMapSize.x / 2 * Chunk.ChunkEdgeLength &&
				   pos.y >= -chunkMapSize.y / 2 * Chunk.ChunkEdgeLength &&
				   pos.y < chunkMapSize.y / 2 * Chunk.ChunkEdgeLength;
        }
    }
}