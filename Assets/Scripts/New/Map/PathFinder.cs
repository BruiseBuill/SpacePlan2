using BF.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;
using System.Threading.Tasks;

namespace MapSearch
{
	public class PathFinder :Single<PathFinder>
	{
		byte searchCounter = 0;
		const int MaxThreadCount = 10;
		int usedThreadCount = 0;
        List<Vector2Int> searchDirection = new List<Vector2Int>()
		{
			Vector2Int.up,
			Vector2Int.down,
			Vector2Int.left,
			Vector2Int.right,
			Vector2Int.one,
			-Vector2Int.one,
			new Vector2Int(1,-1),
			new Vector2Int(-1,1),
        };

		class PathGrid
		{
			public FullGrid fullGrid;
			public PathGrid lastPathGrid;
			public int totalUsedCost;

			public PathGrid(FullGrid fullGrid, PathGrid lastPathGrid = null, int totalUsedCost = 0)
			{
				this.fullGrid = fullGrid;
				this.lastPathGrid = lastPathGrid;
				this.totalUsedCost = totalUsedCost;
            }
			public bool IsSame(PathGrid pathGrid)
			{
				return fullGrid.chunk.chunkIndex == pathGrid.fullGrid.chunk.chunkIndex && fullGrid.grid == pathGrid.fullGrid.grid;
			}
        }
		enum FailCode {DirectLine=0,Astar=1 }



		public async void FindPath(FullGrid start, FullGrid end, ref Action<List<FullGrid>> onAskFindPath)
		{
            List<FullGrid> path = await Task.Run(() => FindProcess(start, end));
            onAskFindPath.Invoke(FindProcess(start, end));
        }
		List<FullGrid> FindProcess(FullGrid start, FullGrid end)
		{
            FindPathDirectLine(start, end);
        }
        void FindPathDirectLinePhysics(FullGrid start, FullGrid end)
		{
			var worldPos_start = MapManager.Instance().FullGrid2WorldPos(start);
			var worldPos_end = MapManager.Instance().FullGrid2WorldPos(end);

			var direction = (worldPos_end - worldPos_start).normalized;
			var distance = Vector3.Distance(worldPos_start, worldPos_end);
			var col = Physics2D.Raycast(worldPos_start, direction, distance);
			if (col)
			{
                FindPathFail(FailCode.DirectLine,start,end);
            }

		}
		void FindPathDirectLine(FullGrid start, FullGrid end)
		{
			var startPos = start.chunk.chunkIndex * Chunk.ChunkEdgeLength + start.grid;
			var endPos = end.chunk.chunkIndex * Chunk.ChunkEdgeLength + end.grid;

			var list = GetLineCells(startPos, endPos);
			var inter = new FullGrid(start.grid, start.chunk);
			for (int i = 0; i < list.Count - 1; i++) 
			{
				MapManager.Instance().FullGridMove(inter, list[i + 1] - list[i]);
                if (inter.chunk.costs[Chunk.Get1DGridIndex(inter.grid)] > 0b11111110) 
				{
					FindPathFail(FailCode.DirectLine, start, end);
					return;
                }
			}
			FindPathSuccess(new List<FullGrid>() { end });
        }
        List<Vector2Int> GetLineCells(Vector2Int a, Vector2Int b)
        {
            List<Vector2Int> result = new List<Vector2Int>();

            int x0 = a.x;
            int y0 = a.y;
            int x1 = b.x;
            int y1 = b.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int err = dx - dy;

            int x = x0;
            int y = y0;

            result.Add(new Vector2Int(x, y));

            while (x != x1 || y != y1)
            {
                int e2 = err << 1; // err * 2

                // 注意：不是 else if
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                    result.Add(new Vector2Int(x, y));
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                    result.Add(new Vector2Int(x, y));
                }
            }

            return result;
        }

        void FindPathAStar(FullGrid start, FullGrid end)
		{
			PriorityList<PathGrid> openList = new PriorityList<PathGrid>();
			List<PathGrid> closeList = new List<PathGrid>();

            start.chunk.pathFindCounter[Chunk.Get1DGridIndex(start.grid)] = searchCounter;
            openList.Add(new PathGrid(start, null), 0);

			PathGrid target = new PathGrid(end);
			while (openList.Count > 0) 
			{
				PathGrid present = openList[0];
				openList.RemoveFirst();
                if (present.IsSame(target))
				{
                    CutPath(present);
					return;
				}
				for(int i = 0; i < searchDirection.Count; i++)
				{
					var neighborGrid = MapManager.Instance().FullGridOffset(present.fullGrid, searchDirection[i]);
					if (neighborGrid.chunk.pathFindCounter[Chunk.Get1DGridIndex(neighborGrid.grid)] == searchCounter) 
					{
						continue;
					}
					neighborGrid.chunk.pathFindCounter[Chunk.Get1DGridIndex(neighborGrid.grid)] = searchCounter;

                    int newCost = present.totalUsedCost + neighborGrid.chunk.costs[Chunk.Get1DGridIndex(neighborGrid.grid)];
					openList.Add(new PathGrid(neighborGrid, present, newCost), newCost + ExpectFunc(neighborGrid, end));
                }
			}
			FindPathFail(FailCode.Astar, start, end);
        }
		void FindPathSuccess(List<FullGrid> list)
		{

		}
		void FindPathFail(FailCode failCode, FullGrid start, FullGrid end)
		{
			switch (failCode)
			{
				case FailCode.DirectLine:
					FindPathAStar(start,end);
					break;
				case FailCode.Astar:
					
					break;
            }
		}
        int ExpectFunc(FullGrid start,FullGrid end)
		{
			Vector2Int a = start.chunk.chunkIndex * Chunk.ChunkEdgeLength + start.grid;
			Vector2Int b = end.chunk.chunkIndex * Chunk.ChunkEdgeLength + end.grid;
			Vector2Int c = b - a;
			return Mathf.Abs(c.x) + Mathf.Abs(c.y);
		}
        void CutPath(PathGrid target)
        {
			var current = target;
			var pathList = new List<FullGrid>();
			while (current.lastPathGrid != null) 
			{
				pathList.Add(current.fullGrid);
				current = current.lastPathGrid;
            }
            //共线点删除
            int index = pathList.Count - 1;
            while (pathList.Count > 0) 
			{
				var a = pathList[index] - pathList[index - 1];
				var b = pathList[index - 1] - pathList[index - 2];
                if (a == b)
				{
					pathList.RemoveAt(index - 1);
                }
				index--;
            }
			FindPathSuccess(pathList);
        }
    }
}