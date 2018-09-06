using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Single;
using System.IO.Compression;

namespace Meteo.IO
{
    public class InputFile : MatrixFile
    {
        const ushort GZipMagic = 0x8b1f;


        public InputFile(string fileName)
            : base(fileName)
        {
        }

        protected override void InternalLoad()
        {
            try
            {
                string[] lines = null;
                bool decompress = false;

                using (FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
                {
                    // Detect if we're reading a compressed GZIP file
                    BinaryReader br = new BinaryReader(fileStream);
                    ushort header = br.ReadUInt16();
                    decompress = (header == GZipMagic);

                    // Move back to the beginning of the file
                    fileStream.Seek(0, SeekOrigin.Begin);

                    if (decompress)
                    {
                        using (GZipStream zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                        {
                            List<byte> bytes = new List<byte>();

                            int b = 0;
                            while (b >= 0)
                            {
                                b = zipStream.ReadByte();
                                if (b < 0)
                                    break;

                                bytes.Add((byte)b);
                            }

                            string str = Encoding.ASCII.GetString(bytes.ToArray()).Replace("\r\n", "\n");
                            lines = str.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        }
                    }
                    else
                    {
                        lines = File.ReadAllLines(_fileName);
                    }
                }

                string[] elements = lines[0].Split(" ,".ToCharArray());

                _matrix = new DenseMatrix(lines.Length, elements.Length);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    elements = line.Split(" ,".ToCharArray());

                    for (int j = 0; j < elements.Length; j++)
                    {
                        float elemValue = 0;
                        float.TryParse(elements[j], out elemValue);
                        _matrix.At(i, j, elemValue);
                    }
                }
            }
            catch(Exception ex)
            {
                _matrix = null;
            }
        }
    }
}
