using System;
using System.Collections.Generic;
using UnityEngine;

namespace BspLib.Wad
{
    public class TextureWithMipmaps : Texture
    {
        public TextureWithMipmaps(string name) : base(name)
        {
        }
		public TextureWithMipmaps(string name, Texture2D bitmap) : base(name, bitmap)
        {
        }

		private List<Texture2D> _mipmaps = new List<Texture2D>();

		public Texture2D GetMipmap(int level)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException("level");

            return _mipmaps[level - 1];
        }

		public void AddMipmap(int level, Texture2D bitmap)
        {
            if (level <= 0)
                throw new ArgumentOutOfRangeException("level");

            if (GetWidth(level) != bitmap.width)
                throw new ArgumentOutOfRangeException("bitmap");
            if (GetHeight(level) != bitmap.height)
                throw new ArgumentOutOfRangeException("bitmap");

            level--;
            while (_mipmaps.Count < level)
                _mipmaps.Add(null);
            _mipmaps[level] = bitmap;
        }

        public int GetWidth(int level)
        {
            if (level == 0)
                return Bitmap.width;
            else
                return base.Bitmap.width / (int)Math.Pow(2, level);
        }

        public int GetHeight(int level)
        {
            if (level == 0)
                return Bitmap.height;
            else
                return base.Bitmap.height / (int)Math.Pow(2, level);
        }
    }
}