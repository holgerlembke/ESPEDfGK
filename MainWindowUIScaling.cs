using System.Windows.Input;
using System.Windows;

namespace ESPEDfGK
{
    public partial class MainWindow : Window
    {
        //******************************************************************************************************************
        private void setUIscale(double uiscale)
        {
            uiScaler.ScaleX = uiscale;
            uiScaler.ScaleY = uiscale;
            konfiguration.UIScale = uiscale;
        }

        //******************************************************************************************************************
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs args)
        {
            base.OnPreviewMouseWheel(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                setUIscale(uiScaler.ScaleX + ((args.Delta > 0) ? 0.1 : -0.1));
            }
        }

        //******************************************************************************************************************
        protected override void OnPreviewMouseDown(MouseButtonEventArgs args)
        {
            base.OnPreviewMouseDown(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    setUIscale(1.0);
                }
            }
        }
    }
}
