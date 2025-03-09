﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            //string tabName = "Мои плагины";
            //try { application.CreateRibbonTab(tabName); } catch { }

            //RibbonPanel panel = application.CreateRibbonPanel(tabName, "Инструменты");

            //string assemblyPath = Assembly.GetExecutingAssembly().Location;

            //PushButtonData buttonData = new PushButtonData("AI for Revit", "AI for Revit", assemblyPath, "MyPlugin.MyCommand");

            //PushButton pushButton = panel.AddItem(buttonData) as PushButton;

            //Uri smallIcon = new Uri(@"C:\Users\IRINA\Desktop\Папка Полины С\диплом\MyPlugin\Resources\icon.png");
            //Uri largeIcon = new Uri(@"C:\Users\IRINA\Desktop\Папка Полины С\диплом\MyPlugin\Resources\icon.png");

            //pushButton.Image = new BitmapImage(smallIcon);
            //pushButton.LargeImage = new BitmapImage(largeIcon);


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
