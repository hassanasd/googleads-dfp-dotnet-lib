// Copyright 2013, Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Author: api.anash@gmail.com (Anash P. Oommen)

using Google.Api.Ads.Dfp.Lib;
using Google.Api.Ads.Dfp.v201306;
using Google.Api.Ads.Dfp.Util.v201306;

using System;
using System.Collections.Generic;

namespace Google.Api.Ads.Dfp.Examples.v201306 {
  /// <summary>
  /// This code example updates activity groups by adding a company. To
  /// determine which activity groups exist, run GetAllActivityGroups.cs.
  ///
  /// Tags: ActivityGroupService.getActivityGroup
  /// Tags: ActivityGroupService.updateActivityGroups
  /// </summary>
  class UpdateActivityGroups : SampleBase {
    /// <summary>
    /// Returns a description about the code example.
    /// </summary>
    public override string Description {
      get {
        return "This code example updates activity groups by adding a company. To determine " +
            "which activity groups exist, run GetAllActivityGroups.cs.";
      }
    }

    /// <summary>
    /// Main method, to run this code example as a standalone application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static void Main(string[] args) {
      SampleBase codeExample = new UpdateActivityGroups();
      Console.WriteLine(codeExample.Description);
      codeExample.Run(new DfpUser());
    }

    /// <summary>
    /// Run the code example.
    /// </summary>
    /// <param name="user">The DFP user object running the code example.</param>
    public override void Run(DfpUser user) {
      // Get the ActivityGroupService.
      ActivityGroupService activityGroupService =
          (ActivityGroupService) user.GetService(DfpService.v201306.ActivityGroupService);

      // Set the ID of the activity group and the company to update it with.
      int activityGroupId = int.Parse(_T("INSERT_ACTIVITY_GROUP_ID_HERE"));
      long advertiserCompanyId = long.Parse(_T("INSERT_ADVERTISER_COMPANY_ID_HERE"));

      try {
        // Get the activity group.
        ActivityGroup activityGroup = activityGroupService.getActivityGroup(activityGroupId);

        // Update the companies.
        List<long> companyIds = new List<long>(activityGroup.companyIds);
        companyIds.Add(advertiserCompanyId);
        activityGroup.companyIds = companyIds.ToArray();

        // Update the activity groups on the server.
        ActivityGroup[] activityGroups = activityGroupService.updateActivityGroups(
            new ActivityGroup[] {activityGroup});

        // Display results.
        if (activityGroups != null) {
          foreach (ActivityGroup updatedActivityGroup in activityGroups) {
            Console.WriteLine("Activity group with ID \"{0}\" and name \"{1}\" was updated.",
                updatedActivityGroup.id, updatedActivityGroup.name);
          }
        } else {
          Console.WriteLine("No activity groups were updated.");
        }
      } catch (Exception ex) {
        Console.WriteLine("Failed to update activity groups. Exception says \"{0}\"", ex.Message);
      }
    }
  }
}
