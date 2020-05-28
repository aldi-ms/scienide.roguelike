namespace SCiENiDE.Core
{
    public interface IPathNode<out T> where T : IPathNode<T>
    {
        bool IsPath { get; set; }
        bool Visited { get; set; }
        T[] NeighbourNodes { get; }
        NodeTerrain Terrain { get; set; }

        int x { get; }
        int y { get; }
        float fScore { get; set; }

        string ToString();
    }
}