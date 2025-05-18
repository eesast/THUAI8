using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public enum CameraMode
    {
        Free,
        Follow,
    }
    public CameraMode cameraMode = CameraMode.Free;
    public AnimationCurve cameraScaleCurve;
    public RectTransform sideBarRect;
    public float currentScaleTime = 0.5f, basicCameraScale, currentScale;
    public float cameraSpeedMax = 1.5f, cameraSpeed;
    private new Camera camera;
    private bool rendererUpdateFlag;
    Vector2 mousePos;
    void Start()
    {
        camera = GetComponent<Camera>();
        currentScale = cameraScaleCurve.Evaluate(currentScaleTime) * basicCameraScale;
        camera.orthographicSize = currentScale;
        RenderManager.Instance.onRenderEvent += () =>
        {
            if (rendererUpdateFlag)
            {
                var targetPos = PlayerControl.Instance.selectedCharacter.transform.position;
                targetPos.z = -10;
                camera.transform.position = targetPos;
            }
        };
    }

    void Update()
    {
        mousePos = Input.mousePosition;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
         || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)
         || PlayerControl.Instance.selectedCharacter == null)
            cameraMode = CameraMode.Free;
        switch (cameraMode)
        {
            case CameraMode.Free:
                rendererUpdateFlag = false;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
                 || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
                {
                    cameraSpeed = Mathf.Lerp(cameraSpeed, cameraSpeedMax, 0.1f);
                    if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && transform.position.x >= -0.5f)
                        transform.Translate(cameraSpeed * currentScale * Time.deltaTime * Vector3.left);
                    if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && transform.position.x <= 49.5f)
                        transform.Translate(cameraSpeed * currentScale * Time.deltaTime * Vector3.right);
                    if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) && transform.position.y <= 49.5f)
                        transform.Translate(cameraSpeed * currentScale * Time.deltaTime * Vector3.up);
                    if ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) && transform.position.y >= -0.5f)
                        transform.Translate(cameraSpeed * currentScale * Time.deltaTime * Vector3.down);
                }
                else
                {
                    cameraSpeed = Mathf.Lerp(cameraSpeed, 0, 0.1f);
                }
                break;
            case CameraMode.Follow:
                if (PlayerControl.Instance.selectedCharacter)
                {
                    Vector3 targetPos = PlayerControl.Instance.selectedCharacter.transform.position;
                    targetPos.z = -10;
                    if ((transform.position - targetPos).sqrMagnitude > 0.5f)
                    {
                        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 20);
                        rendererUpdateFlag = false;
                    }
                    // Move to Start() and registered as an event of RenderManager in order to avoid dazzling
                    // else
                    //     transform.position = targetPos;
                    else rendererUpdateFlag = true;

                }
                break;
        }

        if (mousePos.x > 0 && mousePos.x < Screen.width && mousePos.y > 0 && mousePos.y < Screen.height)
        {
            var targetPos = cameraMode switch
            {
                CameraMode.Free => camera.ScreenToWorldPoint(mousePos),
                CameraMode.Follow => PlayerControl.Instance.selectedCharacter.transform.position,
                _ => camera.ScreenToWorldPoint(mousePos),
            }; 
            if (targetPos.x > -1 &&
                targetPos.x < 50 &&
                targetPos.y > -1 &&
                targetPos.y < 50)
            {
                if (!sideBarRect || (sideBarRect && !RectTransformUtility.RectangleContainsScreenPoint(sideBarRect, Input.mousePosition, camera)))
                {
                    if (Input.mouseScrollDelta.y < 0)
                    {
                        currentScaleTime = Mathf.Min(1f, currentScaleTime + 0.02f);

                        currentScale = cameraScaleCurve.Evaluate(currentScaleTime) * basicCameraScale;
                        camera.transform.position = targetPos +
                            currentScale / camera.orthographicSize * (camera.transform.position - targetPos);
                        camera.orthographicSize = currentScale;
                    }
                    if (Input.mouseScrollDelta.y > 0)
                    {
                        currentScaleTime = Mathf.Max(0.02f, currentScaleTime - 0.02f);
                        currentScale = cameraScaleCurve.Evaluate(currentScaleTime) * basicCameraScale;
                        camera.transform.position = targetPos +
                            currentScale / camera.orthographicSize * (camera.transform.position - targetPos);
                        camera.orthographicSize = currentScale;
                    }
                }
            }
        }
    }
}
