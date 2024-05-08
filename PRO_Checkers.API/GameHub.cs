using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PRO_Checkers.API
{
    public class GameHub : Hub
    {
        struct ThreadParameters
        {
            public bool backwardEat;
            public bool forcedEat;
            public string gamejs;
            public string colorjs;
            public int depth;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId);

            await Clients.Client(Context.ConnectionId).SendAsync("OnConnection", $"{Context.ConnectionId} has joined...");
            
        }


        public async Task SendToCalculate(string gamejs, string colorjs, bool backwardEat, bool forcedEat, int depth)
        {
            Game game = JsonConvert.DeserializeObject<Game>(gamejs);
            Tile color = JsonConvert.DeserializeObject<Tile>(colorjs);
            Helper.ForceCapture = forcedEat;
            Player.ForceCapture = forcedEat;
            Helper.BackwardCapture = backwardEat;

            //if (ConnectionManager._root == null)
            //{
                ConnectionManager._root = new TreeNode(game);
            //}

            var moves = Player.Moves(game, color);
            foreach (var move in moves)
            {
                ConnectionManager.MovesToBeCalculatedQueue.Enqueue(move);
                ConnectionManager.MovesToBeCalculatedQueueCopy.Enqueue(move);
            }

            ThreadParameters parametres = new ThreadParameters
            {
                backwardEat = backwardEat,
                forcedEat = forcedEat,
                gamejs = gamejs,
                colorjs = colorjs,
                depth = depth
            };
            Thread sendingThread = new Thread(new ParameterizedThreadStart(SendToClientMoves));
            sendingThread.Start(parametres);
            sendingThread.Join();
            while(ConnectionManager.MovesToBeCalculatedQueueCopy.Count > 0) { }
            var bestMove = Player.GetBestMove(ConnectionManager._root, color);
            bool eat = false;
            if(bestMove is Eat)
            {
                eat = true;
            }
            string nextMove = JsonConvert.SerializeObject(bestMove);
            await Clients.Client(Context.ConnectionId).SendAsync("nextMove", nextMove, eat);
        }

        public async void SendToClientMoves(object o)
        {
            ThreadParameters parametres = (ThreadParameters)o;
            while (ConnectionManager.MovesToBeCalculatedQueue.Count > 0)
            {

                foreach (var clientStatus in ConnectionManager.ClientsStatus)
                {
                    if (ConnectionManager.MovesToBeCalculatedQueue.Count == 0)
                    {
                        break;
                    }
                    if (clientStatus.Value)
                    {
                        Console.WriteLine(clientStatus.Key);
                        Console.WriteLine(clientStatus.Value);
                        bool eat = false;
                        if (ConnectionManager.MovesToBeCalculatedQueue.First() is Eat)
                        {
                            eat = true;
                        }
                        ConnectionManager.timeCalc4Client.Add(new Tuple<Move, string, DateTime>(ConnectionManager.MovesToBeCalculatedQueue.First(), clientStatus.Key, DateTime.Now));
                        string movejs = JsonConvert.SerializeObject(ConnectionManager.MovesToBeCalculatedQueue.Dequeue());
                        await Clients.Client(clientStatus.Key).SendAsync("CalculateNextMove", movejs, parametres.gamejs, parametres.colorjs, parametres.backwardEat, parametres.forcedEat, parametres.depth, eat, DateTime.Now);
                        ConnectionManager.ClientsStatus.AddOrUpdate(clientStatus.Key, false, (key, oldValue) => false);
                    }
                }

            }

        }
        public async Task SetClientsToCalculate()
        {
            ConnectionManager.ClientsStatus.TryAdd(Context.ConnectionId, true);

        }
        public async Task ReceiveClientsCalculations(string nodejs, DateTime sendTime, DateTime startTime, DateTime endTime)
        {
            var clientid = Context.ConnectionId;
            DateTime receiveCalcTime = DateTime.Now;
            TreeNode calcMoves = JsonConvert.DeserializeObject<TreeNode>(nodejs);
            ConnectionManager._root.Children.Add(calcMoves);
            ConnectionManager.MovesToBeCalculatedQueueCopy.Dequeue();
            ConnectionManager.ClientsStatus.AddOrUpdate(Context.ConnectionId, true, (key, oldValue) => true);
            
            //ConnectionManager.CalculatedMovesQueue.Enqueue(calcMoves);
            //tu zwrotka do UI zeby ruch obliczony wyslac
            //var sendTime = ConnectionManager.timeCalc4Client.Where(x => x.Item1 == calcMoves.Move && x.Item2 == clientid).First().Item3;
            string csvFilePath = "clients_calculations.csv";
            StringBuilder csvContent = new StringBuilder();
            TimeSpan sendDuration = startTime - sendTime;
            TimeSpan calcDuration =  endTime - startTime;
            TimeSpan receiveDuration = receiveCalcTime - endTime;
            if (!ConnectionManager.AreHeadersWritten)
            {
                string csvHeaders = "id_Klienta;Czas wyslania;Czas obliczen;Czas odebrania";
                csvContent.AppendLine(csvHeaders);
                ConnectionManager.AreHeadersWritten = true;
            }
            //idKlienta, kiedy wyslalismy, kiedy rozpoczal obliczenia, kiedy skonczyl obliczenia, kiedy odebralismy
            string csvLine = $"{clientid};{sendDuration.TotalMilliseconds}ms;{calcDuration.TotalMilliseconds}ms;{receiveDuration.TotalMilliseconds}ms";
            csvContent.AppendLine(csvLine);
            await File.AppendAllTextAsync(csvFilePath, csvContent.ToString());

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client {Context.ConnectionId} disconnected...");
            
            var element = ConnectionManager.ClientsStatus.FirstOrDefault(x => x.Key == Context.ConnectionId);
            if(element.Key != null)
            {
                ConnectionManager.ClientsStatus.TryRemove(element);

            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
