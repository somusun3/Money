﻿using Money.ViewModels;
using Money.ViewModels.Parameters;
using Neptuo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Money.Views.Navigation
{
    internal class ApplicationNavigator : INavigator
    {
        private readonly NavigatorParameterCollection rules;
        private readonly Frame rootFrame;

        public ApplicationNavigator(NavigatorParameterCollection rules, Frame rootFrame)
        {
            Ensure.NotNull(rules, "rules");
            Ensure.NotNull(rootFrame, "rootFrame");
            this.rules = rules;
            this.rootFrame = rootFrame;

            SystemNavigationManager manater = SystemNavigationManager.GetForCurrentView();
            manater.BackRequested += OnBackRequested;

            rootFrame.Navigating += OnRootFrameNavigating;
            rootFrame.Navigated += OnRootFrameNavigated;
        }

        public void GoBack()
        {
            Template template = rootFrame.Content as Template;
            if (template != null)
                NavigateBack(template.ContentFrame);
        }

        public void GoForward()
        {
            Template template = rootFrame.Content as Template;
            if (template != null)
                NavigateForward(template.ContentFrame);
        }

        public INavigatorForm Open(object parameter)
        {
            Ensure.NotNull(parameter, "parameter");

            Template template = rootFrame.Content as Template;
            if (template == null)
                return new PageNavigatorForm(rootFrame, typeof(Template), parameter);
            
            Type pageType;
            Type parameterType = parameter.GetType();
            if (rules.TryGetPageType(parameterType, out pageType))
                return new PageNavigatorForm(template.ContentFrame, pageType, parameter);

            throw Ensure.Exception.InvalidOperation("Missing navigation page for parameter of type '{0}'.", parameterType.FullName);
        }

        private void OnRootFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            Template template = rootFrame.Content as Template;
            if (template != null)
            {
                template.ContentFrame.Navigated -= OnTemplateContentFrameNavigated;
                template.ContentFrame.Navigating -= OnTemplateContentFrameNavigating;
            }
        }

        private void OnRootFrameNavigated(object sender, NavigationEventArgs e)
        {
            Template template = rootFrame.Content as Template;
            if (template != null)
            {
                template.ContentFrame.Navigated += OnTemplateContentFrameNavigated;
                template.ContentFrame.Navigating += OnTemplateContentFrameNavigating;
            }
        }

        private void OnTemplateContentFrameNavigated(object sender, NavigationEventArgs e)
        {
            Frame frame = (Frame)sender;
            EnsureBackButtonVisibility(frame);

            Page page = frame.Content as Page;
            if (page != null)
                page.PointerPressed += OnPagePointerPressed;
        }

        private void OnTemplateContentFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            Frame frame = (Frame)sender;

            Page page = frame.Content as Page;
            if (page != null)
                page.PointerPressed -= OnPagePointerPressed;
        }

        private void OnPagePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                Page page = (Page)sender;
                PointerPoint point = e.GetCurrentPoint(page);
                if (point.Properties.IsXButton1Pressed)
                    NavigateBack(page.Frame);
                else if (point.Properties.IsXButton2Pressed)
                    NavigateForward(page.Frame);
            }
        }

        private void EnsureBackButtonVisibility(Frame rootFrame)
        {
            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.AppViewBackButtonVisibility = rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            Template template = rootFrame.Content as Template;
            if (template == null)
                return;

            if (NavigateBack(template.ContentFrame))
                e.Handled = true;
        }

        private bool NavigateBack(Frame rootFrame)
        {
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack(new DrillInNavigationTransitionInfo());
                return true;
            }

            return false;
        }

        private bool NavigateForward(Frame rootFrame)
        {
            if (rootFrame.CanGoForward)
            {
                rootFrame.GoForward();
                return true;
            }

            return false;
        }
    }
}