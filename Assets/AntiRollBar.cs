using UnityEngine;

/// <summary>
/// Simulates an stabilizer bar across two wheels by applying counter forces
/// based on the difference in suspension compression between the right and left wheel.
/// Reduces body roll while taking corners or crossing uneven terrain.
/// </summary>
public class AntiRollBar : MonoBehaviour
{

	public WheelCollider WheelL;
	public WheelCollider WheelR;
	public float AntiRoll = 5000.0f;

	private Rigidbody vehicle;

	/// <summary>
	/// Stores the rigidBody reference
	/// </summary>
	void Start()
	{
		vehicle = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Called by Unity once per Physics update. Calculates suspension travel on both wheels and applies anti-roll
	/// forces at the wheel positions to counteract body roll.
	/// </summary>
	void FixedUpdate()
	{
		WheelHit hit;
		float travelL = 1.0f;
		float travelR = 1.0f;


		bool groundedL = WheelL.GetGroundHit(out hit);
		if (groundedL)
		{
			travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
		}

		bool groundedR = WheelR.GetGroundHit(out hit);
		if (groundedR)
		{
			travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
		}

		float antiRollForce = (travelL - travelR) * AntiRoll;

		if (groundedL)
			vehicle.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);

		if (groundedR)
			vehicle.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);
	}
}
