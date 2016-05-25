using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using mRemoteNG.UI.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace mRemoteNG.App
{
    public class Screens
    {
        //public static void SendFormToScreen(Screen Screen)
        //{
        //    bool wasMax = false;

        //    if (_mainForm.WindowState == FormWindowState.Maximized)
        //    {
        //        wasMax = true;
        //        _mainForm.WindowState = FormWindowState.Normal;
        //    }

        //    _mainForm.Location = Screen.Bounds.Location;

        //    if (wasMax)
        //    {
        //        _mainForm.WindowState = FormWindowState.Maximized;
        //    }
        //}

        public static void SendPanelToScreen(DockContent Panel, Screen Screen)
        {
            Panel.DockState = DockState.Float;
            Panel.ParentForm.Left = Screen.Bounds.Location.X;
            Panel.ParentForm.Top = Screen.Bounds.Location.Y;
        }
    }
}