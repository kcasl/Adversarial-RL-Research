using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AdTest : Agent
{
    Rigidbody rBody;
    public Transform Target;

    public Renderer floorRd;
    public Material failMAT, successMAT;
    private Material originMAT;
    private int Cnt = 0;

    private int Episode = 0;
    private int SuccessCnt = 0;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        originMAT = floorRd.material;
    }

    IEnumerator RevertMaterial(Material changeMAT)
    {
        floorRd.material = changeMAT;
        yield return new WaitForSeconds(0.2f);
        floorRd.material = originMAT;
    }


    private void FixedUpdate()
    {
        Vector3 gravityUp = Vector3.zero;
    }
    public override void OnEpisodeBegin()
    {
        Episode++;
        Cnt = 0;
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;
        this.transform.localPosition = new Vector3(Random.Range(-4, -11), -23, Random.Range(6, 12));
        this.transform.localRotation = Quaternion.Euler(0, 45, 0);
        Target.localPosition = new Vector3(Random.Range(3, 9), -23, Random.Range(20, 26));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(Target.localPosition);

        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("Obstacle"))
        {
            SetReward(-1.5f);
            EndEpisode();
            StartCoroutine(RevertMaterial(failMAT));
        }
        if (coll.collider.CompareTag("Wall"))
        {
            SetReward(-1f);
            EndEpisode();
            StartCoroutine(RevertMaterial(failMAT));
        }
        if (coll.collider.CompareTag("Target"))
        {
            SuccessCnt++;
            SetReward(5f);
            EndEpisode();
            StartCoroutine(RevertMaterial(successMAT));
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        var dir = Vector3.zero;
        var rot = Vector3.zero;

        //action값은 4가지 행동 중 하나, switch문 case 4개
        var action = actions.DiscreteActions[0];
        switch (action)
        {
            case 1:
                dir = transform.forward * 1f;
                break;
            case 2:
                dir = transform.forward * -1f;
                break;
            //우회전
            case 3:
                rot = transform.up * 1f;
                break;
            //좌회전
            case 4:
                rot = transform.up * -1f;
                break;
        }
        transform.Rotate(rot, Time.fixedDeltaTime * 200.0f);
        rBody.AddForce(dir * 1.5f, ForceMode.VelocityChange);

        if (Cnt == 0 && distanceToTarget < 5f)
        {
            AddReward(2f / distanceToTarget);
            Cnt += 1;
        }
        //걸음수가 많아질수록 리워드 감소
        AddReward(-1.5f / (float) MaxStep);
    }

    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }*/

    void OnApplicationQuit()
    {
        Debug.Log("Adver" + Episode);
        Debug.Log("Adver" + SuccessCnt);
    }

}
