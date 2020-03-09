﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DIV2Tools
{
    /// <summary>
    /// PAL file.
    /// </summary>
    public class PAL
    {
        #region Constants
        const string HEADER_ID = "pal";
        static readonly byte[] HEADER_SIGNATURE = { 0x1A, 0x0D, 0x0A, 0x00 };
        const byte HEADER_VERSION = 0;
        #endregion

        #region Structs
        /// <summary>
        /// Header description.
        /// </summary>
        struct Header // 8 bytes.
        {
            #region Public vars
            public char[] id;           // 3 bytes.
            public byte[] signature;    // 4 bytes.
            public byte version;
            #endregion

            #region Constructor
            public Header(BinaryReader file)
            {
                this.id = file.ReadChars(3);
                this.signature = file.ReadBytes(4);
                this.version = file.ReadByte();
            }
            #endregion

            #region Methods & Functions
            public bool Check()
            {
                return new string(this.id).Equals(PAL.HEADER_ID) &&
                       BitConverter.ToUInt32(this.signature) == BitConverter.ToUInt32(PAL.HEADER_SIGNATURE) &&
                       this.version == PAL.HEADER_VERSION;
            }

            public override string ToString()
            {
                return $"PAL Header:\n- Id: {new string(this.id)}\n- Signature: {BitConverter.ToString(this.signature)}\n- Version: {this.version}\n";
            } 
            #endregion
        }

        /// <summary>
        /// Color structure, with byte components in ranges from 0 to 63.
        /// </summary>
        public struct Color
        {
            #region Public vars
            public byte r, g, b;
            #endregion

            #region Constructor
            public Color(BinaryReader file)
            {
                this.r = file.ReadByte();
                this.g = file.ReadByte();
                this.b = file.ReadByte();
            }
            #endregion

            #region Methods & Functions
            public void Write(BinaryWriter file)
            {
                file.Write(this.r);
                file.Write(this.g);
                file.Write(this.b);
            }

            public override string ToString()
            {
                return $"[{this.r:00}.{this.g:00}.{this.b:00}]";
            } 
            #endregion
        }

        /// <summary>
        /// Color Ranges.
        /// </summary>
        public struct ColorRange // 36 bytes.
        {
            #region Public vars
            public byte colors;
            public byte type;
            public bool isFixed;
            public byte blackColor;
            public byte[] colorRanges; // 32 bytes. 
            #endregion

            #region Constructor
            public ColorRange(BinaryReader file)
            {
                this.colors = file.ReadByte();
                this.type = file.ReadByte();
                this.isFixed = file.ReadBoolean();
                this.blackColor = file.ReadByte();
                this.colorRanges = file.ReadBytes(32);
            }
            #endregion

            #region Method & Functions
            public void Write(BinaryWriter file)
            {
                file.Write(this.colors);
                file.Write(this.type);
                file.Write(this.isFixed);
                file.Write(this.blackColor);
                file.Write(this.colorRanges);
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"- Colors: {this.colors}\n- Type: {this.type}\n- Fixed: {this.isFixed}\n- Black color: {this.blackColor}\n");

                sb.Append("- Color Ranges:\n");
                int column = 0;
                foreach (byte color in this.colorRanges)
                {
                    sb.Append($"{color:000} ");
                    if (++column == 8)
                    {
                        column = 0;
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            } 
            #endregion
        }
        #endregion

        #region Internal vars
        Header _header;
        Color[] _colors = new Color[256];
        ColorRange[] _colorRanges = new ColorRange[16];
        #endregion

        #region Properties
        public IReadOnlyList<Color> Pallete => this._colors;
        public IReadOnlyList<ColorRange> Ranges => this._colorRanges;
        #endregion

        #region Constructor
        /// <summary>
        /// Import a PAL file.
        /// </summary>
        /// <param name="filename">PAL file.</param>
        /// <param name="verbose">Log PAL import data to console. By default is true.</param>
        public PAL(string filename, bool verbose = true)
        {
            using (var file = new BinaryReader(File.OpenRead(filename)))
            {
                Helper.Log($"Reading \"{filename}\"...\n", verbose);

                this._header = new Header(file);
                Helper.Log(this._header.ToString(), verbose);

                if (this._header.Check())
                {
                    this._colors = PAL.ReadPallete(file);
                    Helper.Log(PAL.PrintPallete(this._colors), verbose);

                    this._colorRanges = PAL.ReadColorRanges(file);
                    Helper.Log(PAL.PrintColorRanges(this._colorRanges), verbose);
                }
                else
                {
                    throw new FormatException("Invalid PAL file!");
                }
            }
        }
        #endregion

        #region Methods & Functions
        /// <summary>
        /// Read all colors from a pallete.
        /// </summary>
        /// <param name="file"><see cref="BinaryReader"/> instance of the PAL or compatible file format that contain pallete data.</param>
        /// <returns>Returns all 256 colors from pallete in a <see cref="Color"/> array.</returns>
        /// <remarks>The <see cref="BinaryReader"/> instance must be setup in the first byte of the pallete array in the opened file.</remarks>
        public static Color[] ReadPallete(BinaryReader file)
        {
            var colors = new Color[256];
            
            for (int i = 0; i < 256; i++)
            {
                colors[i] = new Color(file);
            }

            return colors;
        }

        /// <summary>
        /// String representation of the color pallete.
        /// </summary>
        /// <param name="pallete"><see cref="Color"/> array of 256 elements.</param>
        /// <returns>Returns a formatted string table representation of the color pallete to print in console.</returns>
        public static string PrintPallete(Color[] pallete)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Color table:");

            int column = 0;
            for (int i = 0; i < 256; i++)
            {
                sb.Append($"{i:000}:{pallete[i].ToString()} ");
                if (++column == 8)
                {
                    column = 0;
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Read the 16 color ranges associated to a color pallete.
        /// </summary>
        /// <param name="file"><see cref="BinaryReader"/> instance of the PAL or compatible file format that contain color range array data.</param>
        /// <returns>Returns the 16 color ranges from a pallete in a <see cref="ColorRange"/> array.</returns>
        /// <remarks>The <see cref="BinaryReader"/> instance must be setup in the first byte of the first color range structure in the opened file.</remarks>
        public static ColorRange[] ReadColorRanges(BinaryReader file)
        {
            var colorRanges = new ColorRange[16];

            for (int i = 0; i < 16; i++)
            {
                colorRanges[i] = new ColorRange(file);
            }

            return colorRanges;
        }

        /// <summary>
        /// String representation of the color ranges associated to a pallete.
        /// </summary>
        /// <param name="ranges"><see cref="ColorRange"/> array of 16 elements.</param>
        /// <returns>Returns a formatted string table representation of the color ranges associated to a pallete to print in console.</returns>
        public static string PrintColorRanges(ColorRange[] ranges)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Color Ranges:");

            for (int i = 0; i < 16; i++)
            {
                sb.AppendLine($"Range {i:00}:\n{ranges[i].ToString()} ");
            }

            return sb.ToString();
        } 
        #endregion
    }
}
