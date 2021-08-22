using System;
using System.IO;
using System.Linq;
using System.Text;
using IntegrationTests.Common;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

System.Console.OutputEncoding = Encoding.UTF8;
// change working directory to dll location
Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

var client = new TelegramClient(Secrets.AppApiId, Secrets.AppApiHash);
await client.ConnectAsync();

if (!client.IsUserAuthorized())
{
    var hash = await client.SendCodeRequestAsync(Secrets.UserPhoneNumber);
    System.Console.WriteLine("Please enter code from telegram.");
    var code = System.Console.ReadLine();

    await client.MakeAuthAsync(Secrets.UserPhoneNumber, hash, code);
}

System.Console.WriteLine("Show last user chats? (y/n)");
var key = System.Console.ReadKey();

if (key.KeyChar == 'y')
{
    var dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync();
    System.Console.WriteLine("Users:");
    foreach (var user in dialogs.Users.OfType<TLUser>())
    {
        System.Console.WriteLine(
            $"Name: {user.Username}({user.FirstName} {user.LastName}), "
            + $"Id: {user.Id}, AccessHash: {user.AccessHash}");
    }
}
