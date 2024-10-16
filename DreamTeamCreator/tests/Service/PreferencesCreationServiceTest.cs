// using DreamTeamCreatorProjectTests.Service;
// namespace DreamTeamCreatorProject.tests.Service;
// using Xunit;

// public class PreferencesCreationServiceTest
// {
//     [Fact]
//     public void
//         PreferencesList_Size_Should_Match_Number_Of_TeamLeads_And_Juniors()
//     {
//         var teamLeads = GetDefaultJuniors();
//         var juniors = GetDefaultTeamLeads();
//         var service = new PreferencesService();
//
//         var wishlist = service.CreatePreferences(juniors, teamLeads);
//
//         Assert.Equal(teamLeads.Count, wishlist.Count);
//     }
//
//     [Fact]
//     public void Each_Junior_Should_Have_All_TeamLeads_In_Their_Preferences()
//     {
//         var teamLeads = GetDefaultJuniors();
//         var juniors = GetDefaultTeamLeads();
//         var service = new PreferencesService();
//
//         var wishlist = service.CreatePreferences(juniors, teamLeads);
//
//         foreach (var juniorWishList in wishlist)
//         {
//             foreach (var teamLead in teamLeads)
//             {
//                 Assert.Contains(teamLead, juniorWishList.PreferredEmployees);
//             }
//         }
//     }
// }