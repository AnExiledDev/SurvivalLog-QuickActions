using System;
using System.IO;
using System.Threading;
using GameCore.HotUpdate.ReduxUI;
using QuickActions.Core;

namespace QuickActions.Adapters;

/// <summary>
/// The mod's half that lives in the game's web UI: page.js, run in the root document the game's own pages sit in. It
/// lets the row of suggestions scroll sideways. <see cref="ScriptWatch"/> decides when to probe for it and when to send
/// it again; this sends what it asks for and hands it the page's answers.
/// </summary>
internal sealed class WebPage
{
    private const string ScriptResource = "QuickActions.page.js";

    private readonly Action<string> _log;
    private readonly ScriptWatch _watch = new();

    // Made once: a new callback per probe would allocate a delegate and its game-side twin every couple of seconds.
    private readonly Il2CppSystem.Action<string> _onAnswer;

    private string? _answer;
    private string? _script;
    private bool _hasReportedInstall;
    private bool _hasReportedRunFailure;

    public WebPage(Action<string> log)
    {
        _log = log;
        _onAnswer = (Action<string>)TakeAnswer;
    }

    /// <param name="now">Milliseconds on any steady clock.</param>
    public void KeepInstalled(long now)
    {
        switch (_watch.Step(now, Interlocked.Exchange(ref _answer, null)))
        {
            case PageCall.Nothing:
                return;
            case PageCall.Probe:
                if (!Run(ScriptWatch.Probe, _onAnswer)) _watch.NotSent(now);

                return;
            case PageCall.Install:
                Install();

                return;
            default:
                throw new InvalidOperationException("unknown page call");
        }
    }

    private void Install()
    {
        if (!Run(Script(), null) || _hasReportedInstall) return;

        _hasReportedInstall = true;
        _log("page script sent to the web UI");
    }

    /// <summary>The web view calls this with the probe's result, on the main thread or its own.</summary>
    private void TakeAnswer(string? answer) => Interlocked.Exchange(ref _answer, answer ?? "");

    private bool Run(string javaScript, Il2CppSystem.Action<string>? onAnswer)
    {
        var webView = GameSingleton.Existing<ReduxUISystem>()?.GetWebUILayer()?.canvasWebViewPrefab?.WebView;

        if (webView == null) return false;

        try
        {
            webView.ExecuteJavaScript(javaScript, onAnswer);

            return true;
        }
        catch (Exception ex)
        {
            if (!_hasReportedRunFailure) _log("could not run script in the web UI (reported once): " + ex.Message);

            _hasReportedRunFailure = true;

            return false;
        }
    }

    private string Script()
    {
        if (_script != null) return _script;

        using var stream = typeof(WebPage).Assembly.GetManifestResourceStream(ScriptResource) ?? throw new InvalidOperationException(ScriptResource + " is not embedded");
        using var reader = new StreamReader(stream);

        return _script = reader.ReadToEnd();
    }
}
