using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Room
    {
        public int Id { get; set; }
        [Display(Name = "Room name")]
        [Required(ErrorMessage = "Room name is required.")]
        public string Name { get; set; }
        public BoardState Board { get; set; }
        [Display(Name = "Player 1")]
        public User User1 { get; set; }
        public string User1Id { get; set; }
        public bool User1IsActive { get; set; }
        [Display(Name = "Player 2")]
        public TimeSpan User1Time { get; set; }
        public User User2 { get; set; }
        public string User2Id { get; set; }
        public bool User2IsActive { get; set; }
        public TimeSpan User2Time { get; set; }
        public bool ActiveUser { get; set; }
        public bool IsActive { get; set; }

        public string ValidatePlayer(string userId, Move move)
        {
            Field field = Board.GetField(move.startX, move.startY);
            if (userId != User1Id && userId != User2Id)
                return "You are not a player in this game.";
            else if (!IsActive)
                return "The game hasn't started yet.";
            else if ((userId == User1Id && ActiveUser) || (userId == User2Id && !ActiveUser))
                return "Wait for your turn.";
            else if ((field.GetColor() == Color.White && userId == User2Id) || (field.GetColor() == Color.Black && userId == User1Id))
                return "This is enemy piece.";
            return "";
        }
    }
}
