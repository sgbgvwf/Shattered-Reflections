using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Combat.Move;

[RequireComponent(typeof(MoveController))]
public class VisualController : MonoBehaviour
{
    public Camera mainCamera;

    
    [SerializeField]
    public enum MoveTowardVisualModes
    {
        AlwaysTrace,
        OnlyMove,
        NeverTrace,
    }
    [Header("跟随视野转动")]
    public MoveTowardVisualModes visualModes = MoveTowardVisualModes.OnlyMove;

    [Header("旋转插值")]
    public bool openSlerp = true;
    [Range(0f, 100f)]
    public float slerpSpinVelocity = 20f;


    private Transform _cameraTransform;
    private MoveController _moveController;



    private Vector3 direction;
    private bool isTurn;


    private void Awake()
    {
        _cameraTransform = mainCamera.gameObject.GetComponent<Transform>();
        _moveController = GetComponent<MoveController>();
    }


    private void Update()
    {
        visualSpinDeal();
    }

    private void visualSpinDeal()
    {
        isTurn = (direction.normalized - transform.forward).magnitude < 0.01f ? true : false;
        // Debug.Log((direction.normalized - transform.forward).magnitude);

        if (visualModes == MoveTowardVisualModes.NeverTrace)
        {
            return;
        }
        if (visualModes == MoveTowardVisualModes.OnlyMove)
        {


            if (_moveController._inputMove.magnitude == 0)
            {
                if (isTurn)
                {
                    return;
                }
            }

        }



        SpinToPlayer();
    }

    private void SpinToPlayer()
    {



        direction = transform.position - _cameraTransform.position;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        if (!openSlerp)
        {
            transform.rotation = lookRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * slerpSpinVelocity);
        }


    }



}
