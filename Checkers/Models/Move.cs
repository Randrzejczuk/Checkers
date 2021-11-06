using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Move
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public bool Isvalid { get; set; }
        public int? DestroyX { get; set; }
        public int? DestroyY { get; set; }
    }
}
