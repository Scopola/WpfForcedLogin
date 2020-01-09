﻿using System;
using System.IO;
using System.Threading.Tasks;
using WpfBasicForcedLogin.Core.Contracts.Services;
using WpfBasicForcedLogin.Core.Models;
using WpfLightForcedLogin.Contracts.Services;
using WpfLightForcedLogin.Helpers;
using WpfLightForcedLogin.Models;
using WpfLightForcedLogin.ViewModels;

namespace WpfLightForcedLogin.Services
{
    public class UserDataService : IUserDataService, IDisposable
    {
        private readonly IFilesService _filesService;
        private readonly IIdentityService _identityService;
        private readonly IMicrosoftGraphService _microsoftGraphService;
        private readonly AppConfig _config;
        private readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private UserViewModel _user;

        public event EventHandler<UserViewModel> UserDataUpdated;

        public UserDataService(IFilesService filesService, IIdentityService identityService, IMicrosoftGraphService microsoftGraphService, AppConfig config)
        {
            _filesService = filesService;
            _identityService = identityService;
            _microsoftGraphService = microsoftGraphService;
            _config = config;
        }

        public void Initialize()
        {
            _identityService.LoggedIn += OnLoggedIn;
            _identityService.LoggedOut += OnLoggedOut;
        }

        public void Dispose()
        {
            _identityService.LoggedIn -= OnLoggedIn;
            _identityService.LoggedOut -= OnLoggedOut;
        }

        public UserViewModel GetUser()
        {
            if (_user == null)
            {
                _user = GetUserFromCache();
                if (_user == null)
                {
                    _user = GetDefaultUserData();
                }
            }

            return _user;
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            _user = await GetUserFromGraphApiAsync();
            UserDataUpdated?.Invoke(this, _user);
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            _user = null;
            var folderPath = Path.Combine(_localAppData, _config.ConfigurationsFolder);
            var fileName = _config.UserFileName;
            _filesService.Save<User>(folderPath, fileName, null);
        }

        private UserViewModel GetUserFromCache()
        {
            var folderPath = Path.Combine(_localAppData, _config.ConfigurationsFolder);
            var fileName = _config.UserFileName;
            var cacheData = _filesService.Read<User>(folderPath, fileName);
            return GetUserViewModelFromData(cacheData);
        }

        private async Task<UserViewModel> GetUserFromGraphApiAsync()
        {
            var accessToken = await _identityService.GetAccessTokenForGraphAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var userData = await _microsoftGraphService.GetUserInfoAsync(accessToken);
            if (userData != null)
            {
                userData.Photo = await _microsoftGraphService.GetUserPhoto(accessToken);
                var folderPath = Path.Combine(_localAppData, _config.ConfigurationsFolder);
                var fileName = _config.UserFileName;
                _filesService.Save<User>(folderPath, fileName, userData);
            }

            return GetUserViewModelFromData(userData);
        }

        private UserViewModel GetUserViewModelFromData(User userData)
        {
            if (userData == null)
            {
                return null;
            }

            var userPhoto = string.IsNullOrEmpty(userData.Photo)
                ? ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
                : ImageHelper.ImageFromString(userData.Photo);

            return new UserViewModel()
            {
                Name = userData.DisplayName,
                UserPrincipalName = userData.UserPrincipalName,
                Photo = userPhoto
            };
        }

        private UserViewModel GetDefaultUserData()
        {
            return new UserViewModel()
            {
                Name = _identityService.GetAccountUserName(),
                Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            };
        }
    }
}
