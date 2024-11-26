#nullable enable
using System;
using UnityEngine;

namespace ElympicsPlayPad.Utility
{
    internal static class SpriteUtil
    {
        public static Texture2D? TryConvertToSprite(byte[] buffer)
        {
            if (buffer is not { Length: > 0 })
                return null;

            var texture = new Texture2D(2, 2);
            try
            {
                texture.LoadImage(buffer);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
            return texture;
        }
    }
}
