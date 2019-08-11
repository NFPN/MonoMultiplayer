using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var user = txtUsername.Text;
            var host = txtHostname.Text;
            var port = txtPort.Text;
            var color = txtColor.Text;

            if (!Regex.IsMatch(user, @"^[a-zA-Z]+$") || user.Length > 16)
            {
                MessageBox.Show("Por favor, usa um nick normal parça, no maximo 16 letras e sem acentos e frescuras :)");
                return;
            }

            if (!IsValidHex(color))
            {
                MessageBox.Show("Precisa ser um código HEX -> procura #FFFFFF no google e sucesso");
                return;
            }
            var c = HexToColor(color);

            Process.Start("Client.exe", $"{host} {port} {user} {c.R} {c.G} {c.B}");
            Application.Exit();
        }

        public static Color HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");

            int red = 0;
            int green = 0;
            int blue = 0;

            if (hexColor.Length == 6)
            {
                //#RRGGBB
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);


            }
            else if (hexColor.Length == 3)
            {
                //#RGB
                red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }

            return Color.FromArgb(red, green, blue);
        }

        public static bool IsValidHex(string hexColor)
        {
            if (hexColor.StartsWith("#"))
                return hexColor.Length == 7 || hexColor.Length == 4;
            else
                return hexColor.Length == 6 || hexColor.Length == 3;
        }

        private void TxtColor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                panel1.BackColor = HexToColor(txtColor.Text);
            }
            catch
            { }
        }
    }
}
