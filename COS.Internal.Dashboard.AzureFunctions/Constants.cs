namespace COS.Internal.Dashboard.AzureFunctions;

public static class Constants
{
	public const string PlanningCenterAppID = "PLANNING_CENTER_APP_ID";
	public const string PlanningCenterSecret = "PLANNING_CENTER_SECRET";
	public const string PlanningCenterApiUrl = "PLANNING_CENTER_API_URL";

	public const string HttpClientName = "DefaultClient";

	public const string DashboardFunctionName = "CosDashboard";
	public const string DashboardFunctionRoute = $"{DashboardFunctionName}/{{*remainder}}";

	public const string AuthenticationFunctionName = "Authenticate";
}
