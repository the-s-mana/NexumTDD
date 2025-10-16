using SurrealDb.Net;
using SurrealDb.Net.Models.Auth;

namespace Nexum.Server.Data;

public sealed class SurrealDbProvider : ISurrealDbProvider, IDisposable
{
    public SurrealDbClient Client { get; }

    public SurrealDbProvider(IConfiguration configuration)
    {
        var settings = configuration.GetSection("SurrealDbSettings");
        var endpoint = settings["Endpoint"]!;
        var username = settings["Username"]!;
        var password = settings["Password"]!;
        var ns = settings["Namespace"]!;
        var db = settings["Database"]!;

        // สร้าง Client และเชื่อมต่อทันทีใน Constructor
        Client = new SurrealDbClient(endpoint);

        Console.WriteLine($"username={username}, password={password}");
        Console.WriteLine($"Namespace={ns}, Database={db}");
        Console.WriteLine($"endpoint={endpoint}");

        // ใช้ RootAuth สำหรับ user ระดับ root
        //Client.SignIn(new RootAuth
        //{
        //    Username = username,
        //    Password = password
        //}).Wait();

        Client.SignIn(
                new SurrealDb.Net.Models.Auth.DatabaseAuth
                {
                    Username = username,
                    Password = password,
                    Namespace = ns,
                    Database = db
                }).Wait();

        // หลังจาก SignIn สำเร็จแล้ว ค่อยใช้คำสั่ง Use เพื่อเลือก Namespace และ Database
        Client.Use(ns, db).Wait();
    }

    // ทำความสะอาดเมื่อแอปปิดตัวลง
    public void Dispose()
    {
        Client.Dispose();
    }
}