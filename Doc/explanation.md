---
outline: [2,4]
---
# Explanation

[[toc]]

## Service (Disambiguation)

The term _Service_ is very overloaded and varies depending on the context and which layer of the software stack you are looking at.

### Object Orientation

When working with object-orientated programming the term _Service_ usually comes up in the context of Inversion of Control and Dependency Injection. This has a scope limited to the currently executing process.
[Autofac defines a service as](https://docs.autofac.org/en/latest/glossary.html)
> A well-defined behavioral contract shared between a providing and a consuming Component; generally these are interfaces, or some abstraction of a Component

### Service Orientation

When working with service-oriented programming a service is a behaviour contract that is accessible via an address over a network.
This has a much broader scope spanning: processes, machines, and networks.

::: tip
SoEx (Shortened from Service Oriented Example) is focused completely on the Service Orientated paradigm
:::


## Why does SoEx exist?

SoEx exists so that you can take the exact same implementation code for a set of services and host it:

* Within a cluster management framework (k8s, dapr, etc)
* as multiple process on multiple machines
* as multiple processes on a single machine
* InProc - In single process on a single machine

The code of the services does not change, the only difference is the host which determines how and in what combinations
the service is exposed to callers.

This means that a system that can be deployed by a cluster, can also have a dev experience of pressing F5 and debugging
within the simplicity of a single process.

SoEx also allows you to swap out transports and support message security without relying on a specific hosting
framework or transport. SoEx currently supports: InProc, Azure Service Bus, NATS, NamedPipes, Dapr


## Your (business) code should not know SoEx exists!

Service implementations should not directly reference the SoEx Framework. It is recommended that you create an "iFx"
layer in between. Within the iFx library references to SoEx are in a single place allowing you to replace SoEx with
another framework. The iFx builds on top of the SoEx primitives to implement and update company specific conventions
or policies.

::: info
the "iFX" layer is the place to glue together the hosting framework such as ASP.NET Core and SoEx.
:::

## What is a Topology?

A SoEx Topology is a configuration describing the structure of a service oriented system. In normal circumstances you should not be contructiong a Topology by hand, you should use convention or a builder. Within a single process you can choose to host:

* [System](reference/#system)
* [SubSystem](reference/#subsystem)
* [Component](reference/#host)


