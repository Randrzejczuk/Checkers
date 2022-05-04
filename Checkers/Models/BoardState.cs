using Checkers.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class BoardState
    {
        public int Id { get; set; }
        public List<Field> Fields { get; set; }
        public Field LastMoved { get; set; }
        public Field this[int x,int y]
        {
            get => Fields.Find(f => f.X == x && f.Y == y);
        }
        public BoardState()
        {
        }
        public BoardState (BoardState board)
        {
            Fields = new List<Field>();
            foreach(Field field in board.Fields)
            {
                Fields.Add(new Field
                {
                    X = field.X,
                    Y = field.Y,
                    State = field.State
                });
            }
        }
        /*
         * Initializes board
         */
        public void Init()
        {
            LastMoved = null;
            if (Fields == null)
            {
                Fields = new List<Field>();
                for (int i = 0; i < 64; i++)
                    Fields.Add(new Field()
                    {
                        X = (i % 8) + 1,
                        Y = (i / 8) + 1
                    });
            }
            foreach (Field field in Fields)
            {
                if (field.X % 2 == field.Y % 2)
                    field.State = State.Unused;
                else if (field.Y <= 3)
                    field.State = State.Black;
                else if (field.Y >= 6)
                    field.State = State.White;
                else
                    field.State = State.Empty;
            }
        }
        /*
         * Sets state of the board received in string form
         */
        public void SetState(string state)
        {
            List<Field> ActiveFields = Fields
                .Where(x => x.State != State.Unused)
                .OrderBy(x => x.Y)
                .ThenBy(x => x.X)
                .ToList();
            var stateArray = state.ToCharArray();
            for (int i = 0; i < 32; i++)
            {
                ActiveFields[i].State = (stateArray[i]) switch
                {
                    'w' => State.White,
                    'W' => State.White_Q,
                    'b' => State.Black,
                    'B' => State.Black_Q,
                    _ => State.Empty,
                };
            }
        }
        /*
         * Returns board as string
         */
        public string GetState()
        {
            string result = string.Empty;
            List<Field> ActiveFields = Fields
               .Where(x => x.State != State.Unused)
               .OrderBy(x => x.Y)
               .ThenBy(x => x.X)
               .ToList();
            for (int i = 0; i < 32; i++)
            {
                switch (ActiveFields[i].State)
                {
                    case State.White:
                        result += "w";
                        break;
                    case State.White_Q:
                        result += "W";
                        break;
                    case State.Black:
                        result += "b";
                        break;
                    case State.Black_Q:
                        result += "B";
                        break;
                    case State.Empty:
                        result += "X";
                        break;
                }
            }
            return result;
        }
        /*
         *Validates requested move
         */
        public string ValidateMove(Move move)
        {
            Field attacker = this[move.StartX, move.StartY];
            if (LastMoved != null && attacker.GetColor() == LastMoved.GetColor())
            {
                if (attacker != LastMoved)
                    return "You have to attack with the same piece.";
            }
            IEnumerable<Field> attacks;
            attacks = Fields.Where(f => f.GetColor() == attacker.GetColor());
            attacks = attacks.Where(f => f.CanAttack(this) == true);
            if (!attacks.Any())
                move.Isvalid = true;
            else if (attacks.Any(f=>f.X == move.StartX && f.Y == move.StartY) && Math.Abs(move.StartX - move.TargetX) == 2)
                move.Isvalid = true;
            if (move.Isvalid)
            {
                if (Math.Abs(move.StartX - move.TargetX) == 2)
                {
                    move.DestroyX = (move.StartX + move.TargetX) / 2;
                    move.DestroyY = (move.StartY + move.TargetY) / 2;
                }
                return "";
            }
            return "You have to attack enemy piece.";
        }
        /*
         * Realizes requested move
         */
        public BoardState RecordMovement(Move move)
        {
            Field start = this[move.StartX, move.StartY];
            Field target = this[move.TargetX, move.TargetY];
            target.State = start.State;
            start.State = State.Empty;
            if ((target.Y == 8 && target.State == State.Black)|| (target.Y == 1 && target.State == State.White))
                target.Promote();

            if (move.DestroyX != null)
            {
                Field destroy = this[(int)move.DestroyX, (int)move.DestroyY];
                destroy.State = State.Empty;
            }
            LastMoved = target;
            return this;
        }
        /*
         * Returns winner if there are no avaiable moves for player
         */
        public Color CheckWinner ()
        {

            bool whiteLooses = !Fields.Where(f => f.GetColor() == Color.White).Any(f => f.CanMove(this));
            bool blackLooses = !Fields.Where(f => f.GetColor() == Color.Black).Any(f => f.CanMove(this));

            if (whiteLooses)
                return Color.Black;
            if (blackLooses)
                return Color.White;
            return Color.None;
        }
        /*
         * Finds all avaiable moves for player
         */
        private List<AiMove> GetAvailableMoves(Color color)
        {
            List<AiMove> validMoves = new List<AiMove>();
            IEnumerable<Field> attacks = Fields
                .Where(f => f.GetColor() == color)
                .Where(f => f.CanAttack(this) == true);
            if (attacks.Any())
            {
                foreach (Field field in attacks)
                {
                    validMoves.AddRange(field.GetAttacks(this));
                }
            }
            else
            {
                IEnumerable<Field> moves = Fields
               .Where(f => f.GetColor() == color)
               .Where(f => f.CanMove(this) == true);
                if (moves.Any())
                {
                    foreach (Field field in moves)
                    {
                        validMoves.AddRange(field.GetMoves(this));
                    }
                }
            }
            return validMoves;
        }
        /*
         * Recursive function that predicts best next movements and returns the best currently avaiable
         * depth sets how many recursions will occur
         */
        public AiMove GetBestMove(bool isAiTurn, Color botColor, BoardState board, int depth)
        {
            List<AiMove> AvailableMoves;
            AiMove selectedMove;
            if (isAiTurn)
                AvailableMoves = board.GetAvailableMoves(botColor);
            else
            {
                Color playerColor = botColor == Color.White ? Color.Black : Color.White;
                AvailableMoves = board.GetAvailableMoves(playerColor);
            }
            //Escape condition
            if (depth == 0)
            {
                foreach (AiMove move in AvailableMoves)
                {
                    BoardState afterMove = new BoardState(board);
                    afterMove = afterMove.RecordMovement(move);
                    Field target = afterMove[move.TargetX, move.TargetY];
                    if (move.DestroyX != null && target.CanAttack(afterMove))
                        move.Score++;
                }
                selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                return selectedMove;
            }
            else
            {
                foreach (AiMove move in AvailableMoves)
                {
                    BoardState afterMove = new BoardState(board);
                    afterMove = afterMove.RecordMovement(move);
                    Field target = afterMove[move.TargetX, move.TargetY];
                    if (move.DestroyX != null && target.CanAttack(afterMove))
                    {
                        move.Score++;
                        AiMove nextMove = GetBestMove(isAiTurn, botColor, afterMove, depth - 1);
                        if (nextMove != null)
                            move.Score += nextMove.Score;
                    }
                    else
                    {
                        AiMove nextMove = GetBestMove(!isAiTurn, botColor, afterMove, depth - 1);
                        if (nextMove != null)
                            move.Score -= nextMove.Score;
                    }
                }
                selectedMove = AvailableMoves.FirstOrDefault(m => m.Score == AvailableMoves.Max(a => a.Score));
                return selectedMove;
            }
        }
    }
}
