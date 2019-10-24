﻿using System;
using System.ComponentModel;
using System.Linq;
using AzureBlobStorageSampleApp.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(EntryCustomRederer))]
namespace AzureBlobStorageSampleApp.iOS
{
    public class EntryCustomRederer : EntryRenderer
    {
        enum Theme { Light, Dark }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null && Control != null)
                Control.AllEditingEvents -= HandleAllEditingEvents;

            if (e.NewElement != null && Control != null)
                Control.AllEditingEvents += HandleAllEditingEvents;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control != null)
            {
                Control.Layer.BorderColor = UIColor.LightGray.CGColor;
                Control.Layer.BorderWidth = 0.25f;
                Control.Layer.CornerRadius = 5;
            }
        }

        void HandleAllEditingEvents(object sender, EventArgs e)
        {
            if (Control.Subviews.OfType<UIButton>().FirstOrDefault() is UIButton clearButton
                && clearButton.CurrentImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate) is UIImage clearButtonImage)
            {
                var operatingSystemTheme = GetOperatingSystemTheme();

                if (operatingSystemTheme is Theme.Dark)
                {
                    clearButton.SetImage(clearButtonImage, UIControlState.Normal);
                    clearButton.TintColor = UIColor.DarkGray;
                }
                else if (operatingSystemTheme is Theme.Light)
                {
                    clearButton.SetImage(clearButtonImage, UIControlState.Normal);
                    clearButton.TintColor = UIColor.LightGray;
                }
            }
        }

        Theme GetOperatingSystemTheme()
        {
            var currentUIViewController = GetVisibleViewController();

            var userInterfaceStyle = currentUIViewController.TraitCollection.UserInterfaceStyle;

            return userInterfaceStyle switch
            {
                UIUserInterfaceStyle.Light => Theme.Light,
                UIUserInterfaceStyle.Dark => Theme.Dark,
                _ => throw new NotSupportedException($"UIUserInterfaceStyle {userInterfaceStyle} not supported"),
            };
        }

        static UIViewController GetVisibleViewController()
        {
            var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

            return rootController.PresentedViewController switch
            {
                UINavigationController navigationController => navigationController.TopViewController,
                UITabBarController tabBarController => tabBarController.SelectedViewController,
                null => rootController,
                _ => rootController.PresentedViewController,
            };
        }
    }
}
