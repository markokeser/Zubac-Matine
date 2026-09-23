using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zubac.Helpers
{
    /// <summary>
    /// Small presentation helpers shared by the Razor views (icons, badges, formatting).
    /// </summary>
    public static class UiHelpers
    {
        /// <summary>Renders an icon from the inline SVG sprite (Views/Shared/_Icons.cshtml).</summary>
        public static IHtmlContent Icon(this IHtmlHelper html, string name, string? cssClass = null)
        {
            var cls = string.IsNullOrWhiteSpace(cssClass) ? "icon" : $"icon {cssClass}";
            return new HtmlString($"<svg class=\"{cls}\" aria-hidden=\"true\" focusable=\"false\"><use href=\"#i-{name}\"></use></svg>");
        }

        public static string Money(decimal value) =>
            value.ToString("0.00", CultureInfo.InvariantCulture) + " €";

        /// <summary>Invariant decimal for HTML attributes and inputs (always a dot separator).</summary>
        public static string Invariant(decimal value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture);

        public static string Initials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Split(new[] { ' ', '_', '.', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";

            return name.Length >= 2
                ? name.Substring(0, 2).ToUpperInvariant()
                : name.ToUpperInvariant();
        }

        public static string RankName(int rank) => rank switch
        {
            0 => "Bartender",
            1 => "Waiter",
            2 => "Manager",
            3 => "Admin",
            _ => "Staff"
        };

        public static string RankName(string? rank) =>
            int.TryParse(rank, out var value) ? RankName(value) : "Staff";

        public static string RankIcon(int rank) => rank switch
        {
            0 => "martini",
            1 => "utensils",
            2 => "briefcase",
            3 => "crown",
            _ => "user"
        };

        public static string RankTone(int rank) => rank switch
        {
            0 => "violet",
            1 => "info",
            2 => "teal",
            3 => "gold",
            _ => "muted"
        };

        /// <summary>Colour tone used for a menu article type badge.</summary>
        public static string TypeTone(string? type) => (type ?? "").Trim().ToLowerInvariant() switch
        {
            "red wine" => "rose",
            "rose wine" => "rose",
            "white wine" => "amber",
            "beer" => "amber",
            "cocktail" => "violet",
            "spirit" => "gold",
            "nonalcoholic" or "non alcoholic" or "non-alcoholic" => "teal",
            "soup" => "amber",
            "salad" => "success",
            "meat" => "rose",
            "fish" => "info",
            "dessert" or "desert" => "violet",
            _ => "muted"
        };

        public static string TypeLabel(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return "Other";

            return type.Trim().ToLowerInvariant() switch
            {
                "nonalcoholic" or "non alcoholic" => "Non-alcoholic",
                "desert" => "Dessert",
                "rose wine" => "Rosé Wine",
                _ => type.Trim()
            };
        }

        public static string TimeAgo(DateTime created)
        {
            var span = DateTime.Now - created;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} h {span.Minutes} min ago";
            return created.ToString("dd MMM, HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
