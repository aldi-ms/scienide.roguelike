using UnityEngine;

namespace SCiENiDE.Core.GameObjects
{
    public interface IPathNode
    {
        bool IsPath { get; set; }
        bool Visited { get; set; }
        NodeTerrain Terrain { get; set; }

        int X { get; }
        int Y { get; }
        Vector2 Coords { get; }
        float fScore { get; set; }

        string ToLongString();
    }
}