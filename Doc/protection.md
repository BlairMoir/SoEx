# Message Protection

Normally you should be using a secure transport; e.g. Azure Service Bus, or GRPC with TLS/mTLS.
Doing so means that you have already authenticated your callers, and provided secrecy between your endpoint and
the service. The catch is that anyone with access to a TLS terminating proxy or to the administration of the Azure
Service bus is still able to access the payload in the clear.

Message protection is an additional layer that allows the system to maintain secrecy from those administrating
third party infrastructure that you may be required to rely on.

## IMessageProtection


The `IMessageProtection.Protect(...)` method is called in the Channel and Endpoint pipelines after
`IMessageSerializer.Serialize<T>(...)` and `IMessageProtection.Unprotect(...)` 
is called before `IMessageSerializer.Deserialize<T>(...)`


# SoEx.Protection

SoEx.Protection is an example IMessageProtection implementation, we recommend you review the code to identify 
if it is suitable for your specific threat model / requirements. To avoid reinventing the wheel we have
used [jose-jwt](https://github.com/dvsekhvalnov/jose-jwt) to provide a message envelope and to carry out
encoding and decoding. The SoEx.Protection.Jose implementations will encrypt each message  with its own key.
Because of the constraints of a publisher not knowing who its subscribers are, an event message key is wrapped
with a single key that is known to all subscribers.




