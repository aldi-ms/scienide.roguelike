using UnityEngine;

namespace SCiENiDE.Core
{
    public class Utils
    {
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

        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return worldPosition;
        }

        //public static void InitializePathNodeNeighbours(BaseGrid<PathNode> map)
        //{
        //    for (int x = 0; x < map.GetWidth(); x++)
        //    {
        //        for (int y = 0; y < map.GetHeight(); y++)
        //        {
        //            PathNode currentNode = map.GetGridCell(x, y);
        //        }
        //    }
        //}
    }
}
