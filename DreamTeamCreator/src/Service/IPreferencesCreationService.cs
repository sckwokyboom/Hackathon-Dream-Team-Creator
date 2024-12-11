using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public interface IPreferencesCreationService
{
    PotentialTeamMembersPreferences GeneratePreferences(PotentialTeamMembers potentialTeamMembers);
}