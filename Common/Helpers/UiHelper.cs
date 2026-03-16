
using System.Windows.Forms;

namespace MyMacros
{
    public static class UiHelper
    {
    	
		public static void ShowLogForm(string logText)
		{
		    System.Windows.Forms.Form logForm = new System.Windows.Forms.Form();
		    logForm.Text = "Лог макроса";
		    logForm.Width = 600;
		    logForm.Height = 400;
		
		    System.Windows.Forms.TextBox logBox = new System.Windows.Forms.TextBox();
		    logBox.Multiline = true;
		    logBox.Dock = DockStyle.Fill;
		    logBox.ScrollBars = ScrollBars.Vertical;
		    logBox.ReadOnly = true;
		    logBox.Text = logText;
		
		    logForm.Controls.Add(logBox);
		
		    // Показываем модельess окно
		    logForm.Show(); // НЕ блокирует Revit
		}

	}
}
