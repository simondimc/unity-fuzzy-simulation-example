using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAgentManager : AgentManager {
    
    public void ToggleAttractionDrive(bool v) {
        this.FuzzyController.SetDriveEnabled("attraction", v);
    }

    public void ToggleRepulsionDrive(bool v) {
        this.FuzzyController.SetDriveEnabled("repulsion", v);
    }

    public void ToggleAlignmentDrive(bool v) {
        this.FuzzyController.SetDriveEnabled("alignment", v);
    }

    public void ToggleWallRepulsionDrive(bool v) {
        this.FuzzyController.SetDriveEnabled("wall repulsion", v);
    }

}
