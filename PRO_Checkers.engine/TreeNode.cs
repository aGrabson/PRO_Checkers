using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PRO_Checkers.engine
{
    public class TreeNode
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Game GameState { get; set; }
        public Move Move { get; set; }
        public Eat EatMove { get; set; } = null;
        public int WeightWhite { get; set; }
        public int WeightBlack { get; set; }
        public List<TreeNode> Children { get; set; }
        public List<Eat> NestedEats { get; set; }

        public TreeNode(Game gameState, Move move = null)
        {
            GameState = gameState;
            Move = move;
            Children = new List<TreeNode>();
            NestedEats = new List<Eat>();
        }
        public TreeNode FindNodeById(Guid id)
        {
            
            if (this.Id == id)
            {
                return this;
            }

            List<TreeNode> nodesCopy = new List<TreeNode>(Children);

            foreach (var child in nodesCopy)
            {
                var result = child.FindNodeById(id);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        public List<TreeNode> GetLeafNodes()
        {
            var leafNodes = new List<TreeNode>();
            CollectLeafNodes(this, leafNodes);
            return leafNodes;
        }

        private void CollectLeafNodes(TreeNode node, List<TreeNode> leafNodes)
        {
            if (node.Children == null || node.Children.Count == 0)
            {
                leafNodes.Add(node);
            }
            else
            {
                foreach (var child in node.Children)
                {
                    CollectLeafNodes(child, leafNodes);
                }
            }
        }
    }
}
