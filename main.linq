<Query Kind="Program">
  <Namespace>Microsoft.AspNetCore.Builder</Namespace>
  <Namespace>Microsoft.AspNetCore.Http</Namespace>
  <Namespace>Microsoft.AspNetCore.HttpOverrides</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>System.ComponentModel</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

#load "./DumpContainerLogger"
#load "./WebhooksWebApi"

// TODO proper resource cleanup on the background workers, especially the asp.net core worker
// for now, use the LINQPad functionality to forcibly cleanup/exit a script
// macOS: Option + Command + .
// Windows: Ctl+Shift+F5

// these are the various spots where we'll show data on the screen
// keeps us organized
DumpContainer dcPinggy = new DumpContainer();
DumpContainer dcApi = new DumpContainer();
DumpContainer dcReceiver = new DumpContainer();
DumpContainer dcReceiverResponse = new DumpContainer();

// when pinggy has started this variable will get set with their https address
// it's a dynamic address that changes at each startup
string pinggyHttps = "http://localhost:5000";

async Task Main()
{
	// output the controls used to generate the webhook
	// customize the data we send, etc.
	BuildWebhooksSenderGui();

	// this little setup organizes our display when running the script
	new List<dynamic> {
		new {pinggy = dcPinggy, webapi = dcApi, receiver = dcReceiver}
	  }.Dump("output");

	// launch our background worker which is creating our connection to pinggy
	// comment out these lines if you want to skip Pinggy.
	// script still works but requests will just go straight to localhost:5000
	BackgroundWorker bw1 = new BackgroundWorker();
	bw1.DoWork += bw_PinggyStart;
	bw1.RunWorkerAsync();

	// launch our background worker to host the asp.net core web api
	BackgroundWorker bw2 = new BackgroundWorker();
	bw2.DoWork += bw_WebApiStart;
	bw2.RunWorkerAsync();
}

// when called, this will open the system default web browser to the web address we pass in
void StartWebBrowser(string uri) =>
	Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });

void BuildWebhooksSenderGui()
{
	string GetInitialValue()
	{
		var obj = new
		{
			name = "rodey"
		};
		string json = System.Text.Json.JsonSerializer.Serialize(obj);
		return json;
	}

	var txtWebhookContent = new LINQPad.Controls.TextArea(GetInitialValue());
	var btnSendWebhook = new LINQPad.Controls.Button("send webhook", async (btn) =>
		{
			using var http = new HttpClient();
			http.BaseAddress = new Uri(pinggyHttps);
			
			var res = await http.PostAsync("23f406a4f37243a68230a12a938c5f9d", new StringContent(txtWebhookContent.Text, new MediaTypeHeaderValue("application/json")));

			dcReceiverResponse.Content = new { 
				res.StatusCode, 
				headers = res.Headers
					.Select(x => new { name = x.Key, value = x.Value.First() })
					.ToList(), 
				content = res.Content.ReadAsStringAsync().Result 
			};
		});

	new { payload = txtWebhookContent, btn = btnSendWebhook, response = dcReceiverResponse }.Dump("webhook command center");
}

private void bw_WebApiStart(object sender, DoWorkEventArgs e)
{
	var api = new WebhooksWebApi(dcReceiver);

	var builder = WebApplication.CreateBuilder();

	// setup header forwarding to get the X-Forwarded headers
	// pinggy is running as a reverse proxy so it'll obscure proto, host, ip
	// UNLESS we setup the header forwarding
	builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			// shocking that after all these years, this line is still required
			// yet completely missing from Microsoft documentation
			// this line is required to get the forwarded headers
			options.KnownProxies.Clear();

			options.ForwardedHeaders =
				ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
		});

	// establish a custom logging provider so that we can direct the logs to our specific DumpContainer object
	// this keeps the UI looking somewhat reasonable and organized
	builder.Logging.ClearProviders();
	builder.Logging.AddDumpContainer(dcApi);

	var app = builder.Build();
	app.UseForwardedHeaders();

	app.Map("/", api.Get);
	app.MapPost($"/23f406a4f37243a68230a12a938c5f9d", api.Post);

	app.Run();
}

private void bw_PinggyStart(object sender, DoWorkEventArgs e)
{
	void StartProcessPinggy()
	{
		var startInfo = new ProcessStartInfo
		{
			FileName = "ssh", // Replace with the command you want to run
			Arguments = "-p 443 -R0:localhost:5000 qr@free.pinggy.io", // Replace with any arguments for the command
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		// Create the process
		using (var process = new Process { StartInfo = startInfo })
		{
			// Set up the event handler to capture the output
			process.OutputDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					if (e.Data.StartsWith("https://"))
					{
						pinggyHttps = e.Data;
					}

					dcPinggy.AppendContent(e.Data);
				}
			};
			process.ErrorDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					dcPinggy.AppendContent(e.Data);
				}
			};

			// Start the process
			process.Start();

			// Begin asynchronous reading of the standard output
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			// Wait for the process to exit
			process.WaitForExit();
		}

	}
	
	// open a tunnel using the freemium service pinggy.io
	// this creates a link between our local port 5000 and their public internet endpoint

	//	var res = OperatingSystem.IsWindows()
	//		? Util.Cmd("ssh -p 443 -R0:localhost:5000 qr@free.pinggy.io", streamResults: true)
	//		: Util.Zsh("ssh -p 443 -R0:localhost:5000 qr@free.pinggy.io", quiet: true, streamResults: false);



	StartProcessPinggy();
	//	foreach (var msg in res)
	//	{
	//		dcPinggy.AppendContent(msg);

	//		// we get the messages coming from the terminal
	//		// look for the https:// endpoint that's opened from the pinggy tunnel
	//		if (msg.StartsWith("https://") && msg.EndsWith(".link"))
	//		{
	//			// open a web browser on our machine to the public https pinggy endpoint
	//#if MACOS
	//			StartWebBrowser(msg);
	//#else
	//			Util.Cmd($"explorer {msg}");
	//#endif
	//		}
	//	}
}

