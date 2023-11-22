using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
/// <summary>
/// Clase que actualiza la informacion de cada mano según la variable time 
/// Primero inicia la clase que gestiona cada mano y empieza una corrutina para medir las variables 
/// </summary>
public class HandsManager 
{
	public HandVariables handIzq;
	public HandVariables handDer;

	public HandsManager(int windowSize)
	{
		handIzq = new HandVariables(TrackedPoseDriver.TrackedPose.LeftPose,windowSize);
		handDer = new HandVariables(TrackedPoseDriver.TrackedPose.RightPose,windowSize);
	}

	//Actualiza los datos de las manos
	public void UpdateHands(float time)
	{
		handDer.UpdateHand(time);
		handIzq.UpdateHand(time);
	}

}


public class HandVariables
{
	public Vector3 posicion;
	VariableMeasurement posicionMagnitude;
	VariableMeasurementVector3 posicionVector;
	VariableMeasurement distanciaRecorrida;
	public VariableMeasurement velocidad;
	VariableMeasurement aceleracion;
	//aux
	Vector3 lastPosicion ;


	//gestos  
	//float riesgo;
	//Nivel inquietud
	//la amplitud es la distancia entre la mano y un eje central 
	//float amplitud ;

	TrackedPoseDriver.TrackedPose hand;
	private int windowSize = 5;
	public HandVariables(TrackedPoseDriver.TrackedPose handPose,int size)
	{
		windowSize = size;
		hand = handPose;
		InitVariables();
	}
	/// <summary>
	/// Inicializa las variables
	/// </summary>
	private void InitVariables()
	{
		//El tamaño de ventana de las estadisticas dinamicas
		Pose po;
		PoseDataSource.TryGetDataFromSource(hand, out po);
		posicion = po.position;
		lastPosicion = posicion;
		velocidad = new VariableMeasurement(windowSize); 
		posicionMagnitude = new VariableMeasurement(windowSize); 
		posicionVector = new VariableMeasurementVector3(windowSize); 
		distanciaRecorrida = new VariableMeasurement(windowSize); 
		aceleracion = new VariableMeasurement(windowSize); 

	}

	

	public void UpdateHand(float time)
	{
		Pose pose;
		//Obtiene un struct con la posicion y rotacion de la mano 
		if (PoseDataSource.TryGetDataFromSource(hand, out pose))
		{
			lastPosicion = posicion;
			posicion = pose.position;
			//Magnitud del vector 
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


