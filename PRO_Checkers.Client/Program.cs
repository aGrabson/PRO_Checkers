using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Xml.Linq;
using System.Diagnostics;

namespace PRO_Checkers.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5202/game-hub")
                .WithServerTimeout(TimeSpan.FromMinutes(5))
                .Build();

            connection.On<string>("OnConnection", (message) =>
            {
                Console.WriteLine($"{message}");
            });

            connection.On<string, string, string, int, bool, DateTime, Guid>("CalculateNextMove", async (movejs, gamejs, colorjs, depth, eat, sendTime, nodeID) =>
            {
                DateTime startTime = DateTime.Now;
                
                Game game = JsonConvert.DeserializeObject<Game>(gamejs);
                Helper.ForceCapture = game.SkipMoves;
                Player.ForceCapture = game.SkipMoves;
                Helper.BackwardCapture = game.BeatBack;
                Tile color = JsonConvert.DeserializeObject<Tile>(colorjs);
                TreeNode node = null;
                if (eat)
                {
                    Eat move = JsonConvert.DeserializeObject<Eat>(movejs);
                    game = game.Move(move);
                    node = new TreeNode(game, move);
                    node.EatMove = move;

                    Player.GetNestedEatMoves(node, move);
                        
                    
                    Console.WriteLine(move.ToString() + "id node dla ktorego liczymy: " + nodeID);
                }
                else
                {
                    Move move = JsonConvert.DeserializeObject<Move>(movejs);
                    game = game.Move(move);
                    node = new TreeNode(game, move);

                    Console.WriteLine(move.ToString() + "id node dla ktorego liczymy: " + nodeID);
                }
                node.WeightWhite = Player.Score(game, Tile.White);
                node.WeightBlack = Player.Score(game, Tile.Black);

                Player.GenerateMoves(node, Helper.ChangeColor(color), depth-1);
                string nodejs = JsonConvert.SerializeObject(node);
                DateTime endTime = DateTime.Now;


                try
                {

                    await connection.InvokeAsync("ReceiveClientsCalculations", nodejs, sendTime, startTime, endTime, nodeID);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd połączenia: {ex.Message}");
                }
                

            });


            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("SetClientsToCalculate");
                bool test = true;

                while (test)
                {
                    string read = Console.ReadLine();
                    if (read == "q")
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
