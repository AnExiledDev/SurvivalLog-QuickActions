using Jint;

namespace QuickActions.Tests;

/// <summary>
/// page.js exactly as the plugin embeds it, run in Jint with Exiled's Toolkit's api.js inside FakeRoot.js, a Root.html
/// whose CoreUI1 frames the tests open, reload and close. Tests talk to it in JavaScript and read back plain strings,
/// numbers and booleans.
/// </summary>
internal sealed class PageScript
{
    private const string ScriptResource = "QuickActions.page.js";

    private readonly Engine _engine = new();

    /// <param name="isToolkitFirst">Whether the game runs the Toolkit's api.js before page.js; false leaves it for <see cref="LoadToolkit"/>.</param>
    public PageScript(bool isToolkitFirst = true)
    {
        _engine.Execute(Resource("QuickActions.Tests.FakeRoot.js"));

        if (isToolkitFirst) LoadToolkit();

        _engine.Execute(Source());
    }

    /// <summary>page.js as the plugin sends it.</summary>
    public static string Source() => Resource(ScriptResource);

    /// <summary>Runs the Toolkit's api.js, the way the game does when it loads after page.js.</summary>
    public void LoadToolkit() => _engine.Execute(Resource("ExiledToolkit.api.js"));

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
