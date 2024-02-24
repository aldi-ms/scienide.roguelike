using System;
using System.Collections.Generic;

namespace SCiENiDE.Core
{
    public class PriorityQueue<T>
    {
        private readonly static T _defaultValue = default;
        private readonly IComparer<T> _comparer;
        private readonly T[] _dataArray;
        private int _firstEmptyIdx;

        public PriorityQueue(Comparer<T> comparer, int capacity)
        {
            _comparer = comparer;
            _dataArray = new T[capacity];
            _firstEmptyIdx = 0;
        }

        public void Push(T value)
        {
            _dataArray[_firstEmptyIdx] = value;
            _firstEmptyIdx++;
            Array.Sort(_dataArray, _comparer);
        }

        public T Pop()
        {
            T returnValue = _dataArray[0];
            _dataArray[0] = _defaultValue;
            Array.Sort(_dataArray, _comparer);

            return returnValue;
        }

        public T Peek()
        {
            if (_dataArray.Length > 0)
                return _dataArray[0];

            return _defaultValue;
        }
    }
}
