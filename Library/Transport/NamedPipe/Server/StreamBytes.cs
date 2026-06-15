
using System.Buffers.Binary;

namespace SoEx.Transport.NamedPipe;
public class StreamBytes
{
    private Stream ioStream;
    private const int MaxBytesPerMessage = 16 * 1024 * 1024;

    public StreamBytes(Stream ioStream)
    {
        this.ioStream = ioStream;
    }

    public byte[] ReadBytes()
    {
        Span<byte> header = new byte[4];
        int length = BinaryPrimitives.ReadInt16BigEndian(header);
        if ((uint)length > MaxBytesPerMessage)
        {
            throw new InvalidDataException("Named pipe payload too large");
        }

        byte[] inBuffer = new byte[length];
        ioStream.ReadExactly(inBuffer, 0, length);

        return inBuffer;
    }

    public int WriteBytes(byte[] bytes)
    {
        if (bytes.Length > MaxBytesPerMessage)
        {
            throw new InvalidDataException("Named pipe payload too large");
        }
        Span<byte> header = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, bytes.Length);
        ioStream.Write(header);
        ioStream.Write(bytes, 0, bytes.Length);
        ioStream.Flush();

        return bytes.Length + 4;
    }
}
