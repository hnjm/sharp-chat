using SharpChat.Bans;
using SharpChat.DataProvider;
using SharpChat.DataProvider.Misuzu;
using SharpChat.Users;
using SharpChat.Users.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using static System.Console;

namespace MisuzuDataProviderTest {
    public static class Program {
        public static void Main() {
            WriteLine("Misuzu Authentication Tester");

            WriteLine($@"Enter token found on {MisuzuConstants.BASE_URL}/login:");
            string[] token = Console.ReadLine().Split(new[] { '_' }, 2);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"SharpChat");

            IDataProvider dataProvider = new MisuzuDataProvider(httpClient);

            long userId = long.Parse(token[0]);
            IPAddress remoteAddr = IPAddress.Parse(@"1.2.4.8");

            IUserAuthResponse authRes;
            try {
                authRes = dataProvider.UserAuthClient.AttemptAuth(new UserAuthRequest(userId, token[1], remoteAddr));

                WriteLine(@"Auth success!");
                WriteLine($@" User ID:   {authRes.UserId}");
                WriteLine($@" Username:  {authRes.Username}");
                WriteLine($@" Colour:    {authRes.Colour.Raw:X8}");
                WriteLine($@" Hierarchy: {authRes.Rank}");
                WriteLine($@" Silenced:  {authRes.SilencedUntil}");
                WriteLine($@" Perms:     {authRes.Permissions}");
            } catch(UserAuthFailedException ex) {
                WriteLine($@"Auth failed: {ex.Message}");
                return;
            }

            WriteLine(@"Bumping last seen...");
            dataProvider.UserBumpClient.SubmitBumpUsers(new[] { new ChatUser(authRes) });

            WriteLine(@"Fetching ban list...");
            IEnumerable<IBanRecord> bans = dataProvider.BanClient.GetBanList();
            WriteLine($@"{bans.Count()} BANS");
            foreach(IBanRecord ban in bans) {
                WriteLine($@"BAN INFO");
                WriteLine($@" User ID:    {ban.UserId}");
                WriteLine($@" Username:   {ban.Username}");
                WriteLine($@" IP Address: {ban.UserIP}");
                WriteLine($@" Expires:    {ban.Expires}");
            }
        }
    }
}
