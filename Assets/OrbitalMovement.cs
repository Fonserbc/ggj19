using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalMovement : MonoBehaviour {

    public GameSettings settings;
    public PlayerSync playerSync;

    float orbitDistance;
    float orbitChangeAngle;
    float[] orbitSpeeds;

    private Vector3 orbitAngle;
    private int orbitSpeedIndex;
    public GameObject refOrbiter;
    public GameObject refOrbit;

    // Use this for initialization
    void Start() {

        orbitSpeeds = new float[settings.speedPeriods.Length];
        for (int i = 0; i < orbitSpeeds.Length; ++i)
        {
            orbitSpeeds[i] = 360f / settings.speedPeriods[i];
        }

        orbitChangeAngle = 360f / settings.longitudeDivisions;

        orbitDistance = settings.radiuses[Mathf.Clamp(playerSync.playerID, 0, settings.radiuses.Length)];

        this.refOrbiter.transform.localPosition = new Vector3(0f, this.orbitDistance, 0f);
        this.refOrbiter.transform.rotation = Quaternion.LookRotation(-refOrbiter.transform.position.normalized, -Vector3.right);
    }

    // Update is called once per frame
    void Update() {
        this.UpdateInput();
        this.UpdateRotation();
    }

    private void UpdateInput()
    {
        // Speed up
        if (Input.GetKeyDown("w") || Input.GetKeyDown("up"))
            if (this.orbitSpeedIndex < (this.orbitSpeeds.Length - 1))
                this.orbitSpeedIndex++;

        // Speed down
        if (Input.GetKeyDown("s") || Input.GetKeyDown("down"))
            if (this.orbitSpeedIndex > 0f)
                this.orbitSpeedIndex--;

        // Change Y axis to the left
        if (Input.GetKeyDown("a") || Input.GetKeyDown("left"))
        {
            //this.orbitAngle.x -= orbitChangeAngle;
            this.orbitAngle.y -= orbitChangeAngle * ((int)playerSync.ownState.currentOrbit.z % 180 == 0? 1f : -1f);
        }

        // Change Y axis to the right
        if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
        {

            //this.orbitAngle.x += orbitChangeAngle;
            this.orbitAngle.y += orbitChangeAngle * ((int)playerSync.ownState.currentOrbit.z % 180 == 0 ? 1f : -1f); ;
        }

    }

    // Update the object "position" by simply updating the angle
    private void UpdateRotation()
    {
        playerSync.ownState.orbitGoal = orbitAngle;
        playerSync.ownState.speedGoal = this.orbitSpeeds[this.orbitSpeedIndex];

        playerSync.UpdateState();

        this.refOrbit.transform.localEulerAngles = playerSync.ownState.currentOrbit;
    }
}
