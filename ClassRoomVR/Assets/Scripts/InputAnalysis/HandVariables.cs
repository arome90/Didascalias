using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
//using UnityEngine.InputSystem.XR;
public class HandVariables : MonoBehaviour
{
	Vector3[] posicion = new Vector3[2];
	Vector3[] lastPosicion = new Vector3[2];

	float[] amplitud = new float[2];
	float riesgo;
	float[] distanciaRecorrida = new float[2];

	//Nivel inquietud
	float[] velocidad = new float[2];
	float[] aceleracion = new float[2];

	//gESTOS CON MANOS 

	enum Hand
	{
		left, right
	}

	private void Update()
	{
		UpdateHands();
	}
	private void UpdateHands()
	{
		UpdateHand(Hand.left);
		UpdateHand(Hand.right);

	}


	private void UpdateHand(Hand hand)
	{
		TrackedPoseDriver.TrackedPose poseDriver = hand == Hand.left ? TrackedPoseDriver.TrackedPose.LeftPose : TrackedPoseDriver.TrackedPose.RightPose;
		int handIndex = (int)hand;
		Pose pose;
		if (PoseDataSource.TryGetDataFromSource(poseDriver, out pose))
		{
			lastPosicion[handIndex] = posicion[handIndex];
			posicion[handIndex] = pose.position;
			var distance = Vector3.Distance(posicion[handIndex], lastPosicion[handIndex]);
			// Calcular amplitud
			amplitud[handIndex] = distance;

			// Calcular distancia recorrida
			distanciaRecorrida[handIndex] += distance;
			var vel = distance / Time.deltaTime;
			// Calcular velocidad
			velocidad[handIndex] = vel;

			// Calcular aceleración
			aceleracion[handIndex] = (velocidad[handIndex] - Vector3.Distance(posicion[handIndex], lastPosicion[handIndex]) / Time.deltaTime) / Time.deltaTime;
		}
	}






	// how many left-rights until we say "you said no"
	const float ShakeCountRequired = 6;

	// how much left/right constitutes half of a shape
	const float ShakeAngularRequirement = 3;

	// each phase of "shake only latches for this long
	const float ShakeTimingRequirement = 0.50f;

	// track if we think you are nodding
	float ShakeInProgress;

	float LastSignificantShakeAngle;
	int LastDigitalShake;
	int ShakeCount;

	void UpdateShakeNo()
	{
		// time out
		if (ShakeInProgress > 0)
		{
			ShakeInProgress -= Time.deltaTime;
			if (ShakeInProgress <= 0)
			{
				//Debug.Log( "Timed out - SHAKE NO");
				ShakeCount = 0;
				LastDigitalShake = 0;
			}
		}

		// how far up/down is your head?
		Pose po;
		PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out po);
		float angle = po.rotation.eulerAngles.y;



		// quantize and study this shake
		int shake = 0;      // neutral, -1 is right, +1 is left
		float deltaAngle = Mathf.DeltaAngle(angle, LastSignificantShakeAngle);
		if (deltaAngle < -ShakeAngularRequirement)
		{
			//Debug.Log( "Right");
			shake = -1;
			LastSignificantShakeAngle = angle;
		}
		else
		{
			if (deltaAngle > +ShakeAngularRequirement)
			{
				//Debug.Log( "Left");
				shake = +1;
				LastSignificantShakeAngle = angle;
			}
		}

		// we've gone left / right enough?
		if (shake != 0)
		{
			// and it was in a different direction than before
			if (shake != LastDigitalShake)
			{
				LastDigitalShake = shake;

				ShakeCount++;

				// reset timing, we think you might still be nodding
				ShakeInProgress = ShakeTimingRequirement;

				if (ShakeCount >= ShakeCountRequired)
				{
					ShakeCount = 0;

					// TODO: perhaps inhibit sensing for a second or so?
					//cl.Play();

					Debug.Log("Nope!!");


				}
			}
		}
	}

}