using QuickActions.Core;
using Xunit;

namespace QuickActions.Tests;

/// <summary>
/// The plugin keeps page.js in the web UI: it asks with a few bytes whether the script is there and sends the whole
/// script only when the page says it is gone. Each step here is one game frame.
/// </summary>
public class ScriptWatchTests
{
    private const long Check = ScriptWatch.CheckMilliseconds;

    [Fact]
    public void The_first_frame_asks_the_page_whether_the_script_is_there()
    {
        Assert.Equal(PageCall.Probe, new ScriptWatch().Step(0, null));
    }

    [Fact]
    public void A_page_that_still_has_the_script_is_never_sent_it_again()
    {
        var watch = new ScriptWatch();
        var sent = new List<PageCall>();

        // A minute of frames at 60 fps, the page answering "there" to every probe the frame after it is sent.
        string? answer = null;

        for (long now = 0; now < 60_000; now += 16)
        {
            var call = watch.Step(now, answer);

            answer = call == PageCall.Probe ? ScriptWatch.Present : null;

            if (call != PageCall.Nothing) sent.Add(call);
        }

        Assert.DoesNotContain(PageCall.Install, sent);

        // One probe every two seconds, not one a frame.
        Assert.InRange(sent.Count, 25, 31);
    }

    [Fact]
    public void A_page_that_lost_the_script_gets_it_once_then_is_only_asked_again()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);

        Assert.Equal(PageCall.Install, watch.Step(16, "missing"));
        Assert.Equal(PageCall.Nothing, watch.Step(32, null));
        Assert.Equal(PageCall.Nothing, watch.Step(16 + Check - 1, null));
        Assert.Equal(PageCall.Probe, watch.Step(16 + Check, null));
    }

    [Fact]
    public void An_answer_worded_some_other_way_sends_the_script_rather_than_leave_the_page_without_it()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);

        Assert.Equal(PageCall.Install, watch.Step(16, "\"ok\""));
    }

    [Fact]
    public void An_empty_answer_sends_the_script()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);

        Assert.Equal(PageCall.Install, watch.Step(16, ""));
    }

    [Fact]
    public void Only_one_probe_is_out_at_a_time()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);

        for (long now = 16; now < ScriptWatch.AnswerWaitMilliseconds; now += 16)
        {
            Assert.Equal(PageCall.Nothing, watch.Step(now, null));
        }
    }

    [Fact]
    public void A_probe_never_answered_is_given_up_and_asked_again_without_sending_the_script()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);

        // Given up: no script, since silence says nothing about the page.
        Assert.Equal(PageCall.Nothing, watch.Step(ScriptWatch.AnswerWaitMilliseconds, null));
        Assert.Equal(PageCall.Probe, watch.Step(ScriptWatch.AnswerWaitMilliseconds + Check, null));
    }

    [Fact]
    public void A_probe_that_could_not_be_sent_is_tried_again_two_seconds_later()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);
        watch.NotSent(0);

        Assert.Equal(PageCall.Nothing, watch.Step(Check - 1, null));
        Assert.Equal(PageCall.Probe, watch.Step(Check, null));
    }

    [Fact]
    public void A_late_answer_to_a_probe_already_given_up_is_ignored()
    {
        var watch = new ScriptWatch();

        watch.Step(0, null);
        watch.Step(ScriptWatch.AnswerWaitMilliseconds, null);

        Assert.Equal(PageCall.Nothing, watch.Step(ScriptWatch.AnswerWaitMilliseconds + 16, "missing"));
    }

    [Fact]
    public void The_probe_reads_the_flag_page_js_sets()
    {
        var pageScript = PageScript.Source();

        Assert.Contains("window.__slquickactions", ScriptWatch.Probe);
        Assert.Contains("window.__slquickactions = true", pageScript);
    }
}
