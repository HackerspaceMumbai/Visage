using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Linq;

namespace Visage.FrontEnd.Shared.Helpers;

public static class BrowserFileExtensions
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };

    /// <summary>
    /// Checks if the file has an allowed image extension (.jpg, .jpeg, .png).
    /// </summary>
    /// <param name="file">The browser file to check.</param>
    /// <returns>True if the file extension is allowed; otherwise, false.</returns>
    public static bool HasAllowedImageExtension(this IBrowserFile file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.Name))
            return false;

        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        return AllowedImageExtensions.Contains(extension);
    }
}