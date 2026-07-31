using CShrimpSharp;
using CShrimpSharp.Collections;
using CShrimpSharp.Concurrency;

Result<int,Failure> parsed=Result.Try(()=>int.Parse("21")).Map(x=>x*2);
parsed.Switch(Console.WriteLine,Console.WriteLine);
Console.WriteLine(new[] { "a", "b" }.AtOrNone(1).GetValueOr("missing"));
var pair=await Shrimp.SyncAsync(async ct=>{await Task.Delay(10,ct);return "profile";},async ct=>{await Task.Delay(5,ct);return 42;});
Console.WriteLine($"{pair.Item1}: {pair.Item2}");
