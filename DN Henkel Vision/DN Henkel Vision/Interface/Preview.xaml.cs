// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Preview : Page
    {
        public ObservableCollection<string> Previews = new();
        
        public Preview()
        {
            this.InitializeComponent();

            Manager.CurrentEditor.CurrentPreview = this;

            SetPreviews();
        }

        private void Cause_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cause.SelectedIndex >= 0)
            {   
                Classification.ItemsSource = DN_Henkel_Vision.Memory.Classification.Classifications[((ComboBox)sender).SelectedIndex].ToList();
            }
        }

        private void Classification_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Classification.SelectedIndex >= 0)
            {
                Type.ItemsSource = DN_Henkel_Vision.Memory.Classification.Types[DN_Henkel_Vision.Memory.Classification.ClassificationsPointers[Cause.SelectedIndex][Classification.SelectedIndex]];
            }
        }

        public string CurrentFaultLabel()
        {
            return (Cache.CurrentReview + 1).ToString() + "/" + Manager.Selected.ReviewFaults.Count.ToString();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Approve_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }

        private void Placement_LostFocus(object sender, RoutedEventArgs e)
        {
            Cache.LastPlacement = Placement.Text;
        }

        public void SetPreviews()
        {
            Previews.Clear();
            
            foreach (Fault fault in Manager.Selected.ReviewFaults)
            {             
                Previews.Add(fault.Component);
            }

            PreviewsList.SelectedIndex = Cache.CurrentReview;
        }

        private void PreviewsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Count.Flyout.IsOpen) { return; }

            Cache.CurrentReview = PreviewsList.SelectedIndex;
            Cache.PreviewFault = Manager.Selected.ReviewFaults[Cache.CurrentReview];

            Manager.CurrentEditor.FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());
        }
    }
}
