// Using top-level statements for simplicity in this console app
using Microsoft.SemanticKernel;
using Azure.AI.OpenAI; // Needed for OpenAIClient types
using System.Net.Http; // Needed for HttpClient
using AIAgentConsole.Services; // Needed for GoogleCSEService
using System; // Needed for Environment, Console, StringComparison
using System.Collections.Generic; // Needed for List<string>
using System.Linq; // Needed for .Take(), .Where() etc.
using System.Threading.Tasks; // Needed for Task

Console.WriteLine("Setting up AI agent...");

// 1. Retrieve credentials from environment variables
var azureOpenAiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var azureOpenAiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var azureOpenAiDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME");
var googleCseApiKey = Environment.GetEnvironmentVariable("GOOGLE_CSE_API_KEY");
var googleCseId = Environment.GetEnvironmentVariable("GOOGLE_CSE_ID");

// More specific environment variable checks
var configErrors = new List<string>();
if (string.IsNullOrEmpty(azureOpenAiEndpoint)) configErrors.Add("AZURE_OPENAI_ENDPOINT");
if (string.IsNullOrEmpty(azureOpenAiApiKey)) configErrors.Add("AZURE_OPENAI_API_KEY");
if (string.IsNullOrEmpty(azureOpenAiDeploymentName)) configErrors.Add("AZURE_OPENAI_DEPLOYMENT_NAME");
if (string.IsNullOrEmpty(googleCseApiKey)) configErrors.Add("GOOGLE_CSE_API_KEY");
if (string.IsNullOrEmpty(googleCseId)) configErrors.Add("GOOGLE_CSE_ID");

if (configErrors.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: The following required environment variables are not set:");
    foreach (var varName in configErrors)
    {
        Console.WriteLine($"- {varName}");
    }
    Console.WriteLine("\nPlease set these environment variables on your system and restart the terminal.");
    Console.ResetColor();
    return; // Exit the application
}

// 2. Build the Semantic Kernel
var builder = Kernel.CreateBuilder();

// Use Azure OpenAI text generation service
builder.AddAzureOpenAIChatCompletion(
    deploymentName: azureOpenAiDeploymentName!, // Add ! here
    endpoint: azureOpenAiEndpoint!, // Add ! here
    apiKey: azureOpenAiApiKey!); // Add ! here

var kernel = builder.Build();
Console.WriteLine("Kernel initialized.");

// 3. Setup Google CSE Service with a single HttpClient instance
// HttpClient should be managed for its lifetime, disposing when the app exits.
// In a simple console app using top-level statements, 'using var' at this scope works.
using var httpClient = new HttpClient();
var googleCseService = new GoogleCSEService(httpClient, googleCseApiKey!, googleCseId!); // Add ! here and here

Console.WriteLine("Google CSE Service initialized.\n");

Console.WriteLine("AI Agent is ready. Type 'exit' to quit.");

// 4. Main application loop for continuous search
while (true)
{
    Console.WriteLine("\nEnter the information you want to verify:");
    var userInput = Console.ReadLine();

    // Check for exit command or empty input
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Exiting AI Agent. Goodbye!");
        break; // Exit the loop
    }

    Console.WriteLine($"\nVerifying: \"{userInput}\"");

    // 5. Perform Search
    List<string>? searchLinks = null; // Initialize as null, now declared as nullable
    try
    {
         searchLinks = await googleCseService.SearchAsync(userInput);
    }
    catch (Exception ex) // Catch potential exceptions from the search service
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"An error occurred during search: {ex.Message}");
        Console.ResetColor();
        // Continue the loop to get new input
        Console.WriteLine("\nSearch and Analysis Complete.");
        Console.WriteLine("Enter 'exit' to quit the application, or enter new information to verify:"); // Added prompt
        continue;
    }


    // Check if search returned results or was null due to error within the service (though service returns empty list on error)
    if (searchLinks == null || searchLinks.Count == 0) // .Count is safe after null check
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        // Check if searchLinks was explicitly null (indicating potential error not caught in service)
        if (searchLinks == null)
        {
             Console.WriteLine("\nSearch returned null. Check Google CSE API key/ID and network.");
        }
        else // searchLinks was not null but empty
        {
            Console.WriteLine("\nSearch completed. No relevant search results were found.");
        }
        Console.ResetColor();
        // Continue the loop to get new input
         Console.WriteLine("\nSearch and Analysis Complete.");
         Console.WriteLine("Enter 'exit' to quit the application, or enter new information to verify:"); // Added prompt
         continue;
    }
    else // Search returned results
    {
         // 6. Prepare Prompt for AI with Search Results
        var prompt = $@"Analyze the following information based *only* on the provided web search results (URLs). Do not use any prior knowledge.
Information to verify: ""{userInput}""
Web Search Results (URLs):
{string.Join("\n- ", searchLinks.Take(10))}

Based *only* on these sources, state whether the information appears verifiable or not. Provide a brief summary of what the sources indicate.
If the sources contradict each other or the information, state that it is not clearly verifiable based on the provided sources.
If no sources directly support the information, state that it is not verifiable based on the provided links.
Your response should ideally start with ""Verifiable: Yes"" or ""Verifiable: No"", followed by your summary.
";

        Console.WriteLine("\nSending information and links to AI for analysis...");

        // 7. Invoke AI Kernel
        string? result = null; // Initialize result as null, now declared as nullable
        try
        {
            result = (await kernel.InvokePromptAsync(prompt))?.GetValue<string>(); // Fix for CS0029
        }
         catch (Exception ex) // Catch potential exceptions during AI invocation
        {
             Console.ForegroundColor = ConsoleColor.Red;
             Console.WriteLine($"An error occurred during AI invocation: {ex.Message}");
             Console.ResetColor();
              // Continue the loop to get new input
             Console.WriteLine("\nSearch and Analysis Complete.");
             Console.WriteLine("Enter 'exit' to quit the application, or enter new information to verify:"); // Added prompt
             continue;
        }


        Console.WriteLine("\n--- AI Verification Result ---");
        // Check if AI result is null or empty
        if (string.IsNullOrWhiteSpace(result))
        {
             Console.ForegroundColor = ConsoleColor.Yellow;
             Console.WriteLine("AI analysis did not return a specific result.");
             Console.ResetColor();
        }
        else
        {
             Console.WriteLine(result);
        }
        Console.WriteLine("------------------------------");

        // 8. Display Links Found with More Detail
        Console.WriteLine($"\nSearch Results Summary:");
        Console.WriteLine($"- Found {searchLinks.Count} links (out of up to 10 requested)."); // searchLinks is guaranteed non-null and > 0 here
        Console.WriteLine("- Links found:");

         foreach (var link in searchLinks) // searchLinks is guaranteed non-null and > 0 here
         {
             Console.WriteLine($"  - {link}"); // Use 2 spaces for indentation
         }
         Console.WriteLine("------------------------------");
    }

     Console.WriteLine("\nSearch and Analysis Complete.");
     // Added prompt after successful search and analysis
     Console.WriteLine("Enter 'exit' to quit the application, or enter new information to verify:");
} // End of while loop

Console.WriteLine("\nAI Agent shutting down.");

// The 'using var httpClient' ensures HttpClient is disposed when the application exits.