using PRO_Checkers.engine;

namespace PRO_Checkers.Cons
{
    public class Program
    {
        static void Main(string[] args)
        {
            var game = new Stack<Tuple<Game, Tile>>();
            var board = Game.NewGame();
            var color = Tile.White;
            Helper.ForceCapture = true;
            Player.ForceCapture = true;
            Helper.BackwardCapture = false;

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            while (!Helper.IsGameFinished(board, color))
            {
                Console.Clear();
                Console.WriteLine($"Play: {color} -> {Player.Score(board, color)} points");
                Console.WriteLine(board);
                Console.WriteLine();


                var suggestedMove = Player.NextBestMove(board, color, 5);
                Console.WriteLine("Suggested move: " + suggestedMove);
                Console.WriteLine(
                    $"Choose an option: {Environment.NewLine}\t " +
                    $"1)Play suggested move {Environment.NewLine}\t " +
                    $"2)Play custom move {Environment.NewLine}\t " +
                    $"3)Undo last move{Environment.NewLine}\t ");

                var optionSelected = Console.ReadLine();
                if (optionSelected == "1")
                {
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
                                        Console.Clear();
                                        Console.WriteLine($"Play: {color} -> {Player.Score(board, color)} points");
                                        Console.WriteLine(board);
                                        Console.WriteLine();
                                        Console.WriteLine($"Next eats) " + eat);
                                        Console.WriteLine($"Type enter");
                                        Console.ReadLine();
                                        board = board.Move(eat);
                                        validMove = true;
                                        var newMovesAfterEat = Player.Moves(board, color).ToArray();
                                        var eatMovesAfterEat = newMovesAfterEat.OfType<Eat>();
                                        if (eatMovesAfterEat.Any(em => em.From.Column == eat.To.Column && em.From.Row == eat.To.Row))
                                        {
                                            Console.WriteLine("You can make another beat.");
                                        }
                                        else
                                        {
                                            validMove = true;
                                            break;
                                        }
                                    }
                                }
                                validMove = true;
                            }
                        }
                    }
                    color = Helper.ChangeColor(color);
                }
                else if (optionSelected == "2")
                {
                    game.Push(new Tuple<Game, Tile>(board, color));

                    var moves = Player.Moves(board, color).ToArray();
                    bool validMove = false;

                    while (!validMove)
                    {
                        for (int i = 0; i < moves.Length; i++)
                        {
                            Console.WriteLine($"{i}) " + moves[i]);
                        }

                        Console.WriteLine("Type move like: 5 1 4 2");
                        string option = Console.ReadLine();
                        string[] numbers = option.Split(" ");

                        if (numbers.Length == 4)
                        {
                            foreach (var move in moves)
                            {
                                if (move.From.Column == int.Parse(numbers.ElementAt(0)) &&
                                    move.From.Row == int.Parse(numbers.ElementAt(1)) &&
                                    move.To.Column == int.Parse(numbers.ElementAt(2)) &&
                                    move.To.Row == int.Parse(numbers.ElementAt(3)))
                                {
                                    board = board.Move(move);
                                    validMove = true;

                                    if (move is Eat)
                                    {
                                        var newMoves = Player.Moves(board, color).ToArray();
                                        var eatMoves = newMoves.OfType<Eat>();
                                        if (eatMoves.Any())
                                        {
                                            bool validMove2 = false;
                                            while (!validMove2)
                                            {
                                                foreach (var eat in eatMoves)
                                                {
                                                    if (move.To.Column == eat.From.Column && move.To.Row == eat.From.Row)
                                                    {
                                                        game.Push(new Tuple<Game, Tile>(board, color));
                                                        Console.Clear();
                                                        Console.WriteLine($"Play: {color} -> {Player.Score(board, color)} points");
                                                        Console.WriteLine(board);
                                                        Console.WriteLine();
                                                        Console.WriteLine($"Next eats) " + eat);
                                                        Console.WriteLine("Type move like: 5 1 4 2");
                                                        string option2 = Console.ReadLine();
                                                        string[] numbers2 = option2.Split(" ");
                                                        if (numbers2.Length == 4)
                                                        {
                                                            if (eat.From.Column == int.Parse(numbers2.ElementAt(0)) &&
                                                                eat.From.Row == int.Parse(numbers2.ElementAt(1)) &&
                                                                eat.To.Column == int.Parse(numbers2.ElementAt(2)) &&
                                                                eat.To.Row == int.Parse(numbers2.ElementAt(3)))
                                                            {
                                                                board = board.Move(eat);
                                                                validMove2 = true;

                                                                var newMovesAfterEat = Player.Moves(board, color).ToArray();
                                                                var eatMovesAfterEat = newMovesAfterEat.OfType<Eat>();
                                                                if (eatMovesAfterEat.Any(em => em.From.Column == eat.To.Column && em.From.Row == eat.To.Row))
                                                                {
                                                                    Console.WriteLine("You can make another beat.");
                                                                }
                                                                else
                                                                {
                                                                    validMove = true;
                                                                    validMove2 = true;
                                                                    break;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine("Invalid move2. Please try again.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Too few arguments2. Please try again.");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    color = Helper.ChangeColor(color);
                                    validMove = true;
                                    break;
                                }
                            }
                            if (!validMove)
                            {
                                Console.WriteLine("Invalid move. Please try again.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Too few arguments. Please try again.");
                        }
                    }
                }
                else if (optionSelected == "3")
                {
                    var undoMove = game.Pop();
                    board = undoMove.Item1;
                    color = undoMove.Item2;
                }
            }
            Console.Clear();
            Console.WriteLine($"End Game");
            Console.WriteLine($"Player: {color} -> {Player.Score(board, color)} points");
            Console.WriteLine($"Player: {Helper.ChangeColor(color)} -> {Player.Score(board, Helper.ChangeColor(color))} points");

        }
    }
}
