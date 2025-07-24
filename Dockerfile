# Windows Docker container for New Horizons performance testing
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019

# Set working directory
WORKDIR C:\app

# Copy project files
COPY . .

# Install necessary tools
RUN powershell -Command \
    "Set-ExecutionPolicy Bypass -Scope Process -Force; \
     [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; \
     iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))"

RUN choco install -y git nuget.commandline

# Restore NuGet packages
RUN nuget restore NewHorizons.sln

# Build the project
RUN msbuild NewHorizons.sln /p:Configuration=Release /p:Platform="Any CPU"

# Create performance test runner
RUN echo 'using System; \
using System.IO; \
using System.Reflection; \
using NewHorizons.Utility; \
\
class Program \
{ \
    static void Main() \
    { \
        Console.WriteLine("=== NEW HORIZONS PERFORMANCE TEST ==="); \
        \
        try \
        { \
            // Simulate Unity Application.persistentDataPath \
            var testPath = Path.Combine(Environment.CurrentDirectory, "TestData"); \
            Directory.CreateDirectory(testPath); \
            \
            // Load NewHorizons assembly \
            var assembly = Assembly.LoadFrom("NewHorizons\\bin\\Release\\NewHorizons.dll"); \
            \
            // Run performance benchmarks \
            var benchmarkType = assembly.GetType("NewHorizons.Utility.PerformanceBenchmark"); \
            var runMethod = benchmarkType.GetMethod("RunCompleteBenchmark"); \
            \
            Console.WriteLine("Starting performance benchmark..."); \
            runMethod.Invoke(null, null); \
            \
            Console.WriteLine("Performance test completed successfully!"); \
        } \
        catch (Exception ex) \
        { \
            Console.WriteLine($"Error: {ex.Message}"); \
            Console.WriteLine($"Stack: {ex.StackTrace}"); \
        } \
    } \
}' > PerformanceTestRunner.cs

# Compile test runner
RUN csc /target:exe /out:PerformanceTestRunner.exe /reference:"NewHorizons\bin\Release\NewHorizons.dll" PerformanceTestRunner.cs

# Set entry point
CMD ["PerformanceTestRunner.exe"]