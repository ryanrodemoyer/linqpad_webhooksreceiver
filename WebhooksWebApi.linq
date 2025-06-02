<Query Kind="Statements">
  <Namespace>Microsoft.AspNetCore.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

public class WebhooksWebApi
{
	public readonly DumpContainer _dc;

	public WebhooksWebApi(DumpContainer dc)
	{
		_dc = dc;
	}

	public IResult Get()
	{
		var res = Results.Content("<name>rodey</name>", "text/html");
		return res;
	}

	public async Task<IResult> Post(HttpRequest req)
	{
		using var sr = new StreamReader(req.Body);
		string body = await sr.ReadToEndAsync();

		_dc.AppendContent(new
		{
			body,
			headers = req.Headers.Select(x => new { name = x.Key, value = x.Value.First() }).ToList(),
			replay = new LINQPad.Controls.Button("replay", (btn) =>
			{
				// Hey, great exercise for the reader. Can you implement the replay button?
			})
		});

		var c = Results.Content("<response>it's working!</response>", "text/html");
		return c;
	}
}