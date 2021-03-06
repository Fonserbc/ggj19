﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbitalMovement : MonoBehaviour {

    public GameSettings settings;
    public PlayerSync playerSync;
    
    float orbitDistance;
    float orbitChangeAngle;
    float[] orbitSpeeds;

    private Vector3 orbitAngle;
    private int orbitSpeedIndex;
    private bool isDockReady;
    public GameObject refOrbiter;
    public GameObject refOrbit;

    [Space]
    public Text directionRight, directionLeft;
    public Text speed;

    public float inputCooldown = 1f;
    float lastInputTimeAcc = 0f;

    // Use this for initialization
    void Start() {

        orbitSpeeds = new float[3];
        int playerShift = playerSync.playerID == 1 ? 0 : 1;

        for (int i = 0; i < orbitSpeeds.Length; ++i)
        {
            orbitSpeeds[i] = 360f / settings.speedPeriods[i + playerShift];
        }

        orbitChangeAngle = 360f / settings.longitudeDivisions;

        orbitDistance = playerSync.isLocal ? settings.outerRadius : settings.innerRadius;

        this.refOrbiter.transform.localPosition = new Vector3(0f, this.orbitDistance, 0f);
        this.refOrbiter.transform.rotation = Quaternion.LookRotation(-refOrbiter.transform.position.normalized, -Vector3.right);
        this.speed.text = "";
        directionLeft.text = "";
        directionRight.text = "";

        orbitSpeedIndex = 1;
        playerSync.ownState.speedGoal = this.orbitSpeeds[this.orbitSpeedIndex];

        orbitAngle.y = playerShift * orbitChangeAngle * Mathf.FloorToInt(settings.longitudeDivisions / 4);
        playerSync.ownState.orbitGoal = orbitAngle;
    }

    // Update is called once per frame
    void Update() {
        if (!Logic.won && playerSync.isLocal) this.UpdateInput();
        this.UpdateRotation();
    }

    private void UpdateInput()
    {
        lastInputTimeAcc -= Time.deltaTime;

        if (!Input.GetKey(KeyCode.Space) && lastInputTimeAcc <= 0f)
        {
            // Speed up
            if (Input.GetKeyDown("w") || Input.GetKeyDown("up"))
            {
                if (this.orbitSpeedIndex < (this.orbitSpeeds.Length - 1))
                    this.orbitSpeedIndex++;

                this.StartCoroutine(this.Speed(this.orbitSpeedIndex));
                lastInputTimeAcc = inputCooldown;
            }

            // Speed down
            if (Input.GetKeyDown("s") || Input.GetKeyDown("down"))
            {
                if (this.orbitSpeedIndex > 0f)
                    this.orbitSpeedIndex--;

                this.StartCoroutine(this.Speed(this.orbitSpeedIndex));
                lastInputTimeAcc = inputCooldown;
            }

            // Change Y axis to the left
            if (Input.GetKeyDown("a") || Input.GetKeyDown("left"))
            {
                //this.orbitAngle.x -= orbitChangeAngle;
                this.orbitAngle.y -= (this.refOrbit.transform.localEulerAngles.z < 180f) ? orbitChangeAngle : -orbitChangeAngle;
                this.StartCoroutine(this.Directions(directionLeft, "< ORBIT " + orbitChangeAngle + "°W"));
                lastInputTimeAcc = inputCooldown;
            }

            // Change Y axis to the right
            if (Input.GetKeyDown("d") || Input.GetKeyDown("right"))
            {

                //this.orbitAngle.x += orbitChangeAngle;
                this.orbitAngle.y += (this.refOrbit.transform.localEulerAngles.z < 180f) ? orbitChangeAngle : -orbitChangeAngle;
                this.StartCoroutine(this.Directions(directionRight, "ORBIT "+ orbitChangeAngle + "°E >"));
                lastInputTimeAcc = inputCooldown;
            }
        }

        playerSync.ownState.orbitGoal = orbitAngle;
    }

    // Update the object "position" by simply updating the angle
    private void UpdateRotation()
    {
        playerSync.ownState.speedGoal = this.orbitSpeeds[this.orbitSpeedIndex];
        playerSync.UpdateState();
        if (playerSync.isLocal || !Logic.won)
        {
            this.refOrbit.transform.localEulerAngles = playerSync.ownState.currentOrbit;
            playerSync.ownState.currentOrbit.z = this.refOrbit.transform.localEulerAngles.z % 360f;
            this.refOrbit.transform.localEulerAngles = playerSync.ownState.currentOrbit;
        }
    }

    private IEnumerator Directions(Text display, string text)
    {
        display.text = text;
        yield return new WaitForSeconds(1f);
        display.text = "";
    }

    private IEnumerator Speed(int speedIndex)
    {
        string text = "";
        for (int i = -1; i < speedIndex; i++)
            text += ">";

        if (this.speed)
        {
            this.speed.text = text;
            yield return new WaitForSeconds(inputCooldown);
            this.speed.text = "";
        }
    }
}
