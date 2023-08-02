using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
public class HeadVariables : MonoBehaviour
{

    private Vector3 miradaPoint;
    private Vector3 position;
    private Vector3 lastPosition;
    //private float aceleracion;

    private float distance;

       
    private VariableMeasurementFloat velocidad;

    private void Start()
    {
		InitMotions();
		Pose po;
        PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out po);
        position = po.position;
        velocidad = gameObject.AddComponent<VariableMeasurementFloat>();
		velocidad.Set();
        StartCoroutine(MeasureSpeed());

    }

    private IEnumerator MeasureSpeed()
    {
        float time = 1f;
        while (true)
        {
            yield return new WaitForSeconds(time);
            UpdateHead();

        }
    }
    private void UpdateHead()
    {

        Pose pose;
        if (PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out pose))
        {
            lastPosition = position;
            position = pose.position;

            distance = Vector3.Distance(position, lastPosition);
            velocidad.Variable = distance / Time.deltaTime;
        }

        //PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Center, out pose);
        //miradaPoint = pose.position;

       // Debug.Log(position + "H");

        //PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.RightEye, out pose);
        //Debug.Log(pose.position +"R");
        //PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.LeftEye, out pose);
        //Debug.Log(pose.position + "L");

    }


	struct Motion
	{
		public float inProgress;
		public float lastSignificantAngle;
		public int lastDigital;
		public int count;
		public string message;
	}
	struct Requirement
	{
		public int count;
		public float timing;
		public float angular;
	}

	Motion shake;
	Motion nod;
	Requirement req;
	private void InitMotions()
	{
		req = new Requirement { count = 6, angular = 3f, timing = 0.75f };
		shake.message = "NO";
		nod.message = "SI";
	}

	private void Update()
    {
		Pose pose;
		PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out pose);
		UpdateMotion(ref shake, GetShakeAngle(pose.rotation));
		UpdateMotion(ref nod,GetNodAngle(pose.rotation));
	}
	
	bool UpdateMotion(ref Motion mot, float angle)
	{
		if (mot.inProgress > 0)
		{
			mot.inProgress -= Time.deltaTime;
			if (mot.inProgress <= 0)
			{
				mot.count = 0;
				mot.lastDigital = 0;
			}
		}

		int gesture = 0;
		float deltaAngle = Mathf.DeltaAngle(angle, mot.lastSignificantAngle);

		if (deltaAngle < -req.angular)
		{
			gesture = -1;
			mot.lastSignificantAngle = angle;
		}
		else if (deltaAngle > +req.angular)
		{
			gesture = +1;
			mot.lastSignificantAngle = angle;
		}

		if (gesture != 0 && gesture != mot.lastDigital)
		{
			mot.lastDigital = gesture;
			mot.count++;
			mot.inProgress = req.timing;

			if (mot.count >= req.count)
			{
				mot.count = 0;
				Debug.Log(mot.message);
				return true;
			}
		}
		return false;
	}


    private float GetShakeAngle(Quaternion rot)
    {
        return  rot.eulerAngles.y;
    }
    private float GetNodAngle(Quaternion rot)
    {
        Vector3 forward = rot * Vector3.forward;
        forward = Vector3.Normalize(forward);

        float forwardY = forward.y;
        return  Mathf.Asin(forwardY) * Mathf.Rad2Deg;
    }

}