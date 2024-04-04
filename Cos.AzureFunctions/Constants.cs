using System;

namespace Cos.AzureFunctions;

public static class EnvironmentVariables
{
	public static class PlanningCenter
	{
		public static class BaseUrls
		{
			public const string Api = "PLANNING_CENTER_API_URL";
			public const string Avatars = "PLANNING_CENTER_AVATARS_URL";
		}

		public static class Credentials
		{
			public const string AppID = "PLANNING_CENTER_APP_ID";
			public const string Secret = "PLANNING_CENTER_SECRET";
		}
	}

	public static class GitHub
	{
		public const string BaseUrl = "GITHUB_API_URL";
		public const string ApiToken = "GITHUB_TOKEN";
		public const string UserAgent = "GITHUB_USER_AGENT";
	}

	public static class Paths
	{
		public const string Resources = "AZURE_STORAGE_RESOURCES_PATH";
	}
}

public static class HttpClientName
{
	public const string Api = "ApiClient";
	public const string Avatars = "AvatarsClient";
	public const string GitHub = "GitHub";
}

public static class Paths
{
	public static readonly string Calendar = 
	$"{Environment.GetEnvironmentVariable(EnvironmentVariables.Paths.Resources) ?? string.Empty}/ical";
	public const string HeadHash = "repos/twcrews/COS.Internal.Dashboard/commits/master";
}

public static class AzureFunctions
{
	public static class Dashboard
	{
		public const string Name = "CosDashboard";
		public const string Route = $"{Name}/{{*remainder}}";
	}

	public static class Avatars
	{
		public const string Name = "Avatars";
		public const string Route = $"{Name}/{{*remainder}}";
	}

	public static class Calendar
	{
		public const string Name = "Calendar";
		public const string Route = $"{Name}/{{calendarName:maxlength(50)?}}";
	}

	public static class Authentication
	{
		public const string Name = "Authenticate";
	}

	public static class Versioning
	{
		public static class HeadHash
		{
			public const string Name = "HeadHash";
		}
	}

	public static class Diagnostic
	{
		public static class FileShare
		{
			public const string Name = "FileShare";
			public const string Route = $"{Name}/{{*remainder}}";
		}
	}
}