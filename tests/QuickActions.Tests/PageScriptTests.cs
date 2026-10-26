using Xunit;

namespace QuickActions.Tests;

/// <summary>
/// The row of suggestions in the web UI scrolls sideways, and chips scrolled out of sight stop taking clicks. Once the
/// page is found it is held: the poll does no work while it stays, and the chips are measured once per change.
/// </summary>
public class PageScriptTests
{
    // Four chips in a 200px row: the fourth is out of sight.
    private const string FourChips = "['Eat bread', 'Sleep', 'Play guitar', 'Repair door']";

    /// <summary>The script with CoreUI1 open and its row fitted: the global <c>page</c> in the global <c>frame</c>.</summary>
    private static PageScript Settled()
    {
        var script = new PageScript();

        script.Run($"var page = corePage({FourChips}); var frame = openFrame(page); poll(); nextFrame();");

        return script;
    }

    [Fact]
    public void Chips_out_of_sight_stop_taking_clicks()
    {
        var script = Settled();

        Assert.Equal("on,on,on,off", script.Text("clickable(page.row())"));
    }

    [Fact]
    public void The_row_takes_the_mouse_wheel_and_its_gaps_count_as_the_row()
    {
        var script = Settled();

        Assert.True(script.Is("page.row().hasAttribute('data-interactive')"));
        Assert.True(script.Is("page.row().listeners.wheel.length === 1 && page.row().listeners.scroll.length === 1"));
    }

    [Fact]
    public void While_the_page_stays_a_poll_searches_and_measures_nothing()
    {
        var script = Settled();

        script.Run("resetCounts(); for (var i = 0; i < 20; i++) poll();");

        Assert.Equal(0, script.Number("counts.searches"));
        Assert.Equal(0, script.Number("counts.measures"));
        Assert.Equal(0, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void A_new_chip_is_fitted_on_the_next_frame()
    {
        var script = Settled();

        script.Run("page.row().appendChild(chip(page.doc, 'Wash')); layOut(page.row()); flush();");

        Assert.Equal(1, script.Number("frameCallbacks.length"));

        script.Run("nextFrame();");

        Assert.Equal("on,on,on,off,off", script.Text("clickable(page.row())"));
    }

    [Fact]
    public void A_chip_that_scrolls_into_sight_after_a_change_takes_clicks_again()
    {
        var script = Settled();

        script.Run("page.row().removeChild(page.row().children[0]); layOut(page.row()); flush(); nextFrame();");

        Assert.Equal("on,on,on", script.Text("clickable(page.row())"));
    }

    [Fact]
    public void Changes_delivered_twice_before_a_frame_still_queue_one_fit()
    {
        var script = Settled();

        script.Run("""
            resetCounts();
            page.row().appendChild(chip(page.doc, 'Wash'));
            flush();
            page.row().appendChild(chip(page.doc, 'Cook'));
            flush();
            """);

        Assert.Equal(1, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void Many_changes_in_one_frame_are_measured_once()
    {
        var script = Settled();

        script.Run("""
            resetCounts();
            page.row().appendChild(chip(page.doc, 'Wash'));
            page.row().appendChild(chip(page.doc, 'Cook'));
            page.row().children[0].classList.add('is-highlight');
            setText(page.row().children[1].children[0], 'Sleep well');
            layOut(page.row());
            flush();
            """);

        Assert.Equal(1, script.Number("frameCallbacks.length"));

        script.Run("nextFrame();");

        // The row and its six chips, read once each.
        Assert.Equal(7, script.Number("counts.measures"));
    }

    [Fact]
    public void A_chip_animation_ending_fits_the_chips_again()
    {
        var script = new PageScript();

        // The last chip is still sliding in when the page is found.
        script.Run($"var page = corePage({FourChips}); page.row().children[3].classList.names.push('chip-enter-active'); openFrame(page); poll(); nextFrame();");
        script.Run("page.row().children[3].classList.remove('chip-enter-active'); flush();");

        Assert.Equal(1, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void The_rows_own_class_change_after_a_scroll_does_not_measure_again()
    {
        var script = Settled();

        script.Run("page.row().classList.toggle('slqa-moved'); flush();");

        Assert.Equal(0, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void A_scroll_fits_the_chips_to_what_is_now_in_sight()
    {
        var script = Settled();

        script.Run("""
            var scrolled = page.row();
            scrolled.scrollLeft = 70;
            layOut(scrolled);
            scrolled.listeners.scroll[0]({ currentTarget: scrolled });
            nextFrame();
            """);

        Assert.Equal("off,on,on,on", script.Text("clickable(page.row())"));

        // The class change that tells the game to re-read its clickable areas wakes no second pass.
        Assert.Equal(0, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void A_row_drawn_after_the_page_was_found_is_fitted_without_searching_the_page()
    {
        var script = new PageScript();

        script.Run("var page = corePage(null); openFrame(page); poll(); nextFrame(); resetCounts();");
        script.Run($"page.middle.appendChild(row(page.doc, {FourChips})); flush(); nextFrame();");

        Assert.Equal("on,on,on,off", script.Text("clickable(page.row())"));
        Assert.True(script.Is("page.row().hasAttribute('data-interactive')"));
        Assert.Equal(0, script.Number("counts.searches"));
    }

    [Fact]
    public void A_row_vue_draws_again_is_watched_and_the_old_one_let_go()
    {
        var script = Settled();

        script.Run($"""
            var oldRow = page.row();
            page.middle.removeChild(oldRow);
            page.middle.appendChild(row(page.doc, {FourChips}));
            flush();
            nextFrame();
            """);

        Assert.Equal("on,on,on,off", script.Text("clickable(page.row())"));

        script.Run("oldRow.appendChild(chip(page.doc, 'Wash')); flush();");

        Assert.Equal(0, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void A_page_reloaded_into_the_same_frame_is_found_again()
    {
        var script = Settled();

        script.Run($"var oldPage = page; page = corePage({FourChips}); frame.contentDocument = page.doc; poll(); nextFrame();");

        Assert.Equal("on,on,on,off", script.Text("clickable(page.row())"));

        // The old page's row still says it is connected; it must not keep waking the script.
        script.Run("oldPage.row().appendChild(chip(oldPage.doc, 'Wash')); flush();");

        Assert.Equal(0, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void A_closed_page_is_let_go_and_the_next_one_found()
    {
        var script = Settled();

        script.Run("closeFrame(frame); poll(); page.row().appendChild(chip(page.doc, 'Wash')); flush();");

        Assert.Equal(0, script.Number("frameCallbacks.length"));

        script.Run($"var next = corePage({FourChips}); openFrame(next); poll(); nextFrame();");

        Assert.Equal("on,on,on,off", script.Text("clickable(next.row())"));
    }

    [Fact]
    public void A_new_best_chip_scrolls_the_row_back_to_the_start()
    {
        var script = Settled();

        script.Run("page.row().scrollLeft = 140; page.row().insertBefore(chip(page.doc, 'Wash'), page.row().children[0]); flush(); nextFrame();");

        Assert.Equal(0, script.Number("page.row().scrollLeft"));
    }

    [Fact]
    public void A_change_behind_the_best_chip_keeps_the_scroll()
    {
        var script = Settled();

        script.Run("page.row().scrollLeft = 140; page.row().appendChild(chip(page.doc, 'Wash')); flush(); nextFrame();");

        Assert.Equal(140, script.Number("page.row().scrollLeft"));
    }

    [Fact]
    public void A_resized_window_fits_the_chips_again()
    {
        var script = Settled();

        script.Run("page.doc.defaultView.fire('resize');");

        Assert.Equal(1, script.Number("frameCallbacks.length"));
    }

    [Fact]
    public void Other_pages_are_left_alone()
    {
        var script = new PageScript();

        script.Run($"var phone = corePage({FourChips}); openFrame(phone, 'https://game.local/Phone.html'); poll(); nextFrame();");

        Assert.False(script.Is("phone.row().hasAttribute('data-interactive')"));
    }

    [Fact]
    public void A_second_copy_of_the_script_adds_no_second_poll()
    {
        var script = new PageScript();

        script.Run(PageScript.Source());

        Assert.Equal(1, script.Number("polls.length"));
    }
}
