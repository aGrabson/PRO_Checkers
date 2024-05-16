using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;

namespace PRO_Checkers.API
{
    public class GameHub : Hub
    {
        struct ThreadParameters
        {
            public bool backwardEat;
            public bool forcedEat;
            public string colorjs;
            public int depth;
        }
        struct ThreadCalcTimeParameters
        {
            public string clientid;
            public TimeSpan sendDuration;
            public TimeSpan calcDuration;
            public TimeSpan receiveDuration;
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
            game.SkipMoves = forcedEat;
            game.BeatBack = backwardEat;
            Helper.ForceCapture = forcedEat;
            Player.ForceCapture = forcedEat;
            Helper.BackwardCapture = backwardEat;

            if (ConnectionManager._root == null)
            {
                ConnectionManager._root = new TreeNode(game);
                ConnectionManager._historyRoot = new TreeNode(game);
            }
            else
            {
                foreach(TreeNode childrenNode in ConnectionManager._root.Children)
                {
                    if(Helper.CompareTileMatrices(game, childrenNode.GameState))
                    {
                        ConnectionManager._root = childrenNode;
                        depth = 1;
                        //dodanie do historii root
                        break;
                    }
                }
            }

            var leafsList = ConnectionManager._root.GetLeafNodes();

            foreach(var leafsNode in leafsList)
            {
                var moves = Player.Moves(leafsNode.GameState, color);
                foreach (var move in moves)
                {
                    ConnectionManager.MovesToBeCalculatedQueue.Enqueue(new Tuple<Move, Game, Guid>(move, leafsNode.GameState, leafsNode.Id));
                    ConnectionManager.MovesToBeCalculatedQueueCopy.Enqueue(new Tuple<Move, Game, Guid>(move, leafsNode.GameState, leafsNode.Id));
                }
            }

            //var moves = Player.Moves(game, color);
            //foreach (var move in moves)
            //{
            //    ConnectionManager.MovesToBeCalculatedQueue.Enqueue(move);
            //    ConnectionManager.MovesToBeCalculatedQueueCopy.Enqueue(move);
            //}

            ThreadParameters parametres = new ThreadParameters
            {
                backwardEat = backwardEat,
                forcedEat = forcedEat,
                colorjs = colorjs,
                depth = depth
            };
            Thread sendingThread = new Thread(new ParameterizedThreadStart(SendToClientMoves));
            sendingThread.Start(parametres);
            sendingThread.Join();
            while(ConnectionManager.MovesToBeCalculatedQueueCopy.Count > 0) { }
            var bestNode = Player.GetBestMove(ConnectionManager._root, color);
            bool eat = false;
            string nextMove = String.Empty;
            //if(childNode.EatMove != null)
            //{
            //    bestMove = childNode.EatMove;
            //}
            //else
            //{
            //    bestMove = childNode.Move;
            //}
            if (bestNode.EatMove != null)
            {
                eat = true;
                nextMove = JsonConvert.SerializeObject(bestNode.EatMove);
            }
            else
            {
                nextMove = JsonConvert.SerializeObject(bestNode.Move);
            }
            ConnectionManager._root = bestNode;
            //dodanie do historii root
            string nestedMovesjs = JsonConvert.SerializeObject(bestNode.NestedEats);
            await Clients.Client(Context.ConnectionId).SendAsync("nextMove", nextMove, eat, nestedMovesjs);
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

                        var queueItem = ConnectionManager.MovesToBeCalculatedQueue.TryDequeue(out var dequeuedItem) ? dequeuedItem : null;
                        bool eat = queueItem.Item1 is Eat;
                        //ConnectionManager.timeCalc4Client.Add(new Tuple<Move, string, DateTime>(ConnectionManager.MovesToBeCalculatedQueue.First(), clientStatus.Key, DateTime.Now));
                        string movejs = JsonConvert.SerializeObject(queueItem.Item1);
                        string gamejs = JsonConvert.SerializeObject(queueItem.Item2);
                        await Clients.Client(clientStatus.Key).SendAsync("CalculateNextMove", movejs, gamejs, parametres.colorjs, parametres.depth, eat, DateTime.Now, queueItem.Item3);
                        ConnectionManager.ClientsStatus.AddOrUpdate(clientStatus.Key, false, (key, oldValue) => false);
                    }
                }

            }

        }
        public async Task SetClientsToCalculate()
        {
            ConnectionManager.ClientsStatus.TryAdd(Context.ConnectionId, true);

        }
        public async void SaveCalculationsTimes(object o)
        {
            ThreadCalcTimeParameters parameters = (ThreadCalcTimeParameters)o;
            string csvFilePath = "clients_calculations.csv";
            StringBuilder csvContent = new StringBuilder();

            lock (ConnectionManager.fileLock)
            {
                if (!ConnectionManager.AreHeadersWritten)
                {
                    string csvHeaders = "id_Klienta;Czas wyslania;Czas obliczen;Czas odebrania";
                    csvContent.AppendLine(csvHeaders);
                    ConnectionManager.AreHeadersWritten = true;
                }

                string csvLine = $"{parameters.clientid};{parameters.sendDuration.TotalMilliseconds}ms;{parameters.calcDuration.TotalMilliseconds}ms;{parameters.receiveDuration.TotalMilliseconds}ms";
                csvContent.AppendLine(csvLine);

                File.AppendAllText(csvFilePath, csvContent.ToString());
            }
        }


        public async Task ReceiveClientsCalculations(string nodejs, DateTime sendTime, DateTime startTime, DateTime endTime, Guid nodeID)
        {
            var clientid = Context.ConnectionId;
            DateTime receiveCalcTime = DateTime.Now;
            TreeNode calcMoves = JsonConvert.DeserializeObject<TreeNode>(nodejs);

            ConnectionManager._root.FindNodeById(nodeID).Children.Add(calcMoves);

            //ConnectionManager._root.Children.Add(calcMoves);
            ConnectionManager.MovesToBeCalculatedQueueCopy.TryDequeue(out _);
            ConnectionManager.ClientsStatus.AddOrUpdate(Context.ConnectionId, true, (key, oldValue) => true);

            //ConnectionManager.CalculatedMovesQueue.Enqueue(calcMoves);
            //tu zwrotka do UI zeby ruch obliczony wyslac
            //var sendTime = ConnectionManager.timeCalc4Client.Where(x => x.Item1 == calcMoves.Move && x.Item2 == clientid).First().Item3;

            TimeSpan sendDuration = startTime - sendTime;
            TimeSpan calcDuration = endTime - startTime;
            TimeSpan receiveDuration = receiveCalcTime - endTime;
            ThreadCalcTimeParameters parametres = new ThreadCalcTimeParameters
            {
                clientid = clientid,
                sendDuration = sendDuration,
                calcDuration = calcDuration,
                receiveDuration = receiveDuration
            };

            Thread calcThread = new Thread(new ParameterizedThreadStart(SaveCalculationsTimes));

            calcThread.Start(parametres);
            calcThread.Join();

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
