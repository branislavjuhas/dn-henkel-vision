using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DN_Henkel_Vision.Memory;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Animation;
using System.Linq;
using Windows.UI.Core;
using Windows.System;
using Microsoft.UI.Input;

namespace DN_Henkel_Vision.Interface
{
    /// <summary>
    /// Previewing page of the application.
    /// </summary>
    public sealed partial class Preview : Page
    {
        private bool _fromui = false;
        
        public ObservableCollection<string> Previews = new();

        public Fault Current = Manager.Selected.ReviewFaults[Cache.CurrentReview];

        /// <summary>
        /// Initializes a new instance of the <see cref="Preview"/> class.
        /// </summary>
        public Preview()
        {
            this.InitializeComponent();

            Manager.CurrentEditor.CurrentPreview = this;

            SetPreviews();
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Causes
        /// ComboBox. It sets the content of the Classification ComboBox to the content of the
        /// selected item in the Causes ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cause_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Cause.SelectedIndex >= 0)
            {
                Classification.ItemsSource = DN_Henkel_Vision.Memory.Classification.LocalClassifications[((ComboBox)sender).SelectedIndex].ToList();
                if (Classification.Items.Count == 1) { Classification.SelectedIndex = 0; return; }

                if (!_fromui) return;
                Classification.Focus(FocusState.Programmatic);
                Classification.IsDropDownOpen = true;
            }
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Classification
        /// ComboBox. It sets the content of the Type ComboBox to the content of the selected item
        /// in the Classification ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Classification_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Classification.SelectedIndex >= 0)
            {
                Type.ItemsSource = DN_Henkel_Vision.Memory.Classification.LocalTypes[DN_Henkel_Vision.Memory.Classification.ClassificationsPointers[Cause.SelectedIndex][Classification.SelectedIndex]];
                if (Type.Items.Count == 1) { Type.SelectedIndex = 0; return; }

                if (!_fromui) return;
                Type.Focus(FocusState.Programmatic);
                Type.IsDropDownOpen = true;
            }
        }

        /// <summary>
        /// This functions generattes the string in format "Current Fault/Total Faults"
        /// waiting to be reviewed by the user.
        /// </summary>
        /// <returns>The string in format "Current Fault/Total Faults"</returns>
        public string CurrentFaultLabel()
        {
            return (Cache.CurrentReview + 1).ToString() + "/" + Manager.Selected.ReviewFaults.Count.ToString();
        }

        /// <summary>
        /// This void is triggered when the user clicks on the Approve button.
        /// It approves the current fault, pushes to done faults and moves to
        /// the next one.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            ApproveFault();

            if (Manager.CurrentEditor.FaultInput.FocusState != FocusState.Unfocused) { return; }

            Manager.CurrentEditor.FaultInput.Focus(FocusState.Programmatic);

            (Manager.CurrentWindow as Environment).UpdateTimebar();
        }

        /// <summary>
        /// This void is triggered when the user clicks on the Approve button. It approves
        /// the current fault, but still shows it in the review list.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Approve_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ApproveFault(true);
        }

        /// <summary>
        /// This void clears the current fault from the review list and moves to the next one.
        /// </summary>
        private void CleanFault()
        {
            // Remove the fault from the review list
            Manager.Selected.ReviewFaults.RemoveAt(Cache.CurrentReview);

            // If there are no more faults to review, return
            if (Manager.Selected.ReviewFaults.Count > 0)
            {
                if (Cache.CurrentReview >= Manager.Selected.ReviewFaults.Count)
                {
                    Cache.CurrentReview = Manager.Selected.ReviewFaults.Count - 1;
                }

                Manager.CurrentEditor.FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());

                return;
            }

            // If there are no more faults to review, return
            Manager.CurrentEditor.Unreview();
        }

        /// <summary>
        /// Approves the current fault and logs the user and machine time to Manager.Selected
        /// </summary>
        /// <param name="keep">If true, does not remove the fault from Manager.Selected</param>
        public async void ApproveFault(bool keep = false)
        {
            // If the component is empty, show a message box
            if (string.IsNullOrEmpty(Placement.Text) && !Manager.Selected.IsNetstal())
            {
                // Show a message box
                ContentDialog message = new()
                {
                    XamlRoot = Manager.CurrentWindow.Content.XamlRoot,
                    Title = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_MissingPlacement/Text")),
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Content = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_MissingPlacementDsn/Text")),
                    PrimaryButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Cancel/Content")),
                    SecondaryButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Continue/Content")),
                    DefaultButton = ContentDialogButton.Primary,
                    RequestedTheme = (Manager.CurrentWindow.Content as Grid).RequestedTheme
                };

                // Show the message box
                ContentDialogResult result = await message.ShowAsync();

                // If the user clicked the primary button, return
                if (result != ContentDialogResult.Secondary) { return; }
            }

            // If the description is in the faults list, it is a duplicate
            if (Manager.Selected.Faults.Any(f => f.Description == Description.Text))
            {
                // Show a message box
                ContentDialog message = new()
                {
                    XamlRoot = Manager.CurrentWindow.Content.XamlRoot,
                    Title = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_DupliciteFault/Text")),
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Content = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_DupliciteFaultDsn/Text")),
                    PrimaryButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:T_Delete/Text")),
                    SecondaryButtonText = Windows.ApplicationModel.Resources.ResourceLoader.GetStringForReference(new Uri("ms-resource:B_Keep/Content")),
                    DefaultButton = ContentDialogButton.Primary
                };

                // Show the message box
                ContentDialogResult result = await message.ShowAsync();

                // If the user clicked the secondary button, return
                if (result != ContentDialogResult.Secondary) { CleanFault(); return; }
            }

            Fault preparate = PrepareFault();

            preparate.Index = ((uint)Memory.Lavender.AppendFault(preparate, Manager.Selected.OrderNumber));
            Manager.Selected.Faults.Insert(0, preparate);

            if (Manager.Selected.ReviewFaults[Cache.CurrentReview].MachineTime <= 0f)
            {
                Manager.Selected.ReviewFaults[Cache.CurrentReview].UserTime = Manager.AverageTime;
            }

            Memory.Lavender.Time += Manager.Selected.ReviewFaults[Cache.CurrentReview].UserTime + Manager.Selected.ReviewFaults[Cache.CurrentReview].MachineTime;

            //If it is not set to remove, function can return, because following code is just for removing the fault
            if (keep) { return; }

            // Remove the fault from the review list
            CleanFault();
        }

        /// <summary>
        /// This void is trigered when the placement text box loses focus. It saves the
        /// content of the text box to the cache.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Arguments of the event</param>
        private void Placement_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Manager.Selected.IsNetstal()) { return; }

            Cache.LastPlacement = Placement.Text;
        }

        /// <summary>
        /// This void sets the content of the Previews list to the components of the
        /// faults waiting to be reviewed.
        /// </summary>
        public void SetPreviews()
        {
            Previews.Clear();
            
            foreach (Fault fault in Manager.Selected.ReviewFaults)
            {             
                Previews.Add(fault.Component);
            }

            PreviewsList.SelectedIndex = Cache.CurrentReview;
        }

        /// <summary>
        /// This void is triggered when the user changes the selected item in the Previews
        /// list. It sets the current review to the selected item and shows the preview of
        /// the fault.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Count.Flyout.IsOpen) { return; }

            Cache.CurrentReview = PreviewsList.SelectedIndex;

            Manager.CurrentEditor.FaultPreview.Navigate(typeof(Preview), null, new SuppressNavigationTransitionInfo());
        }

        /// <summary>
        /// Prepares a new Fault object with the specified values.
        /// </summary>
        /// <returns>A new Fault object.</returns>
        private Fault PrepareFault()
        {
            Fault output = new(Description.Text)
            {
                Component = Component.Text,
                Placement = Placement.Text
            };

            if (Cause.SelectedValue != null) { output.Cause = Cause.SelectedValue.ToString(); }
            else { output.Cause = string.Empty; }

            if (Classification.SelectedValue != null) { output.Classification = Classification.SelectedValue.ToString(); }
            else { output.Classification = string.Empty; }

            if (Type.SelectedValue != null) { output.Type = Type.SelectedValue.ToString(); }
            else { output.Type = string.Empty; }

            output.ClassIndexes[0] = Cause.SelectedIndex;
            output.ClassIndexes[1] = Classification.SelectedIndex;
            output.ClassIndexes[2] = Type.SelectedIndex;

            output.MachineTime = Manager.Selected.ReviewFaults[Cache.CurrentReview].MachineTime;
            output.UserTime = Manager.Selected.ReviewFaults[Cache.CurrentReview].UserTime;

            return output;
        }

        /// <summary>
        /// Event handler for when the Component text box is changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void Component_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Component.Text.Any(char.IsDigit))
            {
                int index = Component.SelectionStart;
                int length = Component.SelectionLength;

                Component.Text = Component.Text.ToUpper();

                Component.SelectionStart = index;
                Component.SelectionLength = length;
            }

            if (Component.Text == Manager.Selected.ReviewFaults[Cache.CurrentReview].Component)
            {
                if (Duplicate.Visibility == Visibility.Collapsed) return;
                Component.Margin = new Thickness(0);
                Component.CornerRadius = new CornerRadius(4);
                Duplicate.Visibility = Visibility.Collapsed;
                Separator.Visibility = Visibility.Collapsed;

                return;
            }

            Component.Margin = new Thickness(33, 0, 0, 0);
            Component.CornerRadius = new CornerRadius(0, 4, 4, 0);
            Duplicate.Visibility = Visibility.Visible;
            Separator.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Detects if the Shift key is being held down.
        /// </summary>
        /// <returns>True if the Shift key is being held down; otherwise false.</returns>
        private bool IsShift()
        {
            CoreVirtualKeyStates states = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);

            return (states & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        /// <summary>
        /// Converts all text in the Placement field to uppercase.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Placement_TextChanged(object sender, TextChangedEventArgs e)
        {
            int index = Placement.SelectionStart;
            int length = Placement.SelectionLength;
            
            Placement.Text = Placement.Text.ToUpper();

            Placement.SelectionStart = index;
            Placement.SelectionLength = length;
        }

        /// <summary>
        /// Handles the event when the Preview grid loads.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The routed event arguments</param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _fromui = true;
        }

        /// <summary>
        /// Handles the event when the duplicate button is clicked.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The routed event arguments</param>
        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            Component.Margin = new Thickness(0);
            Component.CornerRadius = new CornerRadius(4);
            Duplicate.Visibility = Visibility.Collapsed;
            Separator.Visibility = Visibility.Collapsed;

            if (Component.Text == Manager.Selected.ReviewFaults[Cache.CurrentReview].Component) { return; }
            if (string.IsNullOrEmpty(Manager.Selected.ReviewFaults[Cache.CurrentReview].Component))
            {
                Manager.Selected.ReviewFaults[Cache.CurrentReview].Component = Component.Text;
                return;
            }

            Description.Text = Description.Text.Replace(Manager.Selected.ReviewFaults[Cache.CurrentReview].Component, Component.Text);

            Manager.Selected.ReviewFaults[Cache.CurrentReview].Component = Component.Text;
        }
    }
}
