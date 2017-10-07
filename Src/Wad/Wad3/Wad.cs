using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BspLib.Wad.Wad3
{
    public static class Wad
    {

        #region Parsing

        public static void Load(WadFile wad, Stream stream)
        {
            if (wad == null)
                throw new ArgumentNullException("wad");
            if (stream == null)
                throw new ArgumentOutOfRangeException("stream");

            var reader = new BinaryReader(stream);

            var startOffset = stream.Position;// Position of start in stream (current - version)

            // Validate version
            uint version = reader.ReadUInt32();
            if (version != Version)
                throw new BspLib.Wad.Exceptions.WadVersionNotSupportedException(version);

            // Header
            int num = reader.ReadInt32(); // Number of entries, not textures
            int offset = reader.ReadInt32();

            var entries = new List<WadEntry>();

            // Offset of entries
            // Often bedore end of file
            stream.Position = offset + startOffset;
            for (int i = 0; i < num; i++)
            {
                var entry = WadEntry.Read(reader);
                // Is a texture
                if (entry.Type == WadEntry.TextureType)
                {
                    // Compressed
                    if (entry.Compressed)
                    {
                        // Could not find how it works (no documentation).
                        // In official github ( https://github.com/ValveSoftware/halflife/blob/5d761709a31ce1e71488f2668321de05f791b405/utils/common/wadlib.c on line 301 ):
                        //    // F I X M E: do compression
                        Console.Error.WriteLine("WadEntry ID={0}, Name='{1}' is compressed texture which is not supported. Skipping.", i, entry.Name_s);
                    }
                    // Not Compressed
                    else
                        entries.Add(entry);
                }
                // Not a texture
                else
                {
                    Console.Error.WriteLine("WadEntry ID={0}, Name='{1}' has unknown / unsupported type 0x{2:X2}.", i, entry.Name_s, entry.Type);
                }
            }

            // Load all textures
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                stream.Position = entry.PositionInFile + startOffset;

                var texture_info = TextureLumpInfo.Load(reader);
                var texture = texture_info.LoadTexture(reader, entry.PositionInFile);

                wad.Textures.Add(texture);
            }
        }

        #endregion

        /// <summary>
        /// 0x57 = W,
        /// 0x41 = A,
        /// 0x44 = D,
        /// 0x33 = 3
        /// </summary>
        public const uint Version = 0x33444157;

        class WadEntry
        {
            public WadEntry(int positionInFile, int size, byte type, char[] name) : this(positionInFile, size, size, type, false, name)
            {
            }
            public WadEntry(int positionInFile, int size, byte type, byte[] name) : this(positionInFile, size, size, type, false, name)
            {
            }

            public WadEntry(int positionInFile, int size, int uncompressedSize, byte type, bool compressed, char[] name) : this(positionInFile, size, uncompressedSize, type, compressed, 0, name)
            {
            }
            public WadEntry(int positionInFile, int size, int uncompressedSize, byte type, bool compressed, byte[] name) : this(positionInFile, size, uncompressedSize, type, compressed, 0, name)
            {
            }

            public WadEntry(int positionInFile, int size, int uncompressedSize, byte type, bool compressed, ushort dummy, char[] name)
            {
                this.PositionInFile = positionInFile;
                this.Size = size;
                this.UncompressedSize = uncompressedSize;
                this.Type = type;
                this.Compressed = compressed;
                this.Dummy = dummy;

                if (name.Length != NameLength)
                    throw new ArgumentOutOfRangeException("name");

                this.Name = name;
            }
            public WadEntry(int positionInFile, int size, int uncompressedSize, byte type, bool compressed, ushort dummy, byte[] name)
            {
                this.PositionInFile = positionInFile;
                this.Size = size;
                this.UncompressedSize = uncompressedSize;
                this.Type = type;
                this.Compressed = compressed;
                this.Dummy = dummy;

                if (name.Length != NameLength)
                    throw new ArgumentOutOfRangeException("name");

                var n = new char[name.Length];
                for (int i = 0; i < n.Length; i++)
                    n[i] = (char)name[i];
                this.Name = n;
            }

            /// <summary>
            /// Type used for textures
            /// </summary>
            public const byte TextureType = 0x43;//67

            /// <summary>
            /// Offset in WAD file
            /// </summary>
            public int PositionInFile;

            /// <summary>
            /// Size in file
            /// </summary>
            public int Size;

            /// <summary>
            /// Uncompressed size in file
            /// </summary>
            public int UncompressedSize;

            /// <summary>
            /// Type of entry
            /// </summary>
            public byte Type;

            /// <summary>
            /// Are textures compressed
            /// </summary>
            public bool Compressed;

            /// <summary>
            /// Not used bytes
            /// </summary>
            public ushort Dummy;

            /// <summary>
            /// Null terminated string.
            /// Length = NameLength (16)
            /// </summary>
            public char[] Name;
            public string Name_s
            {
                get
                {
                    return new string(Name, 0, Array.IndexOf(Name, '\0'));//THINK new string(char*)
                }
            }

            public const int NameLength = 16;

            public static WadEntry Read(BinaryReader reader)
            {
                return new WadEntry(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadByte(), reader.ReadByte() != 0, reader.ReadUInt16(), reader.ReadBytes(16));
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write(PositionInFile);
                writer.Write(Size);
                writer.Write(UncompressedSize);
                writer.Write(Type);
                writer.Write(Dummy);

                writer.Flush();

                for (int i = 0; i < NameLength; i++)
                    writer.Write(Name[i]);

                writer.Flush();
            }
        }

        #region Parsing texture list

        public static string[] GetTextureList(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var reader = new BinaryReader(stream);

            var startOffset = stream.Position;

            // Validate version
            uint version = reader.ReadUInt32();
            if (version != Version)
                throw new BspLib.Wad.Exceptions.WadVersionNotSupportedException(version);

            int num = reader.ReadInt32();
            int offset = reader.ReadInt32();

            List<string> names = new List<string>(num);

            stream.Position = offset + startOffset;
            for (int i = 0; i < num; i++)
            {
                var entry = WadEntry.Read(reader);
                // Is a texture
                if (entry.Type == WadEntry.TextureType)
                {
                    // Compressed
                    if (entry.Compressed)
                    {
                        // Could not find how it works (no documentation).
                        // In official github ( https://github.com/ValveSoftware/halflife/blob/5d761709a31ce1e71488f2668321de05f791b405/utils/common/wadlib.c on line 301 ):
                        //    // F I X M E: do compression
                        Console.Error.WriteLine("WadEntry ID={0}, Name='{1}' is compressed texture which is not supported. Skipping.", i, entry.Name_s);
                    }
                    // Not Compressed
                    else
                    {
                        names.Add(entry.Name_s);
                    }
                }
                // Not a texture
                else
                {
                    Console.Error.WriteLine("WadEntry ID={0}, Name='{1}' has unknown / unsupported type 0x{2:X2}.", i, entry.Name_s, entry.Type);
                }
            }

            return names.ToArray();
        }

        #endregion
    }
}