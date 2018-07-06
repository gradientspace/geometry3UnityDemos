using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace g3
{

    public class SimpleMouseManip : MonoBehaviour
    {
        public GameObject TargetObject;

        public float ScrollSpeed = 1.0f;
        public float RotateSpeed = 1.0f;
        public float PanSpeed = 0.1f;

        Vector3f last_mouse_pos;
        Quaternionf initial_rotation;

        Camera mainCamera;
        public void Start()
        {
            mainCamera = Camera.main;
            last_mouse_pos = Input.mousePosition;
            initial_rotation = TargetObject.transform.rotation;
        }

        public void Update()
        {
            Vector3f delta = (Vector3f)Input.mousePosition - last_mouse_pos;
            Transform x = mainCamera.transform;

            if ( Input.mouseScrollDelta.y != 0 ) {
                if (mainCamera.orthographic) {
                    mainCamera.orthographicSize = MathUtil.Clamp(
                        mainCamera.orthographicSize - ScrollSpeed * Input.mouseScrollDelta.y, 1, 1000);
                }
            } else if ( Input.GetMouseButton(1) || Input.GetMouseButton(2) ) {
                mainCamera.transform.position += 
                    (-PanSpeed * delta.x * x.right) +
                    (-PanSpeed * delta.y * x.up);

            } else if ( Input.GetMouseButton(0) ) {
                Quaternionf rotatelr = Quaternionf.AxisAngleD(x.up, -RotateSpeed * delta.x);
                Quaternionf rotateud = Quaternionf.AxisAngleD(x.right, RotateSpeed * delta.y);
                Quaternionf cur_rotation = TargetObject.transform.rotation;
                Quaternion new_rotation = rotatelr * rotateud * cur_rotation;
                TargetObject.transform.rotation = new_rotation;
            }

            last_mouse_pos = Input.mousePosition;
        }


    }

}
