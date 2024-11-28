using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using static UnityEngine.GraphicsBuffer;

public class AdversarialAgent : Agent
{
    public GameObject rangeObject;
    public GameObject Block;
    public Transform Target;
    private int Cnt;


    Rigidbody rBody;
    BoxCollider rangeCollider;

    void Start()
    {
        rangeCollider = rangeObject.GetComponent<BoxCollider>();
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.Range(-5, 4), -22, Random.Range(12, 20));
        this.transform.localRotation = Quaternion.Euler(0, -45, 0);
        Cnt = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(Target.localPosition);

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public void SuccessBlocking()
    {
        SetReward(3f);
        EndEpisode();
    }
    public void SuccessBlockingByWall()
    {
        SetReward(0.5f);
        EndEpisode();
    }

    public void FailBlocking()
    {
        SetReward(-5f);
        EndEpisode();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        var dir = Vector3.zero;

        var action = actions.DiscreteActions[0];
        switch (action)
        {
            case 1:
                dir = transform.forward * 1f;
                break;
            case 2:
                dir = transform.forward * -1f;
                break;
            
        }
        rBody.AddForce(dir * 0.1f, ForceMode.VelocityChange);

        if (Cnt == 0 && distanceToTarget < 5f)
        {
            AddReward(0.4f / distanceToTarget);
            Cnt += 1;
        }

        AddReward(-1.5f / (float)MaxStep);
    }

}

