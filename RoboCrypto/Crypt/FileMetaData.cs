using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Xam.Applications.RoboCrypto.Sync;
using Xam.Applications.RoboCrypto.Resources;
using SecurityDriven.Inferno.Extensions;
using SecurityDriven.Inferno;
using Xam.Applications.RoboCrypto.Common;

namespace Xam.Applications.RoboCrypto.Crypt
{
    /// <summary>
    /// Provides methods to insert and remove custom meta data
    /// that is used to hold the original name of a file.
    /// </summary>
    public class FileMetaData
    {
        #region Private vars
        private const byte VerMajor = 1;
        private const byte VerMinor = 0;
        private const int SignatureBytes = 34;
        // bytes reserved for future use
        private const int ReservedBytes = 16;
        private const int MaxSourceFileNameBytes = 248;
        private const int MetaBufferSize = SignatureBytes + ReservedBytes + MaxSourceFileNameBytes + 4;
        private const int IdxFileLen = SignatureBytes + ReservedBytes;
        private const int IdxFileName = IdxFileLen + 4;
        private byte[] headerSignature;
        #endregion

        /************************************************************************/
        
        #region Constructor
        public FileMetaData()
        {
            headerSignature = new byte[SignatureBytes] 
            { 
                VerMajor, VerMinor,
                49, 70, 50, 132, 146, 200, 23, 183, 106, 143, 100, 100, 31, 205, 233, 197, 
                15, 227, 160, 66, 60, 117, 226, 95, 35, 242, 176, 214, 79, 11, 202, 28 
            };
        }
        #endregion

        /************************************************************************/
        
        #region Public Methods
        /// <summary>
        /// Adds our meta data header to the specified byte array in preparation for encryption.
        /// </summary>
        /// <param name="plainBytes">The plain bytes.</param>
        /// <param name="source">The FileInfo object that represents the file that is being encrypted.</param>
        /// <returns>A new array that has the meta data header, followed by the bytes of <paramref name="plainBytes"/>.</returns>
        public byte[] AddHeader(byte[] plainBytes, FileInfo source)
        {
            Validation.ValidateNull(plainBytes, "AddHeader.PlainBytes");
            Validation.ValidateNull(source, "AddHeader.Source");

            byte[] metaBuffer = new CryptoRandom().NextBytes(MetaBufferSize);
            Array.Copy(headerSignature, metaBuffer, SignatureBytes);
            /**
             * Put the file name in the header. This is done regardless of whether or not
             * we've hashed the original file name. Makes things simpler and we can pull
             * it out if we need it during decryption.
             * 
             * Note: There's no longer an option to NOT hash.
             */
            byte[] sourceFileNameBytes = source.Name.ToBytes();
            if (sourceFileNameBytes.Length > MaxSourceFileNameBytes)
            {
                // This is unlikely; a file name (not the whole path) would need to be greater than 126 chars in length.
                // However, if so, the consumer of this method (CandidateSource) will output an error message
                return null;
            }
            /**
             * Store the length of the source file name bytes. 
             * MaxSourceFileNameBytes is 252, so this could be cast to byte, but we'll store it 
             * as a little-endian int in case of future need.
             */
            byte[] sourceLen = BitConverter.GetBytes(sourceFileNameBytes.Length);
            if (!BitConverter.IsLittleEndian) Array.Reverse(sourceLen);

            Array.Copy(sourceLen, 0, metaBuffer, IdxFileLen, sourceLen.Length);
            Array.Copy(sourceFileNameBytes, 0, metaBuffer, IdxFileName, sourceFileNameBytes.Length);

            byte[] returnBytes = new byte[MetaBufferSize + plainBytes.Length];
            Array.Copy(metaBuffer, returnBytes, MetaBufferSize);
            Array.Copy(plainBytes, 0, returnBytes, MetaBufferSize, plainBytes.Length);
            return returnBytes;
        }

        /// <summary>
        /// Removes the meta data header.
        /// </summary>
        /// <param name="plainBytes">The plain bytes</param>
        /// <returns>A RemoveHeaderResult object that contains the original bytes (no header), and the original file name.</returns>
        public RemoveHeaderResult RemoveHeader(byte[] plainBytes)
        {
            Validation.ValidateNull(plainBytes, "RemoveHeader.PlainBytes");
            if (!IsSignatureValid(plainBytes))
            {
                throw new CryptographicException(Strings.InvalidMetadataSignature);
            }
            /**
             * Get the length of the source file bytes that are in the header
             */
            byte[] sourceLen = new byte[4];
            Array.Copy(plainBytes, IdxFileLen, sourceLen, 0, 4);
            if (!BitConverter.IsLittleEndian) Array.Reverse(sourceLen);
            int sourceBytesLen = BitConverter.ToInt32(sourceLen, 0);

            byte[] sourceBytes = new byte[sourceBytesLen];
            Array.Copy(plainBytes, IdxFileName, sourceBytes, 0, sourceBytesLen);

            string originalFileName = sourceBytes.FromBytes();
            byte[] originalBytes = new byte[plainBytes.Length - MetaBufferSize];
            Array.Copy(plainBytes, MetaBufferSize, originalBytes, 0, plainBytes.Length - MetaBufferSize);
            return new RemoveHeaderResult(originalBytes, originalFileName);
        }
        #endregion

        /************************************************************************/
        
        #region Private Methods
        /// <summary>
        /// Validates that the meta data signature is correct.
        /// </summary>
        /// <param name="plainBytes">The plain bytes</param>
        /// <returns>true if the signature is valid; otherwise, false.</returns>
        private bool IsSignatureValid(byte[] plainBytes)
        {
            if (plainBytes.Length < MetaBufferSize) return false;
            for (int k = 0; k < SignatureBytes; k++)
            {
                if (plainBytes[k] != headerSignature[k])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}