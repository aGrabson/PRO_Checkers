using PRO_Checkers.engine;
using System.Collections.Concurrent;

namespace PRO_Checkers.API
{
    public static class ConnectionManager
    {
        public static ConcurrentDictionary<string, bool> ClientsStatus = new ConcurrentDictionary<string, bool>();
        public static Queue<Move> MovesToBeCalculatedQueue = new Queue<Move>();
        public static Queue<Move> MovesToBeCalculatedQueueCopy = new Queue<Move>();
        public static Queue<Move> CalculatedMovesQueue = new Queue<Move>();
        public static TreeNode _root = null;
        public static List<Tuple<Move, string, DateTime>> timeCalc4Client = new List<Tuple<Move, string, DateTime>>();
        public static bool AreHeadersWritten = false;

    }
}
