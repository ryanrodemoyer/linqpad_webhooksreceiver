<Query Kind="Statements">
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

// these classes for ILogger are what's required from microsoft/asp.net core to establish a custom logger
// our custom logger allows us to send the logs to a DumpContainer for optimized data display in our script
// https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
public class DumpContainerLogger : ILogger
{
	private readonly string _categoryName;
	private readonly DumpContainer _dc;

	public DumpContainerLogger(string categoryName, DumpContainer dc)
	{
		_categoryName = categoryName;
		_dc = dc;
	}

	public IDisposable BeginScope<TState>(TState state) => null;

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		if (!IsEnabled(logLevel)) return;

		var message = $"{formatter(state, exception)}";
		//		var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {formatter(state, exception)}";
		if (exception != null)
		{
			message += $"\nException: {exception}";
		}

		_dc.AppendContent(message);
	}
}

public class DumpContainerProvider : ILoggerProvider
{
	private readonly DumpContainer _dc;

	public DumpContainerProvider(DumpContainer dc)
	{
		_dc = dc;
	}

	public ILogger CreateLogger(string categoryName)
	{
		return new DumpContainerLogger(categoryName, _dc);
	}

	public void Dispose() { }
}

public static class DumpContainerLoggerExtensions
{
	public static ILoggingBuilder AddDumpContainer(this ILoggingBuilder builder, DumpContainer dc)
	{
		builder.AddProvider(new DumpContainerProvider(dc));
		return builder;
	}
}

