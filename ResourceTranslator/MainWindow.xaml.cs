using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace ResourceTranslator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int KEY_INDEX = 2;

        private Dictionary<EnumLanguage, int> tMap = new Dictionary<EnumLanguage, int>
        {
            { EnumLanguage.ZHCN, 3 },
            { EnumLanguage.ENUS, 4 },
            { EnumLanguage.PTBR, 5 },
            { EnumLanguage.ESES, 6 }
        };

        public TranslationDict Dict;

        private EnumLanguage transLang;
        private EnumLanguage sourceLang;
        private EnumLanguage targetLang;

        public MainWindow()
        {
            InitializeComponent();
            Dict = new TranslationDict();
            transLang = EnumLanguage.ZHCN;
            comboTranslation.ItemsSource = tMap.Keys;
            comboSourceLang.ItemsSource = tMap.Keys;
            comboTargetLang.ItemsSource = tMap.Keys;
            comboTranslation.SelectedIndex = 0;
            comboSourceLang.SelectedIndex = 0;
            comboTargetLang.SelectedIndex = 0;
        }

        private void comboTranslation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboTranslation.SelectedIndex > 0)
            {
                transLang = (EnumLanguage)(comboTranslation.SelectedValue);
            }
        }

        private void btnAppendTranslation_Click(object sender, RoutedEventArgs e)
        {
            lstTranslationDict.ItemsSource = null;

            var ofd = new OpenFileDialog();
            ofd.Filter = "Supported Files|*.xlsx;*.xaml;*.resx|Excel Files|*.xlsx|XAML Files|*.xaml|Resource Files|*.resx";
            ofd.Multiselect = false;
            if(ofd.ShowDialog().Value)
            {
                var filename = ofd.FileName;
                TranslationDict trans = null;
                if(filename.EndsWith(".xlsx"))
                {
                    trans = StringResourceHelper.LoadFromXlsx(filename, KEY_INDEX, tMap);
                }
                else if(filename.EndsWith(".xaml"))
                {
                    trans = StringResourceHelper.LoadFromXaml(filename, transLang);
                }
                else if(filename.EndsWith(".resx"))
                {
                    trans = StringResourceHelper.LoadFromResx(filename, transLang);
                }
                else
                {
                    trans = new TranslationDict();
                }
                Dict.Combine(trans);                
            }
            lstTranslationDict.ItemsSource = Dict.Keys;
        }

        private void btnClearTranslation_Click(object sender, RoutedEventArgs e)
        {
            var ret = MessageBox.Show(this, "Are you sure to clear?", "Tip", MessageBoxButton.YesNo);
            if(ret == MessageBoxResult.Yes)
            {
                Dict.Clear();
                lstTranslationDict.ItemsSource = null;
            }
        }

        private void btnExportTranslation_Click(object sender, RoutedEventArgs e)
        {
            if (Dict.Count == 0)
            {
                MessageBox.Show(this, "Translation dict is empty");
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Excel Files|*.xlsx";
            sfd.AddExtension = true;
            if (sfd.ShowDialog().Value)
            {
                if (File.Exists(sfd.FileName))
                {
                    File.Delete(sfd.FileName);
                }
                StringResourceHelper.SaveAsXlsx(Dict, sfd.FileName);
            }
        }

        private void comboSourceLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboSourceLang.SelectedIndex > 0)
            {
                sourceLang = (EnumLanguage)(comboSourceLang.SelectedValue);
            }
        }

        private void comboTargetLang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboTargetLang.SelectedIndex > 0)
            {
                targetLang = (EnumLanguage)(comboTargetLang.SelectedValue);
            }
        }

        private void btnOpenSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Supported Files|*.xaml;*.resx|Resource Files|*.resx|XAML Files|*.xaml";
            ofd.Multiselect = false;
            if (ofd.ShowDialog().Value)
            {
                tbSourceFile.Text = ofd.FileName;
                var i = ofd.FileName.LastIndexOf('.');
                tbTargetFile.Text = ofd.FileName.Substring(0, i) + "-1" + ofd.FileName.Substring(i);
            }
        }

        private void btnSaveTargetFile_Click(object sender, RoutedEventArgs e)
        {
            if (Dict.Count == 0)
            {
                MessageBox.Show(this, "Translation dict is empty");
                return;
            }

            var targetFile = tbTargetFile.Text;
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                MessageBox.Show(this, "Target filename is empty");
                return;
            }

            if (targetFile.EndsWith(".resx"))
            {
                StringResourceHelper.ReplaceResx(Dict, transLang, tbSourceFile.Text, targetFile);
            }
            else if (targetFile.EndsWith(".xaml"))
            {
                StringResourceHelper.ReplaceXaml(Dict, transLang, tbSourceFile.Text, targetFile);
            }
        }
    }
}
