//using Autodesk.Revit.UI;
//using System;
//using System.IO;
//using System.Reflection;

//namespace RevitTools.DuctAccessoryAnnotation
//{
//    public class App : IExternalApplication
//    {
//        public Result OnStartup(UIControlledApplication application)
//        {
//            string tabName = "RevitTools";
//            string panelName = "Accessories";

//            try
//            {
//                // Создаём вкладку (если её нет)
//                try { application.CreateRibbonTab(tabName); } catch { }

//                // Создаём панель
//                RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

//                // Путь к DLL
//                string assemblyPath = Assembly.GetExecutingAssembly().Location;

//                // Кнопка Fire Dampers
//                PushButtonData fireBtn = new PushButtonData(
//                    "FireDampers",
//                    "Przeciwpożarowe klapy",
//                    assemblyPath,
//                    "RevitTools.DuctAccessoryAnnotation.FireDampersCommand"
//                );

//                // Кнопка Silencers
//                PushButtonData silnsBtn = new PushButtonData(
//                    "Silencers",
//                    "Tłumiki",
//                    assemblyPath,
//                    "RevitTools.DuctAccessoryAnnotation.SilencerAnnotationCommand"
//                );

//                panel.AddItem(fireBtn);
//                panel.AddItem(silnsBtn);

//                return Result.Succeeded;
//            }
//            catch (Exception ex)
//            {
//                TaskDialog.Show("Ribbon Error", ex.Message);
//                return Result.Failed;
//            }
//        }

//        public Result OnShutdown(UIControlledApplication application)
//        {
//            return Result.Succeeded;
//        }
//    }
//}
using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace RevitTools.DuctAccessoryAnnotation
{

    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "RevitTools";
            string panelName = "Accessories";

            try
            {
                try { application.CreateRibbonTab(tabName); } catch { }

                RibbonPanel panel = GetOrCreatePanel(application, tabName, panelName);

                string assemblyPath = Assembly.GetExecutingAssembly().Location;

                PushButtonData fireBtn = new PushButtonData(
                    "FireDampers",
                    "Przeciwpożarowe klapy",
                    assemblyPath,
                    "RevitTools.DuctAccessoryAnnotation.FireDampersCommand"
                );

                PushButtonData silnsBtn = new PushButtonData(
                    "Silencers",
                    "Tłumiki",
                    assemblyPath,
                    "RevitTools.DuctAccessoryAnnotation.SilencerAnnotationCommand"
                );
                PushButtonData balancBtn = new PushButtonData(
                    "BalancingDampers",
                    "Przepustnicy",
                    assemblyPath,
                    "RevitTools.DuctAccessoryAnnotation.BalancingDampersCommand"
                );
                panel.AddItem(fireBtn);
                panel.AddItem(silnsBtn);
                panel.AddItem(balancBtn);

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
