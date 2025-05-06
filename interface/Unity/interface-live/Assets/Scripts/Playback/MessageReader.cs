
using Google.Protobuf;
using Protobuf;
using System;
using System.IO;
using System.IO.Compression;

namespace Playback
{
    public class FileFormatNotLegalException : Exception
    {
        public FileFormatNotLegalException()
        {

        }
        public override string Message => $"The file is not a legal playback file for THUAI6.";
    }

    public class MessageReader : IDisposable
    {
        private MemoryStream stream;
        private CodedInputStream cos;
        private GZipStream gzs;
        private byte[] buffer;
        public bool Finished { get; private set; } = false;

        public readonly uint teamCount;
        public readonly uint playerCount;

        const int bufferMaxSize = 10 * 1024 * 1024;       // 10M

        public MessageReader(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
            var prefixLen = PlayBackConstant.Prefix.Length;
            byte[] bt = new byte[prefixLen + sizeof(UInt32) * 2];
            stream.Read(bt, 0, bt.Length);
            for (int i = 0; i < prefixLen; ++i)
            {
                if (bt[i] != PlayBackConstant.Prefix[i]) throw new FileFormatNotLegalException();
            }

            teamCount = BitConverter.ToUInt32(bt, prefixLen);
            playerCount = BitConverter.ToUInt32(bt, prefixLen + sizeof(UInt32));

            gzs = new GZipStream(stream, CompressionMode.Decompress);
            var tmpBuffer = new byte[bufferMaxSize];
            var bufferSize = gzs.Read(tmpBuffer);
            if (bufferSize == 0)
            {
                buffer = tmpBuffer;
                Finished = true;
            }
            else if (bufferSize != bufferMaxSize)       // 不留空位，防止 CodedInputStream 获取信息错误
            {
                if (bufferSize == 0)
                {
                    Finished = true;
                }
                buffer = new byte[bufferSize];
                Array.Copy(tmpBuffer, buffer, bufferSize);
            }
            else
            {
                buffer = tmpBuffer;
            }
            cos = new CodedInputStream(buffer);
        }

        public MessageToClient? ReadOne()
        {
        beginRead:
            if (Finished)
                return null;
            var pos = cos.Position;
            try
            {
                MessageToClient? msg = new MessageToClient();
                cos.ReadMessage(msg);
                return msg;
            }
            catch (InvalidProtocolBufferException)
            {
                var leftByte = buffer.Length - pos;     // 上次读取剩余的字节
                if (buffer.Length < bufferMaxSize / 2)
                {
                    var newBuffer = new byte[bufferMaxSize];
                    for (int i = 0; i < leftByte; i++)
                    {
                        newBuffer[i] = buffer[pos + i];
                    }
                    buffer = newBuffer;
                }
                else
                {
                    for (int i = 0; i < leftByte; ++i)
                    {
                        buffer[i] = buffer[pos + i];
                    }
                }
                var bufferSize = gzs.Read(buffer, (int)leftByte, (int)(buffer.Length - leftByte)) + leftByte;
                if (bufferSize == leftByte)
                {
                    Finished = true;
                    return null;
                }
                if (bufferSize != buffer.Length)        // 不留空位，防止 CodedInputStream 获取信息错误
                {
                    var tmpBuffer = new byte[bufferSize];
                    Array.Copy(buffer, tmpBuffer, bufferSize);
                    buffer = tmpBuffer;
                }
                cos = new CodedInputStream(buffer);
                goto beginRead;
            }
        }

        public void Dispose()
        {
            Finished = true;
            if (stream == null)
                return;
            if (stream.CanRead)
            {
                stream.Close();
            }
        }

        ~MessageReader()
        {
            Dispose();
        }
    }
}