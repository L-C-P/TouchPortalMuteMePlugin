using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TouchPortalSDK.Configuration;
using TPMuteMe;

Assembly assembly = Assembly.GetExecutingAssembly();
String baseDirectory = Path.GetDirectoryName(assembly.Location)!;

// Build configuration:
IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                                      .SetBasePath(baseDirectory)
                                      .AddJsonFile("appsettings.json")
                                      .Build();

// Standard method for build a ServiceProvider in .Net:
ServiceCollection serviceCollection = new ServiceCollection();

// Add logging
serviceCollection.AddLogging(configure =>
{
    configure.AddSimpleConsole(options => options.TimestampFormat = "[yyyy.MM.dd HH:mm:ss] ");
    configure.AddConfiguration(configurationRoot.GetSection("Logging"));
});

// Registering the Plugin to the IoC container:
serviceCollection.AddTouchPortalSdk(configurationRoot);
serviceCollection.AddSingleton<MuteMePlugin>();
serviceCollection.AddSingleton<MuteMe>();

// Use your IoC framework to resolve the plugin with it's dependencies,
ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(true);
MuteMePlugin plugin = serviceProvider.GetRequiredService<MuteMePlugin>();

// Run it
plugin.Run();
