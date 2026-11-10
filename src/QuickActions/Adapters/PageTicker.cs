using System;
using ExiledToolkit.Game;
using UnityEngine;

namespace QuickActions.Adapters;

/// <summary>The Unity object that keeps the page script in the web UI. A failure is logged once, never rethrown into the game.</summary>
public sealed class PageTicker : MonoBehaviour
{
    internal static PatchKit? Hooks;
    internal static WebPage? Page;

    public PageTicker(IntPtr pointer) : base(pointer)
    {
    }

    private void Update()
    {
        try
        {
            // A managed clock: Unity's time getters go through the game and allocate on every call.
            Page?.KeepInstalled(Environment.TickCount64);
        }
        catch (Exception ex)
        {
            Hooks?.Fail("keeping the page script in the web UI, the row may not scroll", ex);
        }
    }
}
