using Microsoft.Playwright;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace Visage.Tests.Visual.Infrastructure;

/// <summary>
/// Provides visual comparison capabilities for UI testing using Playwright MCP.
/// This class handles screenshot capture, comparison, and reporting of visual differences.
/// </summary>
public class VisualTestingEngine
{
    private readonly string _baselineDirectory;
    private readonly string _actualDirectory;
    private readonly string _diffDirectory;
    private readonly double _threshold;

    public VisualTestingEngine(string baselineDirectory = "test-results/visual-baselines", 
                              string actualDirectory = "test-results/visual-actual", 
                              string diffDirectory = "test-results/visual-diffs",
                              double threshold = 0.2)
    {
        _baselineDirectory = baselineDirectory;
        _actualDirectory = actualDirectory;
        _diffDirectory = diffDirectory;
        _threshold = threshold;

        // Ensure directories exist
        Directory.CreateDirectory(_baselineDirectory);
        Directory.CreateDirectory(_actualDirectory);
        Directory.CreateDirectory(_diffDirectory);
    }

    /// <summary>
    /// Captures a screenshot and compares it with the baseline image.
    /// If no baseline exists, the current screenshot becomes the baseline.
    /// </summary>
    public async Task<VisualTestResult> CompareScreenshotAsync(IPage page, string testName, PageScreenshotOptions? options = null)
    {
        var baselinePath = Path.Combine(_baselineDirectory, $"{testName}.png");
        var actualPath = Path.Combine(_actualDirectory, $"{testName}.png");
        var diffPath = Path.Combine(_diffDirectory, $"{testName}.png");

        // Capture current screenshot
        var screenshotBytes = await page.ScreenshotAsync(options ?? new PageScreenshotOptions 
        { 
            FullPage = true,
            Type = ScreenshotType.Png
        });
        
        await File.WriteAllBytesAsync(actualPath, screenshotBytes);

        // If no baseline exists, create it
        if (!File.Exists(baselinePath))
        {
            await File.WriteAllBytesAsync(baselinePath, screenshotBytes);
            return new VisualTestResult
            {
                TestName = testName,
                Status = VisualTestStatus.BaselineCreated,
                BaselinePath = baselinePath,
                ActualPath = actualPath,
                Message = "Baseline image created"
            };
        }

        // Compare images
        var comparisonResult = await CompareImagesAsync(baselinePath, actualPath, diffPath);
        
        return new VisualTestResult
        {
            TestName = testName,
            Status = comparisonResult.DifferencePercentage <= _threshold ? VisualTestStatus.Passed : VisualTestStatus.Failed,
            BaselinePath = baselinePath,
            ActualPath = actualPath,
            DiffPath = comparisonResult.DifferencePercentage > _threshold ? diffPath : null,
            DifferencePercentage = comparisonResult.DifferencePercentage,
            Message = comparisonResult.DifferencePercentage <= _threshold 
                ? "Visual test passed" 
                : $"Visual differences detected: {comparisonResult.DifferencePercentage:F2}% difference"
        };
    }

    /// <summary>
    /// Captures a screenshot of a specific element and compares it with baseline.
    /// </summary>
    public async Task<VisualTestResult> CompareElementScreenshotAsync(ILocator element, string testName, LocatorScreenshotOptions? options = null)
    {
        var baselinePath = Path.Combine(_baselineDirectory, $"{testName}.png");
        var actualPath = Path.Combine(_actualDirectory, $"{testName}.png");
        var diffPath = Path.Combine(_diffDirectory, $"{testName}.png");

        // Capture current screenshot
        var screenshotBytes = await element.ScreenshotAsync(options ?? new LocatorScreenshotOptions
        {
            Type = ScreenshotType.Png
        });
        
        await File.WriteAllBytesAsync(actualPath, screenshotBytes);

        // If no baseline exists, create it
        if (!File.Exists(baselinePath))
        {
            await File.WriteAllBytesAsync(baselinePath, screenshotBytes);
            return new VisualTestResult
            {
                TestName = testName,
                Status = VisualTestStatus.BaselineCreated,
                BaselinePath = baselinePath,
                ActualPath = actualPath,
                Message = "Baseline image created"
            };
        }

        // Compare images
        var comparisonResult = await CompareImagesAsync(baselinePath, actualPath, diffPath);
        
        return new VisualTestResult
        {
            TestName = testName,
            Status = comparisonResult.DifferencePercentage <= _threshold ? VisualTestStatus.Passed : VisualTestStatus.Failed,
            BaselinePath = baselinePath,
            ActualPath = actualPath,
            DiffPath = comparisonResult.DifferencePercentage > _threshold ? diffPath : null,
            DifferencePercentage = comparisonResult.DifferencePercentage,
            Message = comparisonResult.DifferencePercentage <= _threshold 
                ? "Visual test passed" 
                : $"Visual differences detected: {comparisonResult.DifferencePercentage:F2}% difference"
        };
    }

    private async Task<ImageComparisonResult> CompareImagesAsync(string baselinePath, string actualPath, string diffPath)
    {
        using var baselineImage = await Image.LoadAsync<Rgba32>(baselinePath);
        using var actualImage = await Image.LoadAsync<Rgba32>(actualPath);

        if (baselineImage.Width != actualImage.Width || baselineImage.Height != actualImage.Height)
        {
            return new ImageComparisonResult
            {
                DifferencePercentage = 100.0,
                Message = "Images have different dimensions"
            };
        }

        var totalPixels = baselineImage.Width * baselineImage.Height;
        var differentPixels = 0;

        using var diffImage = new Image<Rgba32>(baselineImage.Width, baselineImage.Height);

        for (int y = 0; y < baselineImage.Height; y++)
        {
            for (int x = 0; x < baselineImage.Width; x++)
            {
                var baselinePixel = baselineImage[x, y];
                var actualPixel = actualImage[x, y];

                var colorDifference = Math.Abs(baselinePixel.R - actualPixel.R) +
                                    Math.Abs(baselinePixel.G - actualPixel.G) +
                                    Math.Abs(baselinePixel.B - actualPixel.B);

                if (colorDifference > 30) // Threshold for pixel difference
                {
                    differentPixels++;
                    diffImage[x, y] = new Rgba32(255, 0, 0, 255); // Red for differences
                }
                else
                {
                    diffImage[x, y] = new Rgba32(baselinePixel.R, baselinePixel.G, baselinePixel.B, 100); // Semi-transparent original
                }
            }
        }

        var differencePercentage = (double)differentPixels / totalPixels * 100.0;

        if (differencePercentage > _threshold)
        {
            await diffImage.SaveAsPngAsync(diffPath);
        }

        return new ImageComparisonResult
        {
            DifferencePercentage = differencePercentage,
            DifferentPixels = differentPixels,
            TotalPixels = totalPixels
        };
    }
}

public class VisualTestResult
{
    public required string TestName { get; set; }
    public VisualTestStatus Status { get; set; }
    public string? BaselinePath { get; set; }
    public string? ActualPath { get; set; }
    public string? DiffPath { get; set; }
    public double DifferencePercentage { get; set; }
    public string? Message { get; set; }
}

public enum VisualTestStatus
{
    Passed,
    Failed,
    BaselineCreated
}

public class ImageComparisonResult
{
    public double DifferencePercentage { get; set; }
    public int DifferentPixels { get; set; }
    public int TotalPixels { get; set; }
    public string? Message { get; set; }
}