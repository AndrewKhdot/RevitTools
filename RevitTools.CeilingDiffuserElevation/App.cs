using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace RevitTools.CeilingDiffuserElevation
{

    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "RevitTools";
            string panelName = "Diffusers";

            try
            {
                try { application.CreateRibbonTab(tabName); } catch { }

                RibbonPanel panel = GetOrCreatePanel(application, tabName, panelName);

                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData difBtn = new PushButtonData(
                    "DiffusersLevel",
                    "Wysokość nawiewników",
                    assemblyPath,
                    "RevitTools.CeilingDiffuserElevation.CeilingDiffuserElevationCommand"
                );

                panel.AddItem(difBtn);


                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("RevitTools - Ribbon Error", ex.Message);
                return Result.Failed;
            }
        }

        private RibbonPanel GetOrCreatePanel(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel p in app.GetRibbonPanels(tabName))
            {
                if (p.Name == panelName)
                    return p;
            }

            return app.CreateRibbonPanel(tabName, panelName);
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}

