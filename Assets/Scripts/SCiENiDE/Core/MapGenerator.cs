using Assets.Scripts.SCiENiDE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class MapGenerator
    {
        private bool _useRandomSeed;

        private int _width;
        private int _height;
        private int _totalNodeCount;
        private int _seed;
        private int _fillPercent;
        private int _smoothing;

        private RectInt[] _rooms;
        private Grid<IPathNode> _map;

        public MapGenerator(
            int width,
            int height,
            bool useRandomSeed = true,
            int seed = -1,
            Action<Grid<IPathNode>, Component[,]> displayCallback = null,
            int fillPercent = 43,
            int smoothing = 2)
        {
            _width = width;
            _height = height;
            _totalNodeCount = _width * _height;
            _seed = seed;
            _useRandomSeed = useRandomSeed;
            _fillPercent = fillPercent;
            _smoothing = smoothing;

            if (_useRandomSeed)
            {
                _seed = Environment.TickCount;
                Random.InitState(_seed);

                Debug.Log($"Init PRNG with random seed [{_seed}].");
            }
            else
            {
                Random.InitState(_seed);

                Debug.Log($"Init PRNG with seed [{_seed}].");
            }

            int minRoomsRange = 8;
            int maxRoomsRange = 20;
            _rooms = new RectInt[Random.Range(minRoomsRange, maxRoomsRange)];

            _map = new Grid<IPathNode>(
                _width, _height, 5f, new Vector3(-110f, -60f),
                (x, y) =>
                {
                    return new PathNode(x, y, MoveDifficulty.Easy);
                },
                displayCallback);
        }

        public Grid<IPathNode> Map
        {
            get { return _map; }
            private set { _map = value; }
        }

        public Grid<IPathNode> GenerateMap(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Rooms:
                    {
                        GenerateRooms();
                        for (int i = 0; i < _smoothing; i++)
                        {
                            _map.RunCARuleset(mapType);
                        }

                        ProcessMap();
                    }
                    break;

                case MapType.RandomFill:
                    {
                        RandomFillMap(_fillPercent);
                        _map.TriggerAllGridCellsChanged();
                        for (int i = 0; i < _smoothing; i++)
                        {
                            _map.RunCARuleset(mapType);
                        }
                    }
                    break;

                case MapType.SolidFill:
                    {
                        Debug.LogWarning($"GenerateMap for {mapType.ToString()} is not defined!");
                    }
                    break;

                default:
                    break;
            }

            return _map;
        }

        private void ProcessMap()
        {
            Dictionary<MoveDifficulty, List<Room>> roomRegions = GetMapRegions();
            Dictionary<MoveDifficulty, List<Room>> survivingRoomRegions = new Dictionary<MoveDifficulty, List<Room>>();
            foreach (MoveDifficulty moveDifficultyKey in roomRegions.Keys)
            {
                if (moveDifficultyKey == MoveDifficulty.NotWalkable) continue;

                foreach (Room room in roomRegions[moveDifficultyKey])
                {
                    if (room.Size < 6)
                    {
                        foreach (var node in room.Tiles)
                        {
                            node.Terrain.Difficulty = MoveDifficulty.NotWalkable;
                        }
                    }
                    else
                    {
                        if (!survivingRoomRegions.ContainsKey(moveDifficultyKey))
                        {
                            survivingRoomRegions[moveDifficultyKey] = new List<Room> { room };
                        }
                        else
                        {
                            survivingRoomRegions[moveDifficultyKey].Add(room);
                        }
                    }
                }
            }

            _map.TriggerAllGridCellsChanged();

            List<Room> flatRoomList = survivingRoomRegions
                .Select(x => x.Value)
                .SelectMany(x => x).ToList();
            flatRoomList.Sort();

            flatRoomList[0].IsMainRoom = true;
            flatRoomList[0].IsAccesibleFromMainRoom = true;

            ConnectRooms(flatRoomList);
        }
        private void ConnectRooms(List<Room> rooms, bool forceConnect = false)
        {
            if (rooms.Count <= 1)
            {
                return;
            }

            List<Room> roomListA = new List<Room>();
            List<Room> roomListB = new List<Room>();

            if (forceConnect)
            {
                foreach (Room room in rooms)
                {
                    if (room.IsAccesibleFromMainRoom)
                    {
                        roomListB.Add(room);
                    }
                    else
                    {
                        roomListA.Add(room);
                    }
                }
            }
            else
            {
                roomListA = rooms;
                roomListB = rooms;
            }

            int bestDistance = int.MaxValue;
            Room bestRoomA = null;
            Room bestRoomB = null;
            IPathNode bestNodeA = null;
            IPathNode bestNodeB = null;
            bool matchFound = false;
            foreach (Room roomA in roomListA)
            {
                if (!forceConnect)
                {
                    matchFound = false;
                    if (roomA.NeighbourRooms.Count > 0)
                    {
                        continue;
                    }
                }
                foreach (Room roomB in roomListB)
                {
                    if (roomA == roomB || roomA.IsConnectedTo(roomB))
                    {
                        continue;
                    }

                    for (int wallTileA = 0; wallTileA < roomA.EdgeTiles.Count; wallTileA++)
                    {
                        for (int wallTileB = 0; wallTileB < roomB.EdgeTiles.Count; wallTileB++)
                        {
                            int distanceBetweenRooms = (int)(Mathf.Pow(roomA.EdgeTiles[wallTileA].X - roomB.EdgeTiles[wallTileB].X, 2) +
                                Mathf.Pow(roomA.EdgeTiles[wallTileA].Y - roomB.EdgeTiles[wallTileB].Y, 2));

                            if (distanceBetweenRooms < bestDistance || !matchFound)
                            {
                                matchFound = true;
                                bestDistance = distanceBetweenRooms;
                                bestNodeA = roomA.EdgeTiles[wallTileA];
                                bestNodeB = roomB.EdgeTiles[wallTileB];
                                bestRoomA = roomA;
                                bestRoomB = roomB;
                            }
                        }
                    }
                }

                if (matchFound && !forceConnect)
                {
                    CreatePassage(bestRoomA, bestRoomB, bestNodeA, bestNodeB);
                }
            }

            if (matchFound && forceConnect)
            {
                CreatePassage(bestRoomA, bestRoomB, bestNodeA, bestNodeB);
                ConnectRooms(rooms, true);
            }
            if (!forceConnect)
            {
                ConnectRooms(rooms, true);
            }
        }
        private void CreatePassage(Room a, Room b, IPathNode nodeA, IPathNode nodeB)
        {
            Debug.Log($"Connecting room nodes [{nodeA}] => [{nodeB}]");
            Room.ConnectRooms(a, b);

            Vector3 startVector = _map.GetWorldPosition(nodeA.X, nodeA.Y, true);
            Vector3 endVector = _map.GetWorldPosition(nodeB.X, nodeB.Y, true);

            var passageNodes = new List<IPathNode>();
            int diagonalDistance = Utils.DiagonalDistance(nodeA.X, nodeA.Y, nodeB.X, nodeB.Y);
            for (int i = 0; i <= diagonalDistance; i++)
            {
                float t = diagonalDistance == 0 ? 0f : (float)i / diagonalDistance;
                _map.WorldPositionToGridPosition(Vector3.Lerp(startVector, endVector, t), out int nodeX, out int nodeY);
                passageNodes.Add(_map[nodeX, nodeY]);
            }

            foreach (var n in passageNodes)
            {
                n.Terrain.Difficulty = MoveDifficulty.Easy;
                _map.TriggerOnGridCellChanged(n.X, n.Y);
            }
        }
        private Vector3 CoordToWorldPoint(int x, int y)
        {
            return new Vector3(-_width / 2 + .5f + x, 2, -_height / 2 + .5f + y);
        }
        private Dictionary<MoveDifficulty, List<Room>> GetMapRegions()
        {
            Dictionary<MoveDifficulty, List<Room>> roomRegions = new Dictionary<MoveDifficulty, List<Room>>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var currentNode = _map.GetPathNode(x, y);
                    if (currentNode == null) continue;

                    MoveDifficulty currentTerrain = currentNode.Terrain.Difficulty;

                    if (!roomRegions.ContainsKey(currentTerrain)
                        || !roomRegions[currentTerrain].Any(r => r.Tiles.Contains(currentNode)))
                    {
                        var currentRegion = GetRegionTiles(x, y, _map);
                        Room room = new Room(currentRegion, _map);
                        if (roomRegions.ContainsKey(currentTerrain))
                        {
                            roomRegions[currentTerrain].Add(room);
                        }
                        else
                        {
                            roomRegions[currentTerrain] = new List<Room> { room };
                        }
                    }
                }
            }

            return roomRegions;
        }
        private void GenerateRooms() 
        {
            Debug.Log($"Generating [{_rooms.Length}] rooms.");

            for (int i = 0; i < _rooms.Length; i++)
            {
                Vector2Int roomSize = GetRandomRoomSize();
                RectInt tempRoom = GenerateRoom(roomSize.x, roomSize.y);

                if (!tempRoom.Equals(default))
                {
                    _rooms[i] = tempRoom;
                }
            }
        }
        private Vector2Int GetRandomRoomSize()
        {
            return new Vector2Int(Random.Range(3, 7), Random.Range(3, 6));
        }
        private RectInt GenerateRoom(
            int roomWidth,
            int roomHeight,
            MoveDifficulty moveDifficulty = MoveDifficulty.Easy,
            int maxTries = 15)
        {
            int mapX;
            int mapY;
            int tries = 0;

            RectInt createdRoom;
            do
            {
                mapX = Random.Range(0, _width - roomWidth);
                mapY = Random.Range(0, _height - roomHeight);
                createdRoom = new RectInt(mapX, mapY, roomWidth, roomHeight);

                tries++;
                if (tries > maxTries)
                {
                    Debug.Log($"MaxTries [{maxTries}] for {nameof(GenerateRoom)} reached.");
                    return default;
                }
            } while (_rooms.Any(x => x.Overlaps(createdRoom))); //mapX + width >= map.Width || mapY + height >= map.Height ||

            Debug.Log($"Creating a room at [{mapX}:{mapY}], w/h = {roomWidth}/{roomHeight}.");

            for (int x = mapX; x < mapX + roomWidth; x++)
            {
                for (int y = mapY; y < mapY + roomHeight; y++)
                {
                    _map[x, y].Terrain.Difficulty = moveDifficulty;
                }
            }

            return createdRoom;
        }
        private void RandomFillMap(int wallPercent = 43)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var z = Random.Range(0, 100);
                    _map[x, y].Terrain.Difficulty = z < wallPercent ? MoveDifficulty.NotWalkable : MoveDifficulty.Easy;
                }
            }
        }
        private List<IPathNode> GetRegionTiles(int x, int y, Grid<IPathNode> map)
        {
            var startNode = _map.GetPathNode(x, y);
            if (startNode == null)
            {
                return null;
            }

            var open = new Queue<IPathNode>();
            var closed = new List<IPathNode>();

            open.Enqueue(startNode);

            while (open.Count > 0)
            {
                var current = open.Dequeue();
                closed.Add(current);

                var neigbourNodesPerDifficulty = map.GetNeighbourNodesCached(current.Coords)
                    .Where(n => n.Terrain.Difficulty == startNode.Terrain.Difficulty);
                foreach (var neighbour in neigbourNodesPerDifficulty)
                {
                    if (!closed.Contains(neighbour) && !open.Contains(neighbour))
                    {
                        open.Enqueue(neighbour);
                    }
                }
            }

            return closed;
        }
        private bool IsValidMapBounds(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }
    }
}
