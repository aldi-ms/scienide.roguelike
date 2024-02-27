using SCiENiDE.Core.GameObjects;
using SCiENiDE.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class Grid
    {
        public event EventHandler<OnGridCellChangedEventArgs> OnGridCellChanged;

        private readonly int _width;
        private readonly int _height;
        private readonly int _cellSize;
        private readonly PathNode[,] _gridArray;
        private readonly Component[,] _cellVisualArray;
        private readonly Dictionary<Vector2, List<PathNode>> _neighbourCache;
        private Vector3 _originPosition;

        public Grid(
            int width,
            int height,
            int cellSize,
            Vector3 bottomLeftOrigin)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = bottomLeftOrigin;
            _neighbourCache = new Dictionary<Vector2, List<PathNode>>();

            _gridArray = new PathNode[_width, _height];
            _cellVisualArray = new Component[_width, _height];

            ForEachCoordinate((x, y) => _gridArray[x, y] = new PathNode(x, y, MoveDifficulty.NotWalkable));
            InitializeComponentArray(_cellVisualArray);
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

        public List<PathNode> GetNeighbourNodesWithCache(Vector2 nodeCoordinates)
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

            return neighbourNodes;
        }

        public void SetGridCell(int x, int y, PathNode value)
        {
            if (IsValidBounds(x, y))
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
            if (IsValidBounds(x, y))
            {
                return _gridArray[x, y];
            }

            return default;
        }

        public PathNode GetGridCellAtMousePosition()
        {
            var worldPosition = Utils.GetMouseWorldPosition();
            WorldPositionToGridPosition(worldPosition, out int x, out int y);
            return GetPathNode(x, y);
        }

        public void TriggerAllGridCellsChanged()
        {
            ForEachCoordinate((x, y) => TriggerOnGridCellChanged(x, y));
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

        public void ForEachCoordinate(Action<int, int> action)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    action(x, y);
                }
            }
        }

        public bool IsValidBounds(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        private void InitializeComponentArray(Component[,] visualArray)
        {
            var cellSizeVector = new Vector3(CellSize, CellSize) * .5f;

            ForEachCoordinate((x, y) =>
                    visualArray[x, y] = Utils.CreateMapCell(
                        CellSize,
                        Utils.GetPathNodeColor(GetPathNode(x, y)),
                        GetWorldPosition(x, y) + cellSizeVector));
        }
    }

    public class OnGridCellChangedEventArgs
    {
        public int x;
        public int y;
        public Component[,] CellMap;
    }
}
