1. To run the application install the latest .NET SDK and Visual Solution. Open the solution in Visual Studio, build the solution and run Santander.Api project.
2. Caching, resilience, fault tolerance, low-latency optimizations, and overload protection strategies are all configurable via appsettings.json.
3. The one thing that was not clear in the task was if the ordering was supposed to be applied before taking n record or after so I assumed that we wanted to order first and then take n records.
4. We could implement more logging and potentially more specific exception handling.
5. We could also add some functional and integration tests for Santander.API and unit tests for GetBestStoriesQueryHandler