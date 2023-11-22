namespace COS.Internal.Dashboard.AzureFunctions;

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
}

public static class HttpClientNames
{
	public const string Api = "ApiClient";
	public const string Avatars = "AvatarsClient";
}

public static class Paths
{
	public const string Resources = "/mnt/resources";
	public const string Calendar = $"{Resources}/ical";
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

	public static class Diagnostic
	{
		public static class FileShare
		{
			public const string Name = "FileShare";
			public const string Route = $"{Name}/{{*remainder}}";
		}
	}
}