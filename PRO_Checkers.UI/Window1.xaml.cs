using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PRO_Checkers.engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PRO_Checkers.UI
{
    /// <summary>
    /// Logika interakcji dla klasy Window1.xaml
    /// </summary>
    public partial class Window1 : Window
{
        private const int Size = 50; // Size of each square
        private Ellipse movingPiece = null;
        private Point pieceOffset;
        private string typeOfGame = "";
        private bool backwardEat;
        private bool forcedEat;
        private Stack<Tuple<Game, Tile>> game;
        private Game board;
        private Tile color;
        private int startRow, startCol;
        private bool GameFinished = false;
        private int depth = 4;
        public bool flag = false;
        public Eat eatMove = null;
        public HubConnection connection;
        public Window1(string typeOfGame, bool backwardEat, bool forcedEat, string ipaddress = "localhost")
    {
        InitializeComponent();
            connection = new HubConnectionBuilder()
                .WithUrl($"http://{ipaddress}:5202/game-hub")
                .WithServerTimeout(TimeSpan.FromMinutes(5))
                .Build();
            connection.On<string, bool, string>("nextMove", async (nextMove, eat, nestedMovesjs) =>
            {
                game.Push(new Tuple<Game, Tile>(board, color));
                if (eat)
                {
                    Eat move = JsonConvert.DeserializeObject<Eat>(nextMove);
                    List<Eat>? nestedEats = JsonConvert.DeserializeObject<List<Eat>>(nestedMovesjs);
                    board = board.Move(move);
                    GenerateCheckerboard();

                    foreach (var eatMoveTest in nestedEats)
                    {
                        game.Push(new Tuple<Game, Tile>(board, color));
                        board = board.Move(eatMoveTest);
                        GenerateCheckerboard();
                    }

                    //eatMove = move;
                    //await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, depth);
                    //flag = true;
                }
                else
                {
                    Move move = JsonConvert.DeserializeObject<Move>(nextMove);
                    board = board.Move(move);
                    GenerateCheckerboard();
                    
                    //flag = false;
                }
                ChangeTurn();
                CheckIfEndGame();
                if (typeOfGame == "computerVsComputer" && (!Helper.IsGameFinished(board, color)))
                {
                    await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, 2);
                }


                //if (flag)
                //{
                //    if (eat)
                //    {
                //        Eat move = JsonConvert.DeserializeObject<Eat>(nextMove);
                //        if (eatMove.To.Column == move.From.Column && eatMove.To.Row == move.From.Row)
                //        {
                //            game.Push(new Tuple<Game, Tile>(board, color));
                //            board = board.Move(move);
                //            await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, depth);
                //            flag = true;
                //        }
                //        else
                //        {
                //            ChangeTurn();
                //            flag = false;
                //        }

                    //        GenerateCheckerboard();
                    //        CheckIfEndGame();
                    //    }
                    //    else
                    //    {
                    //        ChangeTurn();
                    //        CheckIfEndGame();
                    //        flag = false;
                    //    }
                    //}
                    //else
                    //{
                    //    game.Push(new Tuple<Game, Tile>(board, color));
                    //    if (eat)
                    //    {
                    //        Eat move = JsonConvert.DeserializeObject<Eat>(nextMove);
                    //        board = board.Move(move);
                    //        eatMove = move;
                    //        await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, depth);
                    //        flag = true;
                    //    }
                    //    else
                    //    {
                    //        Move move = JsonConvert.DeserializeObject<Move>(nextMove);
                    //        board = board.Move(move);
                    //        ChangeTurn();
                    //        flag = false;
                    //    }
                    //    GenerateCheckerboard();
                    //    CheckIfEndGame();
                    //}

            });
            StartHubConnectionAsync();
            //try
            //{
            //    await connection.StartAsync();
            //    Game board = Game.NewGame();
            //    Tile color = Tile.White;
            //    bool backwardEat = true;
            //    bool forcedEat = true;

            //    //Game game, bool backwardEat, bool forcedEat, int depth, Tile color
            //    await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, 4);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Błąd połączenia: {ex.Message}");
            //}
            this.typeOfGame = typeOfGame;
        this.backwardEat = backwardEat;
        this.forcedEat = forcedEat;
        game = new Stack<Tuple<Game, Tile>>();
        board = Game.NewGame();
        color = Tile.White;
        Helper.ForceCapture = forcedEat;
        Player.ForceCapture = forcedEat;
        Helper.BackwardCapture = backwardEat;
            TurnLabel.Content = "Tura gracza: " + (color == Tile.White || color == Tile.QueenWhite ? "białego" : "czarnego");
            ScoreWhite.Content = "Punkty białego: " + Player.Score(board, Tile.White);
            ScoreBlack.Content = "Punkty czarnego: " + Player.Score(board, Tile.Black);
            GenerateCheckerboard();
            
        }
        private async Task StartHubConnectionAsync()
        {
            try
            {
                await connection.StartAsync();
                // Tutaj możesz dodać dodatkowe operacje po rozpoczęciu połączenia, jeśli są potrzebne
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd połączenia: {ex.Message}");
                // Tutaj możesz obsłużyć błąd połączenia, jeśli wystąpi
            }
        }
        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (typeOfGame == "computerVsComputer")
            {
                await PlayComputerVsComputer();
            }
        }

        private async Task PlayComputerVsComputer()
        {
            
            await ComputerMove();
            //await Task.Delay(1000);
            
        }

        public void GenerateCheckerboard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                bool isBlackSquare = false;
                int blackSquareCounter = 1; // Licznik dla czarnych pól

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        var square = new Rectangle
                        {
                            Width = Size,
                            Height = Size,
                            Fill = isBlackSquare ? Brushes.Black : Brushes.White
                        };

                        Canvas.SetTop(square, i * Size);
                        Canvas.SetLeft(square, j * Size);
                        CheckerCanvas.Children.Add(square);
                        // Dodawanie numeracji tylko do czarnych pól
                        if (isBlackSquare)
                        {
                            var textBlock = new TextBlock
                            {
                                Text = blackSquareCounter.ToString(),
                                Foreground = Brushes.White, // Kolor tekstu
                                FontWeight = FontWeights.Bold,
                                FontSize = 12
                            };

                            Canvas.SetTop(textBlock, i * Size + (Size / 2) - 5); // Centrowanie tekstu w pionie
                            Canvas.SetLeft(textBlock, j * Size + (Size / 2) - 5); // Centrowanie tekstu w poziomie
                            CheckerCanvas.Children.Add(textBlock);

                            blackSquareCounter++; // Zwiększanie licznika czarnych pól
                        }
                        isBlackSquare = !isBlackSquare;

                        
                    }
                    isBlackSquare = !isBlackSquare;
                }

                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        Tile tile = board.GetTile(Position.FromCoors(7 - row, col));
                        if (tile != Tile.Empty)
                        {
                            Brush colorBrush = (tile == Tile.Black || tile == Tile.QueenBlack) ? Brushes.Gray : Brushes.White;
                            AddChecker(row, col, colorBrush, tile == Tile.QueenBlack || tile == Tile.QueenWhite);
                        }
                    }
                }
            });
        }


        private void AddChecker(int i, int j, Brush color, bool isQueen)
            {
                var checker = new Ellipse
                {
                    Width = Size * 0.8,
                    Height = Size * 0.8,
                    Fill = color
                };
            if (isQueen)
            {
                checker.Stroke = Brushes.Gold;
                checker.StrokeThickness = 5;
            }

            Canvas.SetTop(checker, i * Size + Size * 0.1);
                Canvas.SetLeft(checker, j * Size + Size * 0.1);
                CheckerCanvas.Children.Add(checker);

                checker.MouseDown += Checker_MouseDown;
            }

        private void Checker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var checker = sender as Ellipse;

            var position = e.GetPosition(CheckerCanvas);
            int row = (int)(position.Y / Size);
            int col = (int)(position.X / Size);
            startRow = (int)(position.Y / Size);
            startCol = (int)(position.X / Size);
            if ((row + col) % 2 == 0)
                return;

            double offsetX = (Size - checker.Width) / 2;
            double offsetY = (Size - checker.Height) / 2;

            Canvas.SetTop(checker, row * Size + offsetY);
            Canvas.SetLeft(checker, col * Size + offsetX);

            movingPiece = checker;
            pieceOffset = e.GetPosition(movingPiece);
            checker.CaptureMouse();
        }

        private void CheckerCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingPiece != null)
            {
                var position = e.GetPosition(CheckerCanvas);
                int row = (int)(position.Y / Size);
                int col = (int)(position.X / Size);

                if ((row + col) % 2 != 0)
                {
                    double offsetX = (Size - movingPiece.Width) / 2;
                    double offsetY = (Size - movingPiece.Height) / 2;
                    Canvas.SetTop(movingPiece, row * Size + offsetY);
                    Canvas.SetLeft(movingPiece, col * Size + offsetX);
                }
            }
        }
        public void CheckIfEndGame()
        {
            if (Helper.IsGameFinished(board, color))
            {
                MessageBox.Show("Koniec gry!", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                GameFinished = true;
            }
        }
        private async void CheckerCanvas_MouseUp(object sender, MouseButtonEventArgs e)
            {
                if (movingPiece != null)
                {
                    movingPiece.ReleaseMouseCapture();
                var position = e.GetPosition(CheckerCanvas);
                int newRow = (int)(position.Y / Size);
                int newCol = (int)(position.X / Size);

                var moves = Player.Moves(board, color).ToArray();
                bool validMove = false;
                bool validMove2 = false;

                foreach (var move in moves)
                    {
                        if (move.From.Column == startCol &&
                            move.From.Row == 7 - startRow &&
                            move.To.Column == newCol &&
                            move.To.Row == 7 - newRow)
                        {
                        game.Push(new Tuple<Game, Tile>(board, color));
                        board = board.Move(move);
                            validMove = true;
                        
                        if (move is Eat)
                            {
                                var newMoves = Player.Moves(board, color).ToArray();
                                var eatMoves = newMoves.OfType<Eat>();
                                if (eatMoves.Any())
                                {
                                        foreach (var eat in eatMoves)
                                        {
                                            if (move.To.Column == eat.From.Column && move.To.Row == eat.From.Row)
                                            {
                                        MessageBox.Show("Kolejne bicie dozwolone.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                                        validMove2 = true;
                                            }
                                        }
                                }
                            }
                    }
                    }
                    if (!validMove)
                    {
                        MessageBox.Show("Nieprawidłowy ruch! Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        ResetPiecePosition();
                    }if (validMove && !validMove2)
                    {
                    //MessageBox.Show("Zmiana zawodnika.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChangeTurn();
                    GenerateCheckerboard();
                    CheckIfEndGame();
                    if (!GameFinished)
                    {
                        await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, depth);
                        //await ComputerMove();
                    }
                    depth = 2;
                }
                GenerateCheckerboard();
                movingPiece = null;
                
                
                }
            }
        private async Task ComputerMove()
        {
            await connection.InvokeAsync("SendToCalculate", JsonConvert.SerializeObject(board), JsonConvert.SerializeObject(color), backwardEat, forcedEat, depth);
            //ChangeTurn();
            //GenerateCheckerboard();
            //CheckIfEndGame();
            //await Task.Delay(100);
            //var suggestedMove = Player.NextBestMove(board, color, depth);
            //game.Push(new Tuple<Game, Tile>(board, color));
            //board = board.Move(suggestedMove);
            //if (suggestedMove is Eat)
            //{
            //    var newMoves = Player.Moves(board, color).ToArray();
            //    var eatMoves = newMoves.OfType<Eat>();
            //    if (eatMoves.Any())
            //    {
            //        bool validMove = false;
                    
            //        while (!validMove)
            //        {
            //            foreach (var eat in eatMoves)
            //            {
            //                if (suggestedMove.To.Column == eat.From.Column && suggestedMove.To.Row == eat.From.Row)
            //                {
            //                    game.Push(new Tuple<Game, Tile>(board, color));
            //                   // await Task.Delay(1000);
            //                    board = board.Move(eat);
            //                    validMove = true;
            //                    var newMovesAfterEat = Player.Moves(board, color).ToArray();
            //                    var eatMovesAfterEat = newMovesAfterEat.OfType<Eat>();
            //                    validMove = true;
            //                }
            //            }
            //            validMove = true;
            //        }
            //    }
            //}
            
            
        }
        private void ResetPiecePosition()
        {
            if (movingPiece != null)
            {
                double offsetX = (Size - movingPiece.Width) / 2;
                double offsetY = (Size - movingPiece.Height) / 2;

                Canvas.SetTop(movingPiece, startRow * Size + offsetY);
                Canvas.SetLeft(movingPiece, startCol * Size + offsetX);
            }
        }
        public void ChangeTurn()
        {
            color = Helper.ChangeColor(color);
            TurnLabel.Dispatcher.Invoke(() =>
            {
                TurnLabel.Content = "Tura gracza: " + (color == Tile.White ? "białego" : "czarnego");
            });
            ScoreWhite.Dispatcher.Invoke(() =>
            {
                ScoreWhite.Content = "Punkty białego: " + Player.Score(board, Tile.White);
            });
            ScoreBlack.Dispatcher.Invoke(() =>
            {
                ScoreBlack.Content = "Punkty czarnego: " + Player.Score(board, Tile.Black);
            });
            //TurnLabel.Content = "Tura gracza: " + (color == Tile.White ? "białego" : "czarnego");
            //ScoreWhite.Content = "Punkty białego: " + Player.Score(board, Tile.White);
            //ScoreBlack.Content = "Punkty czarnego: " + Player.Score(board, Tile.Black);
        }
    }
}
