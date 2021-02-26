# Service Oriented Examples (SoEx)
Example Vertical Swathe of a Method Informed System

Leveraging [dapr](https://dapr.io/) where possible, this a template for building microservice architectures, using the [IDesign Method](http://www.idesign.net/).

Here I intend to demonstrate techniques such as workflows and [strategy](https://en.wikipedia.org/wiki/Strategy_pattern) switching based on context. Allowing things such as per customer modifications that do not jeopardize system stability for the other users of the system.

# Todo
* Create Dapr Demos
    * Hello webapi
    * Hello grpc
    * Hello pubsub
    * Hello headers
    * ...
* Method Demos
    * Build IFx to enforce convention on service communication as per method guidelines
    * Implement Ambient Context
    * Create a real System Design for a problem that requires the demonstration of the described techniques.
    * Define or reference a taxonomy for all terms used e.g. "Business Event"
    * ...

# Examples to demonstrate in a service oriented context
* Workflow Manager
* Strategy Pattern 
* Adapter Pattern
* Ambient Context
* Business Events

# Non goals
Currently this project will not:
* Demonstrate in a language other than C#
* Autogenerate clients for other languages
