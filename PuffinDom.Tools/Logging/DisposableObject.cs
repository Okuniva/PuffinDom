using System;
using System.Collections.Generic;

namespace Core.Tools.Disposables;

public class DisposableObject : IDisposable
{
    private readonly List<Action> _onDisposedActions = new();

    private bool _isDisposed;

    public DisposableObject WhenDisposed(Action action)
    {
        _onDisposedActions.Add(action);
        return this;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            if(!_isDisposed)
                throw new Exception($"Detected double dispose of instance of {nameof(DisposableObject)} class");
            return;
        }

        _onDisposedActions.ForEach(action => action());
        _isDisposed = true;
    }
}