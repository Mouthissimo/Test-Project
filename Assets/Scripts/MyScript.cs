﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyScript : MonoBehaviour, IWeapon
{
    [Header("Translation & Rotation")]
    [SerializeField] private float m_TranslationSpeed;
    [SerializeField] private float m_RotSpeed;
    Transform m_Transform;
    Rigidbody m_Rigidbody;

    //  Networking
    byte[] m_bytes;

    // raycast
    int m_nRays = 10;
    [SerializeField] LayerMask m_RaycastLayerMask;
    Vector3 m_MinDistancePos = Vector3.zero;


    [Header("Ball Shoot")]
    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] Transform m_BallSpawnPoint;
    [SerializeField] float m_BallInitTranslationSpeed;
    [SerializeField] float m_BallShotCooldownDuration;

    float m_BallShotNextTime;   //  temps dans le futur auquel le tir est autorisé

    private void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        m_BallShotNextTime = Time.time;
    }

    private void Update()
    {
        float minDistance = float.PositiveInfinity;
        Vector3 origin = m_Transform.position + Vector3.up;

        for (int i = 0; i < m_nRays; i++)
        {
            float angle = i * 2 * Mathf.PI / m_nRays;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            Debug.DrawLine(origin, origin + dir * 100f);

            RaycastHit hit;

            if(Physics.Raycast(origin, dir, out hit, float.PositiveInfinity, m_RaycastLayerMask.value))
            {
                float dist = Vector3.Distance(origin, hit.point);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    m_MinDistancePos = hit.point;
                }
            }
        }
    }

    void FixedUpdate()
    {
        float vAxis = Input.GetAxis("Vertical");

        Vector3 newVelocity = vAxis * m_Transform.forward * m_TranslationSpeed;
        Vector3 deltaVelocity = newVelocity - m_Rigidbody.velocity;

        m_Rigidbody.AddForce(deltaVelocity, ForceMode.VelocityChange);

        float hAxis = Input.GetAxis("Horizontal");
        float rotAngle = hAxis * m_RotSpeed * Time.deltaTime;

        Quaternion qRot = Quaternion.AngleAxis(rotAngle, m_Transform.up);
        m_Rigidbody.MoveRotation(qRot * m_Rigidbody.rotation);

        bool isFiring = Input.GetButton("Fire1");
        
        if (isFiring && Time.fixedTime > m_BallShotNextTime)
        {
            ShootBall();
            m_BallShotNextTime = Time.fixedTime + m_BallShotCooldownDuration;
        }

        Debug.DrawRay(m_Transform.position, m_Rigidbody.velocity, Color.red, 2);
    }

    public void ShootBall()
    {
        GameObject newBallGO = Instantiate(m_BallPrefab);
        newBallGO.transform.position = m_BallSpawnPoint.position;
        Rigidbody newBallRB = newBallGO.GetComponent<Rigidbody>();
        newBallRB.AddForce(m_BallSpawnPoint.forward * m_BallInitTranslationSpeed, ForceMode.VelocityChange);
    }


    IEnumerator FetchDataFromWeb(string url)
    {
        UnityWebRequest request = new UnityWebRequest(url);
        AsyncOperation async = request.SendWebRequest();
        yield return async;

        if (string.IsNullOrEmpty(request.error))
        {
             m_bytes = request.uploadHandler.data;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_MinDistancePos, 0.25f);
    }
}
