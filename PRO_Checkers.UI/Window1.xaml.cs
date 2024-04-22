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
        private int depth = 3;
        public Window1(string typeOfGame, bool backwardEat, bool forcedEat)
    {
        InitializeComponent();

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
            while (!Helper.IsGameFinished(board, color))
            {
                await ComputerMove();
                //await Task.Delay(1000);
            }
        }

        private void GenerateCheckerboard()
            {
                bool isBlackSquare = false;
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
        private void CheckIfEndGame()
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
                        await ComputerMove();
                    }
                }
                GenerateCheckerboard();
                movingPiece = null;
                
                
                }
            }
        private async Task ComputerMove()
        {
            await Task.Delay(100);
            var suggestedMove = Player.NextBestMove(board, color, depth);
            game.Push(new Tuple<Game, Tile>(board, color));
            board = board.Move(suggestedMove);
            if (suggestedMove is Eat)
            {
                var newMoves = Player.Moves(board, color).ToArray();
                var eatMoves = newMoves.OfType<Eat>();
                if (eatMoves.Any())
                {
                    bool validMove = false;
                    
                    while (!validMove)
                    {
                        foreach (var eat in eatMoves)
                        {
                            if (suggestedMove.To.Column == eat.From.Column && suggestedMove.To.Row == eat.From.Row)
                            {
                                game.Push(new Tuple<Game, Tile>(board, color));
                               // await Task.Delay(1000);
                                board = board.Move(eat);
                                validMove = true;
                                var newMovesAfterEat = Player.Moves(board, color).ToArray();
                                var eatMovesAfterEat = newMovesAfterEat.OfType<Eat>();
                                validMove = true;
                            }
                        }
                        validMove = true;
                    }
                }
            }
            ChangeTurn();
            GenerateCheckerboard();
            CheckIfEndGame();
            
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
        private void ChangeTurn()
        {
            color = Helper.ChangeColor(color);
            TurnLabel.Content = "Tura gracza: " + (color == Tile.White ? "białego" : "czarnego");
            ScoreWhite.Content = "Punkty białego: " + Player.Score(board, Tile.White);
            ScoreBlack.Content = "Punkty czarnego: " + Player.Score(board, Tile.Black);
        }
    }
}
