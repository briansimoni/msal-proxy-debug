From: https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code?pivots=dotnet-6-0

One possibly interesting thing that this tutorial introduces is top-level-statements

In the latest version of C#, a new feature named top-level statements lets you omit the Program class and the Main method. Most existing C# programs don't use top-level statements, so this tutorial doesn't use this new feature. But it's available in C# 10, and whether you use it in your programs is a matter of style preference.


For building self-contained apps, its not actually that cool. It creates even more files and dlls than before because that is how it distributes the runtime :(

`dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=true`

however, I founds if you add another parameter, you can actually get just one file

`dotnet publish -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true --configuration Release`


## Dependency Management

https://docs.microsoft.com/en-us/dotnet/core/tools/dependencies
when you use the dotnet CLI, it downloads packages from nuget

`dotnet add package Microsoft.Identity.Web --version 1.21.1`


## HOWEVER
This don't work with IIS :(
https://github.com/dotnet/runtime/issues/41482


## Proxies
It appears that behind the scenes, RestSharp, MSAL, and Microsoft Identity Web all use System.Net.Proxy when configured to use a proxy. One can infer that the same implementation of the HTTP and TLS protocols are used across all libraries. If there is a network-related issue, the errors should look the same regardless of which library is under test.

In my test environment when I made these calls through an http proxy with man in the middle CA, I got the expected ssl validation errors. When I imported the CA into the Windows trust store, the program then worked as expected. This confirms that .NET Core System.Net.Proxy and/or http clients use the system trust store.

