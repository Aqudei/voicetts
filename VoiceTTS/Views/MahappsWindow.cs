using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls;
using Prism.Services.Dialogs;

namespace VoiceTTS.Views
{
    public class MahappsWindow : MetroWindow, IDialogWindow
    {
        public IDialogResult Result { get; set; }
    }
}
