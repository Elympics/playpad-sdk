#nullable enable
using System;
using UnityEngine;

namespace ElympicsPlayPad.Utility
{
    internal static class SpriteUtil
    {
        public static Texture2D? TryConvertToSprite(string image)
        {
            if (string.IsNullOrEmpty(image))
                return null;

            var texture = new Texture2D(2, 2);
            try
            {
                var buffer = Convert.FromBase64String(image);
                _ = texture.LoadImage(buffer);
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
