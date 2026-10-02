---
outline: [2,4]
---


# Reference

## Hosting Abstractions

### IChannel
Used by the proxy to transmit a message to an endpoint

### IDispatcher
Used by the host to execute the operation on the service being hosted

### IEndpoint
Used by a host to listen and recieve message

### IMessageSerializer
Single globally registered serializer - seraliezes and deserializes every opertion request and response
::: info
In the future instead of being global it should be configured per Binding / Transport 
:::

## Hosting

### DefaultDispatcher
* Starts diagnostic activity
* Flows ambient context based on policy
* Adds ambient context based on policy to logging and tracing scope properties
* Invokes service operations via reflection


### ErrorInterceptor
Intercepts any exceptions on the host side and logs them

### JsonMessageSerializer

Default serializer

### ProxyFactory

Returns a DynamicProxy for a service interface intercepted by ProxyInterceptor

### ProxyInterceptor

* Starts diagnostifc activity
* Locates the channel using the transport factory
* Sends message using IChannel

### TransportFactory

Provides the channel for a given interface

## Context
### Abstractions
#### IAmbientContext

Store and retrieve ambient context

#### IContextFlowPolicy

Copy AmbientContexts between source and destination on request or response
Return Scope properties for logging and tracing based on the AmbientContext


### Implementaiton

#### AmbientContext

Default implementaiton for storing AmbientContext using ConcurrentDictionary

#### SerializedContext

Combines conccurent dictionary with IMessageSerializer when transfering across service boundaries

## Proxy

Indirectly resolve a proxy via s_asyncLocalLifetimeScope

## Transports

### InProc

Binding, Endpoint, Channel, Transport for communicating between services hosted in the same process.

#### InProcBinding
#### InProcChannel
#### InProcEndpoint
#### InProcTransport


### Azure Relay

Binding, Endpoint, Channel, Transport for communicating between services over the Azure Hybrid Relay

#### RelayBinding
#### RelayChannel
#### RelayConfig
#### RelayEndpoint
#### RelayTransport


### NamedPipe

Binding, Endpoint, Channel, Transport for communicating between services over NamedPipe

::: tip
This binding is slow

Intended for experimenting with hosting topologies without relying on an external framework for discovery or manually configuring ports.
:::


#### NamedPipeBinding
#### NamedPipeChannel
#### NamedPipeEndpoint
#### NamedPipeEventBinding
#### NamedPipeEventChannel
#### NamedPipeEventEndpoint

::: danger
* No reliablity
* No retry

Do not use NamedPipeEventEndpoint in production, uses a System.Threading Channel internally, 
:::

#### NamedPipeEventTransport
#### NamedPipeExtensions
#### NamedPipeTransport

### Sevice Bus Queue

Binding, Endpoint, Channel, Transport for communicating between services over Azure Service Bus

#### SBConfig
#### SBQueueBinding
#### SBQueueChannel
#### SBQueueEndpoint
#### SBQueueExtensions
#### SBQueueTransport


## Topology

Datamodel that defines which services are hosted. Transports the services communicate using, and boundaries between subsystems.

### Binding

Contract and Transport for communication

### Client

Binding information to reach a service

### Host

Makes a component available over an endpoint

* Implementation Type
* Endpoints - List of multiple binding to listen on to provide the service
* Isolated ServiceCollection definition to be used by the implementation

### Host Mock

Concrete implemntation object for a Host instead of the Type for use during testing

### Subsystem

* EntryPoint - Host defined as single point of entry accessible by other services in the system
* Components - Hosts that are only accessible  to other services defined within the SubSystem
* Clients - Bindings for services that are accessible within the subsystem

### System

* List of subsystems hosted in the current process
* List of clients that the process can use (depending on binding) to access in process or over the network.

### Transport

* Address to listen on or connect to
* Concrete Channel to use for proxies
* Concrete Endpoint to use as listener when hosting

## Diagrams

### Call pipeline
```d2
:::config
:::
grid-columns: 1
vertical-gap: 120
client: "" {
  style.fill: transparent
  style.stroke: transparent
  grid-columns: 3
  horizontal-gap: 160
  pad1: "" {width: 406; height: 1; style.fill: transparent; style.stroke: transparent}
  stack: "" {
    style.fill: transparent
    style.stroke: transparent
    grid-columns: 1
    calls: "" {
      style.fill: transparent
      style.stroke: transparent
      grid-columns: 1
      caller: Caller
      proxy: "Proxy.ForService<IMyManager>()"
      interceptor: ProxyInterceptor
      caller -> proxy -> interceptor
    }
    transport factory: TransportFactory {
      label.near: top-left
      grid-columns: 1
      client: "Client<IMyManager>"
      binding: Binding
      client channel: "Transport.ClientChannel"
      channel: "TransportChannel<IMyManager>"
      client -> binding -> client channel -> channel
    }
    calls.interceptor -> transport factory.client: "Client()"
  }
  pad2: "" {width: 406; height: 1; style.fill: transparent; style.stroke: transparent}
}
client pipeline: ChannelPipeline {
  label.near: top-left
  grid-rows: 1
  horizontal-gap: 29
  serialize: "" {
    style.fill: transparent
    style.stroke: transparent
    step: "IMessageSerializer.Serialize()"
  }
  protect: "" {
    style.fill: transparent
    style.stroke: transparent
    step: "IMessageProtection.Protect()"
  }
  channel: IChannel {
    label.near: top-left
    grid-columns: 1
    vertical-gap: 20
    bind: "Bind()"
    invoke: "InvokeResult()"
  }
  unprotect: "" {
    style.fill: transparent
    style.stroke: transparent
    step: "IMessageProtection.Unprotect()"
  }
  deserialize: "" {
    style.fill: transparent
    style.stroke: transparent
    step: "IMessageSerializer.Deserialize()"
  }
  serialize.step -> protect.step -> channel.invoke -> unprotect.step -> deserialize.step
}
service: "" {
  style.fill: transparent
  style.stroke: transparent
  grid-columns: 1
  entry: "" {
    style.fill: transparent
    style.stroke: transparent
    grid-columns: 3
    pad1: "" {width: 684; height: 1; style.fill: transparent; style.stroke: transparent}
    endpoint: "TransportEndpoint<IMyManager>"
    pad2: "" {width: 684; height: 1; style.fill: transparent; style.stroke: transparent}
  }
  endpoint pipeline: EndpointPipeline {
    label.near: top-left
    grid-rows: 1
    horizontal-gap: 0
    unprotect: "" {
      style.fill: transparent
      style.stroke: transparent
      step: "IMessageProtection.Unprotect()"
    }
    deserialize: "" {
      style.fill: transparent
      style.stroke: transparent
      step: "IMessageSerializer.Deserialize()"
    }
    dispatch: "" {
      style.fill: transparent
      style.stroke: transparent
      step: "IDispatcher.Dispatch()"
    }
    serialize: "" {
      style.fill: transparent
      style.stroke: transparent
      step: "IMessageSerializer.Serialize()"
    }
    protect: "" {
      style.fill: transparent
      style.stroke: transparent
      step: "IMessageProtection.Protect()"
    }
    unprotect.step -> deserialize.step -> dispatch.step -> serialize.step -> protect.step
  }
  handler: "" {
    style.fill: transparent
    style.stroke: transparent
    grid-columns: 3
    pad1: "" {width: 685; height: 1; style.fill: transparent; style.stroke: transparent}
    chain: "" {
      style.fill: transparent
      style.stroke: transparent
      grid-columns: 1
      interceptors: "ServiceInterceptors[...] "
      service: MyManager
      interceptors -> service
    }
    pad2: "" {width: 685; height: 1; style.fill: transparent; style.stroke: transparent}
  }
  entry.endpoint -> endpoint pipeline.unprotect.step
  endpoint pipeline.protect.step -> entry.endpoint
  endpoint pipeline.dispatch.step -> handler.chain.interceptors
}
client.stack.transport factory.channel -> client pipeline.channel.bind
client.stack.calls.interceptor -> client pipeline.serialize.step
client pipeline.channel.invoke <-> service.entry.endpoint
client pipeline.deserialize.step -> client.stack.calls.interceptor

```

### Host
```d2
:::config
:::

process: dotnet Host Process {
  host: "Microsoft.Extensions.Hosting.IHost" {
    grid-columns: 2
    lifetime: EndpointLifetimeService
    system: System {
      subsystem: SubSystem {
        entry point: EntryPoint {
          endpoint: "TransportEndpoint<IMyManager>"
          MyManager
          proxy: "Client<IMyEngine>"
          endpoint -> MyManager
          MyManager -> proxy
        }
        component: Component {
          endpoint: "TransportEndpoint<IMyEngine>"
          MyEngine
          endpoint -> MyEngine
        }
        entry point.proxy -> component.endpoint
      }
    }
    lifetime -> system.subsystem.entry point.endpoint: "Listen()"
    lifetime -> system.subsystem.component.endpoint: "Listen()"
  }
}

```

### Single Proc multi host 
```d2
:::config
:::

process: dotnet Host Process {
  grid-columns: 2
  singleton: Singleton {
    listeners: InProcListeners {
      grid-columns: 1
      register: "Register()"
      forAddress: "ForAddressFor"
    }
  }
  hosts: "" {
    grid-rows: 2
    style.fill: transparent
    style.stroke: transparent
    client host: Client IHost {
      grid-columns: 1 
      proxy: "Proxy.ForService<IMyManager>()"
      channel: "InProcChannel<IMyManager>"
      proxy -> channel
    }
    service host: Service IHost {
      system: System {
        subsystem: SubSystem {
            grid-columns: 1
          entry point: EntryPoint {
            endpoint: "InProcEndpoint<IMyManager>"
            MyManager
            channel: "InProcChannel<IMyEngine>"
            endpoint -> MyManager
            MyManager -> channel
          }
          component: Component {
           grid-columns: 1
            endpoint: "InProcEndpoint<IMyEngine>"
            MyEngine
            endpoint -> MyEngine
          }
        }
      }
    }
  }
  hosts.service host.system.subsystem.entry point.endpoint -> singleton.listeners.register: "Register()"
  hosts.service host.system.subsystem.component.endpoint -> singleton.listeners.register: "Register()"
  hosts.client host.channel -> singleton.listeners.forAddress: "ForAddress()"
  hosts.service host.system.subsystem.entry point.channel -> singleton.listeners.forAddress: "ForAddress()"
}

```
