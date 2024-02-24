using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class Grid<T>
    {
        public event EventHandler<OnGridCellChangedEventArgs> OnGridCellChanged;

        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly T[,] _gridArray;
        private readonly Component[,] _cellVisualArray;
        private Vector3 _originPosition;
        private Dictionary<Vector2, IEnumerable<T>> _neighbourCache;

        public Grid(
            int width, 
            int height, 
            float cellSize, 
            Vector3 originPosition, 
            Func<int, int, T> createGridCellFunc,
            Action<Grid<T>, Component[,]> displayCallback = null)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;
            _neighbourCache= new Dictionary<Vector2, IEnumerable<T>>();

            _gridArray = new T[_width, _height];
            _cellVisualArray = new Component[_width, _height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridCellFunc(x, y);
                }
            }

            displayCallback?.Invoke(this, _cellVisualArray);
        }

        public T this[int x, int y]
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

        public IEnumerable<T> GetNeighbourNodesCached(Vector2 nodeCoordinates)
        {
            // TODO: implement option to reset neighbours of a cell/ reload cache
            if (_neighbourCache.ContainsKey(nodeCoordinates))
            {
                return _neighbourCache[nodeCoordinates];
            }

            var neighbourNodes = new List<T>();
            for (int dX = -1; dX <= 1; dX++)
            {
                for (int dY = -1; dY <= 1; dY++)
                {
                    if (dX == 0 && dY == 0)
                    {
                        continue;
                    }

                    T node = GetPathNode((int)nodeCoordinates.x + dX, (int)nodeCoordinates.y + dY);
                    if (node != null) // && node.Terrain.Difficulty != MoveDifficulty.NotWalkable
                    {
                        neighbourNodes.Add(node);
                    }
                }
            }

            _neighbourCache.Add(nodeCoordinates, neighbourNodes);

            return neighbourNodes.ToArray();
        }
        
        public void SetGridCell(int x, int y, T value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _gridArray[x, y] = value;
                OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y, CellMap = _cellVisualArray });
            }
        }

        public void SetGridCell(Vector3 worldPosition, T value)
        {
            WorldPositionToGridPosition(worldPosition, out int x, out int y);
            SetGridCell(x, y, value);
        }

        public T GetPathNode(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }

            return default;
        }

        public T GetGridCell(Vector3 worldPosition)
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

        public class OnGridCellChangedEventArgs
        {
            public int x;
            public int y;
            public Component[,] CellMap;
        }
    }
}
