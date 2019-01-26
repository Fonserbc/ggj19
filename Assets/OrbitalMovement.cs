using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalMovement : MonoBehaviour {

    public float orbitDistance;
    public float orbitChangeAngle;
    public float[] orbitSpeeds;
    public bool inputActive;

    private Vector3 orbitAngle;
    private int orbitSpeedIndex;
    private GameObject refOrbiter;
    private GameObject refOrbit;

    // Use this for initialization
    void Start() {

        this.refOrbit = this.transform.Find("Orbit").gameObject;
        this.refOrbiter = this.refOrbit.transform.Find("Orbiter").gameObject;
    }

    // Update is called once per frame
    void Update() {
        if (this.inputActive)
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
            this.orbitAngle.x -= orbitChangeAngle;
            this.orbitAngle.y -= orbitChangeAngle;
        }

        // Change Y axis to the right
        if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
        {

            this.orbitAngle.x += orbitChangeAngle;
            this.orbitAngle.y += orbitChangeAngle;
        }

    }

    // Update the object "position" by simply updating the angle
    private void UpdateRotation()
    {
        this.orbitAngle.z += this.orbitSpeeds[this.orbitSpeedIndex];
        this.refOrbit.transform.localEulerAngles = orbitAngle;
        this.refOrbiter.transform.localPosition = new Vector3(this.orbitDistance, 0f, 0f);
    }
}
