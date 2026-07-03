using UnityEngine;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Helpers
{
    public class SpriteHelper
    {
        private readonly Sprite[] _sprites;
        private readonly Sprite _fallbackSprite;
        private readonly tk2dSpriteCollectionData collection;

        public SpriteHelper(string collectionName, bool lazy = true)
        {
            collection = ETGMod.Assets.FindCollectionOfName(collectionName);
            _fallbackSprite = CreateSquareSprite();

            if (collection == null)
            {
                return;
            }

            var frameCount = collection.spriteDefinitions.Length;
            _sprites = new Sprite[frameCount];

            if (!lazy)
            {
                for (int f = 0; f < frameCount; f++)
                {
                    BuildForFrame(f);
                }
            }
        }

        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }

        private Sprite BuildForFrame(int frame)
        {
            if (_sprites == null)
            {
                return _fallbackSprite;
            }

            var def = collection.spriteDefinitions[frame];
            if (def == null)
            {
                return _fallbackSprite;
            }
            var tex = def.material?.mainTexture as Texture2D;

            if (def == null)
            {
                return _fallbackSprite;
            }

            _sprites[frame] = Sprite.Create(
                tex,
                UvToRect(def.uvs, tex),
                new Vector2(0.5f, 0.5f),
                16f
            );
            return _sprites[frame];
        }

        public Sprite GetFrame(int frame)
        {
            if (_sprites == null)
            {
                return _fallbackSprite;
            }

            if (frame < 0 || frame >= _sprites.Length)
            {
                throw new System.IndexOutOfRangeException(
                    $"Cannot set frame out of bounds frame: {frame}"
                );
            }

            return _sprites[frame] ?? BuildForFrame(frame);
        }

        public static Rect UvToRect(Vector2[] uvs, Texture tex)
        {
            float x = uvs[0].x * tex.width;
            float y = uvs[0].y * tex.height;
            float w = Mathf.Abs(uvs[1].x - uvs[0].x) * tex.width;
            float h = Mathf.Abs(uvs[2].y - uvs[0].y) * tex.height;
            return new Rect(x, y, w, h);
        }
    }
}
