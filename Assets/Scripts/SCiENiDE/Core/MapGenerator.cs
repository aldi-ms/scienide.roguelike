using SCiENiDE.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCiENiDE.Core
{
    public class MapGenerator
    {
        private int _width;
        private int _height;
        private Action<BaseGrid<MapNode>, TextMesh[,]> _debug;
        private RectInt[] _rooms;

        public MapGenerator(int width, int height, Action<BaseGrid<MapNode>, TextMesh[,]> showDebug = null)
        {
            _width = width;
            _height = height;
            _debug = showDebug;
            _rooms = new RectInt[Random.Range(7, 18)];
        }

        public BaseGrid<MapNode> GenerateMap()
        {
            BaseGrid<MapNode> map = new BaseGrid<MapNode>(
                _width, _height, 6.5f, new Vector3(-110f, -60f),
                (grid, x, y) =>
                {
                    return new MapNode(grid, x, y, MoveDifficulty.NotWalkable);
                },
                _debug);

            for (int i = 0; i < _rooms.Length; i++)
            {
                Vector2Int roomSize = GetRandomRoomSize();
                var tempRoom = GenerateRoom(map, roomSize.x, roomSize.y);

                if (!tempRoom.Equals(default))
                    _rooms[i] = tempRoom;
            }

            return map;
        }


        private Vector2Int GetRandomRoomSize()
        {
            return new Vector2Int(Random.Range(3, 7), Random.Range(3, 6));
        }

        private RectInt GenerateRoom(BaseGrid<MapNode> map, int roomWidth, int roomHeight, int maxTries = 14)
        {
            int mapX;
            int mapY;
            int tries = 0;

            RectInt tempRoom;
            do
            {
                tries++;
                if (tries > maxTries)
                {
                    Debug.Log($"MaxTries [{maxTries}] for {nameof(GenerateRoom)} reached.");
                    return default;
                }
                mapX = Random.Range(0, map.Width - roomWidth);
                mapY = Random.Range(0, map.Height - roomHeight);
                tempRoom = new RectInt(mapX, mapY, roomWidth, roomHeight);

            } while (_rooms.Any(x => x.Overlaps(tempRoom))); //mapX + width >= map.Width || mapY + height >= map.Height ||

            Debug.Log($"Creating a room with [{mapX}:{mapY}] and width/height =  {roomWidth}/{roomHeight}.");

            for (int x = mapX; x < mapX + roomWidth; x++)
            {
                for (int y = mapY; y < mapY + roomHeight; y++)
                {
                    map[x, y].Terrain.Difficulty = MoveDifficulty.Easy;
                    map.TriggerOnGridCellChanged(x, y);
                }
            }

            return tempRoom;
        }

        private enum MapFeature
        {
            Rooms,
            Terrain,
            Unit,
            Object
        }
    }
}
