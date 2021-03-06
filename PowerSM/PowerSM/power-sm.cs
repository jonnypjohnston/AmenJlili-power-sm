﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System.Reflection;
using SolidWorksTools.File;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace PowerSM
{
    [Guid("B2A0B714-0984-435A-A2DF-D3AA3E8E806D")]
    [ComVisible(true)]
    public class PowerSM: SwAddin
    {
        /* Notes to developers:
        - Make sure Embed Interop Types for the reference solidworks.interop.sldworks in the properties window is set to true.
        -  From project properties, select the target platform to 64x under the build tab
        -  Check Register for COM Interop under the build tab // need to run vs as admin when building
        */

         int AddInCookie;
         SldWorks swApp;

        #region Connect To SolidWorks 
        public bool ConnectToSW(object swAppObj, int SessionCookie)
        {
            swApp = (SldWorks)swAppObj;
            AddInCookie = SessionCookie;
            swApp.SetAddinCallbackInfo(0, this, AddInCookie);
            BuildMenu();
            return true;
        }

        public bool DisconnectFromSW()
        {
            DestroyMenu();
            GC.Collect();
            return true;
        }
        #endregion

        #region COMRegistration

        [ComRegisterFunction()]
        private static void RegisterAssembly(Type t)
        {
            string KeyPath = string.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);
            RegistryKey rk = Registry.LocalMachine.CreateSubKey(KeyPath);
            rk.SetValue(null, 1); // 1: Add-in will load at start-up
            rk.SetValue("Title", "Power-SM"); // Title
            rk.SetValue("Description", "SolidWorks Add-in for Sheet Metal"); // Description

#region Bitmap handling region
            BitmapHandler iBmp = new BitmapHandler();

            Assembly thisAssembly;

            thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            var rm = new System.Resources.ResourceManager("PowerSM.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            Bitmap add_in = (Bitmap)rm.GetObject("add_icon");
           
            // Copy the bitmap to a suitable permanent location with a meaningful filename 

            String addInPath = System.IO.Path.GetDirectoryName(thisAssembly.Location);
            String iconPath = System.IO.Path.Combine(addInPath, "add_icon.bmp");
            add_in.Save(iconPath);
         


            #endregion
            // Register the icon location 
            rk.SetValue("Icon Path", iconPath);

            
        }

        [ComUnregisterFunction()]
        private static void UnRegisterAssembly(Type t)
        {
            string KeyPath = string.Format(@"Software\SolidWorks\Addins\{0:b}", t.GUID);
            Registry.LocalMachine.DeleteSubKey(KeyPath);
        }
        
        #endregion      
    
        #region UIMethod
        private void BuildMenu()
        {
           
            int DocType = (int)swDocumentTypes_e.swDocNONE;
            swApp.AddMenu(DocType, "PowerSM", 1);
            swApp.AddMenuItem4(DocType, AddInCookie, "Power Geometry Tool@PowerSM", 1, "GeometryToolMethod", "3", "Power Geometry Tool", "");
        }
        private void DestroyMenu()
        {
          
            int DocType = (int)swDocumentTypes_e.swDocNONE;
            swApp.RemoveMenu(DocType, "PowerSM", "GeometryToolMethod");
            swApp.RemoveMenu(DocType, "Power Geometry Tool@PowerSM", "GeometryToolMethod");
    

        }
        #endregion

        #region Add-in Implementation
        // Callback methods must be public, otherwise call from menu item fails 
        public void GeometryToolMethod()
        {
            var swFrame = new swFrame(Process.GetProcessById(swApp.GetProcessID()).MainWindowHandle);
            var  f = new PowerGeometryForm(swApp);
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog(swFrame);
        }
        public void ConvertToolMethod()
        {
           
        }
        #endregion

    }


    public class swFrame : IWin32Window
    {
        public IntPtr Handle { get; set; }

        public swFrame(IntPtr handle)
        {
            Handle = handle;
        }
    }
}


