﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.WinTrust;
using Utilizr.Win32.WinTrust.Structs;
using DWORD = System.UInt32;

namespace Utilizr.Win.Security
{
    public class WinCatalog
    {
        // Catalog Version is (0X100 = 256) for Catalog Version 1
        private static readonly int _catalogVersion1 = 256;

        // Catalog Version is (0X200 = 512) for Catalog Version 2
        private static readonly int _catalogVersion2 = 512;

        /// <summary>
        /// Make list of hashes for given Catalog File
        /// </summary>
        /// <param name="catalogFilePath"> Path to the folder having catalog file </param>
        /// <param name="excludedPatterns"></param>
        /// <param name="catalogVersion"> The version of input catalog we read from catalog meta data after opening it.</param>
        /// <returns> Dictionary mapping files relative paths to HashValues </returns>
        public static IEnumerable<string> GetHashesFromCatalog(string catalogFilePath)
        {
            IntPtr resultCatalog = WinTrust.CryptCATOpen(catalogFilePath, 0, IntPtr.Zero, 1, 0);
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            int catalogVersion = 0;

            if (resultCatalog != INVALID_HANDLE_VALUE)
            {
                try
                {
                    catalogVersion = GetCatalogVersion(resultCatalog);

                    IntPtr memberInfo = IntPtr.Zero;
                    // Navigate all members in Catalog files and get their relative paths and hashes
                    do
                    {
                        memberInfo = WinTrust.CryptCATEnumerateMember(resultCatalog, memberInfo);
                        if (memberInfo != IntPtr.Zero)
                        {
                            CRYPTCATMEMBER currentMember = (CRYPTCATMEMBER)Marshal.PtrToStructure(memberInfo, typeof(CRYPTCATMEMBER));
                            SIP_INDIRECT_DATA pIndirectData = (SIP_INDIRECT_DATA)Marshal.PtrToStructure(currentMember.pIndirectData, typeof(SIP_INDIRECT_DATA));

                            // For Catalog version 2 CryptoAPI puts hashes of file attributes(relative path in our case) in Catalog as well
                            // We validate those along with file hashes so we are skipping duplicate entries
                            if (!((catalogVersion == 2) && (pIndirectData.DigestAlgorithm.pszObjId.Equals(new Oid("SHA1").Value, StringComparison.OrdinalIgnoreCase))))
                            {
                                yield return currentMember.pwszReferenceTag;
                            }
                        }
                    } while (memberInfo != IntPtr.Zero);
                }
                finally
                {
                    WinTrust.CryptCATClose(resultCatalog);
                }
            }
            else
            {
                throw new Exception("Unable to open catalog file");
            }
        }

        /// <summary>
        /// Find out the Version of Catalog by reading its Meta data. We can have either version 1 or version 2 catalog
        /// </summary>
        /// <param name="catalogHandle"> Handle to open catalog file </param>
        /// <returns> Version of the catalog </returns>
        private static int GetCatalogVersion(IntPtr catalogHandle)
        {
            int catalogVersion = -1;

            IntPtr catalogData = WinTrust.CryptCATStoreFromHandle(catalogHandle);
            CRYPTCATSTORE catalogInfo = (CRYPTCATSTORE)Marshal.PtrToStructure(catalogData, typeof(CRYPTCATSTORE));

            if (catalogInfo.dwPublicVersion == _catalogVersion2)
            {
                catalogVersion = 2;
            }
            // One Windows 7 this API sent version information as decimal 1 not hex (0X100 = 256)
            // so we are checking for that value as well. Reason we are not checking for version 2 above in
            // this scenario because catalog version 2 is not supported on win7.
            else if ((catalogInfo.dwPublicVersion == _catalogVersion1) || (catalogInfo.dwPublicVersion == 1))
            {
                catalogVersion = 1;
            }
            else
            {
                throw new Exception("Unknown catalog version: " + catalogInfo.dwPublicVersion);
            }
            return catalogVersion;
        }

        /// <summary>
        /// Make a hash for the file
        /// </summary>
        /// <param name="filePath"> Path of the file </param>
        /// <param name="hashAlgorithm"> Used to calculate Hash </param>
        /// <returns> HashValue for the file </returns>
        public static string CalculateFileHash(string filePath, string hashAlgorithm)
        {
            string hashValue = string.Empty;
            IntPtr catAdmin = IntPtr.Zero;

            // To get handle to the hash algorithm to be used to calculate hashes
            if (!WinTrust.CryptCATAdminAcquireContext2(ref catAdmin, IntPtr.Zero, hashAlgorithm, IntPtr.Zero, 0))
            {
                throw new Exception("Unable to acquire hash algorithm context");
            }

            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            // Open the file that is to be hashed for reading and get its handle
            IntPtr fileHandle = Kernel32.CreateFileW(
                filePath,
                FileAccessRightsFlags.GENERIC_READ,
                FileShareRightsFlags.FILE_SHARE_NONE,
                IntPtr.Zero,
                FileCreationDispositionFlags.OPEN_EXISTING,
                FileAttributeFlags.NORMAL,
                IntPtr.Zero
            );
            if (fileHandle != INVALID_HANDLE_VALUE)
            {
                try
                {
                    DWORD hashBufferSize = 0;
                    IntPtr hashBuffer = IntPtr.Zero;

                    // Call first time to get the size of expected buffer to hold new hash value
                    if (!WinTrust.CryptCATAdminCalcHashFromFileHandle2(catAdmin, fileHandle, ref hashBufferSize, hashBuffer, 0))
                    {
                        throw new Exception("Unable to create file hash");
                    }

                    int size = (int)hashBufferSize;
                    hashBuffer = Marshal.AllocHGlobal(size);
                    try
                    {
                        // Call second time to actually get the hash value
                        if (!WinTrust.CryptCATAdminCalcHashFromFileHandle2(catAdmin, fileHandle, ref hashBufferSize, hashBuffer, 0))
                        {
                            throw new Exception("Unable to create file hash");
                        }

                        byte[] hashBytes = new byte[size];
                        Marshal.Copy(hashBuffer, hashBytes, 0, size);
                        hashValue = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                    }
                    finally
                    {
                        if (hashBuffer != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(hashBuffer);
                        }
                    }
                }
                finally
                {
                    WinTrust.CryptCATAdminReleaseContext(catAdmin, 0);
                    Kernel32.CloseHandle(fileHandle);
                }
            }
            else
            {
                throw new Exception("Unable to open file to hash");
            }
            return hashValue;
        }
    }
}