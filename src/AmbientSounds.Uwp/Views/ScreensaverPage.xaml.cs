﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class ScreensaverPage : Page
    {
        public ScreensaverPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ScreensaverPageViewModel>();
            ViewModel.Loaded += OnViewModelLoaded;
        }

        public ScreensaverPageViewModel ViewModel => (ScreensaverPageViewModel)this.DataContext;

        private bool ShowBackButton => !App.IsTenFoot;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var settings = App.Services.GetRequiredService<IUserSettings>();
            bool useDarkScreensaver = settings.Get<bool>(UserSettingsConstants.DarkScreensasver);
            if (useDarkScreensaver)
            {
                VisualStateManager.GoToState(this, nameof(DarkScreensaverState), false);
            }
            else
            {
                await ViewModel.InitializeAsync();
            }

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "screensaver" },
                { "darkscreensaver", useDarkScreensaver ? "true" : "false" }
            });

            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown += CataloguePage_KeyDown;
            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested += OnBackRequested;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Loaded -= OnViewModelLoaded;

            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown -= CataloguePage_KeyDown;
            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested -= OnBackRequested;

            SettingsFlyout?.Items?.Clear();
        }

        private void OnViewModelLoaded(object sender, System.EventArgs e)
        {
            if (!ViewModel.SettingsButtonVisible)
            {
                return;
            }

            foreach (var item in ViewModel.MenuItems)
            {
                var menuItem = new ToggleMenuFlyoutItem
                {
                    DataContext = item,
                    Text = item.Text,
                    IsChecked = item == ViewModel.CurrentSelection
                };
                menuItem.Click += OnMenuItemClicked;

                SettingsFlyout.Items.Add(menuItem);
            }
        }

        private void OnMenuItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleMenuFlyoutItem toggleItem &&
                toggleItem.DataContext is ToggleMenuItem dc)
            {
                foreach (var item in SettingsFlyout.Items)
                {
                    if (item is ToggleMenuFlyoutItem menuItem)
                    {
                        menuItem.IsChecked = menuItem == toggleItem;
                    }
                }

                dc.Command.Execute(dc.CommandParameter);
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            GoBack();
        }

        private void CataloguePage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Escape)
            {
                GoBack();
                args.Handled = true;
            }
        }

        private void GoBack()
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.GoBack(nameof(ScreensaverPage));
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
    }
}
