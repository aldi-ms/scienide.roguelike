using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace SCiENiDE.Core
{
    public class Utils
    {
        public static Color GetPathNodeColor(IPathNode pathNode)
        {
            switch (pathNode.Terrain.Difficulty)
            {
                case MoveDifficulty.Easy:
                    return Color.green;

                case MoveDifficulty.Medium:
                    return Color.yellow;

                case MoveDifficulty.Hard:
                    return Color.red;

                case MoveDifficulty.NotWalkable:
                    return Color.gray;

                case MoveDifficulty.None:
                default:
                    return Color.white;
            }
        }

        private readonly static Dictionary<Color, Texture2D> _textureMap = new Dictionary<Color, Texture2D>();
        public static Texture2D GetSharedSingleColorTexture2D(Color color)
        {
            if (_textureMap.ContainsKey(color)) 
                return _textureMap[color];

            var texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, color);
            texture.Apply();

            _textureMap[color] = texture;
            return _textureMap[color];
        }

        public static TextMesh CreateWorldText(
            string text,
            Transform parent = null,
            Vector3 localPosition = default(Vector3),
            int fontSize = 40,
            Color color = default(Color),
            TextAnchor textAnchor = TextAnchor.UpperLeft,
            TextAlignment textAlignment = TextAlignment.Left,
            int sortingOrder = 0)
        {
            GameObject gameObject = new GameObject("WorldText", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        public static Component CreateMapCell(
            Transform parent,
            float size,
            Color color,
            Vector3 localPosition = default(Vector3))
        {
            // Create a new 16x16 texture
            int textureSize = Mathf.FloorToInt(size);
            Texture2D redTexture = new Texture2D(textureSize, textureSize);

            // Fill the texture with red color
            //Color red = new Color(1f, 0f, 0f, 1f); // Red (RGBA)
            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    redTexture.SetPixel(x, y, color);
                }
            }
            redTexture.Apply(); // Apply changes to the texture

            // Create a new GameObject with a Sprite Renderer
            GameObject rectangleObject = new GameObject("RedRectangle");
            SpriteRenderer spriteRenderer = rectangleObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(redTexture, new Rect(0, 0, textureSize, textureSize), Vector2.one * 0.5f, 5);

            // Adjust position, scale, etc. as needed
            rectangleObject.transform.localPosition = localPosition;
            //Debug.Log(localPosition);
            rectangleObject.transform.localScale = new Vector3(size, size, 0);

            return spriteRenderer;
        }

        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return worldPosition;
        }

        public static int DiagonalDistance(int startX, int startY, int endX, int endY)
        {
            int dx = endX - startX;
            int dy = endY - startY;
            return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        }
    }
}
