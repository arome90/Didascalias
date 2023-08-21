using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
//using UnityEngine.InputSystem.XR;

public class HandsManager : MonoBehaviour
{
	HandVariable handIzq;
	HandVariable handDer;
	float time = 1f;


	private void Start()
	{
		InitHands();
		StartCoroutine(MeasureSpeed());
	}

	private void InitHands()
	{
		handIzq = new HandVariable(TrackedPoseDriver.TrackedPose.LeftPose,  time);
		handDer = new HandVariable(TrackedPoseDriver.TrackedPose.RightPose, time);
	}


	private IEnumerator MeasureSpeed()
	{
		while (true)
		{
			yield return new WaitForSeconds(time);
			UpdateHands();

		}
	}
	private void UpdateHands()
	{
		handDer.UpdateHand();
		handIzq.UpdateHand();
	}

}


public class HandVariable
{
	Vector3 posicion;
	VariableMeasurement posicionMagnitude;
	VariableMeasurementVector3 posicionVector;
	//la amplitud es la distancia entre la mano y una eje central 
	float amplitud ;
	VariableMeasurement distanciaRecorrida;
	float riesgo;

	//Nivel inquietud
	VariableMeasurement velocidad;
	VariableMeasurement aceleracion;

	//aux
	Vector3 lastPosicion ;



	//gESTOS CON MANOS 
	float time;

	TrackedPoseDriver.TrackedPose hand;
	public HandVariable(TrackedPoseDriver.TrackedPose handPose,float tim)
	{
		hand = handPose;
		time = tim;
		InitVariables();
	}

	private void InitVariables()
	{

		Pose po;
		PoseDataSource.TryGetDataFromSource(hand, out po);
		posicion = po.position;
		velocidad = new VariableMeasurement(5);

	}

	

	public void UpdateHand()
	{
		Pose pose;
		if (PoseDataSource.TryGetDataFromSource(hand, out pose))
		{
			lastPosicion = posicion;
			posicion = pose.position;
			//Magnitud
			posicionMagnitude.Variable = posicion.magnitude;
			//Vector3
			posicionVector.Variable = posicion;

			float distance = Vector3.Distance(posicion, lastPosicion);

			distanciaRecorrida.Variable = distance;

			float vel = distance / time;
			velocidad.Variable = vel;
			
			aceleracion.Variable = (velocidad.Variable - distance / time) / time;
		}


	}

}


