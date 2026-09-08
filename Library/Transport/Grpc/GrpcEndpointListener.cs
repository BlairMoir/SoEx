using System.Net;
using Grpc.AspNetCore.Server.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using SoEx.Transport.Grpc.Protection;

namespace SoEx.Transport.Grpc;

public class GrpcEndpointListener
{
    public abstract record ListenAddress(int Port)
    {
        public record Any(int Port) : ListenAddress(Port);
        public record IP(IPAddress Address, int Port): ListenAddress(Port);

        public static ListenAddress From(GrpcConfig config)
        {
            if (string.IsNullOrEmpty(config.BindAddress))
                return new Any(config.Port);

            if (config.BindAddress is "*" or "+" or "0.0.0.0")
                return new Any(config.Port);

            return new IP(IPAddress.Parse(config.BindAddress), config.Port);
        }

        public bool ConflictsWith(ListenAddress other)
        {
            return other.Port == Port && !Equals(other);
        }
    }

    private readonly Dictionary<ListenAddress, GrpcProtection> _listenEndpoints = new();

    private WebApplication? _listener;
    private List<IServiceMethodProvider<GrpcEndpointService>> _dispatchProviders = new();

    internal void Bind(GrpcConfig[] configs)
    {
        foreach (var config in configs)
        {
            Bind(config);
        }
    }

    private void Bind(GrpcConfig config)
    {
        var listenAddress = ListenAddress.From(config);
        if (_listenEndpoints.TryGetValue(listenAddress, out var protection))
        {
            if(!protection.Equals(config.Protection))
                throw new InvalidOperationException($"Protection {config.Protection} does not match protection {protection} for {listenAddress}");

            return;
        }

        var existing = _listenEndpoints.Keys.FirstOrDefault(k => k.ConflictsWith(listenAddress));
        if (existing is not null)
        {
            throw new InvalidOperationException($"Listen address {listenAddress} conflicts with {existing}");
        }
        _listenEndpoints.Add(listenAddress, config.Protection);
    }

    internal void RegisterDispatch(IServiceMethodProvider<GrpcEndpointService> provider)
    {
        _dispatchProviders.Add(provider);
    }

    internal async Task Listen()
    {
        if (_listener is null)
        {
            var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());
            builder.WebHost.UseKestrelCore();
            builder.WebHost.ConfigureKestrel(ConfigureKestrel);
            builder.Services.AddGrpc();
            foreach (var provider in _dispatchProviders)
            {
                builder.Services.AddSingleton<IServiceMethodProvider<GrpcEndpointService>>(provider);
            }
            _listener = builder.Build();
            _listener.MapGrpcService<GrpcEndpointService>();
            await _listener.StartAsync();
        }
    }

    internal async Task Close()
    {
        if (_listener is not null)
        {
            await _listener.StopAsync();
            await _listener.DisposeAsync();
        }
    }

    private void ConfigureKestrel(KestrelServerOptions k)
    {
        foreach (var (address, protection) in _listenEndpoints)
        {
            if (address is ListenAddress.Any any)
            {
                k.ListenAnyIP(any.Port, o=> ConfigureOptions(o, protection));
            }

            if (address is ListenAddress.IP ip)
            {
                k.Listen( ip.Address, ip.Port, o=> ConfigureOptions(o, protection));
            }
        }
    }

    private void ConfigureOptions(ListenOptions o, GrpcProtection protection)
    {
        o.Protocols = HttpProtocols.Http2;
        if (protection is ClearTextGrpc)
            return;

        if (protection is GrpcCertificate certificate)
        {
            o.UseHttps(certificate.Certificate);
            return;
        }

        if (protection is GrpcCertificateFromPath certificateFromPath)
        {
            o.UseHttps(certificateFromPath.CertPath, certificateFromPath.CertPassword);
            return;
        }

        throw new NotSupportedException($"Unsupported protection {protection}");
    }
}
