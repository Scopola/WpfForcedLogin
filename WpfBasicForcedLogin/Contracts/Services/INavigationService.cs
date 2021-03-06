﻿using System;
using System.Windows.Controls;

namespace WpfBasicForcedLogin.Contracts.Services
{
    public interface INavigationService
    {
        event EventHandler<string> Navigated;

        Frame Frame { get; }

        bool CanGoBack { get; }

        void Initialize(Frame shellFrame);

        bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);

        void GoBack();

        void UnsubscribeNavigation();
    }
}
