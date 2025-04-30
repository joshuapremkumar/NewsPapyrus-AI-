Project Name:
NewsPapyrus AI
Description:
NewsPapyrus AI is an interactive C# console application designed to assist users in quickly verifying the authenticity of claims and information. Leveraging the power of cutting-edge AI and search technologies, NewsPapyrus AI searches for relevant information online (with an initial focus on news sources via Google Programmable Search Engine), analyzes the findings, and reports on the verifiability of the user's input, providing supporting evidence. This tool aims to combat misinformation by making fact-checking accessible and efficient directly from the command line. It get’s sources from reputable news papers all over the globe.
Key Features:
•	Information Verification Engine: Takes user input (claims or statements) and processes it through an AI-powered verification workflow.
•	Web Search Integration: Utilizes Google Programmable Search Engine to search the web for information relevant to the user's query. Configurable to focus on specific sources like newspapers (potential for future refinement).
•	AI-Assisted Analysis: Employs Semantic Kernel and Azure OpenAI models to analyze the context and content of search results and determine the verifiability of the original information based only on the provided sources.
•	Source Reporting: Provides a clear summary of the search process, including the total number of links found and a list of the relevant URLs to review the evidence directly.
•	Interactive Console Interface: Offers a simple, continuous loop command-line interface for users to input multiple queries without restarting the application.
•	Secure Configuration: Manages sensitive API keys and endpoints securely using environment variables.
Use Cases:
•	Quick Fact-Checking: Rapidly verify news headlines, social media claims, or snippets of information found online.
•	Journalism and Research Support: Aid journalists and researchers in initial fact-gathering and source identification for claims.
•	Educational Tool: Help students verify information found for assignments and learn about source checking.
•	Combating Misinformation: Provide a tool for individuals to challenge questionable information with evidence from search results and AI analysis.
Technical Explanation:
NewsPapyrus AI is built as a C# .NET 8 Console Application. It orchestrates its functionality using the Microsoft Semantic Kernel SDK to connect to Azure OpenAI Service for large language model capabilities (like text analysis and summarization). It interacts with the Google Programmable Search Engine API to perform web searches, utilizing standard .NET HttpClient for API calls and JSON processing. The application configuration relies on environment variables for managing API keys and service endpoints securely, separating sensitive information from the codebase. The architecture is designed for clarity and focuses on demonstrating the core verification workflow in a straightforward command-line environment.
Conclusion:
NewsPapyrus AI is a practical demonstration of how AI and search technologies can be combined in a simple application to address the challenge of information verification. It provides users with an accessible way to get an AI-assisted assessment of claims based on web evidence, making it a valuable tool for enhancing trust in information in today's digital landscape.
