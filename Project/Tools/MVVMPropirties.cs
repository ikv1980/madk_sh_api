using ModernWpf.Controls;
using System.Windows.Input;
using System.Windows;

namespace Project.Tools
{
    class MVVMPropirties
    {
        #region AutoSuggestTextBoxQuerySubbmitedCommand

        public static DependencyProperty QuerySubmittedProperty
        = DependencyProperty.RegisterAttached(
            "QuerySubmittedCommand",
            typeof(ICommand),
            typeof(MVVMPropirties),
            new PropertyMetadata(null, OnQuerySubmittedCommandChanged));

        private static void OnQuerySubmittedCommandChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = depObj as AutoSuggestBox;
            if (frameworkElement != null && e.NewValue is ICommand)
            {
                frameworkElement.QuerySubmitted
                  += (o, args) =>
                  {
                      (e.NewValue as ICommand).Execute(null);
                  };
            }
        }

        public static ICommand GetQuerySubmittedCommand(DependencyObject depObj)
        {
            return (ICommand)depObj.GetValue(QuerySubmittedProperty);
        }

        public static void SetQuerySubmittedCommand(DependencyObject depObj, ICommand value)
        {
            depObj.SetValue(QuerySubmittedProperty, value);
        }

        #endregion
    }
}
