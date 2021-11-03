using Checkers.Data;
using Checkers.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Checkers.Timers
{
    public class CustomTimer : Timer
    {
        public CustomTimer(double interval)
        : base(interval)
        {
        }
        public Room room;
        public HubCallerContext callerContext { get; set; }
        public IHubCallerClients<IClientProxy> hubCallerClients { get; set; }
        public ApplicationDbContext context { get; set; }
    }
}
