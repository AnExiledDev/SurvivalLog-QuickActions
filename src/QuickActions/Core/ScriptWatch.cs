namespace QuickActions.Core;

/// <summary>What the plugin sends to the web UI this frame.</summary>
public enum PageCall
{
    Nothing,

    /// <summary>The tiny <see cref="ScriptWatch.Probe"/>, whose answer says whether page.js is still there.</summary>
    Probe,

    /// <summary>The whole page.js.</summary>
    Install,

    /// <summary><see cref="ScriptWatch.TurnOffCall"/>, sent once when the mod is turned off.</summary>
    TurnOff
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

    /// <summary>
    /// Run in the root document; page.js sets the object it reads. The probe also turns the script back on, so after
    /// the mod is turned on again the next probe brings the row back.
    /// </summary>
    public const string Probe = "window.__slquickactions?(window.__slquickactions.setOn(true),'" + Present + "'):'missing'";

    /// <summary>Puts the row back the way the game draws it. A page without the script has nothing to put back.</summary>
    public const string TurnOffCall = "window.__slquickactions&&window.__slquickactions.setOn(false)";

    /// <summary>
    /// The probe's answer when the script is there. Any other answer sends the script, so a reply the web view words
    /// differently costs a needless copy of the script, never a page left without it.
    /// </summary>
    public const string Present = "ok";

    private bool _isAwaitingAnswer;
    private long _answerDueAt;
    private long _nextProbeAt;
    private bool _isOn = true;
    private bool _isTurnOffDue;

    /// <summary>Off, the page gets one call that turns its script off and then nothing until this is on again.</summary>
    public bool IsOn
    {
        set
        {
            if (_isOn && !value) _isTurnOffDue = true;
            if (value) _isTurnOffDue = false;

            _isOn = value;
        }
    }

    /// <summary>
    /// One step per frame. <paramref name="answer"/> is what the page answered since the last step, or null; an answer
    /// that comes while no probe is out is left over from one already given up, and is ignored.
    /// </summary>
    public PageCall Step(long now, string? answer)
    {
        if (!_isOn) return TurnOffOnce();

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

    /// <summary>A probe still out is dropped: its answer says nothing once the script is off.</summary>
    private PageCall TurnOffOnce()
    {
        _isAwaitingAnswer = false;

        if (!_isTurnOffDue) return PageCall.Nothing;

        _isTurnOffDue = false;

        return PageCall.TurnOff;
    }

    private PageCall Settle(long now, string? answer)
    {
        if (answer == null && now < _answerDueAt) return PageCall.Nothing;

        _isAwaitingAnswer = false;
        _nextProbeAt = now + CheckMilliseconds;

        return answer == null || answer == Present ? PageCall.Nothing : PageCall.Install;
    }
}
