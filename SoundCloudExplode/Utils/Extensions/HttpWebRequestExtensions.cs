using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace SoundCloudExplode.Utils.Extensions;

internal static class HttpWebRequestExtensions
{
    static string[] RestrictedHeaders = new string[] {
        "Accept",
        "Connection",
        "Content-Length",
        "Content-Type",
        "Date",
        "Expect",
        "Host",
        "If-Modified-Since",
        "Keep-Alive",
        "Proxy-Connection",
        "Range",
        "Referer",
        "Transfer-Encoding",
        "User-Agent"
    };

    static readonly Dictionary<string, PropertyInfo> HeaderProperties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

    static HttpWebRequestExtensions()
    {
        var type = typeof(HttpWebRequest);
        foreach (string header in RestrictedHeaders)
        {
            string propertyName = header.Replace("-", "");
            var headerProperty = type.GetProperty(propertyName)!;
            HeaderProperties[header] = headerProperty;
        }
    }

    public static void SetRawHeader(this HttpWebRequest request, string name, string value)
    {
        if (HeaderProperties.ContainsKey(name))
        {
            PropertyInfo property = HeaderProperties[name];
            if (property.PropertyType == typeof(DateTime))
                property.SetValue(request, DateTime.Parse(value), null);
            else if (property.PropertyType == typeof(bool))
                property.SetValue(request, bool.Parse(value), null);
            else if (property.PropertyType == typeof(long))
                property.SetValue(request, long.Parse(value), null);
            else
                property.SetValue(request, value, null);
        }
        else
        {
            request.Headers[name] = value;
        }
    }
}