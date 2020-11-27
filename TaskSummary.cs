namespace AsanaAssignToMe
{
    record TaskSummary(TaskSummaryData[] data, NextPageData? next_page);
    record TaskSummaryData(string gid, string name, TaskAssignee? assignee, bool completed);
    record TaskAssignee(string gid, string name);
}
