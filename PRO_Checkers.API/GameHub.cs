using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Drawing;

namespace PRO_Checkers.API
{
    public class GameHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId);

            await Clients.Client(Context.ConnectionId).SendAsync("OnConnection", $"{Context.ConnectionId} has joined...");
            
        }

        public async Task SendToCalculate(string gamejs, string colorjs, bool backwardEat, bool forcedEat, int depth)
        {
            Game game = JsonConvert.DeserializeObject<Game>(gamejs);
            Tile color = JsonConvert.DeserializeObject<Tile>(colorjs);
            // stworzenie gry, obliczamy ruchy dla danego koloru jakim aktualnie jest, sprawdzasz ile masz i podzial ile klientow, ile ruchow i jak gbsa zrobić
            foreach (var connection in ConnectionManager.ConnectionsIDs)
                {
                    Console.WriteLine(connection);
                    await Clients.Client(connection).SendAsync("Calculate", gamejs, colorjs, backwardEat, forcedEat, depth);
                }
        }
        public async Task SetClientsToCalculate()
        {
            ConnectionManager.ConnectionsIDs.Add(Context.ConnectionId);
        }
        public async Task ReceiveClientsCalculations()
        {
            //tu zwrotka do UI zeby ruch obliczony wyslac
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client {Context.ConnectionId} disconnected...");
            ConnectionManager.ConnectionsIDs.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
