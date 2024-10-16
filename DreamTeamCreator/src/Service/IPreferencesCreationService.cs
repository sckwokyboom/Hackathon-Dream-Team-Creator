using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public interface IPreferencesCreationService
{
    // List<EmployeePreferences> CreateRandomTeamMemberPreferences(PotentialTeamMembers potentialTeamMembers);
    PotentialTeamMembersPreferences GeneratePreferences(PotentialTeamMembers potentialTeamMembers);

}