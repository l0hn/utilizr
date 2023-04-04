using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Utilizr.Globalisation.Extensions;

namespace Utilizr.Globalisation.Parsers
{
    public static class MOParser
    {
        //const uint LE_MAGIC = 0x950412de;
        //const uint BE_MAGIC = 0xde120495;

        public static MoParseResult Parse(Stream inputStream)
        {
            var d = new Dictionary<string, string>();
            int headerLength = Marshal.SizeOf(typeof(MoHeader));
            inputStream.Position = 0;
            byte[] header = new byte[headerLength];
            byte[] data = new byte[inputStream.Length];

            inputStream.Read(data, 0, data.Length);
            Array.Copy(data, header, headerLength);

            //todo: handle big-endian systems (some day)
            var moHeader = BytesEx.ToStructureHostEndian<MoHeader>(header);
            //get all the lovely strings
            int stringInfoLength = Marshal.SizeOf(typeof(MOStringInfo));
            byte[] stringInfo = new byte[stringInfoLength];
            MOStringInfo sInfo;
            string[] nStrParts;
            for (int i = 0; i < moHeader!.NUMBER_OF_STRINGS; i++)
            {
                //--original string
                //get the string info
                Array.Copy(data, moHeader.OFFSET_OF_TABLE_WITH_ORIGINAL_STRINGS + (i * stringInfoLength), stringInfo, 0, stringInfoLength);
                sInfo = BytesEx.ToStructureHostEndian<MOStringInfo>(stringInfo);
                //ignore metadata portion
                if (sInfo.LENGTH == 0) continue;
                //read the null terminated source string (only the singular is required as the key)
                string nStr = Encoding.UTF8.GetString(data, (int)sInfo.OFFSET, (int)sInfo.LENGTH);
                nStrParts = nStr.Split('\0');
                string baseKey = nStrParts[0];
                //translated string(s)
                Array.Copy(data, moHeader.OFFSET_OF_TABLE_WITH_TRANSLATION_STRINGS + (i * stringInfoLength), stringInfo, 0, stringInfoLength);
                sInfo = BytesEx.ToStructureHostEndian<MOStringInfo>(stringInfo);
                //read null terminated translated strings
                nStr = Encoding.UTF8.GetString(data, (int)sInfo.OFFSET, (int)sInfo.LENGTH);
                nStrParts = nStr.Split('\0');
                for (int j = 0; j < nStrParts.Length; j++)
                {
                    string key = j == 0 ? baseKey : baseKey + j;
                    d[key] = nStrParts[j];
                }
            }
            return new MoParseResult(moHeader, d);
        }
    }

    public class MoParseResult
    {
        public MoHeader MoHeader { get; set; }
        public Dictionary<string, string> TranslationDictionary { get; set; }

        public MoParseResult(MoHeader header, Dictionary<string, string> translationDictionary)
        {
            MoHeader = header;
            TranslationDictionary = translationDictionary;
        }
    }

    public struct MoHeader
    {
        public uint MAGIC_NUMBER;
        public uint FILE_FORMAT_REVISION;
        public uint NUMBER_OF_STRINGS;
        public uint OFFSET_OF_TABLE_WITH_ORIGINAL_STRINGS;
        public uint OFFSET_OF_TABLE_WITH_TRANSLATION_STRINGS;
        public uint SIZE_OF_HASHING_TABLE;
        public uint OFFSET_OF_HASHING_TABLE;
    }

    public struct MOStringInfo
    {
        public uint LENGTH;
        public uint OFFSET;
    }
}
