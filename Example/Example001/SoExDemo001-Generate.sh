#!/bin/sh
targetDir=$(pwd)
echo "My directory is $targetDir"
if [ ! -r .gitignore ];
then
dotnet new gitignore
fi

if [ ! -r Example001.sln ];
then
dotnet new sln -n Example001
fi

if [ ! -r iFx/Api/Example001.iFx.Api.csproj ];
then
dotnet new classlib -n "Example001.iFx.Api" -o "iFx/Api"
dotnet sln Example001.sln add iFx/Api/Example001.iFx.Api.csproj
if [ -r iFx/Api/Class1.cs ];
then
rm iFx/Api/Class1.cs

fi

fi

if [ ! -r iFx/Service/Example001.iFx.Service.csproj ];
then
dotnet new classlib -n "Example001.iFx.Service" -o "iFx/Service"
dotnet sln Example001.sln add iFx/Service/Example001.iFx.Service.csproj
if [ -r iFx/Service/Class1.cs ];
then
rm iFx/Service/Class1.cs

fi

fi

if [ ! -r iFx/Data/Example001.iFx.Data.csproj ];
then
dotnet new classlib -n "Example001.iFx.Data" -o "iFx/Data"
dotnet sln Example001.sln add iFx/Data/Example001.iFx.Data.csproj
if [ -r iFx/Data/Class1.cs ];
then
rm iFx/Data/Class1.cs

fi

fi

if [ ! -r iFx/Test/Example001.iFx.Test.csproj ];
then
dotnet new classlib -n "Example001.iFx.Test" -o "iFx/Test"
dotnet sln Example001.sln add iFx/Test/Example001.iFx.Test.csproj
if [ -r iFx/Test/Class1.cs ];
then
rm iFx/Test/Class1.cs

fi

fi

if [ ! -r iFx/Container/Example001.iFx.Container.csproj ];
then
dotnet new classlib -n "Example001.iFx.Container" -o "iFx/Container"
dotnet sln Example001.sln add iFx/Container/Example001.iFx.Container.csproj
if [ -r iFx/Container/Class1.cs ];
then
rm iFx/Container/Class1.cs

fi

fi

if [ ! -r iFx/Logging/Example001.iFx.Logging.csproj ];
then
dotnet new classlib -n "Example001.iFx.Logging" -o "iFx/Logging"
dotnet sln Example001.sln add iFx/Logging/Example001.iFx.Logging.csproj
if [ -r iFx/Logging/Class1.cs ];
then
rm iFx/Logging/Class1.cs

fi

fi

if [ ! -r iFx/Configuration/Example001.iFx.Configuration.csproj ];
then
dotnet new classlib -n "Example001.iFx.Configuration" -o "iFx/Configuration"
dotnet sln Example001.sln add iFx/Configuration/Example001.iFx.Configuration.csproj
if [ -r iFx/Configuration/Class1.cs ];
then
rm iFx/Configuration/Class1.cs

fi

fi

if [ ! -r Common/Contract/Example001.Common.Contract.csproj ];
then
dotnet new classlib -n "Example001.Common.Contract" -o "Common/Contract"
dotnet sln Example001.sln add Common/Contract/Example001.Common.Contract.csproj
if [ -r Common/Contract/Class1.cs ];
then
rm Common/Contract/Class1.cs

fi

if [ -r Common/Contract/Program.cs ];
then
rm Common/Contract/Program.cs

fi

fi

cd Common/Contract
dotnet add package Newtonsoft.Json --prerelease
cd ../..
if [ ! -d Common/Contract ];
then
mkdir  Common/Contract
fi
if [ ! -r Common/Contract/CallChainContext.cs ];
then
echo "dXNpbmcgTmV3dG9uc29mdC5Kc29uOwoKbmFtZXNwYWNlIEV4YW1wbGUwMDEuQ29tbW9uLkNvbnRyYWN0CnsKICAgIHB1YmxpYyBjbGFzcyBDYWxsQ2hhaW5Db250ZXh0CiAgICB7CiAgICAgICAgcHJpdmF0ZSByZWFkb25seSBHdWlkIF9jYWxsQ2hhaW5JZDsKCiAgICAgICAgcHVibGljIENhbGxDaGFpbkNvbnRleHQoKQogICAgICAgIHsKICAgICAgICAgICAgX2NhbGxDaGFpbklkID0gR3VpZC5OZXdHdWlkKCk7CiAgICAgICAgfQoKICAgICAgICBbSnNvbkNvbnN0cnVjdG9yXQogICAgICAgIHB1YmxpYyBDYWxsQ2hhaW5Db250ZXh0KEd1aWQgQ2FsbENoYWluSWQpCiAgICAgICAgewogICAgICAgICAgICBfY2FsbENoYWluSWQgPSBDYWxsQ2hhaW5JZDsKICAgICAgICB9CgogICAgICAgIHB1YmxpYyBHdWlkIENhbGxDaGFpbklkID0+IF9jYWxsQ2hhaW5JZDsKICAgIH0KfQ==" | base64 -d > Common/Contract/CallChainContext.cs

fi
if [ ! -d Common/Contract ];
then
mkdir  Common/Contract
fi
if [ ! -r Common/Contract/CountContext.cs ];
then
echo "dXNpbmcgTmV3dG9uc29mdC5Kc29uOwoKbmFtZXNwYWNlIEV4YW1wbGUwMDEuQ29tbW9uLkNvbnRyYWN0CnsKICAgIHB1YmxpYyBjbGFzcyBDb3VudENvbnRleHQKICAgIHsKICAgICAgICBwcml2YXRlIGludCBfaG9wQ291bnQgPSAxOwoKICAgICAgICBwdWJsaWMgQ291bnRDb250ZXh0KCkKICAgICAgICB7CgogICAgICAgIH0KCiAgICAgICAgcHVibGljIENvdW50Q29udGV4dChDb3VudENvbnRleHQgcGFyZW50KQogICAgICAgIHsKICAgICAgICAgICAgX2hvcENvdW50ID0gcGFyZW50LkhvcENvdW50ICsgMTsKICAgICAgICB9CgogICAgICAgIFtKc29uQ29uc3RydWN0b3JdCiAgICAgICAgcHVibGljIENvdW50Q29udGV4dChpbnQgSG9wQ291bnQpCiAgICAgICAgewogICAgICAgICAgICBfaG9wQ291bnQgPSBIb3BDb3VudDsKICAgICAgICB9CgogICAgICAgIHB1YmxpYyBpbnQgSG9wQ291bnQgPT4gX2hvcENvdW50OwogICAgfQp9Cg==" | base64 -d > Common/Contract/CountContext.cs

fi
if [ ! -r Common/Policy/Example001.Common.Policy.csproj ];
then
dotnet new classlib -n "Example001.Common.Policy" -o "Common/Policy"
dotnet sln Example001.sln add Common/Policy/Example001.Common.Policy.csproj
if [ -r Common/Policy/Class1.cs ];
then
rm Common/Policy/Class1.cs

fi

if [ -r Common/Policy/Program.cs ];
then
rm Common/Policy/Program.cs

fi

fi

cd Common/Policy
dotnet add package SoEx.Context.Abstractions --prerelease
cd ../..
dotnet add Common/Policy/Example001.Common.Policy.csproj reference Common/Contract/Example001.Common.Contract.csproj
if [ ! -d Common/Policy ];
then
mkdir  Common/Policy
fi
if [ ! -r Common/Policy/ContextFlowPolicy.cs ];
then
echo "dXNpbmcgRXhhbXBsZTAwMS5Db21tb24uQ29udHJhY3Q7CnVzaW5nIFNvRXguQ29udGV4dDsKbmFtZXNwYWNlIEV4YW1wbGUwMDEuQ29tbW9uLlBvbGljeTsKCnB1YmxpYyBjbGFzcyBDb250ZXh0Rmxvd1BvbGljeSA6IElDb250ZXh0Rmxvd1BvbGljeQp7CiAgICBwdWJsaWMgdm9pZCBJbmNvbWluZyhJQW1iaWVudENvbnRleHQgc291cmNlLCBJQW1iaWVudENvbnRleHQgZGVzdGluYXRpb24pCiAgICB7CiAgICAgICAgc291cmNlLkNvcHlJZkV4aXN0czxDYWxsQ2hhaW5Db250ZXh0PihkZXN0aW5hdGlvbik7CgogICAgICAgIENvdW50Q29udGV4dCBjb3VudENvbnRleHQ7CiAgICAgICAgaWYgKHNvdXJjZS5Db250YWluczxDb3VudENvbnRleHQ+KCkpCiAgICAgICAgewogICAgICAgICAgICBjb3VudENvbnRleHQgPSBuZXcgQ291bnRDb250ZXh0KHNvdXJjZS5HZXQ8Q291bnRDb250ZXh0PigpKTsKICAgICAgICB9CiAgICAgICAgZWxzZQogICAgICAgIHsKICAgICAgICAgICAgY291bnRDb250ZXh0ID0gbmV3IENvdW50Q29udGV4dCgpOwogICAgICAgIH0KICAgICAgICBkZXN0aW5hdGlvbi5TZXRJZk5vdEV4aXN0cygoKSA9PiBjb3VudENvbnRleHQpOwogICAgICAgIGRlc3RpbmF0aW9uLlNldElmTm90RXhpc3RzKCgpID0+IG5ldyBDYWxsQ2hhaW5Db250ZXh0KCkpOwogICAgfQoKICAgIHB1YmxpYyB2b2lkIE91dGdvaW5nKElBbWJpZW50Q29udGV4dCBzb3VyY2UsIElBbWJpZW50Q29udGV4dCBkZXN0aW5hdGlvbikKICAgIHsKICAgICAgICBzb3VyY2UuQ29weUlmRXhpc3RzPENvdW50Q29udGV4dD4oZGVzdGluYXRpb24pOwogICAgfQoKICAgIHB1YmxpYyB2b2lkIENvcHkoSUFtYmllbnRDb250ZXh0IHNvdXJjZSwgSUFtYmllbnRDb250ZXh0IGRlc3RpbmF0aW9uKQogICAgewogICAgICAgIHNvdXJjZS5Db3B5SWZFeGlzdHM8Q2FsbENoYWluQ29udGV4dD4oZGVzdGluYXRpb24pOwogICAgICAgIHNvdXJjZS5Db3B5SWZFeGlzdHM8Q291bnRDb250ZXh0PihkZXN0aW5hdGlvbik7CiAgICB9CiAgICBwdWJsaWMgSURpY3Rpb25hcnk8c3RyaW5nLCBvYmplY3Q+IFNjb3BlUHJvcGVydGllcyhJQW1iaWVudENvbnRleHQgY29udGV4dCkKICAgIHsKICAgICAgICByZXR1cm4gbmV3IERpY3Rpb25hcnk8c3RyaW5nLCBvYmplY3Q+KCkKICAgICAgICB7CiAgICAgICAgICAgICB7IkhvcENvdW50IiwgY29udGV4dC5HZXQ8Q291bnRDb250ZXh0PigpLkhvcENvdW50fSwKICAgICAgICAgICAgIHsiQ2FsbENoYWluSWQiLCBjb250ZXh0LkdldDxDYWxsQ2hhaW5Db250ZXh0PigpLkNhbGxDaGFpbklkfQogICAgICAgIH07CiAgICB9Cn0K" | base64 -d > Common/Policy/ContextFlowPolicy.cs

fi
if [ ! -r iFx/Hosting/InProc/Example001.iFx.Hosting.InProc.csproj ];
then
dotnet new classlib -n "Example001.iFx.Hosting.InProc" -o "iFx/Hosting/InProc"
dotnet sln Example001.sln add iFx/Hosting/InProc/Example001.iFx.Hosting.InProc.csproj
if [ -r iFx/Hosting/InProc/Class1.cs ];
then
rm iFx/Hosting/InProc/Class1.cs

fi

if [ -r iFx/Hosting/InProc/Program.cs ];
then
rm iFx/Hosting/InProc/Program.cs

fi

fi

cd iFx/Hosting/InProc
dotnet add package SoEx.Hosting.InProc --prerelease
cd ../../..
cd iFx/Hosting/InProc
dotnet add package Microsoft.Extensions.Hosting --prerelease
cd ../../..
cd iFx/Hosting/InProc
dotnet add package Serilog.Extensions.Hosting --prerelease
cd ../../..
cd iFx/Hosting/InProc
dotnet add package Serilog.Sinks.Console --prerelease
cd ../../..
cd iFx/Hosting/InProc
dotnet add package Serilog.Sinks.Seq --prerelease
cd ../../..
cd iFx/Hosting/InProc
dotnet add package SerilogTracing --prerelease
cd ../../..
if [ ! -d iFx/Hosting/InProc ];
then
mkdir  iFx/Hosting/InProc
fi
if [ ! -r iFx/Hosting/InProc/Host.cs ];
then
echo "CnVzaW5nIE1pY3Jvc29mdC5FeHRlbnNpb25zLkRlcGVuZGVuY3lJbmplY3Rpb247CnVzaW5nIE1pY3Jvc29mdC5FeHRlbnNpb25zLkhvc3Rpbmc7CnVzaW5nIFNlcmlsb2c7CnVzaW5nIFNvRXguSW5Qcm9jOwp1c2luZyBTeXN0ZW0uRGlhZ25vc3RpY3M7CnVzaW5nIFN5c3RlbS5SZWZsZWN0aW9uOwoKbmFtZXNwYWNlIEV4YW1wbGUwMDEuaUZ4Lkhvc3Rpbmc7CnB1YmxpYyBzdGF0aWMgY2xhc3MgSG9zdAp7CiAgICBwdWJsaWMgc3RhdGljIElIb3N0QnVpbGRlciBJblByb2Moc3RyaW5nW10gYXJncywgRnVuYzxUYXNrPj8gdGVzdEFjdGlvbiA9IG51bGwpCiAgICB7CiAgICAgICAgc3RyaW5nPyBhc3NlbWJseU5hbWUgPSB0eXBlb2YoSG9zdCkuQXNzZW1ibHkuRnVsbE5hbWU7CiAgICAgICAgRGVidWcuQXNzZXJ0KGFzc2VtYmx5TmFtZSBpcyBub3QgbnVsbCk7CiAgICAgICAgc3RyaW5nIGNvbXBhbnlOYW1lc3BhY2UgPSBhc3NlbWJseU5hbWUuU3BsaXQoIi4iKVswXTsKICAgICAgICB2YXIgaG9zdEFzc2VtYmx5TmFtZSA9IEFzc2VtYmx5LkdldENhbGxpbmdBc3NlbWJseSgpLkdldE5hbWUoKS5OYW1lOwogICAgICAgIHJldHVybiBNaWNyb3NvZnQuRXh0ZW5zaW9ucy5Ib3N0aW5nLkhvc3QuQ3JlYXRlRGVmYXVsdEJ1aWxkZXIoYXJncykKICAgICAgICAgICAgICAgIC5JblByb2NJZngoSW50ZXJjZXB0b3JzLEZpbmRTZXJ2aWNlVHlwZXMoY29tcGFueU5hbWVzcGFjZSkpCiAgICAgICAgICAgICAgICAuQ29uZmlndXJlU2VydmljZXMoKGhvc3RDb250ZXh0LCBzZXJ2aWNlcykgPT4KICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICBzZXJ2aWNlcy5BZGRTZXJpbG9nKChzZXJ2aWNlcywgbG9nZ2VyQ29uZmlndXJhdGlvbikgPT4gbG9nZ2VyQ29uZmlndXJhdGlvbgogICAgICAgICAgICAgICAgICAgIC5NaW5pbXVtTGV2ZWwuRGVidWcoKQogICAgICAgICAgICAgICAgICAgIC5FbnJpY2guRnJvbUxvZ0NvbnRleHQoKQogICAgICAgICAgICAgICAgICAgIC5FbnJpY2guV2l0aFByb3BlcnR5KG5hbWVvZihob3N0QXNzZW1ibHlOYW1lKSwgaG9zdEFzc2VtYmx5TmFtZSkKICAgICAgICAgICAgICAgICAgICAuV3JpdGVUby5TZXEoImh0dHA6Ly9sb2NhbGhvc3Q6NTM0MSIpCiAgICAgICAgICAgICAgICAgICAgLldyaXRlVG8uQ29uc29sZSgpKTsgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICB9KTsKICAgIH0KCiAgICBwdWJsaWMgc3RhdGljIFRhc2sgUnVuSW5Qcm9jKHN0cmluZ1tdIGFyZ3MsIEZ1bmM8VGFzaz4/IHRlc3RBY3Rpb24gPSBudWxsKQogICAgewogICAgICAgIHJldHVybiBJblByb2MoYXJncywgdGVzdEFjdGlvbikuUnVuQ29uc29sZUFzeW5jKCk7CiAgICB9CgogICAgcHJpdmF0ZSBzdGF0aWMgVHlwZVtdIEludGVyY2VwdG9ycyhMaXN0PFR5cGU+IGludGVyY2VwdG9yTGlzdCkKICAgIHsKICAgICAgICBpbnRlcmNlcHRvckxpc3QuSW5zZXJ0KDEsIHR5cGVvZihTZXJpbG9nVHJhY2luZ0ludGVyY2VwdG9yKSk7CiAgICAgICAgcmV0dXJuIFsuLiBpbnRlcmNlcHRvckxpc3RdOwogICAgfQoKICAgIHByaXZhdGUgc3RhdGljIFR5cGVbXSBGaW5kU2VydmljZVR5cGVzKHN0cmluZyBjb21wYW55KQogICAgewogICAgICAgIHN0cmluZ1tdIHNlcnZpY2VTdWZmaXhDb252ZW50aW9uS2V5d29yZHMgPSBbIk1hbmFnZXIiLCAiRW5naW5lIiwgIkFjY2VzcyIsICJVdGlsaXR5Il07CiAgICAgICAgTGlzdDxUeXBlPiBmb3VuZFR5cGVzID0gW107CgogICAgICAgIHN0cmluZz8gcGF0aCA9IFBhdGguR2V0RGlyZWN0b3J5TmFtZShBc3NlbWJseS5HZXRFeGVjdXRpbmdBc3NlbWJseSgpLkxvY2F0aW9uKTsKICAgICAgICB2YXIgYXNzZW1ibHlGaWxlcyA9IERpcmVjdG9yeS5HZXRGaWxlcyhwYXRoISwgJCJ7Y29tcGFueX0uKi5TZXJ2aWNlLmRsbCIsIFNlYXJjaE9wdGlvbi5Ub3BEaXJlY3RvcnlPbmx5KTsKICAgICAgICBmb3JlYWNoICh2YXIgYXNzZW1ibHlGaWxlIGluIGFzc2VtYmx5RmlsZXMpCiAgICAgICAgewogICAgICAgICAgICB2YXIgYXNzZW1ibHkgPSBBc3NlbWJseS5Mb2FkRnJvbShhc3NlbWJseUZpbGUpOwogICAgICAgICAgICB2YXIgdHlwZXMgPSBhc3NlbWJseS5HZXRUeXBlcygpLldoZXJlKHQgPT4gc2VydmljZVN1ZmZpeENvbnZlbnRpb25LZXl3b3Jkcy5BbnkocyA9PiB0Lk5hbWUuRW5kc1dpdGgocykpKTsKICAgICAgICAgICAgZm91bmRUeXBlcy5BZGRSYW5nZSh0eXBlcyk7CgogICAgICAgIH0KICAgICAgICByZXR1cm4gZm91bmRUeXBlcy5Ub0FycmF5KCk7CiAgICB9Cn0K" | base64 -d > iFx/Hosting/InProc/Host.cs

fi
if [ ! -d iFx/Hosting/InProc/Test ];
then
mkdir  iFx/Hosting/InProc/Test
fi
if [ ! -r iFx/Hosting/InProc/Test/TestService.cs ];
then
echo "dXNpbmcgTWljcm9zb2Z0LkV4dGVuc2lvbnMuSG9zdGluZzsKCm5hbWVzcGFjZSBFeGFtcGxlMDAxLmlGeC5Ib3N0aW5nLlRlc3QKewogICAgcHVibGljIGNsYXNzIFRlc3RTZXJ2aWNlIDogQmFja2dyb3VuZFNlcnZpY2UKICAgIHsKICAgICAgICBwcml2YXRlIHJlYWRvbmx5IElIb3N0QXBwbGljYXRpb25MaWZldGltZSBtX0FwcExpZmV0aW1lOwogICAgICAgIHByaXZhdGUgcmVhZG9ubHkgRnVuYzxUYXNrPiBtX1Rlc3RBY3Rpb247CgogICAgICAgIHB1YmxpYyBUZXN0U2VydmljZSgKICAgICAgICAgICAgSUhvc3RBcHBsaWNhdGlvbkxpZmV0aW1lIGFwcExpZmV0aW1lLAogICAgICAgICAgICBGdW5jPFRhc2s+IHRlc3RBY3Rpb24pCiAgICAgICAgewogICAgICAgICAgICBtX0FwcExpZmV0aW1lID0gYXBwTGlmZXRpbWU7CiAgICAgICAgICAgIG1fVGVzdEFjdGlvbiA9IHRlc3RBY3Rpb247CiAgICAgICAgfQoKICAgICAgICBwcm90ZWN0ZWQgb3ZlcnJpZGUgYXN5bmMgVGFzayBFeGVjdXRlQXN5bmMoQ2FuY2VsbGF0aW9uVG9rZW4gc3RvcHBpbmdUb2tlbikKICAgICAgICB7CiAgICAgICAgICAgIGF3YWl0IFRhc2suRGVsYXkoMTAwMCk7CiAgICAgICAgICAgIGF3YWl0IG1fVGVzdEFjdGlvbi5JbnZva2UoKS5Db25maWd1cmVBd2FpdChmYWxzZSk7CiAgICAgICAgICAgIG1fQXBwTGlmZXRpbWUuU3RvcEFwcGxpY2F0aW9uKCk7CiAgICAgICAgfQogICAgfQp9Cg==" | base64 -d > iFx/Hosting/InProc/Test/TestService.cs

fi
if [ ! -d iFx/Hosting/InProc/Test ];
then
mkdir  iFx/Hosting/InProc/Test
fi
if [ ! -r iFx/Hosting/InProc/Test/TestServiceExtensions.cs ];
then
echo "dXNpbmcgU3lzdGVtLkRpYWdub3N0aWNzOwp1c2luZyBTeXN0ZW0uVGhyZWFkaW5nLlRhc2tzOwp1c2luZyBNaWNyb3NvZnQuRXh0ZW5zaW9ucy5EZXBlbmRlbmN5SW5qZWN0aW9uOwp1c2luZyBNaWNyb3NvZnQuRXh0ZW5zaW9ucy5Ib3N0aW5nOwoKbmFtZXNwYWNlIEV4YW1wbGUwMDEuaUZ4Lkhvc3RpbmcuVGVzdAp7CiAgICBwdWJsaWMgc3RhdGljIGNsYXNzIFRlc3RTZXJ2aWNlRXh0ZW5zaW9ucwogICAgewogICAgICAgIHB1YmxpYyBzdGF0aWMgSUhvc3RCdWlsZGVyIEFkZEhvc3RUZXN0KAogICAgICAgICAgICB0aGlzIElIb3N0QnVpbGRlciBob3N0LAogICAgICAgICAgICBGdW5jPFRhc2s+IHRlc3RBY3Rpb24pCiAgICAgICAgewogICAgICAgICAgICByZXR1cm4gaG9zdC5Db25maWd1cmVTZXJ2aWNlcygoaG9zdENvbnRleHQsIHNlcnZpY2VzKSA9PgogICAgICAgICAgICB7CiAgICAgICAgICAgICAgICBpZiAodGVzdEFjdGlvbiBpcyBub3QgbnVsbCkKICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICBzZXJ2aWNlcy5BZGRIb3N0ZWRTZXJ2aWNlKHNlcnZpY2VQcm92aWRlciA9PgogICAgICAgICAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgICAgICAgICAgSUhvc3RBcHBsaWNhdGlvbkxpZmV0aW1lPyBob3N0QXBwbGljYXRpb25MaWZldGltZSA9IHNlcnZpY2VQcm92aWRlci5HZXRTZXJ2aWNlPElIb3N0QXBwbGljYXRpb25MaWZldGltZT4oKTsKICAgICAgICAgICAgICAgICAgICAgICAgRGVidWcuQXNzZXJ0KGhvc3RBcHBsaWNhdGlvbkxpZmV0aW1lIGlzIG5vdCBudWxsKTsKCiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiBuZXcgVGVzdFNlcnZpY2UoaG9zdEFwcGxpY2F0aW9uTGlmZXRpbWUsIHRlc3RBY3Rpb24pOwogICAgICAgICAgICAgICAgICAgIH0pOwogICAgICAgICAgICAgICAgfQogICAgICAgICAgICB9KTsKICAgICAgICB9CgogICAgfQp9Cg==" | base64 -d > iFx/Hosting/InProc/Test/TestServiceExtensions.cs

fi
if [ ! -d iFx/Hosting/InProc/Interceptors ];
then
mkdir  iFx/Hosting/InProc/Interceptors
fi
if [ ! -r iFx/Hosting/InProc/Interceptors/SerilogTracingInterceptor.cs ];
then
echo "dXNpbmcgQ2FzdGxlLkR5bmFtaWNQcm94eTsKdXNpbmcgU2VyaWxvZ1RyYWNpbmc7CgpuYW1lc3BhY2UgRXhhbXBsZTAwMS5pRngKewogICAgcHVibGljIGNsYXNzIFNlcmlsb2dUcmFjaW5nSW50ZXJjZXB0b3IgOiBBc3luY0ludGVyY2VwdG9yQmFzZSwgSUludGVyY2VwdG9yCiAgICB7CiAgICAgICAgcHJpdmF0ZSByZWFkb25seSBTZXJpbG9nLklMb2dnZXIgX2xvZ2dlcjsKICAgICAgICBwdWJsaWMgU2VyaWxvZ1RyYWNpbmdJbnRlcmNlcHRvcihTZXJpbG9nLklMb2dnZXIgbG9nZ2VyKQogICAgICAgIHsKICAgICAgICAgICAgX2xvZ2dlciA9IGxvZ2dlcjsKICAgICAgICB9CgogICAgICAgIHB1YmxpYyB2b2lkIEludGVyY2VwdChJSW52b2NhdGlvbiBpbnZvY2F0aW9uKQogICAgICAgIHsKICAgICAgICAgICAgdGhpcy5Ub0ludGVyY2VwdG9yKCkuSW50ZXJjZXB0KGludm9jYXRpb24pOwogICAgICAgIH0KCiAgICAgICAgcHJvdGVjdGVkIG92ZXJyaWRlIGFzeW5jIFRhc2sgSW50ZXJjZXB0QXN5bmMoSUludm9jYXRpb24gaW52b2NhdGlvbiwgSUludm9jYXRpb25Qcm9jZWVkSW5mbyBwcm9jZWVkSW5mbywgRnVuYzxJSW52b2NhdGlvbiwgSUludm9jYXRpb25Qcm9jZWVkSW5mbywgVGFzaz4gcHJvY2VlZCkKICAgICAgICB7CiAgICAgICAgICAgIHVzaW5nIChMb2dnZXJBY3Rpdml0eSBhY3Rpdml0eSA9IF9sb2dnZXIuU3RhcnRBY3Rpdml0eSgie1NlcnZpY2V9IHtPcGVyYXRpb259IiwgaW52b2NhdGlvbi5UYXJnZXRUeXBlPy5OYW1lLCBpbnZvY2F0aW9uLk1ldGhvZD8uTmFtZSkpCiAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgIGF3YWl0IHByb2NlZWQoaW52b2NhdGlvbiwgcHJvY2VlZEluZm8pLkNvbmZpZ3VyZUF3YWl0KGZhbHNlKTsKICAgICAgICAgICAgfQogICAgICAgIH0KCiAgICAgICAgcHJvdGVjdGVkIG92ZXJyaWRlIGFzeW5jIFRhc2s8VFJlc3VsdD4gSW50ZXJjZXB0QXN5bmM8VFJlc3VsdD4oSUludm9jYXRpb24gaW52b2NhdGlvbiwgSUludm9jYXRpb25Qcm9jZWVkSW5mbyBwcm9jZWVkSW5mbywgRnVuYzxJSW52b2NhdGlvbiwgSUludm9jYXRpb25Qcm9jZWVkSW5mbywgVGFzazxUUmVzdWx0Pj4gcHJvY2VlZCkKICAgICAgICB7CiAgICAgICAgICAgIHVzaW5nIChMb2dnZXJBY3Rpdml0eSBhY3Rpdml0eSA9IF9sb2dnZXIuU3RhcnRBY3Rpdml0eSgie1NlcnZpY2V9IHtPcGVyYXRpb259IiwgaW52b2NhdGlvbi5UYXJnZXRUeXBlPy5OYW1lLCBpbnZvY2F0aW9uLk1ldGhvZD8uTmFtZSkpCiAgICAgICAgICAgIHsKICAgICAgICAgICAgICAgIFRSZXN1bHQ/IHJlc3VsdCA9IGF3YWl0IHByb2NlZWQoaW52b2NhdGlvbiwgcHJvY2VlZEluZm8pLkNvbmZpZ3VyZUF3YWl0KGZhbHNlKTsKICAgICAgICAgICAgICAgIHJldHVybiByZXN1bHQ7CiAgICAgICAgICAgIH0KICAgICAgICB9CiAgICB9Cn0K" | base64 -d > iFx/Hosting/InProc/Interceptors/SerilogTracingInterceptor.cs

fi
if [ ! -r iFx/Proxy/Example001.iFx.Proxy.csproj ];
then
dotnet new classlib -n "Example001.iFx.Proxy" -o "iFx/Proxy"
dotnet sln Example001.sln add iFx/Proxy/Example001.iFx.Proxy.csproj
if [ -r iFx/Proxy/Class1.cs ];
then
rm iFx/Proxy/Class1.cs

fi

if [ -r iFx/Proxy/Program.cs ];
then
rm iFx/Proxy/Program.cs

fi

fi

cd iFx/Proxy
dotnet add package SoEx.Proxy --prerelease
cd ../..
if [ ! -d iFx/Proxy ];
then
mkdir  iFx/Proxy
fi
if [ ! -r iFx/Proxy/Proxy.cs ];
then
echo "bmFtZXNwYWNlIEV4YW1wbGUwMDEuaUZ4LlByb3h5CnsKICAgIHB1YmxpYyBzdGF0aWMgY2xhc3MgUHJveHkKICAgIHsKICAgICAgICBwdWJsaWMgc3RhdGljIEkgRm9yU2VydmljZTxJPigpIHdoZXJlIEkgOiBjbGFzcwogICAgICAgIHsKICAgICAgICAgICAgcmV0dXJuIFNvRXguUHJveHkuRm9yU2VydmljZTxJPigpOwogICAgICAgIH0KCiAgICAgICAgcHVibGljIHN0YXRpYyBJIEZvckNvbXBvbmVudDxJPihvYmplY3Qgc2VydmljZSkgd2hlcmUgSSA6IGNsYXNzCiAgICAgICAgewogICAgICAgICAgICByZXR1cm4gU29FeC5Qcm94eS5Gb3JDb21wb25lbnQ8ST4oc2VydmljZSk7CiAgICAgICAgfQogICAgfQp9" | base64 -d > iFx/Proxy/Proxy.cs

fi
if [ ! -r Host/InProc/Example001.Host.InProc.csproj ];
then
dotnet new console -n "Example001.Host.InProc" -o "Host/InProc"
dotnet sln Example001.sln add Host/InProc/Example001.Host.InProc.csproj
if [ -r Host/InProc/Class1.cs ];
then
rm Host/InProc/Class1.cs

fi

if [ -r Host/InProc/Program.cs ];
then
rm Host/InProc/Program.cs

fi

fi

cd Host/InProc
dotnet add package Microsoft.Extensions.DependencyInjection --prerelease
cd ../..
cd Host/InProc
dotnet add package Microsoft.Extensions.Hosting --prerelease
cd ../..
cd Host/InProc
dotnet add package SoEx.Context.Abstractions --prerelease
cd ../..
dotnet add Host/InProc/Example001.Host.InProc.csproj reference Common/Policy/Example001.Common.Policy.csproj
dotnet add Host/InProc/Example001.Host.InProc.csproj reference iFx/Hosting/InProc/Example001.iFx.Hosting.InProc.csproj
dotnet add Host/InProc/Example001.Host.InProc.csproj reference iFx/Proxy/Example001.iFx.Proxy.csproj
if [ ! -d Host/InProc ];
then
mkdir  Host/InProc
fi
if [ ! -r Host/InProc/Program.cs ];
then
echo "dXNpbmcgTWljcm9zb2Z0LkV4dGVuc2lvbnMuRGVwZW5kZW5jeUluamVjdGlvbjsKdXNpbmcgTWljcm9zb2Z0LkV4dGVuc2lvbnMuSG9zdGluZzsKdXNpbmcgRXhhbXBsZTAwMS5Db21tb24uUG9saWN5Owp1c2luZyBFeGFtcGxlMDAxLmlGeC5Ib3N0aW5nLlRlc3Q7CnVzaW5nIEV4YW1wbGUwMDEuaUZ4LlByb3h5Owp1c2luZyBTb0V4LkNvbnRleHQ7CgoKbmFtZXNwYWNlIEV4YW1wbGUwMDEuSG9zdC5JblByb2MKewogICAgY2xhc3MgUHJvZ3JhbQogICAgewogICAgICAgIHN0YXRpYyB2b2lkIE1haW4oc3RyaW5nW10gYXJncykKICAgICAgICB7ICAgICAgICAgICAgCiAgICAgICAgICAgIHZhciBob3N0QnVpbGRlciA9IEV4YW1wbGUwMDEuaUZ4Lkhvc3RpbmcuSG9zdC5JblByb2MoYXJncykKICAgICAgICAgICAgLkNvbmZpZ3VyZVNlcnZpY2VzKGMgPT4KICAgICAgICAgICAgewogICAgICAgICAgICAgICAgYy5BZGRTaW5nbGV0b248SUNvbnRleHRGbG93UG9saWN5LCBDb250ZXh0Rmxvd1BvbGljeT4oKTsKICAgICAgICAgICAgfSk7CiAgICAgICAgICAgIGhvc3RCdWlsZGVyLkFkZEhvc3RUZXN0KEhvc3RUZXN0KTsKICAgICAgICAgICAgaG9zdEJ1aWxkZXIuQnVpbGQoKS5SdW4oKTsKICAgICAgICB9CgogICAgICAgIHByaXZhdGUgc3RhdGljIGFzeW5jIFRhc2sgSG9zdFRlc3QoKQogICAgICAgIHsKICAgICAgICAgICAgLy8gdmFyIHByb3h5ID0gUHJveHkuRm9yU2VydmljZTxJX1hYWF9NYW5hZ2VyPigpOwogICAgICAgICAgICAvLyBhd2FpdCBwcm94eS5fT3BlcmF0aW9uXygpOwogICAgICAgICAgICB0aHJvdyBuZXcgTm90SW1wbGVtZW50ZWRFeGNlcHRpb24oKTsKICAgICAgICB9CiAgICB9Cn0=" | base64 -d > Host/InProc/Program.cs

fi
if [ ! -r Component/Manager/Membership/Interface/Example001.Manager.Membership.Interface.csproj ];
then
dotnet new classlib -n "Example001.Manager.Membership.Interface" -o "Component/Manager/Membership/Interface"
dotnet sln Example001.sln add Component/Manager/Membership/Interface/Example001.Manager.Membership.Interface.csproj
if [ -r Component/Manager/Membership/Interface/Class1.cs ];
then
rm Component/Manager/Membership/Interface/Class1.cs

fi

fi
if [ ! -r Component/Manager/Membership/Service/Example001.Manager.Membership.Service.csproj ];
then
dotnet new classlib -n "Example001.Manager.Membership.Service" -o "Component/Manager/Membership/Service"
dotnet sln Example001.sln add Component/Manager/Membership/Service/Example001.Manager.Membership.Service.csproj
dotnet add Component/Manager/Membership/Service/Example001.Manager.Membership.Service.csproj reference Component/Manager/Membership/Interface/Example001.Manager.Membership.Interface.csproj
if [ -r Component/Manager/Membership/Service/Class1.cs ];
then
rm Component/Manager/Membership/Service/Class1.cs

fi

fi
dotnet add Host/InProc/Example001.Host.InProc.csproj reference Component/Manager/Membership/Service/Example001.Manager.Membership.Service.csproj
dotnet add Host/InProc/Example001.Host.InProc.csproj reference Component/Manager/Membership/Interface/Example001.Manager.Membership.Interface.csproj
if [ ! -r Component/Access/Customer/Interface/Example001.Access.Customer.Interface.csproj ];
then
dotnet new classlib -n "Example001.Access.Customer.Interface" -o "Component/Access/Customer/Interface"
dotnet sln Example001.sln add Component/Access/Customer/Interface/Example001.Access.Customer.Interface.csproj
if [ -r Component/Access/Customer/Interface/Class1.cs ];
then
rm Component/Access/Customer/Interface/Class1.cs

fi

fi
if [ ! -r Component/Access/Customer/Service/Example001.Access.Customer.Service.csproj ];
then
dotnet new classlib -n "Example001.Access.Customer.Service" -o "Component/Access/Customer/Service"
dotnet sln Example001.sln add Component/Access/Customer/Service/Example001.Access.Customer.Service.csproj
dotnet add Component/Access/Customer/Service/Example001.Access.Customer.Service.csproj reference Component/Access/Customer/Interface/Example001.Access.Customer.Interface.csproj
if [ -r Component/Access/Customer/Service/Class1.cs ];
then
rm Component/Access/Customer/Service/Class1.cs

fi

fi
dotnet add Host/InProc/Example001.Host.InProc.csproj reference Component/Access/Customer/Service/Example001.Access.Customer.Service.csproj
if [ ! -d Component/Manager/Membership/Interface/Common ];
then
mkdir  Component/Manager/Membership/Interface/Common
fi
if [ ! -d Component/Manager/Membership/Interface ];
then
mkdir  Component/Manager/Membership/Interface
fi
if [ ! -r Component/Manager/Membership/Interface/IMembershipManager.cs ];
then
echo "dXNpbmcgU3lzdGVtLlRocmVhZGluZy5UYXNrczsNCg0KbmFtZXNwYWNlIEV4YW1wbGUwMDEuTWFuYWdlci5NZW1iZXJzaGlwLkludGVyZmFjZQ0Kew0KICAgIHB1YmxpYyBpbnRlcmZhY2UgSU1lbWJlcnNoaXBNYW5hZ2VyDQogICAgew0KICAgICAgICBUYXNrIFByb2ZpbGUoKTsNCiAgICB9DQp9" | base64 -d > Component/Manager/Membership/Interface/IMembershipManager.cs

fi
if [ ! -d Component/Access/Customer/Interface/Common ];
then
mkdir  Component/Access/Customer/Interface/Common
fi
if [ ! -d Component/Access/Customer/Interface ];
then
mkdir  Component/Access/Customer/Interface
fi
if [ ! -r Component/Access/Customer/Interface/ICustomerAccess.cs ];
then
echo "dXNpbmcgU3lzdGVtLlRocmVhZGluZy5UYXNrczsNCg0KbmFtZXNwYWNlIEV4YW1wbGUwMDEuQWNjZXNzLkN1c3RvbWVyLkludGVyZmFjZQ0Kew0KICAgIHB1YmxpYyBpbnRlcmZhY2UgSUN1c3RvbWVyQWNjZXNzDQogICAgew0KICAgICAgICBUYXNrIEZpbHRlcigpOw0KICAgIH0NCn0=" | base64 -d > Component/Access/Customer/Interface/ICustomerAccess.cs

fi
