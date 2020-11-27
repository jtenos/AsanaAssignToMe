namespace AsanaAssignToMe
{
    record AssignUserRequest(AssignUserRequestData data);
    record AssignUserRequestData(string assignee);
    record AssignUserResponse(TaskSummaryData data);
}
