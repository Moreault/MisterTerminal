![MisterTerminal](https://github.com/Moreault/MisterTerminal/blob/master/misterterminal.png)

# MisterTerminal
A high level library to easily and cleanly build smarter console applications.


## Getting started

MisterTerminal uses [AutoInject](https://github.com/Moreault/AutoInject "AutoInject") to automatically add all MisterTerminal services. You do still need to manually call AddAutoInjectServices or use [AssemblyInitializer](https://github.com/Moreault/AssemblyInitializer "AssemblyInitializer") which makes it even easier.

If you dislike automatic injection and prefer boilerplate, you can use the following method:


```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMisterTerminal();
}
```

Afterwards, you can inject the ITerminal interface and use it like so :

```c#
_terminal.Write("Some text");

//This line will only appear if your project is running in debug mode (it will print on both the application console AND the debugging console in case you only have access to the latter.)
_terminal.Debug.Write("Some debug text");

//If you only need to use the console for debugging purposes then you should inject IDebugTerminal directly
_debugTerminal.Write("Other debug text");
```

Check out the MisterTerminal.Sample for more examples.