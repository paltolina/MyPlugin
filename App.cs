using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls.Ribbon;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    public class App : IExternalApplication /*Связывание окна с кнопкой в Ribbon UI*/
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location,
                    iconsDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";
            string tabName = "Мои плагины";

            // Создаем вкладку и панель
            application.CreateRibbonTab(tabName);
            {
                RibbonPanel panel = application.CreateRibbonPanel(tabName, "AI for Revit");

                // Добавляем кнопку запуска плагина

                panel.AddItem(new PushButtonData(nameof(MyCommand), "AI for Revit", assemblyLocation, typeof(MyCommand).FullName)
                {
                    LargeImage = new BitmapImage(new Uri(iconsDirectoryPath + "icon.png"))
                });
            }
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
