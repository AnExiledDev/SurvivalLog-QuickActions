namespace QuickActions.Core;

/// <summary>What the plugin sends to the web UI this frame.</summary>
public enum PageCall
{
    Nothing,

    /// <summary>The tiny <see cref="ScriptWatch.Probe"/>, whose answer says whether page.js is still there.</summary>
    Probe,

    /// <summary>The whole page.js.</summary>
    Install
}

/// <summary>
/// Keeps page.js in the web UI without sending it over and over. The web view can reload at any time and lose the
/// script, so every couple of seconds a probe of a few bytes asks whether it is still there, and the script itself
/// goes only when the page answers that it is not. One probe is out at a time.
/// </summary>
public sealed class ScriptWatch
{
    public const long CheckMilliseconds = 2000;

    /// <summary>A probe the web view never answered (it reloaded, or the call was lost) is given up and sent again.</summary>
    public const long AnswerWaitMilliseconds = 5000;

    /// <summary>Run in the root document; page.js sets the flag it reads.</summary>
    public const string Probe = "window.__slquickactions?'" + Present + "':'missing'";

    /// <summary>
    /// The probe's answer when the script is there. Any other answer sends the script, so a reply the web view words
    /// differently costs a needless copy of the script, never a page left without it.
    /// </summary>
    public const string Present = "ok";

    private bool _isAwaitingAnswer;
    private long _answerDueAt;
    private long _nextProbeAt;

    /// <summary>
    /// One step per frame. <paramref name="answer"/> is what the page answered since the last step, or null; an answer
    /// that comes while no probe is out is left over from one already given up, and is ignored.
    /// </summary>
    public PageCall Step(long now, string? answer)
    {
        if (_isAwaitingAnswer) return Settle(now, answer);

        if (now < _nextProbeAt) return PageCall.Nothing;

        _isAwaitingAnswer = true;
        _answerDueAt = now + AnswerWaitMilliseconds;

        return PageCall.Probe;
    }

    /// <summary>The probe from the last step never left, because the game's web view is not up yet.</summary>
    public void NotSent(long now)
    {
        _isAwaitingAnswer = false;
        _nextProbeAt = now + CheckMilliseconds;
    }

    private PageCall Settle(long now, string? answer)
    {
        if (answer == null && now < _answerDueAt) return PageCall.Nothing;

        _isAwaitingAnswer = false;
        _nextProbeAt = now + CheckMilliseconds;

        return answer == null || answer == Present ? PageCall.Nothing : PageCall.Install;
    }
}
