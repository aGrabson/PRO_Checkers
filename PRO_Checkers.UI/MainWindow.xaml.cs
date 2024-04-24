using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PRO_Checkers.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String typeOfGame = "";
        private bool backwardEat = true;
        private bool forcedEat = true;
        

        public MainWindow()
        {
            InitializeComponent();
            
            

            
        }

        private bool ValidateIP(string text)
        {
            string pattern = @"^((\d{1,3})\.){3}(\d{1,3})$";

            if (!Regex.IsMatch(text, pattern))
            {
                return false;
            }
            return true;
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "Wprowadź IP...")
            {
                textBox.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Wprowadź IP...";
            }
        }
        private void computerVsComputerButtonClick(object sender, RoutedEventArgs e)
        {
            this.backwardEat = BackwardCheckbox.IsChecked.Value;
            this.forcedEat = ForcedCheckbox.IsChecked.Value;
            this.typeOfGame = "computerVsComputer";
            string text = textBox.Text;
            if (ValidateIP(text))
            {
                Window1 secondWindow = new Window1(typeOfGame, backwardEat, forcedEat, text);
                secondWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Adres IP jest niepoprawny.");
            }
        }

        private void computerVsPlayerButtonClick(object sender, RoutedEventArgs e)
        {
            this.backwardEat = BackwardCheckbox.IsChecked.Value;
            this.forcedEat = ForcedCheckbox.IsChecked.Value;
            this.typeOfGame = "computerVsPlayer";
            string text = textBox.Text;
            if (ValidateIP(text))
            {
                Window1 secondWindow = new Window1(typeOfGame, backwardEat, forcedEat, text);
                secondWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Adres IP jest niepoprawny.");
            }
        }

        private void backwardEatCheckbox(object sender, RoutedEventArgs e)
        {
        }

        private void forcedEatCheckbox(object sender, RoutedEventArgs e)
        {

        }

    }
}