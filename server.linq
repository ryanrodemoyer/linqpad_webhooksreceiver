<Query Kind="Program">
  <Namespace>Microsoft.AspNetCore.Builder</Namespace>
  <Namespace>Microsoft.AspNetCore.Http</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

void Main()
{
	string html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>React Functional Components with CDN</title>
    <script src=""https://cdn.tailwindcss.com""></script>
</head>
<body class=""min-h-screen bg-gray-100 flex items-center justify-center p-4"">
    <div id=""root""></div>

    <!-- Load React and ReactDOM from a CDN -->
    <script src=""https://unpkg.com/react@17/umd/react.production.min.js""></script>
    <script src=""https://unpkg.com/react-dom@17/umd/react-dom.production.min.js""></script>

    <!-- Babel for JSX support -->
    <script src=""https://unpkg.com/@babel/standalone/babel.min.js""></script>

    <script type=""text/babel"">
        function App() {
            const [count, setCount] = React.useState(0);

            return (
                <div className=""max-w-md mx-auto bg-white rounded-xl shadow-lg p-6 md:p-8"">
                    <h1 className=""text-2xl font-bold text-gray-800 mb-4 text-center"">Counter App</h1>
                    <div className=""flex items-center justify-center mb-6"">
                        <p className=""text-lg text-gray-600 mr-2"">Count:</p>
                        <span className=""text-3xl font-semibold text-indigo-600"">{count}</span>
                    </div>
                    <div className=""flex justify-center space-x-4"">
                        <button 
                            onClick={() => setCount(count + 1)}
                            className=""px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2""
                        >
                            Increment
                        </button>
                        <button 
                            onClick={() => setCount(count - 1)}
                            className=""px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2""
                        >
                            Decrement
                        </button>
                        <button 
                            onClick={() => setCount(0)}
                            className=""px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2""
                        >
                            Reset
                        </button>
                    </div>
                </div>
            );
        }

        ReactDOM.render(<App />, document.getElementById('root'));
    </script>
</body>
</html>
	";

	string guid = Guid.NewGuid().ToString().Replace("-", "").ToLower().Dump();

	var builder = WebApplication.CreateBuilder();
	var app = builder.Build();

	app.Map("/", () => Results.Content(html, "text/html"));

	app.MapPost($"/23f406a4f37243a68230a12a938c5f9d", async (HttpRequest req) =>
	{
		try
		{
			using var sr = new StreamReader(req.Body);
			string body = await sr.ReadToEndAsync();
			Console.WriteLine("received!");
			Console.WriteLine(body);

			var c = Results.Content("ok", "text/html");
			return c;
		}
		catch (Exception ex)
		{
			ex.Dump("exception");
		}
	
		return Results.Content("<name>rodey</name>", "text/html");
	});

	app.Run();
}