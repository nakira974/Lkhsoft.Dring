# Extensible Server with MEF

## Overview

This project demonstrates how to create an extensible server using the Managed Extensibility Framework (MEF). 
The server is designed to be highly extensible, allowing developers to easily add new functionalities through MEF. 
Additionally, the server supports the use of LUA as its internal language for creating batch scripts and other automation tasks.

## Features

- **Extensibility with MEF**: The server leverages MEF to allow for the dynamic discovery and composition of parts. This makes it easy to add new features and commands without modifying the core server code.
- **LUA Scripting**: The server uses LUA as its internal scripting language. This allows users to create batch scripts, automate tasks, and extend the server's functionality using LUA scripts.
- **Command-Line Interface (CLI)**: The server includes a robust CLI for interacting with the server, executing commands, and managing tasks.

## Create a configuration file
To configure the server, you need to create an app.config file with the following structure:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <appSettings>
        <add key="CertificatePath" value="C:\\Users\\octocat\\.ssl\\server_certificate.pfx" />
        <add key="CertificatePassword" value="passwd" />
        <add key="PluginsPath" value="C:\\Users\\octocat\\Server\\Plugins" />
        <add key="CommandsPath" value="C:\\Users\\octocat\\Server\\Plugins\\Commands" />
        <add key="UdpPort" value="60500" />
        <add key="TcpPort" value="9091" />
    </appSettings>
</configuration>
```
- CertificatePath: The path to the server certificate file.
- CertificatePassword: The password for the server certificate.
- PluginsPath: The path to the directory containing the server plugins.
- CommandsPath: The path to the directory containing the command plugins.
- UdpPort: The UDP port number for the server.
- TcpPort: The TCP port number for the server.

## Extending the Server with MEF

To add new commands or functionalities to the server, you can create new classes that implement the ICommand interface and export them using MEF. 
Here is an example of how to create a new command:

```cs
using System;
using System.ComponentModel.Composition;

[Export(typeof(ICommand))]
[ExportMetadata("CommandName", "MYCOMMAND")]
public class MyCommand : ICommand
{
    public void Execute(params object[] args)
    {
        Console.WriteLine("MyCommand executed.");
        // Add your command logic here
    }
}
```

## Using LUA Scripting

The server supports LUA as its internal scripting language. 
You can use the CLI command called 'LUA' to launch an interpreter. 
Using the server's LUA API (which you can extend by implementing ```cs public interface ILuaDelegate```), you can create batch scripts, automate tasks, and enhance the server's functionality. 
Here is an example of a simple LUA command:

```lua
print("Hello, world!")
```
