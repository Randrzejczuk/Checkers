using System.Collections.Generic;

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
        /*
         * Changes piece to queen piece
         */
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
        /*
         * Checks if selected piece can attack
         */
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
                        var field = board[X + 1, Y + 1];
                        if (State == State.White_Q)
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board[X + 2, Y + 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board[X + 2, Y + 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                    if (X > 2)
                    {
                        var field = board[X - 1, Y + 1];
                        if (State == State.White_Q)
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board[X - 2, Y + 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board[X - 2, Y + 2];
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
                        var field = board[X + 1, Y - 1];
                        if (State == State.Black_Q)
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board[X + 2, Y - 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board[X + 2, Y - 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                    if (X > 2)
                    {
                        var field = board[X - 1, Y - 1];
                        if (State == State.Black_Q)
                        {
                            if (field.GetColor() == Color.White)
                            {
                                field = board[X - 2, Y - 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                        else
                        {
                            if (field.GetColor() == Color.Black)
                            {
                                field = board[X - 2, Y - 2];
                                if (field.State == State.Empty)
                                    return true;
                            }
                        }
                    }
                }
            }
            return false; 
        }
        /*
         * Checks if selected piece can move
         */
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
                        Field field = board[X - 1, Y + 1];
                        if (field.State == State.Empty)
                            return true;
                    }
                    if (X != 8)
                    {
                        Field field = board[X + 1, Y + 1];
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
                        Field field = board[X - 1, Y - 1];
                        if (field.State == State.Empty)
                            return true;
                    }
                    if (X != 8)
                    {
                        Field field = board[X + 1, Y - 1];
                        if (field.State == State.Empty)
                            return true;
                    }
                }
            }
            return CanAttack(board);
        }
        /*
         * Returns color of selected piece
         */
        public Color GetColor()
        {
            if (State == State.White || State == State.White_Q)
                return Color.White;
            if (State == State.Black || State == State.Black_Q)
                return Color.Black;
            return Color.None;
        }
        /*
         * Returns list of avaiable attacks for selected piece
         */
        public List<AiMove> GetAttacks(BoardState board)
        {
            List<AiMove> moves = new List<AiMove>();
            if (State != State.White)
            {
                if (Y < 7)
                {
                    if (X < 7)
                    {
                        Field NextField = board[X + 1, Y + 1];
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board[X + 2, Y + 2];
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
                        Field NextField = board[X - 1, Y + 1];
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board[X - 2, Y + 2];
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
                        Field NextField = board[X + 1, Y - 1];
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board[X + 2, Y - 2];
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
                        Field NextField = board[X - 1, Y - 1];
                        if (NextField.GetColor() != Color.None && NextField.GetColor() != this.GetColor())
                        {
                            Field TargetField = board[X - 2, Y - 2];
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
        /*
         * Returns list of avaiable moves for selected piece
         */
        public List<AiMove> GetMoves(BoardState board)
        {
            List<AiMove> moves = new List<AiMove>();
            if (State != State.White)
            {
                if (Y < 8)
                {
                    if (X < 8)
                    {
                        Field TargetField = board[X + 1, Y + 1];
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
                        Field TargetField = board[X - 1, Y + 1];
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
                        Field TargetField = board[X + 1, Y - 1];
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
                        Field TargetField = board[X - 1, Y - 1];
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
