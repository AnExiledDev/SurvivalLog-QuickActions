using Jint;

namespace QuickActions.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint inside FakeRoot.js, a Root.html whose CoreUI1 frames the tests
/// open, reload and close. Tests talk to it in JavaScript and read back plain strings, numbers and booleans.
/// </summary>
internal sealed class PageScript
{
    private const string ScriptResource = "QuickActions.page.js";

    private readonly Engine _engine = new();

    public PageScript()
    {
        _engine.Execute(Resource("QuickActions.Tests.FakeRoot.js"));
        _engine.Execute(Source());
    }

    /// <summary>page.js as the plugin sends it.</summary>
    public static string Source() => Resource(ScriptResource);

    public void Run(string statements) => _engine.Execute(statements);

    public string Text(string expression) => _engine.Evaluate($"String({expression})").AsString();

    public bool Is(string expression) => _engine.Evaluate($"!!({expression})").AsBoolean();

    public double Number(string expression) => _engine.Evaluate($"Number({expression})").AsNumber();

    private static string Resource(string name)
    {
        using var stream = typeof(PageScript).Assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException(name + " is not embedded in the test assembly");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
