using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRO_Checkers.engine
{
    public class TreeNode
    {
        public Game GameState { get; set; }
        public Move Move { get; set; }
        public int WeightWhite { get; set; }
        public int WeightBlack { get; set; }
        public List<TreeNode> Children { get; set; }

        public TreeNode(Game gameState, Move move = null)
        {
            GameState = gameState;
            Move = move;
            Children = new List<TreeNode>();
        }
    }
}
