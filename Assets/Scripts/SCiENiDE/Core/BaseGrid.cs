using System;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class BaseGrid<T>
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

        public BaseGrid(int width, int height, float cellSize, Vector3 originPosition, Func<BaseGrid<T>, int, int, T> createGridCellFunc,
            Action<BaseGrid<T>, TextMesh[,]> showDebug = null)
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

            showDebug?.Invoke(this, _debugTextArray);
        }

        public T this[int x, int y]
        {
            get
            {
                return GetGridCell(x, y);
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

        public void TriggetAllGridCellsChanged()
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
            OnGridCellChanged?.Invoke(this, new OnGridCellChangedEventArgs { x = x, y = y });
        }

        private void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
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
    }
}
