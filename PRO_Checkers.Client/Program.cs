using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRO_Checkers.engine;

namespace PRO_Checkers.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5202/game-hub")
                .Build();

            connection.On<string>("OnConnection", (message) =>
            {
                Console.WriteLine($"{message}");
            });

            connection.On<string, string, bool, bool, int>("Calculate", (gamejs, colorjs, backwardEat, forcedEat, depth) =>
            {
                Game game = JsonConvert.DeserializeObject<Game>(gamejs);
                Tile color = JsonConvert.DeserializeObject<Tile>(colorjs);
                Console.WriteLine(game);
            });

            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("SetClientsToCalculate");
                bool test = true;

                while(test) {
                    string read = Console.ReadLine();
                    if(read == "q")
                    {
                        test = false;
                        break;
                    }
                }
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
