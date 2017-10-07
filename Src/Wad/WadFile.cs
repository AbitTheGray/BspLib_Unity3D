using System;
using System.Collections.Generic;
using System.IO;
using BspLib.Wad.Exceptions;
using UnityEngine;

namespace BspLib.Wad
{
    public class WadFile
    {
        public WadFile()
        {
			this.Textures = new List<Texture>();
        }
		public WadFile(params Texture[] textures) : this()
        {
            this.Textures.AddRange(textures);
        }

        public List<Texture> Textures
        {
            get;
        }

        public enum WadVersion : uint
        {
            Unknown = 0,
            Wad2 = Wad.Wad2.Wad.Version,
            Wad3 = Wad.Wad3.Wad.Version,
        }

        #region Loading

        public static void Load(WadFile wad, string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Wad file was not found.");

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(wad, stream);
            }
        }

        public static void Load(WadFile wad, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var reader = new BinaryReader(stream);


            var startPos = reader.BaseStream.Position;

            //4 bytes = 1 int
            uint version = reader.ReadUInt32();

            reader.BaseStream.Position = startPos;


            switch (version)
            {
                // Wad2 (Quake I)
                case (uint)WadVersion.Wad2:
                    {
						Wad2.Wad.Load(wad, stream);
						Debug.Log("Loaded Wad2 file");
                    }
                    break;
                // Wad3 (GoldSource)
                case (uint)WadVersion.Wad3:
                    {
                        Wad3.Wad.Load(wad, stream);
						Debug.Log("Loaded Wad3 file");
                    }
                    break;
                default:
                    //throw new WadVersionNotSupportedException(version);
                    break;
            }
        }

        #endregion

        #region Parsing texture list

        public static string[] GetTextureList(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var reader = new BinaryReader(stream);


            var startPos = reader.BaseStream.Position;

            //4 bytes = 1 int
            uint version = reader.ReadUInt32();

            reader.BaseStream.Position = startPos;


            switch (version)
            {
                // Wad2 (Quake I)
                case (uint)WadVersion.Wad2:
                    {
                        return Wad2.Wad.GetTextureList(stream);
                    }
                // Wad3 (GoldSource)
                case (uint)WadVersion.Wad3:
                    {
                        return Wad3.Wad.GetTextureList(stream);
                    }
            }
            throw new WadVersionNotSupportedException(version);
        }

        #endregion

    }
}