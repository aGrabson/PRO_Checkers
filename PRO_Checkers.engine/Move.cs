using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRO_Checkers.engine
{
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }

        public override string ToString()
        {
            return $"Move {From.Number}: from [{From.Column},{From.Row}] to [{To.Column},{To.Row}]";
        }
    }

    public class Eat : Move
    {
        public List<Position> Eaten { get; set; }

        public override string ToString()
        {
            return base.ToString() + $". And eat [{Eaten.Select(x => x.Number).Aggregate("", (a, b) => $"{a} {b}")}]";
        }
    }
}
