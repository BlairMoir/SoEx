using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using SoEx.Abstractions;

namespace SoEx.Hosting.Serializers.DataContract;


public class DataContractMessageSerializer : IMessageSerializer
{
    DataContractSerializerSettings _serializerSettings;
    private ContractResolver _resolver;

    public DataContractMessageSerializer(KnownTypes knownTypes)
    {
        _resolver = new ContractResolver(new ContractLookup(knownTypes.Types.ToArray()));
        _serializerSettings = new DataContractSerializerSettings()
        {
            DataContractResolver = _resolver
        };
    }

    public T? Deserialize<T>(byte[] @bytes)
    {
        var utf8String = Encoding.UTF8.GetString(bytes);
        using (XmlReader reader = XmlReader.Create(new StringReader(utf8String)))
        {
            DataContractSerializer formatter0 =
                new DataContractSerializer(typeof(T), _serializerSettings);
            return (T?)formatter0.ReadObject(reader);
        }
    }

    public byte[] Serialize<T>(T? @object)
    {
        StringWriter stringWriter = new StringWriter();
        using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
        {
            DataContractSerializer formatter0 =
                new DataContractSerializer(typeof(T), _serializerSettings);
            formatter0.WriteObject(xmlWriter, @object);
        }
        var writtenString = stringWriter.ToString();
        return Encoding.UTF8.GetBytes(writtenString);
    }
}

