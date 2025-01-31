using Autodesk.Revit.UI;

using System.IO;
using System.Reflection;
using System;
using System.Windows.Media.Imaging;
using System.Linq;

namespace FAMILY_INSTANSE_MAPPER
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location,
                   iconsDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";

            string tabName = "KRGP";
            string panelName = "Арматура";
            string ribbonName = "Соединитель\nкаркаса";

            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }

            #region 1. Архитектура
            {
                RibbonPanel panel = application.GetRibbonPanels(tabName).Where(p => p.Name == panelName).FirstOrDefault();
                if (panel == null)
                {
                    panel = application.CreateRibbonPanel(tabName, panelName);
                }

                panel.AddItem(new PushButtonData(nameof(FAMILY_INSTANSE_MAPPER), ribbonName, assemblyLocation, typeof(FAMILY_INSTANSE_MAPPER).FullName)
                {
                    LargeImage = new BitmapImage(new Uri(iconsDirectoryPath + "FAMILY_INSTANSE_MAPPER.png"))
                });
            }
            #endregion


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
