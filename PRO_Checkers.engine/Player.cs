namespace PRO_Checkers.engine
{
    public static class Player
    {
        private static bool _forceCapture = true;

        public static bool ForceCapture
        {
            get { return _forceCapture; }
            set { _forceCapture = value; }
        }
        public class MoveResult
        {
            public MoveResult(Move move, int weight)
            {
                Weight = weight;
                Move = move;
            }
            public int Weight { get; set; }

            public Move Move { get; set; }
        }

        public static Move NextBestMove(Game game, Tile color, int depth)
        {
            if (depth == 0)
            {
                return null;
            }
            List<MoveResult> bestMoves = new List<MoveResult>();


            foreach (var position in game.GetPositions(Helper.GetTileColors(color)))
            {
                var move = NextBestMove(game, color, position, depth);
                if (move != null)
                {
                    bestMoves.Add(move);
                }
            }

            //Eat is obligatory
            if (_forceCapture)
            {
                var eatMoves = bestMoves.Where(x => x.Move is Eat);
                if (eatMoves.Any())
                {
                    return eatMoves.OrderByDescending(x => x.Weight).First().Move;
                }
            }
            
            return bestMoves.OrderByDescending(x => x.Weight).FirstOrDefault()?.Move ?? null;
        }

        public static IEnumerable<Move> Moves(Game game, Tile color)
        {
            List<Move> moves = new List<Move>();
            foreach (var position in game.GetPositions(Helper.GetTileColors(color)))
            {
                var tileMoves = Helper.CalculateMoves(game, position);
                moves.AddRange(tileMoves);
            }
            if (_forceCapture)
            {
                var eatMoves = moves.OfType<Eat>();
                if (eatMoves.Any())
                    return eatMoves;
            }
            return moves;
        }


        public static MoveResult NextBestMove(Game game, Tile color, Position position, int depth)
        {
            if (depth == 0)
                return null;

            Move bestMove = null;
            var bestResult = int.MinValue;

            var moves = Helper.CalculateMoves(game, position);

            foreach (var move in moves)
            {
                var newGame = game.Move(move);
                Move nextBestMoveOther;
                if (move is Eat)
                {
                    var newMoves = Helper.CalculateMoves(newGame, Position.FromCoors(move.To.Row, move.To.Column));
                    var eatMoves = newMoves.OfType<Eat>();
                    if (eatMoves.Any())
                    {
                        bool validMove = false;
                        while (!validMove)
                        {
                            foreach (var eat in eatMoves)
                            {
                                if (move.To.Column == eat.From.Column && move.To.Row == eat.From.Row)
                                {
                                    newGame = newGame.Move(eat);
                                    validMove = true;
                                    var newMovesAfterEat = Helper.CalculateMoves(newGame, Position.FromCoors(eat.To.Row, eat.To.Column)).ToArray();
                                    var eatMovesAfterEat = newMovesAfterEat.OfType<Eat>();
                                    if (!eatMovesAfterEat.Any(em => em.From.Column == eat.To.Column && em.From.Row == eat.To.Row))
                                    {
                                        validMove = true;
                                    }
                                }
                            }
                            validMove = true;
                        }
                    }
                }

                nextBestMoveOther = NextBestMove(newGame, Helper.ChangeColor(color), depth - 1);
                if (nextBestMoveOther != null)
                {
                    newGame = newGame.Move(nextBestMoveOther);
                }

                var result = Score(newGame, color);
                if (result > bestResult)
                {
                    bestMove = move;
                    bestResult = result;
                }
            }

            return new MoveResult(bestMove, bestResult);
        }

        public static int Score(Game game, Tile color)
        {
            var tiles = game.GetPositions(Helper.GetTileColors(color)).Count() + game.GetPositions(Helper.GetTileColors(Helper.ChangeColor(color))).Count();
            var stage = tiles / 10 + 1;

            return
                (stage * 50) * ScoreDiferentBettwenPieces(game, color) +
                ((5 - stage) * 50) * ScoreQueens(game, color) +
                ((5 - stage)) * ScoreDefenceLinePosition(game, color) +
                ((5 - stage)) * ScoreCenterAdvantage(game, color) +
                ((5 - stage)) * ScoreMoveAmount(game, color);
        }

        static int ScoreDiferentBettwenPieces(Game game, Tile color)
        {
            var mePieces = game.GetPositions(Helper.GetTileColors(color));
            var otherPieces = game.GetPositions(Helper.GetTileColors(Helper.ChangeColor(color)));

            var diferentBettwenPieces = mePieces.Count() - otherPieces.Count();
            return diferentBettwenPieces;

        }

        static int ScoreQueens(Game game, Tile color)
        {
            var mePieces = game.GetPositions(Helper.GetTileColors(color));
            var otherPieces = game.GetPositions(Helper.GetTileColors(Helper.ChangeColor(color)));

            var meQueens = mePieces.Select(x => game.GetTile(x)).Count(x => (int)x > 2);
            var otherQueens = otherPieces.Select(x => game.GetTile(x)).Count(x => (int)x > 2);
            var diferentBetwwenQueens = (meQueens - otherQueens);

            return diferentBetwwenQueens;
        }

        static int ScoreDefenceLinePosition(Game game, Tile color)
        {
            var mePieces = game.GetPositions(Helper.GetTileColors(color));

            var defencePositionRow = color == Tile.White ? 0 : 7;
            var amountDefending = mePieces.Where(x => x.Row == defencePositionRow).Count();

            return amountDefending;
        }

        static int ScoreCenterAdvantage(Game game, Tile color)
        {
            var mePieces = game.GetPositions(Helper.GetTileColors(color));
            var goodTiles = new[] { 14, 18, 15, 19 };
            var amount = mePieces.Select(x => goodTiles.Contains(x.Number)).Count();
            return amount;
        }

        static int ScoreMoveAmount(Game game, Tile color)
        {
            var myMoves = Moves(game, color).Count();
            var otherMoves = Moves(game, Helper.ChangeColor(color)).Count();
            return myMoves > otherMoves ? 1 : 0;
        }
    }
}
