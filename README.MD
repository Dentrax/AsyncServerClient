<h1 align="center">AsyncServerClient</h1>

[![MIT Licence](https://badges.frapsoft.com/os/mit/mit.svg?v=103)](https://opensource.org/licenses/mit-license.php)
[![Open Source Love](https://badges.frapsoft.com/os/v1/open-source.png?v=103)](https://github.com/ellerbrock/open-source-badges/)
[![Build Status](https://travis-ci.org/Dentrax/AsyncServerClient.svg?branch=master)](https://travis-ci.org/Dentrax/AsyncServerClient)
[![Sourcegraph](https://img.shields.io/badge/view%20on-Sourcegraph-brightgreen.svg)](https://sourcegraph.com/github.com/Dentrax/AsyncServerClient)

**AsyncServerClient is the simple, open-source, cross-platform Async TCP Server-Client project with SQL support for .NET Assemblies built in .NET Core**

**Warning:** It is an old project I made in `2015`. I converted whole project into .NET Core and made new improvements.

Click here for **[.NET Core](https://docs.microsoft.com/en-us/dotnet/core/)**

[What It Is](#what-it-is)

[How To Use](#how-to-use)

[Features](#features)

[Requirements](#requirements)

[Dependencies](#dependencies)

[About](#about)

[Collaborators](#collaborators)

[Branches](#branches) 

[Copyright & Licensing](#copyright--licensing)

[Contributing](#contributing)

[Contact](#contact)

## What It Is

**AsyncServerClient is the simple, open-source, cross-platform Async TCP Server-Client project with SQL support for .NET Assemblies built in .NET Core SDK**

AsyncServerClient teaches you how TCP based Async Server-Client works actually with the three-way handshake architecture, as simplified as possibly. It is designed in the bare-minimum struct.

**Uses : `.NET Core 2`**

Controls and wizards are available for users to:

> * Deep detect feature to incoming-outgoing packets
> * Analyze server-side or client-side packets with deeper details
> * Send packets with any data type
> * Three-Way TCP [Handshaking](https://en.wikipedia.org/wiki/Handshaking) feature
> * [Blowfish](https://en.wikipedia.org/wiki/Blowfish_(cipher)) encrypted secure packets (You can also add new algorithms easily)
> * Not complex, not hard, best project to understand TCP Async Server-Client Networking

### Screenshot

![ServerClient](https://raw.githubusercontent.com/Dentrax/AsyncServerClient/master/resources/screenshot.png)

## How To Use

1. Clone the project to your computer by executing the following command:
```
$ git clone https://github.com/Dentrax/AsyncServerClient.git
```

2. Navigate to your `AsyncServerClient` folder: 
```
$ cd AsyncServerClient/
```

3. Build the all projects using `dotnet` command:
```
$ dotnet build --configuration Release --output ./bin/Release --verbosity quiet
```

**Warning:** You may want to change your hardcoded Server-Client binding addresses from [here](https://github.com/Dentrax/AsyncServer/blob/master/AsyncServer/Program.cs#L289) and [here](https://github.com/Dentrax/AsyncClient/blob/master/AsyncClient/Program.cs#L28)

4. Run the AsyncServer:
```
$ dotnet add .\AsyncServer\ package System.Data.SqlClient --version 4.6.0

$ cp .\resources\server.ini .\AsyncServer\bin\Release

$ dotnet run --configuration Release --project .\AsyncServer\
```

5. Run the AsyncClient:
```
$ dotnet run --configuration Release --project .\AsyncClient\
```

## Requirements

* You should be familiar with C# programming
* You should be familiar with TCP Socket programming
* You should be familiar with Handshaking
* You should be familiar with Asynchronous Sockets
* You will need a computer on which you have the rights to compile dotnet files

## Dependencies

* .NET Core 2.x SDK (for compiling and testing)
* [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/) (for SQL feature)

## About

AsyncServerClient was created to serve three purposes:

**AsyncServerClient teaches you how Async Server-Client works actually as simplified as possibly**

1. To act as a guide to teach how Async Server-Client works using .NET Core

2. To provide a simplest and easiest way to learn things about Three-Way TCP Handshaking

3. There is a source for you to develop own Server-Client mechanism in dotNET environment using `ServerFramework`

## Collaborators

**Project Manager** - Furkan Türkal (GitHub: **[Dentrax](https://github.com/dentrax)**)

## Branches

We publish source for the **[AsyncServerClient]** in single rolling branch:

The **[master branch](https://github.com/dentrax/AsyncServerClient/tree/master)** is extensively tested and makes a great starting point. Also tracks [live changes](https://github.com/dentrax/AsyncServerClient/commits/master) by commits.

## Copyright & Licensing

The base project code is copyrighted by Furkan 'Dentrax' Türkal and is covered by single licence.

All program code (i.e. cs, .md) is licensed under MIT License unless otherwise specified. Please see the **[LICENSE.md](https://github.com/Dentrax/AsyncServerClient/blob/master/LICENSE)** file for more information.

**ServerFramework**

It was developed and made by Drew 'pushedx' Benton. For more information, please **[click here](https://goo.gl/w9uJjw)**

**References**

While this repository is being prepared, it may have been quoted from some sources. 

If there is an unspecified source or if you think that I made a copyright infringement, please contact with me.

## Contributing

Please check the [CONTRIBUTING.md](CONTRIBUTING.md) file for contribution instructions and naming guidelines.

## Contact

AsyncServerClient was created by Furkan 'Dentrax' Türkal

 * <https://www.furkanturkal.com>
 
You can contact by URL:
    **[CONTACT](https://github.com/dentrax)**

<kbd>Best Regards</kbd>