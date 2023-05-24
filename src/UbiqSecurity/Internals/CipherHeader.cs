using System;
using System.IO;
using System.Net;

namespace UbiqSecurity.Internals
{
    /// <summary>
    /// Binary header for Ubiq ciphertext.
    /// </summary>
    /// <remarks>
    /// The first six bytes form a fixed-length record, which indicates the length
    /// of two variable-length fields that follow.
    /// </remarks>
    internal class CipherHeader
    {
        #region Constants
        /// <summary>
        /// Definitions for <see cref="Flags"/> bit field.
        /// </summary>
        internal const byte FLAGS_AAD_ENABLED = 0x01;
        internal const byte FLAGS_RESERVED_1 =  0x02;
        internal const byte FLAGS_RESERVED_2 =  0x04;
        internal const byte FLAGS_RESERVED_3 =  0x08;
        internal const byte FLAGS_RESERVED_4 =  0x10;
        internal const byte FLAGS_RESERVED_5 =  0x20;
        internal const byte FLAGS_RESERVED_6 =  0x40;
        internal const byte FLAGS_RESERVED_7 =  0x80;
        #endregion

        #region Properties
        internal byte Version { get; set; }

        internal byte Flags { get; set; }

        internal byte AlgorithmId { get; set; }

        /// <summary>Length of Initialization Vector, in bytes.</summary>
        internal byte InitVectorLength { get; set; }

        internal short EncryptedDataKeyLength { get; set; }

        /// <summary>
        /// Variable-length buffer of size <see cref="InitVectorLength"/>
        /// </summary>
        internal byte[] InitVectorBytes { get; set; }

        /// <summary>
        /// Variable-length buffer of size <see cref="EncryptedDataKeyLength"/>
        /// </summary>
        internal byte[] EncryptedDataKeyBytes { get; set; }
        #endregion

        #region Methods
        internal int Length()
        {
            // start with fixed-length parts
            var length = (sizeof(byte) * 4) + sizeof(short);

            // add variable-length parts
            if (InitVectorBytes != null)
            {
                length += InitVectorBytes.Length;
            }

            if (EncryptedDataKeyBytes != null)
            {
                length += EncryptedDataKeyBytes.Length;
            }

            return length;
        }

        internal static CipherHeader Deserialize(Stream stream)
        {
            if (stream.Position != 0)
            {
                throw new ArgumentException("stream not rewound", nameof(stream));
            }
            else if (!stream.CanRead)
            {
                throw new ArgumentException("stream not readable", nameof(stream));
            }
            else if (stream.Length < 6)
            {
                // not enough bytes for fixed-length part
                return null;
            }

            // decode the fixed-length part:
            //  4 x 1-byte
            //  1 x 2-byte
            var cipherHeader = new CipherHeader()
            {
                Version = (byte)stream.ReadByte(),
                Flags = (byte)stream.ReadByte(),
                AlgorithmId = (byte)stream.ReadByte(),
                InitVectorLength = (byte)stream.ReadByte()
            };

            if (cipherHeader.Version != 0)
            {
                throw new InvalidDataException("invalid encryption header version");
            }

            // tricky: read two-byte value in big-endian, then convert to little-endian
            var bigEndianBytes = new byte[2];
            stream.Read(bigEndianBytes, 0, bigEndianBytes.Length);
            cipherHeader.EncryptedDataKeyLength = IPAddress.NetworkToHostOrder(
                BitConverter.ToInt16(bigEndianBytes, 0));

            // at this point, the fixed-length header can tell us if the
            // source stream contains the remaining variable-length fields
            if ((stream.Length - stream.Position) <
                (cipherHeader.InitVectorLength + cipherHeader.EncryptedDataKeyLength))
            {
                // not enough bytes for variable-length part
                return null;
            }

            // good to go... read remainder of header
            cipherHeader.InitVectorBytes = new byte[cipherHeader.InitVectorLength];
            stream.Read(cipherHeader.InitVectorBytes, 0, cipherHeader.InitVectorBytes.Length);

            cipherHeader.EncryptedDataKeyBytes = new byte[cipherHeader.EncryptedDataKeyLength];
            stream.Read(cipherHeader.EncryptedDataKeyBytes, 0, cipherHeader.EncryptedDataKeyBytes.Length);

            // note: at this point, the Stream.Position tells the caller how many bytes
            // were pulled from the Stream
            return cipherHeader;
        }

        internal byte[] Serialize()
        {
            using (var headerStream = new MemoryStream())
            {
                // note: the header blob assumes network (big-endian) byte order
                using (var writer = new BinaryWriter(headerStream))
                {
                    writer.Write(Version);
                    writer.Write(Flags);
                    writer.Write(AlgorithmId);
                    writer.Write(InitVectorLength);

                    // tricky: write two-byte value in big-endian order
                    writer.Write(IPAddress.HostToNetworkOrder(EncryptedDataKeyLength));

                    // write randomly-generated init vector
                    writer.Write(InitVectorBytes);

                    // write server-provided EncryptedDataKey
                    writer.Write(EncryptedDataKeyBytes);

                    writer.Flush();

                    return headerStream.ToArray();
                }
            }
        }
        #endregion
    }
}
