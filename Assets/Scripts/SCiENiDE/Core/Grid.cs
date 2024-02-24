using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class Grid
    {
        public event EventHandler<OnGridCellChangedEventArgs> OnGridCellChanged;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly PathNode[,] _gridArray;
        private readonly Component[,] _cellVisualArray;
        private Vector3 _originPosition;
        private Dictionary<Vector2, IEnumerable<PathNode>> _neighbourCache;

        public Grid(
            int width,
            int height,
            float cellSize,
            Vector3 originPosition,
            Func<int, int, PathNode> createGridCellFunc)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;
            _neighbourCache = new Dictionary<Vector2, IEnumerable<PathNode>>();

            _gridArray = new PathNode[_width, _height];
            _cellVisualArray = new Component[_width, _height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridCellFunc(x, y);
                }
            }

            InitializeComponentArray(ref _cellVisualArray);
        }

        public PathNode this[int x, int y]
        {
            get
            {
                return GetPathNode(x, y);
            }
            set
            {
                SetGridCell(x, y, value);
            }
        }

        public int Width
        {
            get { return _width; }
        }

        public int Height
        {
            get { return _height; }
        }

        public float CellSize
        {
            get { return _cellSize; }
        }

        public IEnumerable<PathNode> GetNeighbourNodesCached(Vector2 nodeCoordinates)
        {
            // TODO: implement option to reset neighbours of a cell/ reload cache
            if (_neighbourCache.ContainsKey(nodeCoordinates))
            {
                return _neighbourCache[nodeCoordinates];
            }

            var neighbourNodes = new List<PathNode>();
            for (int dX = -1; dX <= 1; dX++)
            {
                for (int dY = -1; dY <= 1; dY++)
                {
                    if (dX == 0 && dY == 0)
                    {
                        continue;
                    }

                    PathNode node = GetPathNode((int)nodeCoordinates.x + dX, (int)nodeCoordinates.y + dY);
                    if (node != null) // && node.Terrain.Difficulty != MoveDifficulty.NotWalkable
                    {
                        neighbourNodes.Add(node);
                    }
                }
            }

            _neighbourCache.Add(nodeCoordinates, neighbourNodes);

            return neighbourNodes.ToArray();
        }

        public void SetGridCell(int x, int y, PathNode value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _gridArray[x, y] = value;
                OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y, CellMap = _cellVisualArray });
            }
        }

        public void SetGridCell(Vector3 worldPosition, PathNode value)
        {
            WorldPositionToGridPosition(worldPosition, out int x, out int y);
            SetGridCell(x, y, value);
        }

        public PathNode GetPathNode(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }

            return default;
        }

        public PathNode GetGridCell(Vector3 worldPosition)
        {
            WorldPositionToGridPosition(worldPosition, out int x, out int y);
            return GetPathNode(x, y);
        }

        public void TriggerAllGridCellsChanged()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    TriggerOnGridCellChanged(x, y);
                }
            }
        }

        public void TriggerOnGridCellChanged(int x, int y)
        {
            OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y, CellMap = _cellVisualArray });
        }

        public Vector3 GetWorldPosition(int x, int y, bool centeredOnTile = false)
        {
            Vector3 worldPos = new Vector3(x, y) * _cellSize + _originPosition;
            if (centeredOnTile)
            {
                worldPos.x += _cellSize / 2;
                worldPos.y += _cellSize / 2;
            }
            return worldPos;
        }

        public void WorldPositionToGridPosition(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
        }

        public PathNode GetRandomAvailablePathNode()
        {
            int x, y;
            do
            {
                x = Random.Range(0, Width);
                y = Random.Range(0, Height);
            } while (_gridArray[x, y].Terrain.Difficulty == MoveDifficulty.NotWalkable);

            return GetPathNode(x, y);
        }

        public PathNode GetMousePositionInGrid()
        {
            var mousePos = Utils.GetMouseWorldPosition();
            WorldPositionToGridPosition(mousePos, out int x, out int y);
            return GetPathNode(x, y);
        }

        private void InitializeComponentArray(ref Component[,] visualArray)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    visualArray[x, y] = Utils.CreateMapCell(
                        CellSize,
                        Utils.GetPathNodeColor(GetPathNode(x, y)),
                        GetWorldPosition(x, y) + new Vector3(CellSize, CellSize) * .5f);

                    //displayMap[x, y] = Utils.CreateWorldText(
                    //    $"{x}:{y}",
                    //    null,
                    //    map.GetWorldPosition(x, y) + new Vector3(map.CellSize, map.CellSize) * .5f,
                    //    12,
                    //    GetPathNodeColor(map.GetPathNode(x, y)),
                    //    TextAnchor.MiddleCenter);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, Height), GetWorldPosition(Width, Height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(Width, 0), GetWorldPosition(Width, Height), Color.white, 100f);
        }
    }

    public class OnGridCellChangedEventArgs
    {
        public int x;
        public int y;
        public Component[,] CellMap;
    }
}
