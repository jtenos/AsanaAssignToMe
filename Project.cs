namespace AsanaAssignToMe
{
    record Project(ProjectData[] data);
    record ProjectData(string gid, string name);
}
