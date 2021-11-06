using Checkers.Data;
using Checkers.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Checkers.Models
{
    public class CustomTimer : Timer
    {
        public CustomTimer(double interval)
        : base(interval)
        {
        }
        public Room room;
        public HubCallerContext CallerContext { get; set; }
        public IHubCallerClients<IClientProxy> HubCallerClients { get; set; }
        public ApplicationDbContext Context { get; set; }
    }
}
