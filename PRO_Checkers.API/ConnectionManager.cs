using PRO_Checkers.engine;
using System.Collections.Concurrent;

namespace PRO_Checkers.API
{
    public static class ConnectionManager
    {
        public static ConcurrentDictionary<string, bool> ClientsStatus = new ConcurrentDictionary<string, bool>();
        public static ConcurrentQueue<Tuple<Move, Game, Guid>> MovesToBeCalculatedQueue = new ConcurrentQueue<Tuple<Move, Game, Guid>>();
        public static ConcurrentQueue<Tuple<Move, Game, Guid>> MovesToBeCalculatedQueueCopy = new ConcurrentQueue<Tuple<Move, Game, Guid>>();
        public static TreeNode _root = null;
        public static TreeNode _historyRoot = null;
        public static ConcurrentQueue<Tuple<string, TimeSpan, TimeSpan, TimeSpan>> timeCalc4Client = new ConcurrentQueue<Tuple<string, TimeSpan, TimeSpan, TimeSpan>>();
        public static bool AreHeadersWritten = false;
        public static readonly object fileLock = new object();

    }
}
