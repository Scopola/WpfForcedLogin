﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

using WpfBasicOptionalLogin.Contracts.ViewModels;
using WpfBasicForcedLogin.Core.Contracts.Services;
using WpfBasicForcedLogin.Core.Models;
using WpfBasicOptionalLogin.Helpers;

namespace WpfBasicOptionalLogin.ViewModels
{
    [Restricted]
    public class MasterDetailViewModel : Observable, INavigationAware
    {
        private readonly ISampleDataService _sampleDataService;
        private SampleOrder _selected;

        public SampleOrder Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new ObservableCollection<SampleOrder>();

        public MasterDetailViewModel(ISampleDataService sampleDataService)
        {
            _sampleDataService = sampleDataService;
        }

        public async void OnNavigatedTo(object parameter)
        {
            SampleItems.Clear();

            var data = await _sampleDataService.GetMasterDetailDataAsync();

            foreach (var item in data)
            {
                SampleItems.Add(item);
            }

            Selected = SampleItems.First();
        }

        public void OnNavigatedFrom()
        {
        }
    }
}
