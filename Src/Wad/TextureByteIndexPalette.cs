using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BspLib.Wad
{
    public class TextureByteIndexPalette : Texture
    {
        public TextureByteIndexPalette(string name, int width, int height, byte[] indices, Color[] palette) : base(name)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");
            if (indices == null || indices.LongLength < width * ((long)height))
                throw new ArgumentNullException("indices");
            if (palette == null || palette.Length > 256)
                throw new ArgumentNullException("palette");

            this.Width = width;
            this.Height = height;
            this.Indices = indices;
            this.Palette = palette;

            base.Bitmap = CreateBitmap(Width, Height, Indices, Palette, name);
        }

        public TextureByteIndexPalette(string name, byte[,] indices, Color[] palette) : base(name)
        {
            if (indices == null || indices.GetLength(0) == 0 || indices.GetLength(1) == 0)
                throw new ArgumentNullException("indices");
            if (palette == null || palette.Length > 256)
                throw new ArgumentNullException("palette");

            this.Width = indices.GetLength(0);
            this.Height = indices.GetLength(1);

            byte[] ind = new byte[Width * Height];
            System.Buffer.BlockCopy(indices, 0, ind, 0, ind.Length);
            this.Indices = ind;

            this.Palette = palette;

            base.Bitmap = CreateBitmap(Width, Height, Indices, Palette);
        }

        public TextureByteIndexPalette(Texture texture) : this(texture.Name, texture.Bitmap)
        {
        }

        public TextureByteIndexPalette(Texture texture, Color[] palette) : this(texture.Name, texture.Bitmap, palette)
        {
        }

		public TextureByteIndexPalette(string name, Texture2D image) : base(name, image)
        {
            this.Width = Bitmap.width;
            this.Height = Bitmap.height;

            Color[] palette = new Color[256];
            int palette_free = 0;

            byte[] indexes = new byte[Width + Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var color = Bitmap.GetPixel(x, y);
                    byte index;
                    for (index = 0; index < palette_free; index++)
                        if (palette[index] == color)
                            break;
                    if (index == palette_free)
                    {
                        palette[palette_free] = color;
                        palette_free++;
                        if (palette_free > 256)
                            throw new IndexOutOfRangeException("Too many colors in bitmap");
                    }
                    indexes[x + y * Width] = index;
                }
            }
            this.Indices = Indices;

            Array.Resize(ref palette, palette_free);
            this.Palette = palette;
        }

		public TextureByteIndexPalette(string name, Texture2D image, Color[] palette) : base(name, image)
        {
            if (palette == null || palette.Length > 256)
                throw new ArgumentNullException("palette");

            this.Width = Bitmap.width;
            this.Height = Bitmap.height;
            this.Palette = palette;

            byte[] indexes = new byte[Width + Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var color = Bitmap.GetPixel(x, y);
                    int ind = Array.IndexOf(palette, color);
                    if (ind == -1)
                        throw new IndexOutOfRangeException("Color not found in palette");
                    indexes[x + y * Width] = (byte)ind;
                }
            }
            this.Indices = indexes;
        }

        public TextureByteIndexPalette(string name, int width, int height, Color[] image) : base(name)
        {
            this.Width = width;
            this.Height = height;


            Color[] palette = new Color[256];
            int palette_free = 0;

            byte[] indexes = new byte[Width + Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var color = image[x + y * width];
                    byte index;
                    for (index = 0; index < palette_free; index++)
                        if (palette[index] == color)
                            break;
                    if (index == palette_free)
                    {
                        palette[palette_free] = color;
                        palette_free++;
                        if (palette_free > 256)
                            throw new IndexOutOfRangeException("Too many colors in bitmap");
                    }
                    indexes[x + y * Width] = index;
                }
            }
            this.Indices = Indices;

            Array.Resize(ref palette, palette_free);
            this.Palette = palette;
        }

        public TextureByteIndexPalette(string name, Color[,] image) : base(name)
        {
            this.Width = image.GetLength(0);
            this.Height = image.GetLength(1);

            Color[] palette = new Color[256];
            int palette_free = 0;

            byte[] indexes = new byte[Width + Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var color = image[x, y];
                    byte index;
                    for (index = 0; index < palette_free; index++)
                        if (palette[index] == color)
                            break;
                    if (index == palette_free)
                    {
                        palette[palette_free] = color;
                        palette_free++;
                        if (palette_free > 256)
                            throw new IndexOutOfRangeException("Too many colors in bitmap");
                    }
                    indexes[x + y * Width] = index;
                }
            }
            this.Indices = Indices;

            Array.Resize(ref palette, palette_free);
            this.Palette = palette;
        }

        public int Width
        {
            get;
        }
        public int Height
        {
            get;
        }

        public byte[] Indices
        {
            get;
        }

        public Color[] Palette
        {
            get;
        }

        public byte GetIndex(int x, int y)
        {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException("x");
            if (y < 0 || y >= Width)
                throw new ArgumentOutOfRangeException("y");
            return Indices[x + y * Height];
        }

        public Color GetColor(int x, int y)
        {
            return Palette[GetIndex(x, y)];
        }

		private static Texture2D CreateBitmap(int width, int height, byte[] indices, Color[] palette)
		{
			return CreateBitmap(width, height, indices, palette, null);
		}

		private static Texture2D CreateBitmap(int width, int height, byte[] indices, Color[] palette, string name)
        {
			var bitmap = new Texture2D(width, height);
			bitmap.name = name;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap.SetPixel(x, height - y, palette[indices[x + y * width]]);
                }
            }
			bitmap.Apply();

            return bitmap;
        }

        public enum MipmapAlgorithm
        {
            FirstPixel,
            CropCenter,
            /*
            AveragePixel,
            */
        }

        #region Utilities

        public enum ColorPaletteAlgorithm
        {
            MostUsedColorsRGB,
            BasicPaletteNearestRGB
        }

        public static TextureByteIndexPalette CreateFromBitmap(Texture texture)
		{
			return CreateFromBitmap(texture.Name, texture.Bitmap);
		}

		public static TextureByteIndexPalette CreateFromBitmap(string name, Texture2D bitmap, ColorPaletteAlgorithm algorithm = ColorPaletteAlgorithm.BasicPaletteNearestRGB)
        {
            var indices = new byte[bitmap.width * bitmap.height];
            var colors = new List<Color>();

            // Add all colors into the palette (to count them)
            for (int y = 0; y < bitmap.height; y++)
            {
                for (int x = 0; x < bitmap.width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (!colors.Contains(c))
                        colors.Add(c);
                }
            }

            if (colors.Count <= 256)
            {

                for (int y = 0; y < bitmap.height; y++)
                {
                    for (int x = 0; x < bitmap.width; x++)
                    {
                        Color c = bitmap.GetPixel(x, y);
                        int index = colors.IndexOf(c);
                        indices[x + y * bitmap.width] = (byte)index;
                    }
                }

                return new TextureByteIndexPalette(name, bitmap.width, bitmap.height, indices, colors.ToArray());
            }
            else
            {
                return ReduceColorPalette(name, bitmap, colors, algorithm, 256);
            }
        }

		private static TextureByteIndexPalette ReduceColorPalette(string name, Texture2D bitmap, List<Color> allColors, ColorPaletteAlgorithm algorithm, int targetColors)
        {
            if (targetColors > 256)
                throw new ArgumentOutOfRangeException("targetColors");// Indices over byte.MaxValue are not supported

			byte[] indices = new byte[bitmap.width * bitmap.height];

            switch (algorithm)
            {
                case ColorPaletteAlgorithm.MostUsedColorsRGB:
                    {
                        int[] colorUsage = new int[allColors.Count];

                        // Calculate usage
                        for (int y = 0; y < bitmap.height; y++)
                            for (int x = 0; x < bitmap.width; x++)
                                colorUsage[allColors.IndexOf(bitmap.GetPixel(x, y))]++;

                        var byUsage = allColors.OrderByDescending((col) =>
                        {
                            int index = allColors.IndexOf(col);
                            return colorUsage[index]; // How many times was the color used
                        }).ToArray();
                        Array.Resize(ref byUsage, targetColors);

                        // Get nearest pixel by RGB
                        for (int y = 0; y < bitmap.height; y++)
                            for (int x = 0; x < bitmap.width; x++)
                                indices[x + y * bitmap.width] = (byte)NearestColorRGB(byUsage, bitmap.GetPixel(x, y));

                        return new TextureByteIndexPalette(name, bitmap.width, bitmap.height, indices, byUsage);
                    }
                case ColorPaletteAlgorithm.BasicPaletteNearestRGB:
                    {
                        if (targetColors != 256)
                            throw new ArgumentOutOfRangeException("targetColors");

                        var palette = BasicPalette;

                        // Get nearest pixel by RGB
                        for (int y = 0; y < bitmap.height; y++)
                            for (int x = 0; x < bitmap.width; x++)
                                indices[x + y * bitmap.width] = (byte)NearestColorRGB(palette, bitmap.GetPixel(x, y));

                        return new TextureByteIndexPalette(name, bitmap.width, bitmap.height, indices, palette);
                    }
            }
            throw new NotSupportedException();
        }

        private static int NearestColorRGB(Color[] palette, Color color)
        {
            var nearest = palette.OrderBy((c) =>
            {
				var r = Math.Abs(color.r - c.r);
				var g = Math.Abs(color.g - c.g);
				var b = Math.Abs(color.b - c.b);

                return r + g + b;
            }).First();
            return Array.IndexOf(palette, nearest);
        }

        private static Color[] _basicPalette = null;
        public static Color[] BasicPalette
        {
            get
            {
                if (_basicPalette == null)
                {
                    _basicPalette = new Color[256];

					_basicPalette[255] = Color.blue;

                    for (int i = 0; i < 15; i++)
                    {
                        int gray = (int)(i / 14f * 256);
                        _basicPalette[254 - i] = new Color(gray / 255f, gray / 255f, gray / 255f);
                    }

                    for (int i = 0; i < 16; i++)
                    {
                        float a = i / 15f;
                        float b = a / 2;

                        _basicPalette[255 - 1 * 16 - i] = new Color(a, 0, 0);
                        _basicPalette[255 - 2 * 16 - i] = new Color(0, a, 0);
                        _basicPalette[255 - 3 * 16 - i] = new Color(0, 0, a);

                        _basicPalette[255 - 4 * 16 - i] = new Color(a, a, 0);
                        _basicPalette[255 - 5 * 16 - i] = new Color(a, 0, a);
                        _basicPalette[255 - 6 * 16 - i] = new Color(0, a, a);

                        _basicPalette[255 - 7 * 16 - i] = new Color(a, b, 0);
                        _basicPalette[255 - 8 * 16 - i] = new Color(a, 0, b);

                        _basicPalette[255 - 9 * 16 - i] = new Color(b, a, 0);
                        _basicPalette[255 - 10 * 16 - i] = new Color(0, a, b);

                        _basicPalette[255 - 11 * 16 - i] = new Color(b, 0, a);
                        _basicPalette[255 - 12 * 16 - i] = new Color(0, b, a);

                        _basicPalette[255 - 10 * 16 - i] = new Color(a, a, b);
                        _basicPalette[255 - 11 * 16 - i] = new Color(a, b, a);
                        _basicPalette[255 - 12 * 16 - i] = new Color(b, a, a);

                        _basicPalette[255 - 13 * 16 - i] = new Color(b, b, a);
                        _basicPalette[255 - 14 * 16 - i] = new Color(b, a, b);
						_basicPalette[255 - 15 * 16 - i] = new Color(a, b, b);
                    }
                }
                return _basicPalette;
            }
        }

        public TextureByteIndexPaletteWithMipmaps GenerateMipmaps(int levels, MipmapAlgorithm algorithm = MipmapAlgorithm.FirstPixel)
        {
            var texture = new TextureByteIndexPaletteWithMipmaps(Name, Width, Height, Indices, Palette);

            var width = Width;
            var height = Height;

            for (int level = 1; level < levels; level++)
            {
                var prevMipMap = texture.GetMipmap(level);
                width /= 2;
                height /= 2;

                var indices = new byte[width * height];

                switch (algorithm)
                {
                    case MipmapAlgorithm.FirstPixel:
                        {
                            int prevWidth = (2 * width);

                            for (int y = 0; y < height; y++)
                                for (int x = 0; x < width; x++)
                                    indices[x + y * width] = prevMipMap[2 * x + 2 * y * prevWidth];
                        }
                        break;
                    /*
                case MipmapAlgorithm.AveragePixel:
                    {
                        int prevWidth = (2 * width);

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var c0 = prevMipMap[2 * (x) + 2 * (y) * prevWidth];
                                var c1 = prevMipMap[2 * (x+1) + 2 * (y) * prevWidth];
                                var c2 = prevMipMap[2 * (x) + 2 * (y+1) * prevWidth];
                                var c3 = prevMipMap[2 * (x+1) + 2 * (y+1) * prevWidth];

                                indices[x + y * width] = ???;
                            }
                        }
                    }
                    break;
                    */
                    case MipmapAlgorithm.CropCenter:
                        {
                            int cx = width / 4;
                            int cy = height / 4;
                            int prevWidth = (2 * width);

                            for (int y = 0; y < height; y++)
                                for (int x = 0; x < width; x++)
                                    indices[x + y * width] = prevMipMap[(cx + x) + (y + cy) * prevWidth];
                        }
                        break;
                }

                texture.AddMipmap(level, indices);
            }

            return texture;
        }

        #endregion

    }
}