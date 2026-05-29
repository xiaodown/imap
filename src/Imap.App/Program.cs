namespace Imap.App;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var agent = new Windows.WindowsPrintAgent();
        var startupInitialized = agent.Initialize();

        Application.Run(new MainForm(agent, startupInitialized));
    }
}
