using System.Net.Mime;
using System.Windows.Controls;

namespace VoiceTTS.Views
{
    /// <summary>
    /// Interaction logic for TTSView
    /// </summary>
    public partial class TTSView : UserControl
    {
        public TTSView()
        {
            InitializeComponent();

            TextBoxBody.TextChanged += TextBoxBody_TextChanged;
        }

        private void TextBoxBody_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBoxBody.Focus();
            if (TextBoxBody.Text.EndsWith("/>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length;
            }
            if (TextBoxBody.Text.EndsWith("say-as>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length-9;
            }         
            if (TextBoxBody.Text.EndsWith("emphasis>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length-11;
            }
            if (TextBoxBody.Text.EndsWith("prosody>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length - 10;
            }
            if (TextBoxBody.Text.EndsWith("breathing>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length - 12;
            }
            if (TextBoxBody.Text.EndsWith("soft>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length - 7;
            }            
            if (TextBoxBody.Text.EndsWith("whispered>"))
            {
                TextBoxBody.CaretIndex = TextBoxBody.Text.Length - 12;
            }
        }
    }
}
