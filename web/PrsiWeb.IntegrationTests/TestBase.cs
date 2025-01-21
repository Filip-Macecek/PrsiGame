using Microsoft.AspNetCore.Mvc.Testing;
using System.Diagnostics;


namespace PrsiWeb.IntegrationTests;

[SetUpFixture]
public class TestBase
{
    protected WebApplicationFactory<PrsiWeb.Program> WebApplicationFactory { get; private set; }

    [OneTimeSetUp]
    public void GlobalSetUp()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        WebApplicationFactory = new PrsiWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        Trace.Flush();
        WebApplicationFactory?.Dispose();
    }
}
