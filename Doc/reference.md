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

### Unsafe Thread Channel

Binding, Endpoint, Channel, Transport for communicating between services hosted in the same process

::: danger
* No reliablity
* No retry

Do not use UnsafeThreadChannelEndpoint in production, uses a System.Threading Channel internally, 
:::

#### UnsafeThreadChannelBinding
#### UnsafeThreadChannelChannel
#### UnsafeThreadChannelEndpoint
#### UnsafeThreadChannelExtensions
#### UnsafeThreadChannelTransport
#### UnsafeThreadEventChannel

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
