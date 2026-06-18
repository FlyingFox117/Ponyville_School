using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PonyvilleSchool2._0.Core
{
    public static class TextBoxHelper
    {
        private static readonly Regex _regex =
            new Regex("[^0-9]+");

        public static bool GetOnlyNumbers(DependencyObject obj)
        {
            return (bool)obj.GetValue(OnlyNumbersProperty);
        }

        public static void SetOnlyNumbers(DependencyObject obj, bool value)
        {
            obj.SetValue(OnlyNumbersProperty, value);
        }

        public static readonly DependencyProperty OnlyNumbersProperty =
            DependencyProperty.RegisterAttached(
                "OnlyNumbers",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(false, OnOnlyNumbersChanged));

        private static void OnOnlyNumbersChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                if ((bool)e.NewValue)
                {
                    tb.PreviewTextInput += TextBox_PreviewTextInput;
                    DataObject.AddPastingHandler(tb, OnPaste);
                }
                else
                {
                    tb.PreviewTextInput -= TextBox_PreviewTextInput;
                    DataObject.RemovePastingHandler(tb, OnPaste);
                }
            }
        }

        private static void TextBox_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }

        private static void OnPaste(
            object sender,
            DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text =
                    (string)e.DataObject.GetData(typeof(string));

                if (_regex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
