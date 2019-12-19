﻿using System;
using WpfBasicForcedLogin.Core.Contracts.Services;
using WpfBasicForcedLogin.Core.Helpers;
using WpfBasicForcedLogin.Helpers;
using WpfBasicForcedLogin.Strings;

namespace WpfBasicForcedLogin.ViewModels
{
    public class LogInViewModel : Observable
    {
        private readonly IIdentityService _identityService;
        private string _statusMessage;
        private bool _isBusy;
        private RelayCommand _loginCommand;

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                Set(ref _isBusy, value);
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public RelayCommand LoginCommand => _loginCommand ?? (_loginCommand = new RelayCommand(OnLogin, () => !IsBusy));

        public LogInViewModel(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        private async void OnLogin()
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            var loginResult = await _identityService.LoginAsync();
            StatusMessage = GetStatusMessage(loginResult);
            IsBusy = false;
        }

        private string GetStatusMessage(LoginResultType loginResult)
        {
            switch (loginResult)
            {
                case LoginResultType.Unauthorized:
                    return Resources.StatusUnauthorized;
                case LoginResultType.NoNetworkAvailable:
                    return Resources.StatusNoNetworkAvailable;
                case LoginResultType.UnknownError:
                    return Resources.StatusLoginFails;
                case LoginResultType.Success:
                case LoginResultType.CancelledByUser:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
