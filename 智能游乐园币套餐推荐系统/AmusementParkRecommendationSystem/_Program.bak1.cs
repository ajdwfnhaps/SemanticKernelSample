//using OpenTelemetry;
//using OpenTelemetry.Trace;

//var tracerProvider = Sdk.CreateTracerProviderBuilder()
//    .AddSource("TestSource")
//    .AddConsoleExporter()
//    .Build();

//using var activitySource = new System.Diagnostics.ActivitySource("TestSource");
//using (var activity = activitySource.StartActivity("TestActivity"))
//{
//    activity?.SetTag("foo", "bar");
//    Console.WriteLine("Hello, OpenTelemetry!");
//}

//Console.WriteLine("按任意键退出...");
//Console.ReadKey();