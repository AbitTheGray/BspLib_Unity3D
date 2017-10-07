using System;
using UnityEngine;

namespace BspLib.Wad
{
    public class Texture
    {
        public Texture(string name, Texture2D bitmap) : this(name)
        {
            if (bitmap == null || bitmap.width <= 0 || bitmap.height <= 0)
                throw new ArgumentNullException("bitmap");

            this.Bitmap = bitmap;
        }
        public Texture(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            this.Name = name;
            this.Bitmap = null;
        }

        public string Name
        {
            get;
        }

		public Texture2D Bitmap
        {
            get;
            protected set;
        }
    }
}