﻿using LinqToVisualTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unigram.Common;
using Unigram.Converters;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace Unigram.Controls.Chats
{
    public class ChatListView : PaddedListView
    {
        public DialogViewModel ViewModel => DataContext as DialogViewModel;

        public ScrollViewer ScrollingHost { get; private set; }
        public ItemsStackPanel ItemsStack { get; private set; }

        public ChatListView()
        {
            DefaultStyleKey = typeof(ListView);

            Loaded += OnLoaded;
        }

        public void ScrollToBottom()
        {
            if (ScrollingHost != null)
            {
                ScrollingHost.ChangeView(null, ScrollingHost.ScrollableHeight, null);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
            {
                ItemsStack = panel;
                ItemsStack.SizeChanged += Panel_SizeChanged;

                SetScrollMode();
            }

            if (ScrollingHost.ScrollableHeight < 200 && Items.Count > 0)
            {
                if (ViewModel.IsFirstSliceLoaded != true)
                {
                    await ViewModel.LoadPreviousSliceAsync(false, ItemsStack.LastVisibleIndex == ItemsStack.LastCacheIndex);
                }

                await ViewModel.LoadNextSliceAsync();
            }
        }

        protected override void OnApplyTemplate()
        {
            ScrollingHost = (ScrollViewer)GetTemplateChild("ScrollViewer");
            ScrollingHost.ViewChanged += ScrollingHost_ViewChanged;

            base.OnApplyTemplate();
        }

        private async void Panel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ScrollingHost.ScrollableHeight < 200)
            {
                if (ViewModel.IsFirstSliceLoaded != true)
                {
                    await ViewModel.LoadPreviousSliceAsync(false, ItemsStack.LastVisibleIndex == ItemsStack.LastCacheIndex);
                }

                if (ViewModel.IsLastSliceLoaded != true)
                {
                    await ViewModel.LoadNextSliceAsync();
                }
            }
        }

        private async void ScrollingHost_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (ScrollingHost == null || ItemsStack == null || ViewModel == null)
            {
                return;
            }

            //if (ScrollingHost.VerticalOffset < 200 && ScrollingHost.ScrollableHeight > 0 && !e.IsIntermediate)
            //if (ItemsStack.FirstCacheIndex == 0 && !e.IsIntermediate)
            if (ItemsStack.FirstVisibleIndex == 0 && !e.IsIntermediate)
            {
                await ViewModel.LoadNextSliceAsync(true);
            }
            else if (ScrollingHost.ScrollableHeight - ScrollingHost.VerticalOffset < 200 && ScrollingHost.ScrollableHeight > 0 && !e.IsIntermediate)
            {
                if (ViewModel.IsFirstSliceLoaded != true)
                {
                    await ViewModel.LoadPreviousSliceAsync(true, ItemsStack.LastVisibleIndex == ItemsStack.LastCacheIndex);
                }
            }
        }

        private ItemsUpdatingScrollMode? _pendingMode;
        private bool? _pendingForce;

        public void SetScrollMode()
        {
            if (_pendingMode is ItemsUpdatingScrollMode mode && _pendingForce is bool force)
            {
                _pendingMode = null;
                _pendingForce = null;

                SetScrollMode(mode, force);
            }
        }

        public void SetScrollMode(ItemsUpdatingScrollMode mode, bool force)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;
            if (panel == null)
            {
                _pendingMode = mode;
                _pendingForce = force;

                return;
            }

            var scroll = ScrollingHost;
            if (scroll == null)
            {
                _pendingMode = mode;
                _pendingForce = force;

                return;
            }

            if (mode == ItemsUpdatingScrollMode.KeepItemsInView && (force || scroll.VerticalOffset < 200))
            {
                Debug.WriteLine("Changed scrolling mode to KeepItemsInView");
                Logs.Log.Write("Changed scrolling mode to KeepItemsInView");

                panel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
            }
            else if (mode == ItemsUpdatingScrollMode.KeepLastItemInView && (force || scroll.ScrollableHeight - scroll.VerticalOffset < 200))
            {
                Debug.WriteLine("Changed scrolling mode to KeepLastItemInView");
                Logs.Log.Write("Changed scrolling mode to KeepLastItemInView");

                panel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ChatListViewItem(this);
        }

        protected override bool CantSelect(object item)
        {
            return item is MessageViewModel message && message.IsService();
        }

        protected override long IdFromContainer(DependencyObject container)
        {
            var item = ItemFromContainer(container) as MessageViewModel;
            if (item != null)
            {
                return item.Id;
            }

            return -1;
        }
    }
}
