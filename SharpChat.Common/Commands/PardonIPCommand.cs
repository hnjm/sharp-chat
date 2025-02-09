﻿using SharpChat.DataProvider;
using SharpChat.Packets;
using SharpChat.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SharpChat.Commands {
    public class PardonIPCommand : ICommand {
        private IDataProvider DataProvider { get; }
        private IUser Sender { get; }

        public PardonIPCommand(IDataProvider dataProvider, IUser sender) {
            DataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        public bool IsCommandMatch(string name, IEnumerable<string> args)
            => name == @"pardonip" || name == @"unbanip";

        public bool DispatchCommand(ICommandContext ctx) {
            if(!ctx.User.Can(UserPermissions.BanUser | UserPermissions.KickUser))
                throw new CommandNotAllowedException(ctx.Args);

            string ipAddress = ctx.Args.ElementAtOrDefault(1);
            if(!IPAddress.TryParse(ipAddress, out IPAddress ipAddr))
                throw new NotBannedCommandException(ipAddr?.ToString() ?? @"::");

            DataProvider.BanClient.RemoveBan(ipAddr, success => {
                if(success)
                    ctx.Session.SendPacket(new PardonResponsePacket(Sender, ipAddr));
                else
                    ctx.Session.SendPacket(new NotBannedCommandException(ipAddr.ToString()).ToPacket(Sender));
            }, ex => ctx.Session.SendPacket(new CommandGenericException().ToPacket(Sender)));

            return true;
        }
    }
}
