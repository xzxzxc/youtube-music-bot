using System;
using System.IO;
using System.Linq;
using System.Text;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Exceptions;
using YoutubeMusicBot.IntegrationTests.Common;

Console.OutputEncoding = Encoding.UTF8;
// change working directory to dll location
Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

if (args.Length > 0)
{
    if (args[0] == "-u")
    {
        File.Delete("session.dat");
        return;
    }
}

var client = new TelegramClient(Secrets.AppApiId, Secrets.AppApiHash);
await client.ConnectAsync();

if (!client.IsUserAuthorized())
{
    var hash = await client.SendCodeRequestAsync(Secrets.UserPhoneNumber);
    Console.WriteLine("Please enter code from telegram.");
    var code = Console.ReadLine();

    try
    {
        await client.MakeAuthAsync(Secrets.UserPhoneNumber, hash, code);
    }
    catch (CloudPasswordNeededException)
    {
        var passwordSettings = await client.GetPasswordSetting();
        Console.WriteLine("You have two factor verification enabled. Please enter your password.");
        var password = Console.ReadLine();

        await client.MakeAuthWithPasswordAsync(passwordSettings, password);
    }
}

Console.WriteLine("Show last user chats? (y/n)");
var answer = Console.ReadLine();

if (answer != null && answer.ToLower().FirstOrDefault() == 'y')
{
    var dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync();
    Console.WriteLine("Users:");
    foreach (var user in dialogs.Users.OfType<TLUser>())
    {
        Console.WriteLine(
            $"Name: {user.Username}({user.FirstName} {user.LastName}), "
            + $"Id: {user.Id}, AccessHash: {user.AccessHash}");
    }
}
