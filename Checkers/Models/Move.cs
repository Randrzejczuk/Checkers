using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Move
    {
        public int startX { get; set; }
        public int startY { get; set; }
        public int targetX { get; set; }
        public int targetY { get; set; }
        public bool isvalid { get; set; }
        public int? destroyX { get; set; }
        public int? destroyY { get; set; }
    }
}
