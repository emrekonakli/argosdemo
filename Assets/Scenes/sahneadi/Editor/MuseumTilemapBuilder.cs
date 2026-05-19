using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Argos.Game;

namespace Argos.EditorTools
{
    public static class MuseumTilemapBuilder
    {
        const string ScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string SpriteFolder = "Assets/Scenes/sahneadi/Sprites/Placeholders";
        const string TileFolder = "Assets/Scenes/sahneadi/Tilemaps/Placeholders";

        const int FloorWidth = 20;
        const int FloorHeight = 15;
        const float OriginX = -10f;
        const float OriginY = -7.5f;

        [MenuItem("ARGOS/Build Museum Tilemap")]
        public static void Build()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                else
                    return;
            }

            AssetFolders.Ensure(SpriteFolder);
            AssetFolders.Ensure(TileFolder);

            var floorSprite = CreateColoredSprite("floor_light", new Color(0.78f, 0.78f, 0.74f));
            var wallSprite = CreateColoredSprite("wall_dark", new Color(0.2f, 0.2f, 0.22f));
            var furnitureSprite = CreateColoredSprite("furniture_brown", new Color(0.55f, 0.4f, 0.25f));

            var floorTile = CreateTile("floor_tile", floorSprite);
            var wallTile = CreateTile("wall_tile", wallSprite);
            var furnitureTile = CreateTile("furniture_tile", furnitureSprite);

            BuildSceneHierarchy(floorTile, wallTile, furnitureTile);

            var active = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(active);
            EditorSceneManager.SaveScene(active);
            AssetDatabase.SaveAssets();
            Debug.Log("[ARGOS] Museum Tilemap built; scene + assets saved.");
        }

        static Sprite CreateColoredSprite(string name, Color color)
        {
            string path = $"{SpriteFolder}/{name}.png";

            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Tile CreateTile(string name, Sprite sprite)
        {
            string path = $"{TileFolder}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        static void BuildSceneHierarchy(Tile floor, Tile wall, Tile furniture)
        {
            var old = GameObject.Find("MuseumTilemap");
            if (old != null) Object.DestroyImmediate(old);

            var root = new GameObject("MuseumTilemap");
            root.transform.position = new Vector3(OriginX, OriginY, 0f);
            var grid = root.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // Floor: full 20x15 fill, sortingOrder negative so it sits behind everything.
            var floorTm = CreateTilemap(root.transform, "Floor", -10);
            FillRect(floorTm, 0, 0, FloorWidth, FloorHeight, floor);

            // Walls: one-cell border around the floor; tilemap collider + composite.
            var wallsGo = CreateTilemap(root.transform, "Walls", 5);
            PaintBorder(wallsGo.GetComponent<Tilemap>(), wall);

            var wallCollider = wallsGo.gameObject.AddComponent<TilemapCollider2D>();
            var wallRb = wallsGo.gameObject.AddComponent<Rigidbody2D>();
            wallRb.bodyType = RigidbodyType2D.Static;
            wallsGo.gameObject.AddComponent<CompositeCollider2D>();
            wallCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Furniture: 4 fixed spots, also blocks player.
            var furnitureGo = CreateTilemap(root.transform, "Furniture", 6);
            PaintFurniture(furnitureGo.GetComponent<Tilemap>(), furniture);
            furnitureGo.gameObject.AddComponent<TilemapCollider2D>();

            // Bootstrap'i de procedural duvar üretmekten vazgeçir.
            var bootstrap = Object.FindFirstObjectByType<MuseumSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.buildWalls = false;
                EditorUtility.SetDirty(bootstrap);
            }
        }

        static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tm = go.AddComponent<Tilemap>();
            var tr = go.AddComponent<TilemapRenderer>();
            tr.sortingOrder = sortingOrder;
            return tm;
        }

        static void FillRect(Tilemap tilemap, int x, int y, int w, int h, Tile tile)
        {
            for (int dx = 0; dx < w; dx++)
                for (int dy = 0; dy < h; dy++)
                    tilemap.SetTile(new Vector3Int(x + dx, y + dy, 0), tile);
        }

        static void PaintBorder(Tilemap tilemap, Tile tile)
        {
            for (int x = -1; x <= FloorWidth; x++)
            {
                tilemap.SetTile(new Vector3Int(x, -1, 0), tile);
                tilemap.SetTile(new Vector3Int(x, FloorHeight, 0), tile);
            }
            for (int y = 0; y < FloorHeight; y++)
            {
                tilemap.SetTile(new Vector3Int(-1, y, 0), tile);
                tilemap.SetTile(new Vector3Int(FloorWidth, y, 0), tile);
            }
        }

        static void PaintFurniture(Tilemap tilemap, Tile tile)
        {
            // 4 vitrines spread evenly, leaving open paths through middle.
            Vector3Int[] positions =
            {
                new Vector3Int(3, 3, 0),
                new Vector3Int(16, 3, 0),
                new Vector3Int(3, 11, 0),
                new Vector3Int(16, 11, 0),
            };
            foreach (var p in positions) tilemap.SetTile(p, tile);
        }
    }
}
