using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSearch
{
    [CreateAssetMenu(menuName ="SpacePlan/MapChunk",fileName ="Chunk")]
	public class Chunk:ScriptableObject
	{
        public const int ChunkEdgeLength = 10;
        public const Byte DefaultGridCost = 0b100;
        public const Byte DefaultObstacleCost = 0b11111111;

        public Vector2Int chunkIndex;
        public Byte[] grids;
        public Byte[] costs;
        public Byte[] pathFindCounter;

        public static int Get1DGridIndex(Vector2Int grid)
        {
            return grid.x + grid.y * ChunkEdgeLength;
        }


    }


    public class FullGrid
    {
        public Vector2Int grid;
        public Chunk chunk;

        public FullGrid(Vector2Int grid, Chunk chunk)
        {
            this.grid = grid;
            this.chunk = chunk;
        }
        public static Vector2Int operator -(FullGrid a, FullGrid b)
        {
            return new Vector2Int(a.grid.x + a.chunk.chunkIndex.x * Chunk.ChunkEdgeLength - (b.grid.x + b.chunk.chunkIndex.x * Chunk.ChunkEdgeLength),
                                  a.grid.y + a.chunk.chunkIndex.y * Chunk.ChunkEdgeLength - (b.grid.y + b.chunk.chunkIndex.y * Chunk.ChunkEdgeLength));
        }
    }
}