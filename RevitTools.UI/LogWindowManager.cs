using System.Windows.Forms;

namespace RevitTools.UI
{
    public static class LogWindowManager
    {
        private static LogForm _instance;

        public static void Show()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new LogForm();
                _instance.Show();
            }
            else
            {
                _instance.BringToFront();
            }
        }
    }
}