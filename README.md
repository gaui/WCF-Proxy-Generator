WCF Proxy Generator
===================

Generates WCF proxy classes from endpoints defined in Web.config file, using the [ServiceModel Metadata Utility Tool](http://msdn.microsoft.com/en-us/library/aa347733(v=vs.110).aspx) from Microsoft.

## Setup

1. Install the proxy generator executable file in some location, along with the `SvcUtil.exe` tool.
2. Set up a `BeforeBuild` target in your .csproj file and pass in two arguments, `$(Configuration)` and `$(ProjectDir)`

**Like so...**

    <Target Name="BeforeBuild">
        <Exec Command="ProxyGenerator.exe $(Configuration) $(ProjectDir)" WorkingDirectory="C:\some\tool\directory\" />
    </Target>

**You're done!**

## Process

Before each build the proxy generator will be called with the relevant build configuration *(e.g. Release)* and the path to the project directory. 

**Then...**

1. The proxy generator will read the `Web.config` file in the project directory *(second argument)*.
2. Go through all endpoints defined in `<client>` node under `<system.serviceModel>`.
3. Look up the value of the `address` attribute for the endpoint in the relevant Web.config transform file *(e.g. Web.Release.config)*. This matching is done with the `name` attribute of each endpoint.
4. Call `SvcUtil.exe` with the endpoint `address` and output it in the `Proxies/` folder in the project directory *(second argument)*.

*Note: The first time your project might not build because it doesn't find the proxy classes. All you have to do is include the `Proxies/` folder in your project and build again.*

*Note: Each time you add or remove a WCF service from your Web.config, make sure to add it or remove it in your project file.*

## What could fail

* Wrong arguments passed to proxy generator.
* Project directory *(second argument)* doesn't exist.
* Web.config file in project directory doesn't exist.
* Web.config transformation file in project directory doesn't exist.
* Proxies folder in project directory doesn't exist.
 * It will create it.
* Can't find `<client>` node under `<system.serviceModel>`.
* Can't find a certain endpoint in Web.config transformation file.
* Can't find `SvcUtil.exe` in the same directory.

The proxy generator will return an **error status code** (1) if something fails, so MSBuild will pick that up and the project won't continue to build (if the proxy classes have been generated sometimes before).

## So, why?

When you add a Service Reference in Visual Studio, the following files are generated:

* Service.disco
* Service.wsdl
* configuration.svcinfo
* configration91.svcinfo
* Reference.cs
* Reference.svcmap

4/6 of these files have the URL hardcoded in them. Merging branches with *accidental* changes done to these files, can be a real headache.

Also when you have many web services on many different environments *(test, staging, live)*, you always have to ensure that all references are correct and up to date.

So I decided to make it easier and automate it, so all references to all web services are always correct and up to date.

This idea was extended from this [answer on StackOverflow](http://stackoverflow.com/a/23332361/1053611).
