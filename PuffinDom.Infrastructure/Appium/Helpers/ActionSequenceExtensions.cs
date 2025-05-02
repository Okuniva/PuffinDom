using System;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using PuffinDom.Tools.Extensions;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace PuffinDom.Infrastructure.Appium.Helpers;

public static class ActionSequenceExtensions
{
    public static ActionSequence SetPoint(
        this ActionSequence sequence,
        PointerInputDevice touchDevice,
        int x,
        int y,
        TimeSpan? duration = null)
    {
        sequence.AddAction(
            touchDevice.CreatePointerMove(
                CoordinateOrigin.Viewport,
                x,
                y,
                duration ?? 0.Milliseconds()));

        return sequence;
    }

    public static ActionSequence Wait(this ActionSequence sequence, PointerInputDevice touchDevice, TimeSpan pause)
    {
        if (pause > TimeSpan.Zero)
            sequence.AddAction(touchDevice.CreatePause(pause));

        return sequence;
    }

    public static ActionSequence Down(this ActionSequence sequence, PointerInputDevice touchDevice)
    {
        sequence.AddAction(touchDevice.CreatePointerDown(PointerButton.TouchContact));
        return sequence;
    }

    public static ActionSequence Up(this ActionSequence sequence, PointerInputDevice touchDevice)
    {
        sequence.AddAction(touchDevice.CreatePointerUp(PointerButton.TouchContact));
        return sequence;
    }
}