using System;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class GridBase<T>
    {
        public event EventHandler<OnGridCellChangedEventArgs> OnGridCellChanged;
        public class OnGridCellChangedEventArgs
        {
            public int x;
            public int y;
        }

        private const bool ShowDebug = true;

        private int _width;
        private int _height;
        private float _cellSize;
        private T[,] _gridArray;
        private TextMesh[,] _debugTextArray;
        private Vector3 _originPosition;

        public GridBase(int width, int height, float cellSize, Vector3 originPosition, Func<GridBase<T>, int, int, T> createGridCellFunc)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;

            _gridArray = new T[_width, _height];
            _debugTextArray = new TextMesh[_width, _height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridCellFunc(this, x, y);
                }
            }

            if (ShowDebug)
            {
                for (int x = 0; x < _gridArray.GetLength(0); x++)
                {
                    for (int y = 0; y < _gridArray.GetLength(1); y++)
                    {
                        _debugTextArray[x, y] = Utils.CreateWorldText(
                            _gridArray[x, y]?.ToString(),
                            null,
                            GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f,
                            24,
                            Color.white,
                            TextAnchor.MiddleCenter);

                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                    }
                }

                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

                OnGridCellChanged += (object sender, OnGridCellChangedEventArgs args) =>
                {
                    _debugTextArray[args.x, args.y].text = _gridArray[args.x, args.y]?.ToString();
                };
            }
        }

        public void SetGridCell(int x, int y, T value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _gridArray[x, y] = value;
                OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y });
            }
        }
        public void SetGridCell(Vector3 worldPosition, T value)
        {
            GetXY(worldPosition, out int x, out int y);
            SetGridCell(x, y, value);
        }

        public T GetGridCell(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }

            return default;
        }
        public T GetGridCell(Vector3 worldPosition)
        {
            GetXY(worldPosition, out int x, out int y);
            return GetGridCell(x, y);
        }

        public void TriggerOnGridCellChanged(int x, int y)
        {
            OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y });
        }

        private void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
        }
        private Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _originPosition;
        }
    }
}
