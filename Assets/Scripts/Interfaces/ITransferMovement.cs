using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITransferMovement
{
    public void contributeToVelocity(Vector3 contribution);
    public Vector3 collectContributions();
}
