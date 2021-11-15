using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public enum State
    {
        Empty,
        White,
        White_Q,
        Black,
        Black_Q,
        Unused
    }
    public enum Color
    {
        White,
        Black,
        None
    }
    public class Field
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public State State { get; set; }
        public void Promote()
        {
            switch (State)
            {
                case State.Black:
                    State = State.Black_Q;
                    break;
                case State.White:
                    State = State.White_Q;
                    break;
            }
        }
        public bool CanAttack(BoardState board)
        {
            switch (State)
            {
                case State.Unused:
                    return false;
                case State.Empty:
                    return false;
            }
            if (State != State.White)
            {
                if (Y < 7)
                {
                    if ( X < 7)
                    {
                        var field = board.GetField(X + 1, Y + 1);
                        if (State == State.White_Q)
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board.GetField(X + 2, Y + 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board.GetField(X + 2, Y + 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                    if (X > 2)
                    {
                        var field = board.GetField(X - 1, Y + 1);
                        if (State == State.White_Q)
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board.GetField(X - 2, Y + 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board.GetField(X - 2, Y + 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                }
            }
            if (State != State.Black)
            {
                if (Y > 2)
                {
                    if (X < 7)
                    {
                        var field = board.GetField(X + 1, Y - 1);
                        if (State == State.Black_Q)
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board.GetField(X + 2, Y - 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board.GetField(X + 2, Y - 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                    if (X > 2)
                    {
                        var field = board.GetField(X - 1, Y - 1);
                        if (State == State.Black_Q)
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board.GetField(X - 2, Y - 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board.GetField(X - 2, Y - 2);
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                }
            }
            return false; 
        }
        public bool CanMove(BoardState board)
        {
            switch (State)
            {
                case State.Unused:
                    return false;
                case State.Empty:
                    return false;
            }
            if (State != State.White)
            {
                if (Y != 8)
                {
                    if (X != 1)
                    {
                        Field field = board.GetField(X - 1, Y + 1);
                        if (field.State == State.Empty)
                            return true;
                    }
                    if (X != 8)
                    {
                        Field field = board.GetField(X + 1, Y + 1);
                        if (field.State == State.Empty)
                            return true;
                    }
                }
            }
            if (State != State.Black)
            {
                if (Y != 1)
                {
                    if (X != 1)
                    {
                        Field field = board.GetField(X - 1, Y - 1);
                        if (field.State == State.Empty)
                            return true;
                    }
                    if (X != 8)
                    {
                        Field field = board.GetField(X + 1, Y - 1);
                        if (field.State == State.Empty)
                            return true;
                    }
                }
            }
            return CanAttack(board);
        }
        public Color GetColor()
        {
            if (State == State.White || State == State.White_Q)
                return Color.White;
            if (State == State.Black || State == State.Black_Q)
                return Color.Black;
            return Color.None;
        }
        public List<AiMove> GetAttacks(BoardState board)
        {
            List<AiMove> moves = new List<AiMove>();
            if (State != State.White)
            {
                if (Y < 7)
                {
                    if (X < 7)
                    {
                        Field NextField = board.GetField(X + 1, Y + 1);
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board.GetField(X + 2, Y + 2);
                            if (TargetField.GetColor() == Color.None)
                            {
                                moves.Add(new AiMove()
                                {
                                    StartX = X,
                                    StartY = Y,
                                    DestroyX = NextField.X,
                                    DestroyY = NextField.Y,
                                    TargetX = TargetField.X,
                                    TargetY = TargetField.Y,
                                    Isvalid = true,
                                    Score = State == State.Black && TargetField.Y == 8 ? 2 : 1
                                }) ;
                            }
                        }
                    }
                    if (X > 2)
                    {
                        Field NextField = board.GetField(X - 1, Y + 1);
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board.GetField(X - 2, Y + 2);
                            if (TargetField.GetColor() == Color.None)
                            {
                                moves.Add(new AiMove()
                                {
                                    StartX = X,
                                    StartY = Y,
                                    DestroyX = NextField.X,
                                    DestroyY = NextField.Y,
                                    TargetX = TargetField.X,
                                    TargetY = TargetField.Y,
                                    Isvalid = true,
                                    Score = State == State.Black && TargetField.Y == 8 ? 2 : 1
                                });
                            }
                        }
                    }
                }
            }
            if (State != State.Black)
            {
                if (Y > 2)
                {
                    if (X < 7)
                    {
                        Field NextField = board.GetField(X + 1, Y - 1);
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board.GetField(X + 2, Y - 2);
                            if (TargetField.GetColor() == Color.None)
                            {
                                moves.Add(new AiMove()
                                {
                                    StartX = X,
                                    StartY = Y,
                                    DestroyX = NextField.X,
                                    DestroyY = NextField.Y,
                                    TargetX = TargetField.X,
                                    TargetY = TargetField.Y,
                                    Isvalid = true,
                                    Score = State == State.White && TargetField.Y == 1 ? 2 : 1
                                });

                            }
                        }
                    }
                    if (X > 2)
                    {
                        Field NextField = board.GetField(X - 1, Y - 1);
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board.GetField(X - 2, Y - 2);
                            if (TargetField.GetColor() == Color.None)
                            {
                                moves.Add(new AiMove()
                                {
                                    StartX = X,
                                    StartY = Y,
                                    DestroyX = NextField.X,
                                    DestroyY = NextField.Y,
                                    TargetX = TargetField.X,
                                    TargetY = TargetField.Y,
                                    Isvalid = true,
                                    Score = State == State.White && TargetField.Y == 1 ? 2 : 1
                                });
                            }
                        }
                    }
                }
            }
            return moves;
        }

        public List<AiMove> GetMoves(BoardState board)
        {
            List<AiMove> moves = new List<AiMove>();
            if (State != State.White)
            {
                if (Y < 8)
                {
                    if (X < 8)
                    {
                        Field TargetField = board.GetField(X + 1, Y + 1);
                        if (TargetField.GetColor() == Color.None)
                        {
                            moves.Add(new AiMove()
                            {
                                StartX = X,
                                StartY = Y,
                                TargetX = TargetField.X,
                                TargetY = TargetField.Y,
                                Isvalid = true,
                                Score = State == State.Black && TargetField.Y == 8 ? 1 : 0
                            });
                        }
                    }
                    if (X > 1)
                    {
                        Field TargetField = board.GetField(X - 1, Y + 1);
                        if (TargetField.GetColor() == Color.None)
                        {
                            moves.Add(new AiMove()
                            {
                                StartX = X,
                                StartY = Y,
                                TargetX = TargetField.X,
                                TargetY = TargetField.Y,
                                Isvalid = true,
                                Score = State == State.Black && TargetField.Y == 8 ? 1 : 0
                            });
                        }
                    }
                }
            }
            if (State != State.Black)
            {
                if (Y > 1)
                {
                    if (X < 8)
                    {
                        Field TargetField = board.GetField(X + 1, Y - 1);
                        if (TargetField.GetColor() == Color.None)
                        {
                            moves.Add(new AiMove()
                            {
                                StartX = X,
                                StartY = Y,
                                TargetX = TargetField.X,
                                TargetY = TargetField.Y,
                                Isvalid = true,
                                Score = State == State.White && TargetField.Y == 1 ? 1 : 0
                            });
                        }
                    }
                    if (X > 1)
                    {
                        Field TargetField = board.GetField(X - 1, Y - 1);
                        if (TargetField.GetColor() == Color.None)
                        {
                            moves.Add(new AiMove()
                            {
                                StartX = X,
                                StartY = Y,
                                TargetX = TargetField.X,
                                TargetY = TargetField.Y,
                                Isvalid = true,
                                Score = State == State.White && TargetField.Y == 1 ? 1 : 0
                            });
                        }
                    }
                }
            }
            return moves;
        }
    }
}
