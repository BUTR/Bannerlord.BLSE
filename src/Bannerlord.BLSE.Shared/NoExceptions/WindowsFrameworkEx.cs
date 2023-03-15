using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using TaleWorlds.TwoDimension.Standalone;
using TaleWorlds.TwoDimension.Standalone.Native.Windows;

namespace Bannerlord.BLSE.Shared.NoExceptions;

public sealed class WindowsFrameworkEx
{
    private bool _isActive;
    private FrameworkDomain[]? _frameworkDomains;
    private Thread[]? _frameworkDomainThreads;
    private readonly List<IMessageCommunicator> _messageCommunicators = new();
    private int _abortedThreadCount;

    public WindowsFrameworkThreadConfig ThreadConfig { get; set; }

    public void Initialize(FrameworkDomain[] frameworkDomains)
    {
        _frameworkDomains = frameworkDomains;
        _isActive = true;
        switch (ThreadConfig)
        {
            case WindowsFrameworkThreadConfig.SingleThread:
            {
                _frameworkDomainThreads = new Thread[1];
                CreateThread(0);
                return;
            }
            case WindowsFrameworkThreadConfig.MultiThread:
            {
                _frameworkDomainThreads = new Thread[frameworkDomains.Length];
                for (var i = 0; i < frameworkDomains.Length; i++)
                    CreateThread(i);
                break;
            }
        }
    }

    private void CreateThread(int index)
    {
        if (_frameworkDomainThreads is null || _frameworkDomains is null) return;

#if v110 || v111
        TaleWorlds.Library.Common.SetInvariantCulture();
#endif
        _frameworkDomainThreads[index] = new Thread(MainLoop);
        _frameworkDomainThreads[index].SetApartmentState(ApartmentState.STA);
        _frameworkDomainThreads[index].Name = $"{_frameworkDomains[index]} Thread";
        _frameworkDomainThreads[index].CurrentCulture = CultureInfo.InvariantCulture;
        _frameworkDomainThreads[index].CurrentUICulture = CultureInfo.InvariantCulture;
    }

    public void RegisterMessageCommunicator(IMessageCommunicator communicator) => _messageCommunicators.Add(communicator);
    public void UnRegisterMessageCommunicator(IMessageCommunicator communicator) => _messageCommunicators.Remove(communicator);

    private void MessageLoop()
    {
        if (ThreadConfig == WindowsFrameworkThreadConfig.NoThread)
        {
            for (var i = 0; i < _frameworkDomains?.Length; i++)
                _frameworkDomains[i].Update();
        }
        for (var j = 0; j < _messageCommunicators.Count; j++)
            _messageCommunicators[j].MessageLoop();
    }

    private void MainLoop(object parameter)
    {
        switch (ThreadConfig)
        {
            case WindowsFrameworkThreadConfig.SingleThread:
            {
                while (_isActive)
                {
                    for (var i = 0; i < _frameworkDomains?.Length; i++)
                        _frameworkDomains[i].Update();
                }

                break;
            }
            case WindowsFrameworkThreadConfig.MultiThread:
            {
                var frameworkDomain = parameter as FrameworkDomain;
                while (frameworkDomain is not null && _isActive)
                    frameworkDomain.Update();
                break;
            }
        }

        Interlocked.Increment(ref _abortedThreadCount);
        OnFinalize();
    }

    public void Stop()
    {
        _isActive = false;
    }

    public void Start()
    {
        _isActive = true;

        switch (ThreadConfig)
        {
            case WindowsFrameworkThreadConfig.SingleThread:
            {
                if (_frameworkDomainThreads is null) return;
                _frameworkDomainThreads[0].Start();
                break;
            }
            case WindowsFrameworkThreadConfig.MultiThread:
            {
                if (_frameworkDomainThreads is null) return;
                for (var i = 0; i < _frameworkDomains?.Length; i++)
                    _frameworkDomainThreads[i].Start(_frameworkDomains[i]);
                break;
            }
            case WindowsFrameworkThreadConfig.NoThread:
            {
                while (_isActive)
                {
                    if (User32.PeekMessage(out var message, IntPtr.Zero, 0U, 0U, 1U))
                    {
                        User32.TranslateMessage(ref message);
                        User32.DispatchMessage(ref message);
                    }
                    MessageLoop();
                }
                return;
            }
        }

        while (_isActive)
        {
            if (User32.PeekMessage(out var message, IntPtr.Zero, 0U, 0U, 1U))
            {
                if (message.msg == WindowMessage.Quit) break;
                User32.TranslateMessage(ref message);
                User32.DispatchMessage(ref message);
            }
            MessageLoop();
        }
    }

    private void OnFinalize()
    {
        if (_abortedThreadCount != _frameworkDomainThreads?.Length) return;
        _frameworkDomainThreads = null;

        for (var i = 0; i < _frameworkDomains?.Length; i++)
            _frameworkDomains[i].Destroy();
    }
}