using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Text;
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
        public HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5202/game-hub")
                .Build();


        public MainWindow()
        {
            InitializeComponent();
            
            connection.On<string>("OnConnection", (message) =>
            {
                Console.WriteLine($"{message}");
            });

            
        }

        private void computerVsComputerButtonClick(object sender, RoutedEventArgs e)
        {
            this.backwardEat = BackwardCheckbox.IsChecked.Value;
            this.forcedEat = ForcedCheckbox.IsChecked.Value;
            this.typeOfGame = "computerVsComputer";

            Window1 secondWindow = new Window1(typeOfGame, backwardEat, forcedEat);
            secondWindow.Show();
            this.Close();
        }


        private void computerVsPlayerButtonClick(object sender, RoutedEventArgs e)
        {
            this.backwardEat = BackwardCheckbox.IsChecked.Value;
            this.forcedEat = ForcedCheckbox.IsChecked.Value;
            this.typeOfGame = "computerVsPlayer";
            Window1 secondWindow = new Window1(typeOfGame, backwardEat, forcedEat);
            secondWindow.Show();
            this.Close();
        }

        private void backwardEatCheckbox(object sender, RoutedEventArgs e)
        {
        }

        private void forcedEatCheckbox(object sender, RoutedEventArgs e)
        {

        }

        private async void OnConnection(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.StartAsync();
                Game board = Game.NewGame();
                Tile color = Tile.White;
                bool test = true;
                bool backwardEat = true;
                bool forcedEat = true;

            //Game game, bool backwardEat, bool forcedEat, int depth, Tile color
            await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, 4);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd połączenia: {ex.Message}");
            }
            finally
            {
                await connection.StopAsync();
            }
        }
    }
}