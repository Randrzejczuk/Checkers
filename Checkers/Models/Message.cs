using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime Posted { get; set; }
        public string UserId { get; set; }
        public User User { get; set; } 
        public int? RoomId { get; set; }
        public Room Room { get; set; }
        /*
         * Returns message with posted time and username
         */
        public string MessageToDisplay()
        {
            return $"[{Posted.ToShortTimeString()}] {UserName.Split('@')[0]}: {Text}";
        }
    }
}
