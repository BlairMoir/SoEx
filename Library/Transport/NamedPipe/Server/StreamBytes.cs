
namespace SoEx.Transport.NamedPipe;
public class StreamBytes
{
    private Stream ioStream;

    public StreamBytes(Stream ioStream)
    {
        this.ioStream = ioStream;
    }

    public byte[] ReadBytes()
    {
        int len;
        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        var inBuffer = new byte[len];
        ioStream.ReadExactly(inBuffer, 0, len);

        return inBuffer;
    }

    public int WriteBytes(byte[] bytes)
    {
        int len = bytes.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(bytes, 0, len);
        ioStream.Flush();

        return bytes.Length + 2;
    }
}
