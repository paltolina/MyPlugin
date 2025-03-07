using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Мои плагины";
            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Инструменты");

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData = new PushButtonData("AI for Revit", "AI for Revit", assemblyPath, "MyPlugin.MyCommand");

            PushButton pushButton = panel.AddItem(buttonData) as PushButton;

            Uri smallIcon = new Uri(@"C:\Users\IRINA\Desktop\Папка Полины С\диплом\MyPlugin\Resources\icon.png");
            Uri largeIcon = new Uri(@"C:\Users\IRINA\Desktop\Папка Полины С\диплом\MyPlugin\Resources\icon.png");

            pushButton.Image = new BitmapImage(smallIcon);
            pushButton.LargeImage = new BitmapImage(largeIcon);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
